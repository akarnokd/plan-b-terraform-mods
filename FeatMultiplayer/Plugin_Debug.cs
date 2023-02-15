// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool debugHexToggle;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static void Patch_SSceneHud_OnUpdate_HexDebug()
        {
            if (modEnabled.Value)
            {
                if (Input.GetKeyDown(KeyCode.KeypadDivide))
                {
                    debugHexToggle = !debugHexToggle;
                }
                var coords = GScene3D.mouseoverCoords;
                if (Input.GetKeyDown(KeyCode.Mouse0) && debugHexToggle)
                {
                    LogDebug("Hex info @ " + coords);
                    var flags = (int)GHexes.flags[coords.x, coords.y];
                    LogDebug("    Flags: " + string.Format("{0:X4} = ", (uint)flags) + FlagsToStr((uint)flags));
                    var groundId = GHexes.groundId[coords.x, coords.y];
                    LogDebug("    Ground: id = " + groundId);
                    var groundData = GHexes.groundData[coords.x, coords.y];
                    LogDebug("          data = " + string.Format("{0} (0x{0:X4})", groundData));
                    var contentId = GHexes.contentId[coords.x, coords.y];
                    var contentAt = ContentAt(coords);
                    LogDebug("    Content: id = " + contentId + ", type = " + (contentAt != null ? contentAt.codeName : "null"));
                    var contentData = GHexes.contentData[coords.x, coords.y];
                    LogDebug("           data = " + string.Format("{0} (0x{0:X4})", contentData));
                    var stacks = GHexes.stacks[coords.x, coords.y];
                    if (stacks != null && stacks.stacks != null)
                    {
                        LogDebug("         stacks = " + stacks.stacks.Length);
                        for (int i = 0; i < stacks.stacks.Length; i++)
                        {
                            var stack = stacks.stacks[i];
                            LogDebug("                [" + i + "] : nb = " + stack.nb + ", nbMax = " + stack.nbMax + ", nbBooked = " 
                                + stack.nbBooked + ", item = " + (stack.item?.codeName ?? "null") + ", demand = " + stack.demand);
                        }
                    }
                    else
                    {
                        LogDebug("         stacks = none");
                    }
                }
            }
        }

        static string FlagsToStr(uint flag)
        {
            List<string> list = new();
            Array arr = typeof(GHexes.Flag).GetEnumValues();

            for (int i = 0; i < arr.Length; i++)
            {
                var f = (uint)arr.GetValue(i);
                if ((flag & f) != 0)
                {
                    list.Add(((GHexes.Flag)f).ToString());
                }
            }
            return string.Join(", ", list);
        }
    }
}
