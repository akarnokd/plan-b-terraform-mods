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

        static bool suppressRecipePickAndCopy;

        static bool suppressCopyNotification;

        static bool deferCopy;
        static int2 deferCopySource;
        static int2 deferCopyDestination;

        static MessageUpdateDepotDrones deferredDepotDrones;

        static bool BuildPre(CItem_Content __instance, int2 coords)
        {
            if (!suppressBuildNotification && multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionBuild();
                msg.coords = coords;
                msg.id = __instance.id;
                SendHost(msg);
                LogDebug("MessageActionBuild: Request " + __instance.codeName + " -> " + msg);
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
                SendAllClients(msg);
                LogDebug("MessageActionBuild: Command " + __instance.codeName + " -> " + msg);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "Build")]
        static bool Patch_CItem_ContentFactory_Build_Pre(CItem_ContentFactory __instance, 
            int2 coords, ref int2 ____firstBuildCoords)
        {
            if (!suppressBuildNotification)
            {
                if (multiplayerMode == MultiplayerMode.Client)
                {

                    var msg = new MessageActionBuild();
                    msg.coords = coords;
                    msg.id = __instance.id;

                    // capture if copying of configuration is needed
                    msg.copyMode = __instance.recipes.Length != 0
                        && !SSceneSingleton<SSceneHud_Selection>.Inst.IsCopying()
                        && !(__instance is CItem_ContentCityInOut); 

                    msg.copyFrom = ____firstBuildCoords;
                    SendHost(msg);

                    LogDebug("MessageActionBuild: Request " + __instance.codeName + " -> " + msg);
                    return false;
                }
                else if (multiplayerMode == MultiplayerMode.Host)
                {
                    deferCopy = true;
                    deferCopySource = int2.negative;
                    deferCopyDestination = int2.negative;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "Build")]
        static void Patch_CItem_ContentFactory_Build_Post(CItem_ContentFactory __instance, int2 coords)
        {
            if (!suppressBuildNotification)
            {
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    deferCopy = false;

                    var msg = new MessageActionBuild();
                    msg.coords = coords;
                    msg.id = __instance.id;
                    SendAllClients(msg);
                    LogDebug("MessageActionBuild: Command " + __instance.codeName + " -> " + msg);

                    if (deferCopySource != int2.negative)
                    {
                        Haxx.cItemContentCopy.Invoke(__instance, new object[] { deferCopySource, deferCopyDestination });
                    }
                }
            }
        }

        internal struct BuildHasPriorStack
        {
            internal bool has;
            internal CStack stack;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentDepot), "Build")]
        static bool Patch_CItem_ContentDepot_Build_Pre(
            CItem_ContentDepot __instance, 
            int2 coords, 
            ref int2 ____firstBuildCoords,
            ref BuildHasPriorStack __state)
        {
            if (!suppressBuildNotification)
            {
                if (multiplayerMode == MultiplayerMode.Client)
                {
                    var msg = new MessageActionBuild();
                    msg.coords = coords;
                    msg.id = __instance.id;
                    msg.copyMode = !SSceneSingleton<SSceneHud_Selection>.Inst.IsCopying()
                        && ContentAt(coords) is not CItem_ContentDepot;
                    msg.copyFrom = ____firstBuildCoords;
                    SendHost(msg);
                    LogDebug("MessageActionBuild: Request " + __instance.codeName + " -> " + msg);

                    return false;
                }
                else
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    deferCopy = true;
                    deferCopySource = int2.negative;
                    deferCopyDestination = int2.negative;

                    // preserve any previous depot stack info
                    if (ContentAt(coords) is CItem_ContentDepot)
                    {
                        __state.has = true;
                        __state.stack = __instance.GetStack(coords, 0);
                    }
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentDepot), "Build")]
        static void Patch_CItem_ContentDepot_Build_Post(CItem_ContentDepot __instance, 
            int2 coords, ref BuildHasPriorStack __state)
        {
            if (!suppressBuildNotification && multiplayerMode == MultiplayerMode.Host)
            {
                deferCopy = false;

                var msg = new MessageActionBuild();
                msg.coords = coords;
                msg.id = __instance.id;
                SendAllClients(msg);
                LogDebug("MessageActionBuild: Command " + __instance.codeName + " -> " + msg);

                if (deferCopySource != int2.negative)
                {
                    Haxx.cItemContentCopy.Invoke(__instance, new object[] { deferCopySource, deferCopyDestination });
                }
                // reapply stack after the copy above might have destroyed it
                if (__state.has)
                {
                    ref var st = ref __instance.GetStack(coords, 0);

                    st.item = __state.stack.item;
                    st.nb = __state.stack.nb;

                    var msgs = new MessageUpdateStackAt();
                    msgs.GetSnapshot(coords, 0);
                    SendAllClients(msgs);
                }

                // make sure the drones created come after the client has seen the build command
                var dd = deferredDepotDrones;
                deferredDepotDrones = null;

                if (dd != null)
                {
                    SendAllClients(dd);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SDrones), nameof(SDrones.OnDepotBuilt))]
        static bool Patch_SDrones_OnDepotBuild_Pre(ref int __state)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                return false;
            }
            __state = GDrones.drones.Count;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SDrones), nameof(SDrones.OnDepotBuilt))]
        static void Patch_SDrones_OnDepotBuild_Post(ref int __state)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateDepotDrones();
                msg.GetSnapshot(__state, GDrones.drones.Count);
                deferredDepotDrones = msg;
            }
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
            if (suppressRecipePickAndCopy)
            {
                __result = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Content), "Copy")]
        static bool Patch_CItem_Content_Copy(CItem_Content __instance, int2 coordsFrom, int2 coordsTo)
        {
            if (deferCopy)
            {
                LogDebug("    " + __instance.codeName + " -> CItem_Content::Copy(" + coordsFrom + ", " + coordsTo + ") deferred");
                deferCopySource = coordsFrom;
                deferCopyDestination = coordsTo;
                return false;
            }
            if (!suppressCopyNotification)
            {
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    LogDebug("    " + __instance.codeName + " -> CItem_Content::Copy(" + coordsFrom + ", " + coordsTo + ")");

                    var msg = new MessageActionCopy();
                    msg.codeName = __instance.codeName;
                    msg.fromCoords = coordsFrom;
                    msg.toCoords = coordsTo;
                    SendAllClients(msg);
                }
                else if (multiplayerMode == MultiplayerMode.Client)
                {
                    LogDebug("    " + __instance.codeName + " -> CItem_Content::Copy(" + coordsFrom + ", " + coordsTo + ")");

                    var msg = new MessageActionCopy();
                    msg.codeName = __instance.codeName;
                    msg.fromCoords = coordsFrom;
                    msg.toCoords = coordsTo;
                    SendHost(msg);
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Way), "Build")]
        static bool Patch_CItem_Way_Build_Pre(CItem_Way __instance, int2 coords)
        {
            if (!suppressBuildNotification)
            {
                if (multiplayerMode == MultiplayerMode.Client)
                {
                    var msg = new MessageActionBuild();
                    msg.id = __instance.id;
                    msg.coords = coords;
                    SendHost(msg);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Way), "Build")]
        static void Patch_CItem_Way_Build_Post(CItem_Way __instance, in int2 coords)
        {
            if (!suppressBuildNotification)
            {
                if (multiplayerMode == MultiplayerMode.Host)
                {
                    var msg = new MessageActionBuild();
                    msg.id = __instance.id;
                    msg.coords = coords;
                    msg.overrideId = GHexes.contentId[coords.x, coords.y];
                    SendAllClients(msg);
                }
            }
        }

        // -----------------------------------------------------------------------
        // Message receivers
        // -----------------------------------------------------------------------


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
                    LogDebug("    " + content.codeName + " -> " + msg);

                    if (multiplayerMode == MultiplayerMode.Host)
                    {
                        if (content.nbOwned != 0)
                        {
                            // this essentially skips the custom copy logic
                            suppressRecipePickAndCopy = true;
                            suppressBuildNotification = true;
                            try
                            {
                                Haxx.cItemContentBuild.Invoke(content, new object[] { msg.coords, false });
                            }
                            finally
                            {
                                suppressRecipePickAndCopy = false;
                                suppressBuildNotification = false;
                            }

                            // CWays can change what's actually built
                            msg.overrideId = GHexes.contentId[msg.coords.x, msg.coords.y];

                            // allow the original sender to build and open recipe picker if needed
                            msg.sender.Send(msg); 

                            // everyone else, perform a no-copy no recipe-picking build
                            var msg2 = new MessageActionBuild();
                            msg2.id = msg.id;
                            msg2.coords = msg.coords;
                            msg2.overrideId = msg.overrideId;
                            SendAllClientsExcept(msg.sender, msg);

                            // make sure the drones created come after the client has seen the build command
                            var dd = deferredDepotDrones;
                            deferredDepotDrones = null;

                            if (dd != null)
                            {
                                SendAllClients(dd);
                            }

                            if (msg.copyMode)
                            {
                                if (msg.copyFrom != int2.negative)
                                {
                                    Haxx.cItemContentCopy.Invoke(content, new object[] { msg.copyFrom, msg.coords });
                                }
                            }


                        }
                        else
                        {
                            LogDebug("ReceiveMessageActionBuild: Inventory empty for " + content.codeName);
                        }
                    }
                    else if (multiplayerMode == MultiplayerMode.Client)
                    {
                        suppressBuildNotification = true;
                        suppressRecipePickAndCopy = true;
                        try
                        {
                            // build without copying

                            Haxx.cItemContentBuild.Invoke(content, new object[] { msg.coords, false });

                            if (msg.copyMode)
                            {
                                if (msg.copyFrom == int2.negative)
                                {
                                    SSceneSingleton<SScenePopup>.Inst.ActivateAndShow(false, true, SLoc.Get("Popup_RecipePicking"), null);

                                    if (content is CItem_ContentDepot)
                                    {
                                        SSceneSingleton<SScenePopup>.Inst.ShowItemsPickUp(msg.coords);
                                    }
                                    else
                                    {
                                        SSceneSingleton<SScenePopup>.Inst.Show_Factory_RecipePick(msg.coords);
                                    }

                                    Haxx.cItemContentFirstBuildCoords(content) = msg.coords;
                                }
                                else
                                {
                                    // don't perform the copy, a separate message will arrive
                                }
                            }
                            if (content is CItem_Way)
                            {
                                GHexes.contentId[msg.coords.x, msg.coords.y] = msg.overrideId;
                                SSingleton<SViewWorld>.Inst.OnBuildItem_UpdateTxWorld(msg.coords);
                                Haxx.SBlocks_OnChangeItem(msg.coords, true, false, false);
                                SSingleton<SItems>.Inst.OnChangeContent(content, msg.coords, EModification.replaced);
                            }
                        }
                        finally
                        {
                            suppressBuildNotification = false;
                            suppressRecipePickAndCopy = false;
                        }
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
                            suppressCopyNotification = false;
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

        static void ReceiveMessageUpdateDepotDrones(MessageUpdateDepotDrones msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("MessageUpdateDepotDrones: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("MessageUpdateDepotDrones: Handling " + msg.GetType());

                msg.ApplySnapshot();
            }
            else
            {
                LogWarning("MessageUpdateDepotDrones: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
