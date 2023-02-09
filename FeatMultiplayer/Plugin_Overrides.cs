using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

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
    }
}
