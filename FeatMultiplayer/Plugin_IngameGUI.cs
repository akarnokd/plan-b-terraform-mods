// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using LibCommon;
using System.IO;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static Sprite iconMP;

        static ToolbarTopButton toolbarTopButton;

        static void InitIngameGUI()
        {
            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            var iconPng = LoadPNG(Path.Combine(dir, "IconMP.png"));
            iconMP = Sprite.Create(iconPng, new Rect(0, 0, iconPng.width, iconPng.height), new Vector2(0.5f, 0.5f));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static void Patch_SSceneHud_OnUpdate_IngameGUI()
        {
            if (modEnabled.Value)
            {
                bool isHost = multiplayerMode == MultiplayerMode.Host;
                bool isClient = multiplayerMode == MultiplayerMode.Client;
                UpdateIngameGUI_NetworkButton(isHost, isClient);
            }
            else
            {
                toolbarTopButton?.Destroy();
                toolbarTopButton = null;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnDeactivate")]
        static void Patch_SSceneHud_OnDeactivate_IngameGUI()
        {
            toolbarTopButton?.SetVisible(false);
        }

        static void UpdateIngameGUI_NetworkButton(bool isHost, bool isClient)
        {
            if (isHost || isClient)
            {
                if (toolbarTopButton == null || !toolbarTopButton.IsAvailable())
                {
                    toolbarTopButton = new ToolbarTopButton();
                    toolbarTopButton.Create("FeatMultiplayer_NetworkButton", NetworkButtonOnClick);
                    toolbarTopButton.SetIcon(iconMP);
                }
                if (isHost)
                {
                    toolbarTopButton.SetTooltip(
                        SLoc.Get("FeatMultiplayer.NetworkButton.Host.Title"),
                        SLoc.Get("FeatMultiplayer.NetworkButton.Host.Desc", sessions.Count)
                    );
                }
                else
                {
                    toolbarTopButton.SetTooltip(
                        SLoc.Get("FeatMultiplayer.NetworkButton.Client.Title"),
                        SLoc.Get("FeatMultiplayer.NetworkButton.Client.Desc")
                    );
                }
                toolbarTopButton.SetVisible(true);

                toolbarTopButton.Update(networkButtonLeft.Value, networkButtonSize.Value, autoScale.Value);
            }
            else
            {
                toolbarTopButton?.SetVisible(false);
            }
        }

        static void NetworkButtonOnClick()
        {
            // TODO
        }
    }
}
