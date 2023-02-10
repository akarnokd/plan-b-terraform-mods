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

        /// <summary>
        /// Returns a dictionary of drone id to CDrone instances.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<int, CDrone> GetDronesDictionary()
        {
            var result = new Dictionary<int, CDrone>();

            foreach (var drone in GDrones.drones)
            {
                result.Add(drone.id, drone);
            }

            return result;
        }

        /// <summary>
        /// Returns a dictionary of vehicle id to CVehicle instances.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<int, CVehicle> GetVehiclesDictionary()
        {
            var result = new Dictionary<int, CVehicle>();

            foreach (var line in GWays.lines)
            {
                foreach (var vehicle in line.vehicles)
                {
                    result.Add(vehicle.id, vehicle);
                }
            }

            return result;
        }

        internal static Dictionary<int, CLine> GetLineDictionary()
        {
            var result = new Dictionary<int, CLine>();
            for (int i = 1; i < GWays.lines.Count; i++)
            {
                CLine line = GWays.lines[i];
                result.Add(line.id, line);
            }
            return result;
        }
    }
}
