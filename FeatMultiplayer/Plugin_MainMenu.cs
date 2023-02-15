// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static LibCommon.GUITools;
using Open.Nat;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        static GameObject mainMenuPanel;
        static GameObject mainMenuPanelBackground;
        static GameObject mainMenuPanelBackgroundBorder;

        static GameObject mainMenuPanelHostModeConfig;
        static GameObject mainMenuPanelHostIP;
        static GameObject mainMenuPanelUseUPnPConfig;
        static GameObject mainMenuPanelUPnPStatus;
        static GameObject mainMenuPanelUPnPAddress;
        static GameObject mainMenuPanelClientAddress;
        static List<GameObject> mainMenuPanelClients = new();
        static List<string> mainMenuClientNames = new();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHome), "OnActivate")]
        static void Patch_SSceneHome_OnActivate()
        {
            if (!modEnabled.Value)
            {
                return;
            }
            mainMenuPanel = new GameObject(Naming("MainMenuPanel"));
            
            var canvas = mainMenuPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            mainMenuPanelBackgroundBorder = new GameObject(Naming("MainMenuPanel_Border"));
            mainMenuPanelBackgroundBorder.transform.SetParent(mainMenuPanel.transform, false);
            var img = mainMenuPanelBackgroundBorder.AddComponent<Image>();
            img.color = DEFAULT_PANEL_BORDER_COLOR;

            mainMenuPanelBackground = new GameObject(Naming("MainMenuPanel_Background"));
            mainMenuPanelBackground.transform.SetParent(mainMenuPanelBackgroundBorder.transform, false);
            img = mainMenuPanelBackground.AddComponent<Image>();
            img.color = DEFAULT_PANEL_COLOR;

            CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_HostHeader"), SLoc.Get("FeatMultiplayer.HostConfig"), fontSize.Value, Color.black);

            mainMenuPanelHostModeConfig = CreateBox(mainMenuPanelBackground, Naming("MainMenuPanel_HostMode"), GetHostModeString(), fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

            mainMenuPanelHostIP = CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_HostIP"), SLoc.Get("FeatMultiplayer.HostIP", GetHostLocalAddress(), hostPort.Value), fontSize.Value, Color.black);

            mainMenuPanelUseUPnPConfig = CreateBox(mainMenuPanelBackground, Naming("MainMenuPanel_UseUPnP"), GetUPnPString(), fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

            mainMenuPanelUPnPStatus = CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_UPnPStatus"), SLoc.Get("FeatMultiplayer.UPnPStatus", GetExternalMappingString()), fontSize.Value, Color.black);
            mainMenuPanelUPnPAddress = CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_UPnPAddress"), SLoc.Get("FeatMultiplayer.UPnPAddress", GetExternalAddressString()), fontSize.Value, Color.black);

            CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_HostHeader"), SLoc.Get("FeatMultiplayer.ClientConfig"), fontSize.Value, Color.black);

            mainMenuPanelClientAddress = CreateText(mainMenuPanelBackground, Naming("MainMenuPanel_HostIP"), SLoc.Get("FeatMultiplayer.ClientIP", clientConnectAddress.Value, clientPort.Value), fontSize.Value, Color.black);

            int j = 0;
            foreach (var kv in clientUsers)
            {
                var btn = CreateBox(mainMenuPanelBackground, Naming("MainMenuPanel_Client_" + j), SLoc.Get("FeatMultiplayer.ClientAs", kv.Key), fontSize.Value, DEFAULT_BOX_COLOR, Color.white);
                mainMenuPanelClients.Add(btn);
                mainMenuClientNames.Add(kv.Key);
                j++;
            }

            multiplayerMode = MultiplayerMode.MainMenu;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSceneHome), "OnUpdate")]
        static void Patch_SSceneHome_OnUpdate()
        {
            if (!modEnabled.Value)
            {
                return;
            }

            var eip = externalIP;
            if (eip != null)
            {
                externalIP = null;
                mainMenuPanelUPnPAddress.GetComponent<Text>().text = SLoc.Get("FeatMultiplayer.UPnPAddress", eip);
                ApplyPreferredSize(mainMenuPanelUPnPAddress);
            }
            var emp = externalMap;
            if (emp != null)
            {
                externalMap = null;
                mainMenuPanelUPnPStatus.GetComponent<Text>().text = SLoc.Get("FeatMultiplayer.UPnPStatus", emp);
                ApplyPreferredSize(mainMenuPanelUPnPStatus);
            }

            var border = 5;
            var padding = 5f;


            var w = 0f;
            var h = padding;
            for (int i = 0; i < mainMenuPanelBackground.transform.childCount; i++)
            {
                var child = mainMenuPanelBackground.transform.GetChild(i);

                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    w = Mathf.Max(w, 2 * padding + rt.sizeDelta.x);
                    h += rt.sizeDelta.y + padding;
                }
            }
            var rectBorder = mainMenuPanelBackgroundBorder.GetComponent<RectTransform>();
            rectBorder.sizeDelta = new Vector2(w + 2 * border, h + 2 * border);
            rectBorder.localPosition = new Vector2(Screen.width / 2 - rectBorder.sizeDelta.x / 2, Screen.height / 2 - rectBorder.sizeDelta.y / 2);

            var rectBg = mainMenuPanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = rectBorder.sizeDelta - new Vector2(2 * border, 2 * border);


            var dy = h / 2 - padding;
            for (int i = 0; i < mainMenuPanelBackground.transform.childCount; i++)
            {
                var child = mainMenuPanelBackground.transform.GetChild(i);

                var rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localPosition = new Vector2(- w / 2 + rt.sizeDelta.x / 2 + padding, dy - rt.sizeDelta.y / 2);
                    dy -= rt.sizeDelta.y + padding;
                }
            }

            var mp = GetMouseCanvasPos();
            int j = 0;
            foreach (var client in mainMenuPanelClients)
            {
                if (Within(rectBorder, client.GetComponent<RectTransform>(), mp))
                {
                    client.GetComponentInChildren<Image>().color = DEFAULT_BOX_COLOR_HOVER;
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        string u = mainMenuClientNames[j];
                        string p = clientUsers[u];

                        // thePlugin.StartCoroutine(InterceptEnumerator(ClientJoin(u, p)));
                        thePlugin.StartCoroutine(ClientJoin(u, p));
                        break;
                    }
                }
                else
                {
                    client.GetComponentInChildren<Image>().color = DEFAULT_BOX_COLOR;
                }
                j++;
            }
            Checkbox(rectBorder, mainMenuPanelHostModeConfig, mp, hostMode, GetHostModeString);
            Checkbox(rectBorder, mainMenuPanelUseUPnPConfig, mp, useUPnP, GetUPnPString, () => GetExternalAddressString());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SScene), "OnDeactivate")]
        static void Patch_SScene_OnDeactivate(SScene __instance)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            if (__instance is SSceneHome)
            {
                Destroy(mainMenuPanel);
                mainMenuPanel = null;
                mainMenuPanelClients.Clear();
                mainMenuClientNames.Clear();
            }
        }

        static void Checkbox(RectTransform rectBg, GameObject go, Vector2 mp, ConfigEntry<bool> cfg, Func<string> labelProvider, Action onToggle = null)
        {
            if (Within(rectBg, go.GetComponent<RectTransform>(), mp))
            {
                go.GetComponentInChildren<Image>().color = DEFAULT_BOX_COLOR_HOVER;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    cfg.Value = !cfg.Value;
                    go.GetComponentInChildren<Text>().text = labelProvider();
                    
                    onToggle?.Invoke();
                }
            }
            else
            {
                go.GetComponentInChildren<Image>().color = DEFAULT_BOX_COLOR;
            }
        }

        static string GetHostModeString()
        {
            return SLoc.Get("FeatMultiplayer.HostMode", hostMode.Value ? "X" : "   ");
        }
        static string GetUPnPString()
        {
            return SLoc.Get("FeatMultiplayer.UseUPnP", useUPnP.Value ? "X" : "   ");
        }

        static string GetHostLocalAddress()
        {
            var hostIp = hostServiceAddress.Value;
            IPAddress hostIPAddress = IPAddress.Any;
            if (hostIp == "default" || hostIp == "")
            {
                hostIPAddress = GetMainIPv4();
            }
            else
            if (hostIp == "defaultv6")
            {
                hostIPAddress = GetMainIPv6();
            }
            else
            {
                hostIPAddress = IPAddress.Parse(hostIp);
            }
            return hostIPAddress.ToString();
        }

        static volatile string externalIP;
        static volatile string externalMap;

        static string GetExternalAddressString()
        {
            if (useUPnP.Value)
            {
                var portNum = hostPort.Value;

                Task.Run(async () =>
                {
                    try
                    {
                        var discoverer = new NatDiscoverer();
                        LogInfo("Begin NAT Discovery");
                        var device = await discoverer.DiscoverDeviceAsync().ConfigureAwait(false);
                        LogInfo(device.ToString());
                        LogInfo("Begin Get External IP");
                        // The following hangs indefinitely, not sure why
                        var ip = await device.GetExternalIPAsync().ConfigureAwait(false);
                        LogInfo("External IP = " + ip);
                        externalIP = ip.ToString();

                        try
                        {
                            var mapping = await device.GetSpecificMappingAsync(Protocol.Tcp, portNum).ConfigureAwait(false);
                            LogInfo("Current Mapping = " + mapping);

                            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, portNum, portNum, "The Planet Crafter Multiplayer")).ConfigureAwait(false);
                            externalMap = "ok";
                        }
                        catch (Exception ex)
                        {
                            LogInfo(ex);
                            externalMap = "error";
                        }

                    }
                    catch (Exception ex)
                    {
                        LogInfo(ex);
                        externalIP = "error";
                        externalMap = "error";
                    }
                });

                externalIP = "checking";
                externalMap = "checking";
                return "checking";
            }
            externalIP = "N/A";
            externalMap = "N/A";
            return "N/A";
        }

        static string GetExternalMappingString()
        {
            if (useUPnP.Value)
            {
                return "checking";
            }
            return "N/A";
        }
    }
}
