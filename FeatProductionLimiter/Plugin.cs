using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatProductionLimiter
{
    [BepInPlugin("akarnokd.planbterraformmods.featproductionlimiter", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(modFeatMultiplayerGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string modFeatMultiplayerGuid = "akarnokd.planbterraformmods.featmultiplayer";

        static ConfigEntry<bool> modEnabled;

        static readonly List<string> globalProducts =
            new List<string> {
                "roadway",
                "roadstop",
                "truck",
                "railway",
                "railstop",
                "train",
                "extractor",
                "iceExtractor",
                "pumpingStation",
                "depot",
                "depotMK2",
                "depotMK3",
                "factory",
                "factoryAssemblyPlant",
                "factoryAtmExtractor",
                "factoryGreenhouse",
                "factoryRecycle",
                "factoryFood",
                "landmark",
                "cityDam",
                "forest_pine",
                "forest_leavesHigh",
                // "forest_leavesMultiple",
                // "forest_cactus",
                // "forest_savannah",
                // "forest_coconut",
                "cityIn",
                "cityOut"
            };

        static ConfigEntry<bool> showAll;
        static ConfigEntry<KeyCode> toggleKey;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> itemSize;
        static ConfigEntry<int> maxStatLines;
        static ConfigEntry<int> buttonLeft;
        static ConfigEntry<int> buttonSize;
        static ConfigEntry<bool> autoScale;

        static ManualLogSource logger;

        static Sprite icon;

        static Dictionary<string, CItem> items = new();

        static GameObject limiterPanel;
        static GameObject limiterPanelBackground;
        static GameObject limiterPanelBackground2;
        static GameObject limiterPanelScrollUp;
        static GameObject limiterPanelScrollDown;

        static int limiterPanelOffset;
        static int sortByColumn;
        static bool sortDesc;

        static GameObject limiterButton;
        static GameObject limiterButtonBackground;
        static GameObject limiterButtonBackground2;
        static GameObject limiterButtonIcon;

        static List<LimiterRow> limiterRowsCache = new();
        static LimiterRow limiterPanelHeaderRow;
        static GameObject limiterPanelEmpty;

        static MethodInfo mpApiUpdateItemLimit;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");
            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            showAll = Config.Bind("General", "ShowAll", false, "Always show all products?");

            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.F4, "Key to toggle the limiter panel");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size in the panel");
            itemSize = Config.Bind("General", "ItemSize", 32, "The size of the item's icon in the list");
            buttonLeft = Config.Bind("General", "ButtonLeft", 175, "The button's position relative to the left of the screen");
            buttonSize = Config.Bind("General", "ButtonSize", 50, "The button's width and height");
            maxStatLines = Config.Bind("General", "MaxLines", 16, "How many lines of items to show");
            autoScale = Config.Bind("General", "AutoScale", true, "Scale the position and size of the button with the UI scale of the game?");

            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            var iconPng = LoadPNG(Path.Combine(dir, "Icon.png"));
            icon = Sprite.Create(iconPng, new Rect(0, 0, iconPng.width, iconPng.height), new Vector2(0.5f, 0.5f));

            if (Chainloader.PluginInfos.TryGetValue(modFeatMultiplayerGuid, out var pi))
            {
                logger.LogInfo("Mod " + modFeatMultiplayerGuid + " found. Item limit changes will sync in multiplayer.");
                mpApiUpdateItemLimit = AccessTools.Method(pi.Instance.GetType(), "ApiUpdateItemLimit", new Type[] { typeof(CItem) });
            }
            else
            {
                logger.LogInfo("Mod " + modFeatMultiplayerGuid + " not found.");
            }

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static void SSceneHud_OnUpdate()
        {
            if (modEnabled.Value)
            {
                UpdatePanel();
                UpdateButton();
            }
            else
            {
                if (limiterButton != null)
                {
                    Destroy(limiterButton);
                    limiterButton = null;
                    limiterButtonBackground = null;
                    limiterButtonBackground2 = null;
                    limiterButtonIcon = null;
                }
                if (limiterPanel != null)
                {
                    Destroy(limiterPanel);
                    limiterPanel = null;
                    limiterPanelBackground = null;
                    limiterPanelBackground2 = null;
                    limiterPanelHeaderRow = null;
                    limiterPanelEmpty = null;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnDeactivate")]
        static void SSceneHud_OnDeactivate()
        {
            Destroy(limiterPanel);
            limiterRowsCache.Clear();
        }

        static void UpdateButton()
        {
            if (limiterButton == null)
            {
                limiterButton = new GameObject("FeatProductionLimiterButton");
                var canvas = limiterButton.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 52;

                limiterButtonBackground2 = new GameObject("FeatProductionLimiterButton_BackgroundBorder");
                limiterButtonBackground2.transform.SetParent(limiterButton.transform);

                var img = limiterButtonBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                limiterButtonBackground = new GameObject("FeatProductionLimiterButton_Background");
                limiterButtonBackground.transform.SetParent(limiterButtonBackground2.transform);

                img = limiterButtonBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                limiterButtonIcon = new GameObject("FeatProductionLimiterButton_Icon");
                limiterButtonIcon.transform.SetParent(limiterButtonBackground.transform);

                img = limiterButtonIcon.AddComponent<Image>();
                img.color = Color.white;
                img.sprite = icon;

                limiterButtonBackground2.AddComponent<GraphicRaycaster>();
                var tt = limiterButtonBackground2.AddComponent<CTooltipTarget>();
                tt.text = SLoc.Get("FeatProductionLimiter.Tooltip");
                tt.textDesc = SLoc.Get("FeatProductionLimiter.TooltipDetails", toggleKey.Value);
            }

            float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;

            var padding = 5;

            var rectBg2 = limiterButtonBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(buttonSize.Value + 4 * padding, buttonSize.Value + 4 * padding) * theScale;
            rectBg2.localPosition = new Vector3(-Screen.width / 2 + buttonLeft.Value * theScale + rectBg2.sizeDelta.x / 2, Screen.height / 2 - rectBg2.sizeDelta.y / 2);

            var rectBg = limiterButtonBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * padding * theScale, rectBg2.sizeDelta.y - 2 * padding * theScale);

            var rectIcn = limiterButtonIcon.GetComponent<RectTransform>();
            rectIcn.sizeDelta = new Vector2(buttonSize.Value, buttonSize.Value) * theScale;

            var mp = GetMouseCanvasPos();

            if (IsKeyDown(toggleKey.Value))
            {
                limiterPanel.SetActive(!limiterPanel.activeSelf);
            }
            if (Within(rectBg2, mp))
            {
                limiterButtonBackground.GetComponent<Image>().color = Color.yellow;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    limiterPanel.SetActive(!limiterPanel.activeSelf);
                }
            }
            else
            {
                limiterButtonBackground.GetComponent<Image>().color = DEFAULT_PANEL_COLOR;
            }
        }

        static void UpdatePanel()
        {
            float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;
            var iconSize = itemSize.Value * theScale;

            if (limiterPanel == null)
            {
                limiterPanel = new GameObject("FeatProductionLimiterPanel");
                var canvas = limiterPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 54;

                limiterPanelBackground2 = new GameObject("FeatProductionLimiterPanel_BackgroundBorder");
                limiterPanelBackground2.transform.SetParent(limiterPanel.transform);

                var img = limiterPanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                limiterPanelBackground = new GameObject("FeatProductionLimiterPanel_Background");
                limiterPanelBackground.transform.SetParent(limiterPanelBackground2.transform);

                img = limiterPanelBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                limiterPanelScrollUp = CreateBox(limiterPanelBackground2, "FeatProductionLimiterPanel_ScrollUp", "\u25B2", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                limiterPanelScrollDown = CreateBox(limiterPanelBackground2, "FeatProductionLimiterPanel_ScrollDown", "\u25BC", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                limiterPanel.SetActive(false);

                limiterPanelHeaderRow = new LimiterRow();
                limiterPanelHeaderRow.gIcon = new GameObject("FeatProductionLimiterPanel_HeaderRow_Icon");
                limiterPanelHeaderRow.gIcon.transform.SetParent(limiterPanelBackground.transform);
                limiterPanelHeaderRow.gIcon.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                limiterPanelHeaderRow.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                limiterPanelHeaderRow.gName = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Name", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gInventory = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Inventory", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gZero = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Zero", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gMinus100 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus100", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gMinus10 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus10", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gMinus1 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus1", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gAmount = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Amount", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gPlus1 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus1", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gPlus10 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus10", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gPlus100 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus100", "", fontSize.Value, Color.black);
                limiterPanelHeaderRow.gUnlimited = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Unlimited", "", fontSize.Value, Color.black);

                limiterPanelEmpty = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_NoRows", "<b>No products available</b>", fontSize.Value, Color.black);

                limiterRowsCache.Clear();
                int i = 0;
                foreach (var codeName in globalProducts)
                {
                    var row = new LimiterRow();

                    items.TryGetValue(codeName, out row.item);
                    row.codeName = codeName;
                    row.name = SLoc.Get("ITEM_NAME_" + codeName);
                    limiterRowsCache.Add(row);

                    row.gIcon = new GameObject("FeatProductionLimiterPanel_Row_" + i + "_Icon");
                    row.gIcon.transform.SetParent(limiterPanelBackground.transform);
                    img = row.gIcon.AddComponent<Image>();
                    img.sprite = row.item.icon.Sprite;
                    img.color = row.item.colorItem;
                    row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                    row.gName = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Name", "<b>" + row.name + "</b>", fontSize.Value, Color.black);

                    row.gInventory = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Inventory", "<b>" + string.Format("{0:#,##0}", row.item.nbOwned) + "</b>", fontSize.Value, Color.black);

                    row.gZero = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Zero", "<b> Zero </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                    row.gMinus100 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus100", "<b> -100 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);
                    row.gMinus10 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus10", "<b> -10 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);
                    row.gMinus1 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus1", "<b> -1 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                    row.gAmount = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Amount", "", fontSize.Value, Color.black);

                    row.gPlus1 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus1", "<b> +1 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);
                    row.gPlus10 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus10", "<b> +10 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);
                    row.gPlus100 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus100", "<b> +100 </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                    row.gUnlimited = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Unlimited", "<b> ∞ </b>", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                    i++;
                }
            }

            if (!limiterPanel.activeSelf)
            {
                return;
            }

            int[] maxWidths = new int[] { 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0 };

            foreach (var sr in limiterRowsCache)
            {
                if (sr.item.nbOwnedMax < 0)
                {
                    sr.gAmount.GetComponent<Text>().text = "<b>" + SLoc.Get("FeatProductionLimiter.Unlimited") + "</b>";
                }
                else
                {
                    sr.gAmount.GetComponent<Text>().text = "<b>" + sr.item.nbOwnedMax + "</b>";
                }

                sr.gInventory.GetComponent<Text>().text = string.Format("{0:#,##0}", sr.item.nbOwned);

                ResizeBox(sr.gName, fontSize.Value * theScale);
                ResizeBox(sr.gInventory, fontSize.Value * theScale);
                ResizeBox(sr.gZero, fontSize.Value * theScale);
                ResizeBox(sr.gMinus100, fontSize.Value * theScale);
                ResizeBox(sr.gMinus10, fontSize.Value * theScale);
                ResizeBox(sr.gMinus1, fontSize.Value * theScale);
                ResizeBox(sr.gAmount, fontSize.Value * theScale);
                ResizeBox(sr.gPlus1, fontSize.Value * theScale);
                ResizeBox(sr.gPlus10, fontSize.Value * theScale);
                ResizeBox(sr.gPlus100, fontSize.Value * theScale);
                ResizeBox(sr.gUnlimited, fontSize.Value * theScale);

                int col = 0;
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gName));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gInventory));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gZero));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gMinus100));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gMinus10));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gMinus1));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gAmount));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gPlus1));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gPlus10));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gPlus100));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gUnlimited));

                sr.SetActive(false);
            }

            Comparison<LimiterRow> comp = null;

            if (sortByColumn == 0)
            {
                comp = (a, b) => a.name.CompareTo(b.name);
            }
            if (sortByColumn == 1)
            {
                comp = (a, b) =>
                {
                    var c = a.item.nbOwned.CompareTo(b.item.nbOwned);
                    if (c == 0)
                    {
                        c = a.name.CompareTo(b.name);
                    }
                    return c;
                };
            }
            if (sortByColumn == 2)
            {
                comp = (a, b) =>
                {
                    var limA = a.item.nbOwnedMax;
                    var limB = b.item.nbOwnedMax;
                    if (limA < 0 && limB >= 0)
                    {
                        return 1;
                    }
                    if (limA >= 0 && limB < 0)
                    {
                        return -1;
                    }

                    var c = limA.CompareTo(limB);
                    if (c == 0)
                    {
                        c = a.name.CompareTo(b.name);
                    }
                    return c;
                };
            }

            if (comp != null)
            {
                if (sortDesc)
                {
                    var oldComp = comp;
                    comp = (a, b) => oldComp(b, a);
                }

                limiterRowsCache.Sort(comp);
            }

            List<LimiterRow> rows = new();
            rows.AddRange(limiterRowsCache);

            // hide items not unlocked yet
            if (!showAll.Value)
            {
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    var ri = rows[i];
                    if (!IsUnlocked(ri.item))
                    {
                        rows.RemoveAt(i);
                    }
                }
            }

            var mp = GetMouseCanvasPos();
            if (Within(limiterPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > 0)
                {
                    limiterPanelOffset = Math.Max(0, limiterPanelOffset - 1);
                }
                else
                if (scrollDelta < 0)
                {
                    limiterPanelOffset = limiterPanelOffset + 1;
                }
            }
            int maxNameWidth = 0;
            var vPadding = 10 * theScale;
            var hPadding = 30 * theScale;
            var hPaddingSmall = 10 * theScale;
            int border = 5;

            var maxLines = maxStatLines.Value;

            // adjust max lines depending on the available screen space
            var maxScreenSpace = Screen.height - 200 * theScale;
            var rowHeightAvg = iconSize + vPadding;
            var canShowLines = Math.Max(1, Mathf.FloorToInt(maxScreenSpace / rowHeightAvg));
            if (maxLines > canShowLines)
            {
                maxLines = canShowLines;
            }

            if (limiterPanelOffset + maxLines > rows.Count)
            {
                limiterPanelOffset = Math.Max(0, rows.Count - maxLines);
            }

            limiterPanelScrollUp.SetActive(limiterPanelOffset > 0);
            limiterPanelScrollDown.SetActive(limiterPanelOffset + maxLines < rows.Count);

            if (rows.Count == 0)
            {
                ResizeBox(limiterPanelEmpty, fontSize.Value * theScale);
                maxNameWidth = GetPreferredWidth(limiterPanelEmpty);
                SetLocalPosition(limiterPanelEmpty, 0, 0);
                limiterPanelEmpty.SetActive(true);
                limiterPanelHeaderRow.SetActive(false);
            }
            else
            {
                rows.Insert(limiterPanelOffset, limiterPanelHeaderRow);
                limiterPanelEmpty.SetActive(false);
                limiterPanelHeaderRow.SetActive(true);

                limiterPanelHeaderRow.gName.GetComponent<Text>().text = SLoc.Get("FeatProductionLimiter.Item") + GetSortIndicator(0);
                limiterPanelHeaderRow.gInventory.GetComponent<Text>().text = SLoc.Get("FeatProductionLimiter.Inventory") + GetSortIndicator(1);
                limiterPanelHeaderRow.gAmount.GetComponent<Text>().text = SLoc.Get("FeatProductionLimiter.Amount") + GetSortIndicator(2);

                ResizeBox(limiterPanelHeaderRow.gName, fontSize.Value * theScale);
                ResizeBox(limiterPanelHeaderRow.gInventory, fontSize.Value * theScale);
                ResizeBox(limiterPanelHeaderRow.gAmount, fontSize.Value * theScale);

                ApplyPreferredSize(limiterPanelHeaderRow.gName);
                ApplyPreferredSize(limiterPanelHeaderRow.gInventory);
                ApplyPreferredSize(limiterPanelHeaderRow.gAmount);

                MaxOf(ref maxWidths[0], GetPreferredWidth(limiterPanelHeaderRow.gName));
                MaxOf(ref maxWidths[1], GetPreferredWidth(limiterPanelHeaderRow.gInventory));
                MaxOf(ref maxWidths[6], GetPreferredWidth(limiterPanelHeaderRow.gAmount));

                maxLines++; // header
            }


            var bgHeight = maxLines * (iconSize + vPadding) + vPadding + 2 * border;
            var bgWidth = 2 * border + 2 * vPadding + 5 * hPadding + 6 * hPaddingSmall + iconSize + maxWidths.Sum();

            var rectBg2 = limiterPanelBackground2.GetComponent<RectTransform>();
            // do not resize when the bgWidth does small changes
            var currWidth = rectBg2.sizeDelta.x;
            if (Math.Abs(currWidth - bgWidth) >= 10)
            {
                bgWidth = Mathf.CeilToInt(bgWidth / 10) * 10;
            }
            else
            {
                bgWidth = currWidth;
            }

            rectBg2.sizeDelta = new Vector2(bgWidth, bgHeight);
            rectBg2.localPosition = new Vector3(0, -40 * theScale); // do not overlap the top-center panel

            var rectBg = limiterPanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * border * theScale, rectBg2.sizeDelta.y - 2 * border * theScale);

            ResizeBox(limiterPanelScrollUp, fontSize.Value * theScale);
            ResizeBox(limiterPanelScrollDown, fontSize.Value * theScale);

            limiterPanelScrollUp.GetComponent<RectTransform>().localPosition = new Vector2(0, rectBg2.sizeDelta.y / 2 - 2);
            limiterPanelScrollDown.GetComponent<RectTransform>().localPosition = new Vector2(0, -rectBg2.sizeDelta.y / 2 + 2);

            float dy = rectBg.sizeDelta.y / 2 - vPadding;
            for (int i = limiterPanelOffset; i < rows.Count && i < limiterPanelOffset + maxLines; i++)
            {
                var row = rows[i];

                float y = dy - iconSize / 2;

                float dx = -rectBg.sizeDelta.x / 2 + vPadding;

                SetLocalPosition(row.gIcon, dx + iconSize / 2, y);

                dx += iconSize + hPadding;

                SetLocalPosition(row.gName, dx + GetPreferredWidth(row.gName) / 2, y);

                int col = 0;

                dx += maxWidths[col++] + hPadding;

                SetLocalPosition(row.gInventory, dx + maxWidths[col] - GetPreferredWidth(row.gInventory) / 2, y);

                dx += maxWidths[col++] + hPadding;

                SetLocalPosition(row.gZero, dx + GetPreferredWidth(row.gZero) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gMinus100, dx + GetPreferredWidth(row.gMinus100) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gMinus10, dx + GetPreferredWidth(row.gMinus10) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gMinus1, dx + GetPreferredWidth(row.gMinus1) / 2, y);

                dx += maxWidths[col++] + hPadding;

                SetLocalPosition(row.gAmount, dx + maxWidths[6] - GetPreferredWidth(row.gAmount) / 2, y);

                dx += maxWidths[col++] + hPadding;

                SetLocalPosition(row.gPlus1, dx + GetPreferredWidth(row.gPlus1) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gPlus10, dx + GetPreferredWidth(row.gPlus10) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gPlus100, dx + GetPreferredWidth(row.gPlus100) / 2, y);

                dx += maxWidths[col++] + hPaddingSmall;

                SetLocalPosition(row.gUnlimited, dx + GetPreferredWidth(row.gUnlimited) / 2, y);

                // --- next row

                dy -= iconSize + vPadding;

                row.SetActive(true);

                if (i != limiterPanelOffset)
                {
                    CheckRowButton(rectBg2, mp, row.gZero, ChangeLimitZero(row));
                    CheckRowButton(rectBg2, mp, row.gMinus100, ChangeLimit(row, -100));
                    CheckRowButton(rectBg2, mp, row.gMinus10, ChangeLimit(row, -10));
                    CheckRowButton(rectBg2, mp, row.gMinus1, ChangeLimit(row, -1));
                    CheckRowButton(rectBg2, mp, row.gPlus1, ChangeLimit(row, 1));
                    CheckRowButton(rectBg2, mp, row.gPlus10, ChangeLimit(row, 10));
                    CheckRowButton(rectBg2, mp, row.gPlus100, ChangeLimit(row, 100));
                    CheckRowButton(rectBg2, mp, row.gUnlimited, ChangeLimitUnlimited(row));
                }
            }

            if (limiterPanelHeaderRow.gName.activeSelf)
            {

                CheckMouseSort(limiterPanelHeaderRow.gName, 0);
                CheckMouseSort(limiterPanelHeaderRow.gInventory, 1);
                CheckMouseSort(limiterPanelHeaderRow.gAmount, 2);
            }
        }

        static bool IsUnlocked(CItem item)
        {
            if (item.IsItemUnlocked())
            {
                return true;
            }

            // check if item is designated to be unlocked at all?
            foreach (var ggl in GGame.levels)
            {
                foreach (var ul in ggl.unlockItems)
                {
                    if (ul == item)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static Action ChangeLimitZero(LimiterRow row)
        {
            return () => 
            {
                row.item.nbOwnedMax = 0;
                mpApiUpdateItemLimit?.Invoke(null, new object[] { row.item });
            };
        }
        static Action ChangeLimitUnlimited(LimiterRow row)
        {
            return () =>
            {
                row.item.nbOwnedMax = -1;
                mpApiUpdateItemLimit?.Invoke(null,new object[] { row.item });
            };
        }

        static Action ChangeLimit(LimiterRow row, int delta)
        {
            return () =>
            {
                long d = delta;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    d *= 10;
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        d *= 10;
                    }
                }

                long n = row.item.nbOwnedMax;
                if (n < 0 && d < 0)
                {
                    row.item.nbOwnedMax = 0;
                }
                else
                {
                    if (n < 0)
                    {
                        n = 0;
                    }
                    n += d;
                    row.item.nbOwnedMax = (int)Math.Min(int.MaxValue, Math.Max(0, n));
                }
                mpApiUpdateItemLimit?.Invoke(null, new object[] { row.item });
            };
        }

        static void CheckRowButton(RectTransform rectBg2, Vector2 mp, GameObject button, Action onPress)
        {
            var img = button.GetComponent<Image>();
            if (Within(rectBg2, button.GetComponent<RectTransform>(), mp))
            {
                img.color = DEFAULT_BOX_COLOR_HOVER;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    onPress();
                }
            }
            else
            {
                img.color = DEFAULT_BOX_COLOR;
            }
        }

        static void MaxOf(ref int max, int amount)
        {
            max = Mathf.Max(max, amount);
        }

        static void CheckMouseSort(GameObject go, int col)
        {
            if (Within(limiterPanelBackground2.GetComponent<RectTransform>(), go.GetComponent<RectTransform>(), GetMouseCanvasPos()))
            {
                go.GetComponent<Text>().color = Color.red;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    SetSort(col);
                }
            }
            else
            {
                go.GetComponent<Text>().color = Color.black;
            }
        }

        static void SetSort(int col)
        {
            if (sortByColumn != col)
            {
                sortDesc = false;
            }
            else
            {
                sortDesc = !sortDesc;
            }
            sortByColumn = col;
        }

        static string GetSortIndicator(int col)
        {
            return col == sortByColumn ? (sortDesc ? " \u2193" : " \u2191") : "";
        }

        static void ApplyPreferredSize(GameObject go)
        {
            var txt = go.GetComponent<Text>();
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem), nameof(CItem.Init))]
        static void CItem_Init(CItem __instance)
        {
            items[__instance.codeName] = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            Destroy(limiterPanel);
            limiterRowsCache.Clear();
        }

        internal class LimiterRow
        {
            internal string codeName;
            internal string name;
            internal CItem item;

            internal GameObject gIcon;
            internal GameObject gName;
            internal GameObject gInventory;
            internal GameObject gZero;
            internal GameObject gMinus100;
            internal GameObject gMinus10;
            internal GameObject gMinus1;
            internal GameObject gAmount;
            internal GameObject gPlus1;
            internal GameObject gPlus10;
            internal GameObject gPlus100;
            internal GameObject gUnlimited;

            internal void SetActive(bool active)
            {
                gIcon.SetActive(active);
                gName.SetActive(active);
                gInventory.SetActive(active);
                gZero.SetActive(active);
                gMinus100.SetActive(active);
                gMinus10.SetActive(active);
                gMinus1.SetActive(active);
                gAmount.SetActive(active);
                gPlus1.SetActive(active);
                gPlus10.SetActive(active);
                gPlus100.SetActive(active);
                gUnlimited.SetActive(active);
            }
        }

        // Prevent click-through the panel
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.IsCursorOnGround))]
        static void SMouse_IsCursorOnGround(ref bool __result)
        {
            var mp = GetMouseCanvasPos();
            if (limiterPanel != null && limiterPanel.activeSelf
                && Within(limiterPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                __result = false;
            }
            if (limiterButton != null && limiterButton.activeSelf
                && Within(limiterButtonBackground2.GetComponent<RectTransform>(), mp))
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
                { "FeatProductionLimiter.Tooltip", "Toggle Limiter Settings" },
                { "FeatProductionLimiter.TooltipDetails", "Toggle the Production Limiter settings panel.\nHotkey: {0}.\n\n<i>FeatProductionLimiter mod</i>" },
                { "FeatProductionLimiter.Item", "<i>Item</i>" },
                { "FeatProductionLimiter.Amount", "<i>Limit (pcs)</i>" },
                { "FeatProductionLimiter.Inventory", "<i>Inventory (pcs)</i>" },
                { "FeatProductionLimiter.Unlimited", "Unlimited" },
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatProductionLimiter.Tooltip", "Gyártási korlátok beállítása" },
                { "FeatProductionLimiter.TooltipDetails", "A gyártási korlátok képernyő megjelenítése vagy elrejtése.\nGyorsbillentyű: {0}.\n\n<i>FeatProductionLimiter mod</i>" },
                { "FeatProductionLimiter.Item", "<i>Név</i>" },
                { "FeatProductionLimiter.Amount", "<i>Korlát (db)</i>" },
                { "FeatProductionLimiter.Inventory", "<i>Készleten (db)</i>" },
                { "FeatProductionLimiter.Unlimited", "Korlátlan" },
            });
        }


    }
}
