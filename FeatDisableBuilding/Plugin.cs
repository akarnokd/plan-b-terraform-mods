using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatDisableBuilding
{
    [BepInPlugin("akarnokd.planbterraformmods.featdisablebuilding", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<KeyCode> toggleKey;
        static ConfigEntry<int> panelSize;
        static ConfigEntry<int> panelBottom;
        static ConfigEntry<int> panelLeft;
        static ConfigEntry<bool> autoScale;

        static HashSet<int2> disabledLocations = new HashSet<int2>();

        static readonly int2 dicoCoordinates = new int2 { x = -1_000_100_000, y = 0 };

        static readonly Dictionary<int2, GameObject> overlayIcons = new();

        static ManualLogSource logger;

        static Sprite buildingEnabled;
        static Sprite buildingDisabled;
        static Sprite buildingDisabledOverlay;

        static Color defaultPanelLightColor = new Color(231f / 255, 227f / 255, 243f / 255, 1f);

        static GameObject disablePanel;
        static GameObject disablePanelOverlay;
        static GameObject disableBackground;
        static GameObject disableBackground2;
        static GameObject disableIcon;

        static int cachedScreenWidth;
        static int cachedScreenHeight;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.K, "Key to press while the building is selected to toggle its enabled/disabled state");
            panelSize = Config.Bind("General", "PanelSize", 75, "The panel size");
            panelBottom = Config.Bind("General", "PanelBottom", 35, "The panel position from the bottom of the screen");
            panelLeft = Config.Bind("General", "PanelLeft", 150, "The panel position from the left of the screen");
            autoScale = Config.Bind("General", "AutoScale", true, "Scale the position and size of the button with the UI scale of the game?");

            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            var enabledPng = LoadPNG(Path.Combine(dir, "Building_Enabled.png"));
            buildingEnabled = Sprite.Create(enabledPng, new Rect(0, 0, enabledPng.width, enabledPng.height), new Vector2(0.5f, 0.5f));
            
            var disabledPng = LoadPNG(Path.Combine(dir, "Building_Disabled.png"));
            buildingDisabled = Sprite.Create(disabledPng, new Rect(0, 0, disabledPng.width, disabledPng.height), new Vector2(0.5f, 0.5f));

            var disabledOverlayPng = LoadPNG(Path.Combine(dir, "Building_Disabled_Overlay.png"));
            buildingDisabledOverlay = Sprite.Create(disabledOverlayPng, new Rect(0, 0, disabledOverlayPng.width, disabledOverlayPng.height), new Vector2(0.5f, 0.5f));

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);
        }

        void Update()
        {
            // lags otherwise?
            UpdateOverlay();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static void SSceneHud_OnUpdate()
        {
            if (modEnabled.Value)
            {
                var selCoords = GScene3D.selectionCoords;
                var selBuilding = GScene3D.selectedItem;

                EnsurePanel();

                bool isOverPanel = false;

                if (selCoords.Positive && selBuilding != null)
                {
                    if (selBuilding is CItem_ContentFactory 
                        || selBuilding is CItem_ContentExtractor
                        || selBuilding is CItem_ContentGreenHouse
                        || selBuilding is CItem_ContentPumpingStation
                        || selBuilding is CItem_ContentIceExtractor)
                    {
                       isOverPanel = disablePanel.activeSelf && Within(disableBackground2.GetComponent<RectTransform>(), GetMouseCanvasPos());
                        if (IsKeyDown(toggleKey.Value)
                            || (isOverPanel && Input.GetKeyDown(KeyCode.Mouse0)))
                        {
                            if (disabledLocations.Contains(selCoords))
                            {
                                disabledLocations.Remove(selCoords);
                            }
                            else
                            {
                                disabledLocations.Add(selCoords);
                            }
                            SaveState();
                        }
                    }
                    else
                    {
                        selCoords = int2.negative;
                    }
                }

                UpdatePanel(selCoords, isOverPanel);
            }
            else
            {
                if (disablePanel != null)
                {
                    Destroy(disablePanel);
                    disablePanel = null;
                    disablePanelOverlay = null;
                    disableBackground = null;
                    disableBackground2 = null;
                    disableIcon = null;
                }
            }
        }

        static void EnsurePanel()
        {
            if (cachedScreenWidth != 0 && cachedScreenHeight != 0 && (cachedScreenWidth != Screen.width || cachedScreenHeight != Screen.height))
            {
                Destroy(disablePanel);
                disablePanel = null;
            }
            if (disablePanel == null)
            {
                disablePanel = new GameObject("FeatDisableBuilding");
                var canvas = disablePanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                disablePanelOverlay = new GameObject("FeatDisableBuilding_Overlay");
                disablePanelOverlay.transform.SetParent(disablePanel.transform);

                disableBackground2 = new GameObject("FeatDisableBuilding_BackgroundBorder");
                disableBackground2.transform.SetParent(disablePanel.transform);

                var img = disableBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                disableBackground = new GameObject("FeatDisableBuilding_Background");
                disableBackground.transform.SetParent(disableBackground2.transform);

                img = disableBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                disableIcon = new GameObject("FeatDisableBuilding_Icon");
                disableIcon.transform.SetParent(disableBackground.transform);

                img = disableIcon.AddComponent<Image>();
                img.color = Color.white;

                disableBackground2.AddComponent<GraphicRaycaster>();
                var tt = disableBackground2.AddComponent<CTooltipTarget>();
                tt.text = SLoc.Get("FeatDisableBuilding.Tooltip");
                tt.textDesc = SLoc.Get("FeatDisableBuilding.TooltipDetails", toggleKey.Value);

                cachedScreenWidth = Screen.width;
                cachedScreenHeight = Screen.height;
            }
        }

        static void UpdatePanel(int2 selCoords, bool isOverPanel)
        {
            if (selCoords.Negative)
            {
                disableBackground2?.SetActive(false);
            }
            else
            {
                var padding = 5;

                float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;

                var rectBg2 = disableBackground2.GetComponent<RectTransform>();
                rectBg2.sizeDelta = new Vector2(theScale * (panelSize.Value + 4 * padding), theScale * (panelSize.Value + 4 * padding));
                rectBg2.localPosition = new Vector3(-Screen.width / 2 + theScale * panelLeft.Value + rectBg2.sizeDelta.x / 2, -Screen.height / 2 + theScale * panelBottom.Value + rectBg2.sizeDelta.y / 2);

                var rectBg = disableBackground.GetComponent<RectTransform>();
                rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - theScale * 2 * padding, rectBg2.sizeDelta.y - theScale * 2 * padding);

                var rectIcn = disableIcon.GetComponent<RectTransform>();
                rectIcn.sizeDelta = new Vector2(theScale * panelSize.Value, theScale * panelSize.Value);

                if (disabledLocations.Contains(selCoords))
                {
                    disableIcon.GetComponent<Image>().sprite = buildingDisabled;
                }
                else
                {
                    disableIcon.GetComponent<Image>().sprite = buildingEnabled;
                }

                if (isOverPanel)
                {
                    disableBackground.GetComponent<Image>().color = Color.yellow;
                }
                else
                {
                    disableBackground.GetComponent<Image>().color = defaultPanelLightColor;
                }

                disableBackground2.SetActive(true);
            }
        }

        static void UpdateOverlay()
        {
            if (disablePanelOverlay != null)
            {
                foreach (var coords in disabledLocations)
                {
                    if (!overlayIcons.TryGetValue(coords, out var icon) || icon == null)
                    {
                        icon = new GameObject("Disabled_At_" + coords.x + "_" + coords.y);
                        icon.transform.SetParent(disablePanelOverlay.transform, false);

                        var img = icon.AddComponent<Image>();
                        img.sprite = buildingDisabledOverlay;
                        img.color = Color.white;

                        overlayIcons[coords] = icon;
                    }
                    var rect = icon.GetComponent<RectTransform>();

                    var pos3D = GHexes.Pos(coords);

                    var posCanvas = Camera.main.WorldToScreenPoint(pos3D);

                    var pos3DNeighbor = pos3D;
                    if (coords.x > 0)
                    {
                        pos3DNeighbor = GHexes.Pos(new int2 { x = coords.x - 1, y = coords.y });
                    }
                    else
                    {
                        pos3DNeighbor = GHexes.Pos(new int2 { x = coords.x + 1, y = coords.y });
                    }

                    var posCanvasNeighbor = Camera.main.WorldToScreenPoint(pos3DNeighbor);

                    float scaler = Vector2.Distance(posCanvas, posCanvasNeighbor);

                    rect.localPosition = new Vector2(posCanvas.x, posCanvas.y);
                    rect.sizeDelta = new Vector2(scaler, scaler);
                }

                foreach (var coords in new List<int2>(overlayIcons.Keys))
                {
                    if (!disabledLocations.Contains(coords))
                    {
                        Destroy(overlayIcons[coords]);
                        overlayIcons.Remove(coords);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "CheckStocks2")]
        static void CItem_ContentFactory_CheckStocks2(int2 coords, ref bool __result)
        {
            if (modEnabled.Value && disabledLocations.Contains(coords))
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), "IsExtracting")]
        static void CItem_ContentExtractor_IsExtracting(int2 coords, ref bool __result)
        {
            if (modEnabled.Value && disabledLocations.Contains(coords))
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentPumpingStation), "IsExtracting")]
        static void CItem_ContentPumpingStation_IsExtracting(int2 coords, ref bool __result)
        {
            if (modEnabled.Value && disabledLocations.Contains(coords))
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_Content), nameof(CItem_Content.Destroy))]
        static void CItem_Content_Destroy(int2 coords)
        {
            disabledLocations.Remove(coords);
        }

        static void SaveState()
        {
            StringBuilder sb = new(512);
            foreach (var coords in disabledLocations)
            {
                if (sb.Length != 0)
                {
                    sb.Append(';');
                }
                sb.Append(coords.x).Append(',').Append(coords.y);
            }
            GGame.dicoLandmarks[dicoCoordinates] = sb.ToString();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            RestoreState();
            logger.LogInfo("disabledLocatons.Count = " + disabledLocations.Count);
        }

        static void RestoreState()
        {
            disabledLocations.Clear();
            if (GGame.dicoLandmarks.TryGetValue(dicoCoordinates, out var str))
            {
                var coords = str.Split(';');
                foreach(var coord in coords)
                {
                    var xy = coord.Split(',');
                    if (xy.Length == 2)
                    {
                        try
                        {
                            disabledLocations.Add(new int2 { x = int.Parse(xy[0]), y = int.Parse(xy[1]) });
                        } 
                        catch (Exception ex)
                        {
                            logger.LogError(ex);
                        }
                    }
                }
            }
        }

        // Prevent click-through the panel
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.IsCursorOnGround))]
        static void SMouse_IsCursorOnGround(ref bool __result)
        {
            if (disablePanel != null && disablePanel.activeSelf
                && Within(disableBackground2.GetComponent<RectTransform>(), GetMouseCanvasPos()))
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatDisableBuilding.Tooltip", "Enable/Disable building" },
                { "FeatDisableBuilding.TooltipDetails", "Enable or disable the currently selected building.\nHotkey: {0}.\n\n<i>FeatDisableBuilding mod</i>" }
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatDisableBuilding.Tooltip", "Épület Be/Ki kapcsolása" },
                { "FeatDisableBuilding.TooltipDetails", "A kiválasztott épület be vagy kikapcsolása.\nGyorsbillentyű: {0}.\n\n<i>FeatDisableBuilding mod</i>" }
            });
        }
    }
}
