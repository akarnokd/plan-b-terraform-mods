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
        /// Get the building/content at a specified location.
        /// </summary>
        /// <param name="coords">The coordinates.</param>
        /// <returns>The content object or null if nothing there.</returns>
        internal static CItem_Content ContentAt(in int2 coords)
        {
            return SSingleton<SWorld>.Inst.GetContent(coords);
        }

        /// <summary>
        /// Returns a dictionary of codeName to CItem for locating items via their codeName.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, CItem> GetItemsDictionary()
        {
            var itemsDictionary = new Dictionary<string, CItem>();
            for (int i = 1; i < GItems.items.Count; i++)
            {
                CItem item = GItems.items[i];
                itemsDictionary.Add(item.codeName, item);
            }

            return itemsDictionary;
        }
    }
}
