// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
                DebugHex();

                /*
                if (multiplayerMode == MultiplayerMode.Client || multiplayerMode == MultiplayerMode.ClientJoin)
                {
                    LogDebug("FlagsDebug: " + FlagsToStr((uint)GHexes.flags[1270, 406]));
                }
                */
            }
        }


        static void DebugHex()
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

        internal static string FlagsToStr(uint flag)
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

        // Intercept the enumerators
        internal static IEnumerator InterceptEnumerator(IEnumerator en)
        {
            for (; ; )
            {
                bool has;

                try
                {
                    has = en.MoveNext();
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    throw ex;
                }
                if (has)
                {
                    object v;

                    try
                    {
                        v = en.Current;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        throw ex;
                    }
                    if (v is IEnumerator en2)
                    {
                        yield return InterceptEnumerator(en2);
                    }
                    yield return v;
                }
                else
                {
                    yield break;
                }
            }
        }

        static CallTelemetry viewBlocksTelemetry = new("SViewBlocks");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "GenerateBlockFull")]
        static void Patch_SViewBlocks_GenerateBlockFull_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "GenerateBlockFull")]
        static void Patch_SViewBlocks_GenerateBlockFull_Post()
        {
            viewBlocksTelemetry.AddTelemetry("GenerateBlockFull");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "RegenerateBlockModels")]
        static void Patch_SViewBlocks_RegenerateBlockModels_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "RegenerateBlockModels")]
        static void Patch_SViewBlocks_RegenerateBlockModels_Post()
        {
            viewBlocksTelemetry.AddTelemetry("RegenerateBlockModels");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "UpdateGroundColors")]
        static void Patch_SViewBlocks_UpdateGroundColors_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "UpdateGroundColors")]
        static void Patch_SViewBlocks_UpdateGroundColors_Post()
        {
            viewBlocksTelemetry.AddTelemetry("UpdateGroundColors");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "CreateNewViewBlock", new Type[] { typeof(bool) })]
        static void Patch_SViewBlocks_CreateNewViewBlock_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "CreateNewViewBlock", new Type[] { typeof(bool) })]
        static void Patch_SViewBlocks_CreateNewViewBlock_Post()
        {
            viewBlocksTelemetry.AddTelemetry("CreateNewViewBlock");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "GetViewBlockFree")]
        static void Patch_SViewBlocks_GetViewBlockFree_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "GetViewBlockFree")]
        static void Patch_SViewBlocks_GetViewBlockFree_Post()
        {
            viewBlocksTelemetry.AddTelemetry("GetViewBlockFree");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SViewBlocks), "UpdateVisibleContent")]
        static void Patch_SViewBlocks_UpdateVisibleContent_Pre()
        {
            viewBlocksTelemetry.GetAndReset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SViewBlocks), "UpdateVisibleContent")]
        static void Patch_SViewBlocks_UpdateVisibleContent_Post()
        {
            viewBlocksTelemetry.AddTelemetry("UpdateVisibleContent");
        }
        
    }
}
