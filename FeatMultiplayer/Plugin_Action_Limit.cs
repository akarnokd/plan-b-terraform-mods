// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHud_ItemsBars), nameof(SSceneHud_ItemsBars.OnItemNumberCallback))]
        static void Patch_SSceneHud_ItemBars_OnItemNumberCallback(CUiBarElement ____itemNbMaxClicked)
        {
            ApiUpdateItemLimit(____itemNbMaxClicked.Item);
        }

        /// <summary>
        /// Sends an updated item count and maximum to the host/clients.
        /// </summary>
        /// <param name="item">The item to update on the other side(s).</param>
        public static void ApiUpdateItemLimit(CItem item)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateItem();
                msg.GetSnapshot(item);
                SendAllClients(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageUpdateItem();
                msg.GetSnapshot(item);
                SendHost(msg);
            }
        }

        static void ReceiveMessageUpdateItem(MessageUpdateItem msg)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                msg.ApplySnapshot();
                SendAllClientsExcept(msg.sender, msg);
            }
            else
            {
                msg.ApplySnapshot();
            }
        }
    }
}
