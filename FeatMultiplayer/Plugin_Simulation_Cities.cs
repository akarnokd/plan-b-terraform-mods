using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SCities), nameof(SCities.Update))]
        static bool Patch_SCities_Update_Pre()
        {
            return multiplayerMode != MultiplayerMode.Client;
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
