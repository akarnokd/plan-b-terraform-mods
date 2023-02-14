// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// The vanilla calls this method to randomly create a city block,
        /// which is not good in MP because it overwrites the city center
        /// of the host in contentId[center].
        /// </summary>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentCity), nameof(CItem_ContentCity.CreateCenter))]
        static bool Patch_CItem_ContentCity_CreateCenter()
        {
            return multiplayerMode != MultiplayerMode.ClientJoin;
        }

        static readonly HashSet<int2> cityHexChanges = new();
        static readonly HashSet<int2> cityInOutChanges = new();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update01s))]
        static bool Patch_CCity_Update01s_Pre(CCity __instance, 
            CCityInOutData ___inData,
            CCityInOutData ___outData)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                ___inData.Update01s();
                ___outData.Update01s();
                return false;
            }
            cityInOutChanges.Clear();
            cityHexChanges.Clear();
            foreach (var h in __instance.hexes)
            {
                cityHexChanges.Add(h);
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update01s))]
        static void Patch_CCity_Update01s_Post(CCity __instance) 
        { 
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }
            foreach (var h in __instance.hexes)
            {
                cityHexChanges.Add(h);
            }
            var msg = new MessageUpdateCity();
            msg.GetSnapshot(__instance, cityHexChanges);
            SendAllClients(msg);

            foreach (var h in cityInOutChanges)
            {
                var msgs = new MessageUpdateStacksAt();
                msgs.GetSnapshot(h);
                SendAllClients(msgs);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update1s))]
        static void Patch_CCity_Update1s_Pre(CCity __instance)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }
            cityHexChanges.Clear();
            foreach (var h in __instance.hexes)
            {
                cityHexChanges.Add(h);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update1s))]
        static void Patch_CCity_Update1s_Post(CCity __instance)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }
            foreach (var h in __instance.hexes)
            {
                cityHexChanges.Add(h);
            }
            var msg = new MessageUpdateCity();
            msg.GetSnapshot(__instance, cityHexChanges);
            SendAllClients(msg);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.Update01s))]
        static bool Patch_CItem_ContentCityInOut_Update01s_Pre()
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.Update01s))]
        static void Patch_CItem_ContentCityInOut_Update01s_Post(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateContentData();
                msg.GetSnapshot(coords);
                SendAllClients(msg);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CCityInOutData), nameof(CCityInOutData.Update01s))]
        static bool Patch_CCityInOutData_Update01s_Pre(
            CItem_ContentCityInOut ___itemInOut,
            CCity ___city, ref int ___recipeIndex, 
            ref CRecipe ___recipe)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                // make sure the in/out is updated with the current recipe
                for (int i = 0; i < ___itemInOut.recipes.Length; i++)
                {
                    if (___city.population >= ___itemInOut.recipes[i].cityPop)
                    {
                        ___recipeIndex = i;
                        ___recipe = ___itemInOut.recipes[i];
                        //LogDebug("City " + ___city.name + "Setting recipe to " + ___recipe.codeName);
                    }
                }
                // but don't do the rest of the updates
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentCity), nameof(CItem_ContentCity.FillSelectionUI))]
        static void Patch_CItem_ContentCity_FillSelectionUI(int2 coords, CItemData ___dataCityID)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                CCity ccity = GGame.cities[___dataCityID.GetValue(coords)];
                if (ccity != null)
                {
                    // make sure the cities have their recipes selected.
                    CCityInOutData inOutData = ccity.GetInOutData(false);
                    if (inOutData.recipe == null)
                    {
                        inOutData.Update01s();
                    }

                    CCityInOutData inOutData2 = ccity.GetInOutData(true);
                    if (inOutData2.recipe == null)
                    {
                        inOutData2.Update01s();
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.ProcessCityRecipeIFP))]
        static void Patch_CItem_ContentCityInOut_ProcessCityRecipeIFP(int2 coords)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                cityInOutChanges.Add(coords);
            }
        }
        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        public static bool logDebugCityMessages;

        static void ReceiveMessageUpdateCity(MessageUpdateCity msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                if (logDebugCityMessages)
                {
                    LogDebug("ReceiveMessageUpdateCity: Deferring " + msg.GetType());
                }
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                if (logDebugCityMessages)
                {
                    LogDebug("ReceiveMessageUpdateCity: Handling " + msg.GetType());
                }

                var city = GGame.cities.Find(v => v != null && v.cityId == msg.city.id);

                if (city != null)
                {
                    msg.ApplySnapshot(city);

                    SViewWorld sViewWorld = SSingleton<SViewWorld>.Inst;

                    foreach (var hex in msg.updatedHexes)
                    {
                        var coords = hex.coords;
                        Haxx.SBlocks_OnChangeItem(coords, true, false, false);
                        sViewWorld.OnBuildItem_UpdateTxWorld(coords);
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageUpdateCity: City not found. id = " + msg.city.id);
                }
            }
            else
            {
                LogWarning("ReceiveMessageUpdateCity: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
