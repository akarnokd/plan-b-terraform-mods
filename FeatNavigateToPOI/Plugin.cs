using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace FeatNavigateToPOI
{
    [BepInPlugin("akarnokd.planbterraformmods.featnavigatetopoi", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> maxPois;
        static ConfigEntry<int> panelTop;

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            fontSize = Config.Bind("General", "FontSize", 20, "The font size of the panel text");
            maxPois = Config.Bind("General", "MaxLines", 10, "The maximum number of points of interest to show at once (scrollable)");
            panelTop = Config.Bind("General", "PanelTop", 300, "The top position of the panel relative to the top of the screen");

            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
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
                    scrollTop = 0;
                }
            }
            return true;
        }

        static GameObject poiPanel;
        static GameObject poiPanelBackground;
        static int scrollTop;
        static bool once;

        static void UpdatePanel()
        {
            if (poiPanel == null)
            {
                poiPanel = new GameObject("FeatNavigateToPOI");
                var canvas = poiPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                poiPanelBackground = new GameObject("FeatNavigateToPOI_Background");
                poiPanelBackground.transform.SetParent(poiPanel.transform);

                var img = poiPanelBackground.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.95f);
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

            foreach (var lm in GGame.dicoLandmarks)
            {
                pois.Add(new PoiInfo
                {
                    coords = lm.Key,
                    name = lm.Value
                });
            }

            pois.Sort((a, b) => a.name.CompareTo(b.name));

            if (once)
            {
                //return;
            }
            if (!once && pois.Count != 0)
            {
                once = true;
            }
            for (int i = poiPanelBackground.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(poiPanelBackground.transform.GetChild(i).gameObject);
            }

            float padding = 5;
            float maxWidth = 0;
            float sumHeight = padding;
            int maxLines = maxPois.Value;
            List<GameObject> eachLine = new();
            List<PoiInfo> eachLinePoiInfo = new();

            float rollingY = -padding;

            for (int i = scrollTop; i < pois.Count && i < scrollTop + maxLines; i++)
            {
                
                PoiInfo poi = pois[i];
                var textGo = new GameObject("FeatNavigateToPOI_Poi_" + i);
                textGo.transform.SetParent(poiPanelBackground.transform);

                var txt = textGo.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = fontSize.Value;
                txt.color = Color.white;
                txt.resizeTextForBestFit = false;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.alignment = TextAnchor.MiddleCenter;

                string title = "";
                if (poi.city != null)
                {
                    var statusTxt = SLoc.Get(CItem_ContentCity.statusCity[poi.city.GetStatus()]);
                    title = poi.name + " (City of " + poi.city.population + "; " + statusTxt + ")";
                }
                else
                {
                    title = poi.name + " (Landmark)";
                }
                txt.text = title;

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

            panelRect.localPosition = new Vector2(Screen.width / 2 - maxWidthWithPad / 2, Screen.height / 2 - panelTop.Value - sumHeight / 2);
            panelRect.sizeDelta = new Vector2(maxWidthWithPad, sumHeight);

            var panelX = Screen.width / 2 - maxWidthWithPad;

            var mousePos = Input.mousePosition;

            var canvasPos = new Vector2(-Screen.width / 2 + mousePos.x, -Screen.height / 2 + mousePos.y);

            for (int i = 0; i < eachLine.Count; i++)
            {
                GameObject ln = eachLine[i];
                var rectLine = ln.GetComponent<RectTransform>();

                var lp = rectLine.localPosition;
                rectLine.localPosition = new Vector2(- (maxWidth - lp.x) / 2, lp.y + sumHeight / 2);

                var txt = ln.GetComponent<Text>();

                if (Within(panelRect, rectLine, canvasPos))
                {
                    txt.color = Color.yellow;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        ShowCoords(eachLinePoiInfo[i].coords);
                    }
                }
                else
                {
                    txt.color = Color.white;
                }
            }
        }

        static bool Within(RectTransform parent, RectTransform rt, Vector2 vec)
        {
            return false;
        }

        static void ShowCoords(int2 coords)
        {
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.SetDestination(coords, false);
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.Play();
        }

        internal class PoiInfo
        {
            internal int2 coords;
            internal string name;
            internal CCity city;
        }
    }
}