// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressBuildNotification;

        static bool suppressRecipePick;

        static bool suppressCopyNotification;

        static bool BuildPre(CItem_Content __instance, int2 coords)
        {
            if (!suppressBuildNotification && multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionBuild();
                msg.coords = coords;
                msg.id = __instance.id;
                msg.copyFrom = Haxx.cItemContentFirstBuildCoords(__instance);
                msg.allowRecipePick = msg.copyFrom == int2.negative;
                SendHost(msg);
                LogDebug("MessageActionBuild: Request at " + msg.coords.x + ", " + msg.coords.y
                    + " of " + __instance.codeName + " (copy from " + msg.copyFrom.x + ", " + msg.copyFrom.y + ")");
                return false;
            }
            return true;
        }

        static void BuildPost(CItem_Content __instance, int2 coords)
        {
            if (!suppressBuildNotification && multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageActionBuild();
                msg.coords = coords;
                msg.id = __instance.id;
                msg.copyFrom = Haxx.cItemContentFirstBuildCoords(__instance);
                SendAllClients(msg);
                LogDebug("MessageActionBuild: Command at " + msg.coords.x + ", " + msg.coords.y 
                    + " of " + __instance.codeName + " (copy from " + msg.copyFrom.x + ", " + msg.copyFrom.y + ")"
                    //+ "\n" + Environment.StackTrace
                );
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "Build")]
        static bool Patch_CItem_ContentFactory_Build_Pre(CItem_ContentFactory __instance, int2 coords)
        {
            return BuildPre(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "Build")]
        static void Patch_CItem_ContentFactory_Build_Post(CItem_ContentFactory __instance, int2 coords)
        {
            BuildPost(__instance, coords);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentDepot), "Build")]
        static bool Patch_CItem_ContentDepot_Build_Pre(CItem_ContentDepot __instance, int2 coords)
        {
            return BuildPre(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentDepot), "Build")]
        static void Patch_CItem_ContentDepot_Build_Post(CItem_ContentDepot __instance, int2 coords)
        {
            BuildPost(__instance, coords);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), "Build")]
        static bool Patch_CItem_ContentExtractor_Build_Pre(CItem_ContentExtractor __instance, int2 coords)
        {
            return BuildPre(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), "Build")]
        static void Patch_CItem_ContentExtractor_Build_Post(CItem_ContentExtractor __instance, int2 coords)
        {
            BuildPost(__instance, coords);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Content), "Build")]
        static bool Patch_CItem_Content_Build_Pre(CItem_Content __instance, int2 coords)
        {
            if (__instance is CItem_ContentLandmark)
            {
                return BuildPre(__instance, coords);
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Content), "Build")]
        static void Patch_CItem_Content_Build_Post(CItem_Content __instance, int2 coords)
        {
            if (__instance is CItem_ContentLandmark)
            {
                BuildPost(__instance, coords);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentForest), "Build")]
        static bool Patch_CItem_ContentForest_Build_Pre(CItem_ContentForest __instance, int2 coords)
        {
            return BuildPre(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Content), "Build")]
        static void Patch_CItem_ContentForest_Build_Post(CItem_ContentForest __instance, int2 coords)
        {
            BuildPost(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud_Selection), nameof(SSceneHud_Selection.IsCopying))]
        static void Patch_SSceneHud_Selection_IsCopying(ref bool __result)
        {
            if (suppressRecipePick)
            {
                __result = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Content), "Copy")]
        static void Patch_CItem_Content_Copy(CItem_Content __instance, int2 coordsFrom, int2 coordsTo)
        {
            LogDebug("    " + __instance.codeName + " -> CItem_Content::Copy(" + coordsFrom + ", " + coordsTo + ")");
            if (!suppressCopyNotification)
            {
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    var msg = new MessageActionCopy();
                    msg.codeName = __instance.codeName;
                    msg.fromCoords = coordsFrom;
                    msg.toCoords = coordsTo;
                    SendAllClients(msg);
                }
                else if (multiplayerMode == MultiplayerMode.Client)
                {
                    var msg = new MessageActionCopy();
                    msg.codeName = __instance.codeName;
                    msg.fromCoords = coordsFrom;
                    msg.toCoords = coordsTo;
                    SendHost(msg);
                }
            }
        }

        static void ReceiveMessageActionBuild(MessageActionBuild msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionBuild: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageActionBuild: Handling " + msg.GetType());
                var citem = GItems.items.Find(v => v != null && v.id == msg.id);
                if (citem is CItem_Content content)
                {
                    if (content.nbOwned != 0)
                    {
                        suppressBuildNotification = true;
                        try
                        {
                            if (multiplayerMode == MultiplayerMode.Host)
                            {
                                suppressRecipePick = true;
                            }
                            else
                            {
                                suppressRecipePick = !msg.allowRecipePick;
                            }

                            try
                            {
                                LogDebug("ReceiveMessageActionBuild: Building " + content.codeName
                                    + " at " + msg.coords.x + ", " + msg.coords.y
                                    + " (copy from " + msg.copyFrom.x + ", " + msg.copyFrom.y + ")"
                                    );

                                var saveFirst = Haxx.cItemContentFirstBuildCoords.Invoke(content);
                                try
                                {
                                    Haxx.cItemContentFirstBuildCoords.Invoke(content) = msg.copyFrom;

                                    Haxx.cItemContentBuild.Invoke(content, new object[] { msg.coords, false });

                                    /*
                                    if (msg.copyFrom != int2.negative)
                                    {
                                        Haxx.cItemContentCopy.Invoke(content, new object[] { msg.copyFrom, msg.coords });
                                    }
                                    */
                                }
                                finally
                                {
                                    Haxx.cItemContentFirstBuildCoords.Invoke(content) = saveFirst;
                                }
                            }
                            finally
                            {
                                suppressRecipePick = false;
                            }

                            if (multiplayerMode == MultiplayerMode.Host)
                            {
                                msg.sender.Send(msg); // bounce back

                                var msg2 = new MessageActionBuild();
                                msg2.coords = msg.coords;
                                msg2.id = msg.id;
                                msg2.copyFrom = msg.copyFrom;

                                SendAllClientsExcept(msg.sender, msg2);
                            }
                        }
                        finally
                        {
                            suppressBuildNotification = false;
                        }
                    }
                    else
                    {
                        LogDebug("ReceiveMessageActionBuild: Inventory empty for " + content.codeName);
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageActionBuild: Could not find item with id " + msg.id);
                }
            }
        }

        static void ReceiveMessageActionCopy(MessageActionCopy msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionCopy: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageActionCopy: Handling " + msg.GetType());

                var itemLookup = GetItemsDictionary();

                if (itemLookup.TryGetValue(msg.codeName, out var citem))
                {
                    if (citem is CItem_Content content)
                    {
                        suppressCopyNotification = true;
                        try
                        {
                            Haxx.cItemContentCopy.Invoke(content, new object[] { msg.fromCoords, msg.toCoords });
                        }
                        finally
                        {
                            suppressCopyNotification = true;
                        }
                        if (multiplayerMode == MultiplayerMode.Host)
                        {
                            SendAllClientsExcept(msg.sender, msg);
                        }
                    }
                    else
                    {
                        LogWarning("ReceiveMessageActionCopy: Item " + msg.codeName + " has the wrong type = " + citem.GetType());
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageActionCopy: Unknown item " + msg.codeName);
                }
            }
        }
    }
}
