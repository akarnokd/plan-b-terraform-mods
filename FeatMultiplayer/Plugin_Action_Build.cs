// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using System;
using Unity.Audio;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static bool suppressBuildNotification;

        //static bool suppressRecipePickAndCopy;
        static int2 suppressRecipePickAndCopyCoords;
        static CRecipe[] suppressRecipePickAndCopyRecipes;
        static readonly CRecipe[] suppressRecipePickAndCopyRecipesEmpty = new CRecipe[1];
        static bool suppressRecipePickAndCopyFirstBuild;

        static MessageUpdateDepotDrones deferredDepotDrones;

        static bool BuildPre(CItem_Content __instance, int2 coords)
        {
            if (!suppressBuildNotification && multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionBuild();
                msg.coords = coords;
                msg.id = __instance.id;
                msg.copyFrom = GScene3D.duplicatedCoords;
                msg.firstBuild = GScene3D.isDuplicatingAFirstBuild;
                msg.allowRecipePick = true;
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
                msg.copyFrom = GScene3D.duplicatedCoords;
                msg.firstBuild = GScene3D.isDuplicatingAFirstBuild;
                SendAllClients(msg);
                LogDebug("MessageActionBuild: Command " + __instance.codeName + " -> " + msg);

            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "Build")]
        static bool Patch_CItem_ContentFactory_Build_Pre(CItem_ContentFactory __instance, 
            int2 coords)
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
        static bool Patch_CItem_ContentDepot_Build_Pre(
            CItem_ContentDepot __instance, 
            int2 coords)
        {
            return BuildPre(__instance, coords);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentDepot), "Build")]
        static void Patch_CItem_ContentDepot_Build_Post(CItem_ContentDepot __instance, 
            int2 coords)
        {
            BuildPost(__instance, coords);
            Haxx.SBlocks_OnChangeItem(coords, true, false, false);
            if (multiplayerMode == MultiplayerMode.Host && !suppressBuildNotification)
            {
                var msg1 = new MessageUpdateStackAt();
                msg1.GetSnapshot(coords, 0);
                SendAllClients(msg1);

                SignalDeferredDepotDrones();
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
        static void Patch_SDrones_OnDepotBuild_Post(int2 depotCoords, ref int __state)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateDepotDrones();
                msg.coords = depotCoords;
                msg.GetSnapshot(__state, GDrones.drones.Count);
                deferredDepotDrones = msg;
                LogDebug("Deferring SDrones.OnDepotBuild at " + depotCoords);
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
        [HarmonyPatch(typeof(CItem_ContentForest), "Build")]
        static void Patch_CItem_ContentForest_Build_Post(CItem_ContentForest __instance, int2 coords)
        {
            BuildPost(__instance, coords);
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

        static void SignalDeferredDepotDrones()
        {
            // make sure the drones created come after the client has seen the build command
            var dd = deferredDepotDrones;
            deferredDepotDrones = null;

            if (dd != null)
            {
                LogDebug("    UpdateDepotDrones -> " + dd.coords + ", " + dd.drones.Count);
                SendAllClients(dd);
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
                            suppressBuildNotification = true;
                            suppressLineCreateItemPicking = true;

                            var copyPosSave = GScene3D.duplicatedCoords;
                            GScene3D.duplicatedCoords = msg.copyFrom;
                            var copyFirstSave = GScene3D.isDuplicatingAFirstBuild;
                            GScene3D.isDuplicatingAFirstBuild = msg.firstBuild;

                            try
                            {
                                Haxx.cItemContentBuild.Invoke(content, new object[] { msg.coords, false });
                            }
                            finally
                            {
                                suppressBuildNotification = false;
                                suppressLineCreateItemPicking = false;
                                GScene3D.duplicatedCoords = copyPosSave;
                                GScene3D.isDuplicatingAFirstBuild = copyFirstSave;
                            }

                            // CWays can change what's actually built
                            msg.overrideId = GHexes.contentId[msg.coords.x, msg.coords.y];

                            // allow the original sender to build and open recipe picker if needed
                            msg.sender.Send(msg); 

                            // everyone else, perform a no-copy no recipe-picking build
                            var msgToOtherClients = new MessageActionBuild();
                            msgToOtherClients.id = msg.id;
                            msgToOtherClients.coords = msg.coords;
                            msgToOtherClients.overrideId = msg.overrideId;
                            SendAllClientsExcept(msg.sender, msgToOtherClients);

                            SignalDeferredDepotDrones();
                        }
                        else
                        {
                            LogDebug("ReceiveMessageActionBuild: Inventory empty for " + content.codeName);
                        }
                    }
                    else if (multiplayerMode == MultiplayerMode.Client)
                    {
                        suppressBuildNotification = true;
                        suppressLineCreateItemPicking = !msg.allowRecipePick;

                        var copyPosSave = GScene3D.duplicatedCoords;
                        GScene3D.duplicatedCoords = msg.copyFrom;
                        var copyFirstSave = GScene3D.isDuplicatingAFirstBuild;
                        GScene3D.isDuplicatingAFirstBuild = msg.firstBuild;

                        try
                        {
                            // build without copying

                            Haxx.cItemContentBuild.Invoke(content, new object[] { msg.coords, false });

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
                            suppressLineCreateItemPicking = false;
                            GScene3D.duplicatedCoords = copyPosSave;
                            GScene3D.isDuplicatingAFirstBuild = copyFirstSave;
                        }
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageActionBuild: Could not find item with id " + msg.id);
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
