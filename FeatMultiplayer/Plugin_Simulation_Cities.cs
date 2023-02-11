// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System.Collections.Generic;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        static void SendUpdateContentData(int2 coords)
        {
            var msg = new MessageUpdateContentData();
            msg.GetSnapshot(coords);
            SendAllClients(msg);
        }

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update01s))]
        static void Patch_CCity_Update01s_Pre(CCity __instance, out HashSet<int2> __state)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                __state = null;
                return;
            }
            __state = new(__instance.hexes);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update01s))]
        static void Patch_CCity_Update01s_Post(CCity __instance, ref HashSet<int2> __state) 
        { 
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }
            foreach (var h in __instance.hexes)
            {
                __state.Add(h);
            }
            var msg = new MessageUpdateCity();
            msg.GetSnapshot(__instance, __state);
            SendAllClients(msg);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update1s))]
        static void Patch_CCity_Update1s_Pre(CCity __instance, out HashSet<int2> __state)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                __state = null;
                return;
            }
            __state = new(__instance.hexes);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CCity), nameof(CCity.Update1s))]
        static void Patch_CCity_Update1s_Post(CCity __instance, ref HashSet<int2> __state)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }
            foreach (var h in __instance.hexes)
            {
                __state.Add(h);
            }
            var msg = new MessageUpdateCity();
            msg.GetSnapshot(__instance, __state);
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
                SendUpdateContentData(coords);
            }
        }

        // ------------------------------------------------------------------------------
        // Message receviers
        // ------------------------------------------------------------------------------

        static void ReceiveMessageUpdateCity(MessageUpdateCity msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageUpdateCity: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageUpdateCity: Handling " + msg.GetType());

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
