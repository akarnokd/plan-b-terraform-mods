using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatGotoExhaustedExtractors
{
    [BepInPlugin("akarnokd.planbterraformmods.featgotoexhaustedextractors", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> panelSize;
        static ConfigEntry<int> panelBottom;
        static ConfigEntry<int> panelLeft;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<KeyCode> keyCode;
        static ConfigEntry<bool> autoScale;

        static ManualLogSource logger;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            panelSize = Config.Bind("General", "PanelSize", 75, "The panel size");
            panelBottom = Config.Bind("General", "PanelBottom", 35, "The panel position from the bottom of the screen");
            panelLeft = Config.Bind("General", "PanelLeft", 50, "The panel position from the left of the screen");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size");
            keyCode = Config.Bind("General", "Key", KeyCode.Period, "The shortcut key for locating the idle extractor");
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
                if (idlePanel != null)
                {
                    Destroy(idlePanel);
                    idlePanel = null;
                    idlePanelBackground = null;
                    idlePanelBackground2 = null;
                    idlePanelIcon = null;
                    idleCount = null;
                }
            }
            return true;
        }

        static GameObject idlePanel;
        static GameObject idlePanelBackground;
        static GameObject idlePanelBackground2;
        static GameObject idlePanelIcon;
        static GameObject idleCount;

        static Sprite extractorSprite;
        static int extractorId;

        static HashSet<int2> extractors = new();

        static void UpdatePanel()
        {
            var defaultPanelLightColor = new Color(231f / 255, 227f / 255, 243f / 255, 1f);

            if (idlePanel == null)
            {
                idlePanel = new GameObject("FeatGotoExhaustedExtractors");
                var canvas = idlePanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                idlePanelBackground2 = new GameObject("FeatGotoExhaustedExtractors_BackgroundBorder");
                idlePanelBackground2.transform.SetParent(idlePanel.transform);

                var img = idlePanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                idlePanelBackground = new GameObject("FeatGotoExhaustedExtractors_Background");
                idlePanelBackground.transform.SetParent(idlePanel.transform);

                img = idlePanelBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                idlePanelIcon = new GameObject("FeatGotoExhaustedExtractors_Icon");
                idlePanelIcon.transform.SetParent(idlePanelBackground.transform);

                img = idlePanelIcon.AddComponent<Image>();
                img.color = Color.white;

                idleCount = new GameObject("FeatGotoExhaustedExtractors_Count");
                idleCount.transform.SetParent(idlePanelBackground.transform);

                var txt = idleCount.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = fontSize.Value;
                txt.color = Color.black;
                txt.resizeTextForBestFit = false;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.text = "<b>0</b>";

                var rect = idleCount.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);

                idlePanelBackground2.AddComponent<GraphicRaycaster>();
                var tt = idlePanelBackground2.AddComponent<CTooltipTarget>();
                tt.text = SLoc.Get("FeatGotoExhaustedExtractors.Tooltip");
                tt.textDesc = SLoc.Get("FeatGotoExhaustedExtractors.TooltipDetails", keyCode.Value);

            }

            var padding = 5;

            float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;

            var countText = idleCount.GetComponent<Text>();
            countText.fontSize = Mathf.RoundToInt(fontSize.Value * theScale);
            idleCount.GetComponent<RectTransform>().sizeDelta = new Vector2(countText.preferredWidth, countText.preferredHeight);

            var rectBg = idlePanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(panelSize.Value + 2 * padding, panelSize.Value + 3 * padding + countText.preferredHeight) * theScale;

            var rectBg2 = idlePanelBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(panelSize.Value + 4 * padding, panelSize.Value + 5 * padding + countText.preferredHeight) * theScale;

            rectBg.localPosition = new Vector3(-Screen.width / 2 + panelLeft.Value * theScale + rectBg2.sizeDelta.x / 2, 
                -Screen.height / 2 + panelBottom.Value * theScale + rectBg.sizeDelta.y / 2);
            rectBg2.localPosition = rectBg.localPosition;

            var rectIcon = idlePanelIcon.GetComponent<RectTransform>();
            rectIcon.sizeDelta = new Vector2(panelSize.Value * theScale, panelSize.Value * theScale);
            rectIcon.localPosition = new Vector2(0, padding + countText.preferredHeight / 2);

            var countRect = idleCount.GetComponent<RectTransform>();
            countRect.localPosition = new Vector2(0, -panelSize.Value * theScale / 2 - padding);

            var t = (long)Time.realtimeSinceStartup;
            if ((t & 1) == 0)
            {
                idlePanelBackground.GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f, 1f);
            }
            else
            {
                idlePanelBackground.GetComponent<Image>().color = defaultPanelLightColor;
            }

            idlePanelIcon.GetComponent<Image>().sprite = extractorSprite;

            List<int2> exhausted = new List<int2>();
            foreach (var e in new List<int2>(extractors))
            {
                if (GHexes.ContentId(e) == extractorId)
                {
                    if (GHexes.GroundData(e) == 0)
                    {
                        exhausted.Add(e);
                    }
                }
                else
                {
                    extractors.Remove(e);
                }
            }

            if (exhausted.Count != 0)
            {
                idlePanel.SetActive(true);
                countText.text = "<b>" + exhausted.Count + "</b>";
                var isWithin = Within(rectBg2, GetMouseCanvasPos());
                if (isWithin)
                {
                    idlePanelBackground.GetComponent<Image>().color = Color.yellow;
                }
                if ((isWithin && Input.GetKeyDown(KeyCode.Mouse0))
                    || IsKeyDown(keyCode.Value))
                {
                    exhausted.Shuffle();
                    ShowCoords(exhausted[0]);
                }
            }
            else
            {
                idlePanel.SetActive(false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem), nameof(CItem.Init))]
        static void CItem_Init(CItem __instance)
        {
            if (__instance.codeName == "extractor")
            {
                extractorSprite = __instance.icon.Asset;
                extractorId = __instance.id;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CItem_ContentExtractor_Build(int2 coords)
        {
            extractors.Add(coords);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnActivate")]
        static void SSceneHud_OnActivate()
        {
            extractors.Clear();
        }

        // Prevent click-through the panel
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.IsCursorOnGround))]
        static void SMouse_IsCursorOnGround(ref bool __result)
        {
            if (idlePanel != null && idlePanel.activeSelf
                && Within(idlePanelBackground2.GetComponent<RectTransform>(), GetMouseCanvasPos()))
            {
                __result = false;
            }
        }

        static void ShowCoords(int2 coords)
        {
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.SetDestination(coords);
            SSceneSingleton<SSceneCinematic>.Inst.cameraMovement.Play();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatGotoExhaustedExtractors.Tooltip", "View an exhausted Extractor" },
                { "FeatGotoExhaustedExtractors.TooltipDetails", "Click to view a random exhausted Extractor on the map.\nHotkey: {0}.\n\n<i>FeatGotoExhaustedExtractors mod</i>" }
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatGotoExhaustedExtractors.Tooltip", "Kimerült Kitemelő megtekintése" },
                { "FeatGotoExhaustedExtractors.TooltipDetails", "Kattints egy kimerült Kitermelő megmutatásához a térképen.\nGyorsbillentyű: {0}.\n\n<i>FeatGotoExhaustedExtractors mod</i>" }
            });
        }

    }
}