﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatNavigateToPOI
{
    [BepInPlugin("akarnokd.planbterraformmods.featnavigatetopoi", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> maxPois;
        static ConfigEntry<int> panelTop;
        static ConfigEntry<KeyCode> keyCode;
        static ConfigEntry<bool> autoScale;

        static ManualLogSource logger;

        static Color selectionColor = Color.blue;
        static Color textColor = Color.black;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            fontSize = Config.Bind("General", "FontSize", 20, "The font size of the panel text");
            maxPois = Config.Bind("General", "MaxLines", 10, "The maximum number of points of interest to show at once (scrollable)");
            panelTop = Config.Bind("General", "PanelTop", 300, "The top position of the panel relative to the top of the screen");
            keyCode = Config.Bind("General", "TogglePanelKey", KeyCode.L, "The key to show/hide the panel");
            autoScale = Config.Bind("General", "AutoScale", true, "Scale the position and size of the button with the UI scale of the game?");

            logger = Logger;

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static bool SSceneHud_OnUpdate()
        {
            if (modEnabled.Value)
            {
                UpdatePanel();
                //return false;
            }
            else
            {
                if (poiPanel != null)
                {
                    Destroy(poiPanel);
                    poiPanel = null;
                    poiPanelBackground = null;
                    poiPanelBackground2 = null;
                    scrollTop = 0;
                }
            }
            return true;
        }

        static GameObject poiPanel;
        static GameObject poiPanelBackground;
        static GameObject poiPanelBackground2;
        static GameObject poiPanelScrollUp;
        static GameObject poiPanelScrollDown;
        static int scrollTop;

        static void UpdatePanel()
        {
            if (poiPanel == null)
            {
                poiPanel = new GameObject("FeatNavigateToPOI");
                var canvas = poiPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 51;

                poiPanelBackground2 = new GameObject("FeatNavigateToPOI_BackgroundBorder");
                poiPanelBackground2.transform.SetParent(poiPanel.transform);

                var img = poiPanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                poiPanelBackground = new GameObject("FeatNavigateToPOI_Background");
                poiPanelBackground.transform.SetParent(poiPanel.transform);

                img = poiPanelBackground.AddComponent<Image>();
                img.color = new Color(231f / 255, 227f / 255, 243f / 255, 1f);

                poiPanelScrollUp = CreateBox(poiPanel, "FeatNavigateToPOI_ScrollUp", "\u25B2", fontSize.Value, Color.white, DEFAULT_BOX_COLOR);

                poiPanelScrollDown = CreateBox(poiPanel, "FeatNavigateToPOI_ScrollDown", "\u25BC", fontSize.Value, Color.white, DEFAULT_BOX_COLOR);
            }

            if (IsKeyDown(keyCode.Value))
            {
                poiPanel.SetActive(!poiPanel.activeSelf);
            }

            if (!poiPanel.activeSelf)
            {
                return;
            }

            List<PoiInfo> pois = new();

            foreach (var city in GGame.cities)
            {
                pois.Add(new PoiInfo
                {
                    coords = city.center,
                    name = city.name,
                    city = city
                });
            }

            foreach (var lm in GGame.landmarks)
            {
                // wastly negative keys are used for mod persistence
                if (lm.Coords.Positive)
                {
                    // verify the landmark actually exists, the dicoLandmarks is not deleted when the landmark is demolished
                    var cd = SSingleton<SWorld>.Inst.GetContent(lm.Coords) as CItem_ContentLandmark;
                    if (cd != null)
                    {
                        pois.Add(new PoiInfo
                        {
                            coords = lm.Coords,
                            name = lm.text
                        });
                    }
                }
            }

            pois.Sort((a, b) => a.name.CompareTo(b.name));

            for (int i = poiPanelBackground.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(poiPanelBackground.transform.GetChild(i).gameObject);
            }

            float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;

            float padding = 5 * theScale;
            float border = 5;
            float maxWidth = 0;
            float sumHeight = padding;
            int maxLines = maxPois.Value;
            List<GameObject> eachLine = new();
            List<PoiInfo> eachLinePoiInfo = new();

            float rollingY = -padding;

            var mp = GetMouseCanvasPos();

            if (Within(poiPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > 0)
                {
                    scrollTop = Math.Max(0, scrollTop - 1);
                }
                else
                if (scrollDelta < 0)
                {
                    scrollTop = scrollTop + 1;
                }
            }

            if (scrollTop + maxLines > pois.Count)
            {
                scrollTop = Math.Max(0, pois.Count - maxLines);
            }
            poiPanelScrollUp.SetActive(scrollTop > 0);
            poiPanelScrollDown.SetActive(scrollTop + maxLines < pois.Count);

            for (int i = scrollTop; i < pois.Count && i < scrollTop + maxLines; i++)
            {
                
                PoiInfo poi = pois[i];
                var textGo = new GameObject("FeatNavigateToPOI_Poi_" + i);
                textGo.transform.SetParent(poiPanelBackground.transform);

                var txt = textGo.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = Mathf.RoundToInt(fontSize.Value * theScale);
                txt.color = textColor;
                txt.resizeTextForBestFit = false;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.alignment = TextAnchor.MiddleCenter;

                string title = "";
                if (poi.city != null)
                {
                    var statusTxt = SLoc.Get(CItem_ContentCity.statusCity[poi.city.GetStatus()]);
                    title = SLoc.Get("FeatNavigateToPOI.City", poi.name, (int)poi.city.population, statusTxt);
                }
                else
                {
                    title = SLoc.Get("FeatNavigateToPOI.Landmark", poi.name);
                }
                txt.text = "<b>" + title + "</b>";

                maxWidth = Math.Max(maxWidth, txt.preferredWidth);
                sumHeight += txt.preferredHeight + padding;

                var rectLine = textGo.GetComponent<RectTransform>();

                rectLine.localPosition = new Vector2(txt.preferredWidth, rollingY - txt.preferredHeight / 2);
                rectLine.sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);

                rollingY -= txt.preferredHeight + padding;

                eachLine.Add(textGo);
                eachLinePoiInfo.Add(poi);
            }

            var panelRect = poiPanelBackground.GetComponent<RectTransform>();

            var maxWidthWithPad = maxWidth + padding + padding;

            panelRect.localPosition = new Vector2(Screen.width / 2 - maxWidthWithPad / 2 - border * theScale, Screen.height / 2 - panelTop.Value * theScale - sumHeight / 2);
            panelRect.sizeDelta = new Vector2(maxWidthWithPad, sumHeight);

            var panelRect2 = poiPanelBackground2.GetComponent<RectTransform>();
            panelRect2.localPosition = panelRect.localPosition;
            panelRect2.sizeDelta = new Vector2(panelRect.sizeDelta.x + 2 * border * theScale, panelRect.sizeDelta.y + 2 * border * theScale);

            ResizeBox(poiPanelScrollUp, fontSize.Value * theScale);
            var scrollTopRect = poiPanelScrollUp.GetComponent<RectTransform>();
            scrollTopRect.localPosition = new Vector2(panelRect.localPosition.x, 
                panelRect.localPosition.y + sumHeight / 2 + scrollTopRect.sizeDelta.y / 2 - padding / 2);

            ResizeBox(poiPanelScrollDown, fontSize.Value * theScale);
            var scrollBottomRect = poiPanelScrollDown.GetComponent<RectTransform>();
            scrollBottomRect.localPosition = new Vector2(panelRect.localPosition.x, 
                panelRect.localPosition.y - sumHeight / 2 - scrollTopRect.sizeDelta.y / 2 + padding / 2);

            var panelX = Screen.width / 2 - maxWidthWithPad;

            for (int i = 0; i < eachLine.Count; i++)
            {
                GameObject ln = eachLine[i];
                var rectLine = ln.GetComponent<RectTransform>();

                var lp = rectLine.localPosition;
                rectLine.localPosition = new Vector2(- (maxWidth - lp.x) / 2, lp.y + sumHeight / 2);

                var txt = ln.GetComponent<Text>();

                if (Within(panelRect, rectLine, mp))
                {
                    txt.color = selectionColor;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        ShowCoords(eachLinePoiInfo[i].coords);
                    }
                }
                else
                {
                    txt.color = textColor;
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.Update))]
        static bool SMouse_Update()
        {
            if (poiPanel != null && poiPanel.activeSelf && poiPanelBackground2 != null)
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta != 0)
                {
                    var mp = GetMouseCanvasPos();

                    if (Within(poiPanelBackground2.GetComponent<RectTransform>(), mp))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Prevent click-through the panel
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.IsCursorOnGround))]
        static void SMouse_IsCursorOnGround(ref bool __result)
        {
            if (poiPanel != null && poiPanel.activeSelf
                && Within(poiPanelBackground2.GetComponent<RectTransform>(), GetMouseCanvasPos()))
            {
                __result = false;
            }
        }

        static void ShowCoords(int2 coords)
        {
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.SetDestination(coords);
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.Play();
        }

        internal class PoiInfo
        {
            internal int2 coords;
            internal string name;
            internal CCity city;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatNavigateToPOI.Landmark", "{0} (Landmark)" },
                { "FeatNavigateToPOI.City", "{0} (City of {1}; {2})" }
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatNavigateToPOI.Landmark", "{0} (Torony)" },
                { "FeatNavigateToPOI.City", "{0} (Város {1} fővel; {2})" }
            });
        }
    }
}