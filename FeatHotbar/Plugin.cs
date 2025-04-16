using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LibCommon;
using static LibCommon.GUITools;
using static UnityEngine.ParticleSystem.PlaybackState;
using System.Collections;

namespace FeatHotbar
{
    [BepInPlugin("akarnokd.planbterraformmods.feathotbar", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> panelHeight;
        static ConfigEntry<int> panelBottom;
        static ConfigEntry<bool> autoScale;
        static ConfigEntry<string>[] loadouts;
        static ConfigEntry<int> itemSize;
        static ConfigEntry<int> maxStatLines;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> fontSizeSmall;
        static ConfigEntry<KeyCode> toggleKey;
        static ConfigEntry<int> buildModeDelay;

        static ManualLogSource logger;

        static GameObject hotbarPanel;
        static GameObject hotbarPanelBackground;
        static GameObject[] hotbarPanelSwitches;
        static GameObject[,] hotbarPanelSlots;
        static string[,] hotbarPanelSlotAssignments;
        static int hotbarPanelIndex;

        static GameObject hotbarSelectionPanel;
        static GameObject hotbarSelectionPanelBackground;
        static GameObject hotbarSelectionPanelBackground2;
        static GameObject hotbarSelectionPanelScrollUp;
        static GameObject hotbarSelectionPanelScrollDown;

        static int hotbarSelectionPanelOffset;
        static int sortByColumn;
        static bool sortDesc;

        static List<SelectionRow> selectionRowsCache = new();
        static SelectionRow hotbarSelectionPanelHeaderRow;
        static GameObject hotbarSelectionPanelEmpty;

        static int targetSubpanel = -1;
        static int targetSlot = -1;

        const int numSubpanels = 3;
        const int numButtonsPerPanel = 9;

        static Dictionary<string, CItem> items = new();

        static readonly HashSet<string> excludeItems = [
            "contentNei",
            "crossRailBuoy",
            "crossRoadBuoy"
            ];

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            panelHeight = Config.Bind("General", "PanelHeight", 75, "The height of the panel");
            panelBottom = Config.Bind("General", "PanelBottom", 100, "The distance from the bottom of the screen");
            autoScale = Config.Bind("General", "AutoScale", true, "Scale the position and size of the button with the UI scale of the game?");
            itemSize = Config.Bind("General", "ItemSize", 32, "The size of the item's icon in the building selection list");
            maxStatLines = Config.Bind("General", "MaxLines", 16, "How many lines of items to show in the building selection list");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size in the building selection panel");
            fontSizeSmall = Config.Bind("General", "FontSizeSmall", 12, "The font size of the total current count on buildings");
            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.H, "The key to show/hide the hotbar");
            buildModeDelay = Config.Bind("General", "BuildModeDelay", 100, "Delay the build mode activation by this amount of milliseconds. Should help with click-drag misplacements near the hotbar.");

            hotbarPanelSlotAssignments = new string[numSubpanels, numButtonsPerPanel];
            loadouts = new ConfigEntry<string>[numSubpanels];

            for (int i = 0; i < numSubpanels; i++)
            {
                var cfg = Config.Bind("General", "Loadout" + (i + 1), "", "The list of buildings for subpanel " + (i + 1));
                loadouts[i] = cfg;

                var entries = cfg.Value.Split(',');
                for (int j = 0; j < entries.Length && j < numButtonsPerPanel; j++)
                {
                    hotbarPanelSlotAssignments[i, j] = entries[j];
                }
            }

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
                UpdateSelectionPanel();
            }
            else
            {
                if (hotbarPanel != null)
                {
                    Destroy(hotbarPanel);
                    hotbarPanel = null;
                    hotbarPanelSwitches = null;
                    hotbarPanelBackground = null;
                    hotbarPanelIndex = 0;
                    hotbarPanelSlots = null;
                }
            }
            return true;
        }

        static void UpdatePanel()
        {
            var theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;
            var padding = 5 * theScale;
            var panelH = panelHeight.Value * theScale;

            if (hotbarPanel == null)
            {
                hotbarPanel = new GameObject("FeatHotbarPanel");
                var canvas = hotbarPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 56;

                hotbarPanelBackground = new GameObject("FeatHotbarPanel_Background");
                hotbarPanelBackground.transform.SetParent(hotbarPanel.transform, false);
                hotbarPanelBackground.AddComponent<GraphicRaycaster>();
                var img = hotbarPanelBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR.AlphaMultiplied(0.95f);

                hotbarPanelSwitches = new GameObject[numSubpanels];

                hotbarPanelSlots = new GameObject[numSubpanels, numButtonsPerPanel];

                for (int i = 0; i < hotbarPanelSwitches.Length; i++)
                {
                    var gSwitch = CreateBox(hotbarPanel, "FeatHotbarPanel_Switch_" + i, (i + 1).ToString(), (int)(panelH / numSubpanels) - 1, Color.black, GUITools.DEFAULT_PANEL_COLOR);
                    gSwitch.AddComponent<GraphicRaycaster>();

                    var tt = gSwitch.AddComponent<CTooltipTarget>();
                    tt.text = SLoc.Get("FeatHotbar.Switch.Tooltip");

                    hotbarPanelSwitches[i] = gSwitch;

                    for (int j = 0; j < numButtonsPerPanel; j++)
                    {
                        var gButton = new GameObject("FeatHotbarPanel_Button_" + i + "_" + j);
                        gButton.transform.SetParent(hotbarPanelBackground.transform, false);
                        img = gButton.AddComponent<Image>();
                        img.color = Color.white;

                        gButton.AddComponent<GraphicRaycaster>();
                        tt = gButton.AddComponent<CTooltipTarget>();
                        tt.text = SLoc.Get("FeatHotbar.Button.Empty");
                        tt.textDesc = SLoc.Get("FeatHotbar.Button.Tooltip");

                        CreateBox(gButton, "FeatHotbarPanel_Button_" + i + "_" + j + "_Text", "", fontSizeSmall.Value, new Color(0.75f, 0.75f, 0.75f, 1f),  Color.black);

                        hotbarPanelSlots[i, j] = gButton;
                    }
                }
            }

            if (IsKeyDown(toggleKey.Value))
            {
                hotbarPanel.SetActive(!hotbarPanel.activeSelf);
            }
            if (!hotbarPanel.activeSelf)
            {
                return;
            }

            var th = panelH / numSubpanels;

            var rectBackground = hotbarPanelBackground.GetComponent<RectTransform>();
            rectBackground.sizeDelta = new Vector2(numButtonsPerPanel * panelH, panelH);
            rectBackground.localPosition = new Vector3(0, -Screen.height / 2 + panelBottom.Value * theScale + rectBackground.sizeDelta.y / 2);

            var mp = GetMouseCanvasPos();

            for (int i = 0; i < hotbarPanelSwitches.Length; i++)
            {
                var gSwitch = hotbarPanelSwitches[i];
                var swRect = gSwitch.GetComponent<RectTransform>();

                swRect.sizeDelta = new Vector2(th, th);
                swRect.localPosition = new Vector2(-rectBackground.sizeDelta.x / 2 - swRect.sizeDelta.x / 2, 
                    -Screen.height / 2 + panelBottom.Value * theScale + th / 2 + i * th);

                if (Within(swRect, mp))
                {
                    if (hotbarPanelIndex == i)
                    {
                        swRect.GetComponent<Image>().color = DEFAULT_PANEL_COLOR;
                        swRect.GetComponentInChildren<Text>().color = Color.black;
                    }
                    else
                    { 
                        swRect.GetComponent<Image>().color = DEFAULT_BOX_COLOR_HOVER;
                        swRect.GetComponentInChildren<Text>().color = Color.white;
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        hotbarPanelIndex = i;
                    }
                }
                else
                {
                    if (hotbarPanelIndex == i)
                    {
                        swRect.GetComponent<Image>().color = DEFAULT_PANEL_COLOR;
                        swRect.GetComponentInChildren<Text>().color = Color.black;
                    }
                    else
                    {
                        swRect.GetComponent<Image>().color = DEFAULT_BOX_COLOR;
                        swRect.GetComponentInChildren<Text>().color = Color.white;
                    }
                }

                for (int j = 0; j < numButtonsPerPanel; j++)
                {
                    var gButton = hotbarPanelSlots[i, j];
                    var br = gButton.GetComponent<RectTransform>();

                    br.sizeDelta = new Vector2(panelH - 2 * padding, panelH - 2 * padding);
                    br.localPosition = new Vector2(-numButtonsPerPanel * panelH / 2 + j * panelH + panelH / 2, 0);

                    var img = gButton.GetComponent<Image>();
                    var tool = gButton.GetComponent<CTooltipTarget>();
                    var building = hotbarPanelSlotAssignments[i, j];
                    var isEmpty = string.IsNullOrEmpty(building);
                    if (isEmpty)
                    {
                        tool.text = SLoc.Get("FeatHotbar.Button.Empty");
                    }
                    else
                    {
                        tool.text = SLoc.Get("FeatHotbar.Button.Build", SLoc.Get("ITEM_NAME_" + building));
                    }

                    var shown = i == hotbarPanelIndex;
                    gButton.SetActive(shown);

                    if (shown)
                    {
                        CItem item = null;
                        if (building != null)
                        {
                            items.TryGetValue(building, out item);
                        }

                        var nbTxt = gButton.GetComponentInChildren<Text>(true);
                        var nbBox = nbTxt.gameObject.transform.parent.gameObject;

                        if (item != null)
                        {
                            nbTxt.text = string.Format("<b>{0:#,##0}</b>", item.nbOwned);
                            hotbarPanelSlots[i, j].GetComponent<Image>().sprite = item.icon.Asset;
                            nbBox.SetActive(true);
                        }
                        else
                        {
                            nbTxt.text = "";
                            hotbarPanelSlots[i, j].GetComponent<Image>().sprite = null;
                            nbBox.SetActive(false);
                        }
                        ResizeBox(nbBox, fontSizeSmall.Value * theScale);

                        var nbRect = nbBox.GetComponent<RectTransform>();

                        nbRect.localPosition = new Vector2(br.sizeDelta.x / 2 - nbRect.sizeDelta.x / 2, -br.sizeDelta.y / 2 + nbRect.sizeDelta.y / 2);

                        if (Within(rectBackground, br, mp))
                        {
                            img.color = Color.yellow;
                            if (Input.GetKeyDown(KeyCode.Mouse1) || (isEmpty && Input.GetKeyDown(KeyCode.Mouse0)))
                            {
                                if (targetSubpanel == -1)
                                {
                                    targetSubpanel = i;
                                    targetSlot = j;
                                    hotbarSelectionPanel.SetActive(true);
                                }
                                else
                                {
                                    targetSubpanel = -1;
                                    targetSlot = -1;
                                    hotbarSelectionPanel.SetActive(false);
                                }
                            }
                            if (Input.GetKeyDown(KeyCode.Mouse2))
                            {
                                hotbarPanelSlotAssignments[i, j] = null;
                                PersistHotbar();
                            }
                            if (Input.GetKeyDown(KeyCode.Mouse0) && !isEmpty)
                            {
                                // select for building mode
                                if (item != null)
                                {
                                    SSceneSingleton<SSceneHud_ItemsBars>.Inst.toggleDelete.isOn = false;
                                    var delay = buildModeDelay.Value;
                                    if (delay > 0)
                                    {
                                        GSceneHud.itemInBarSelected = null;
                                        SSceneSingleton<SSceneHud_ItemsBars>.Inst.StartCoroutine(EnterBuildMode(item, delay));
                                    }
                                    else
                                    {
                                        GSceneHud.itemInBarSelected = item;
                                    }
                                }
                            }
                        }
                        else
                        {
                            img.color = Color.white;
                        }
                    }
                }
            }
        }

        static IEnumerator EnterBuildMode(CItem item, int delay)
        {
            yield return new WaitForSecondsRealtime(delay / 1000f);
            GSceneHud.itemInBarSelected = item;
        }

        static void PersistHotbar()
        {
            for (int i = 0; i < numSubpanels; i++)
            {
                var cfg = loadouts[i];

                List<string> lst = new();
                for (int j = 0; j < numButtonsPerPanel; j++)
                {
                    var b = hotbarPanelSlotAssignments[i, j] ?? "";
                    lst.Add(b);
                }
                cfg.Value = string.Join(",", lst);
            }
        }

        static void UpdateSelectionPanel()
        {
            float theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;
            var iconSize = itemSize.Value * theScale;

            if (hotbarSelectionPanel == null)
            {
                hotbarSelectionPanel = new GameObject("FeatHotbarSelectionPanel");
                var canvas = hotbarSelectionPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 54;

                hotbarSelectionPanelBackground2 = new GameObject("FeatHotbarSelectionPanel_BackgroundBorder");
                hotbarSelectionPanelBackground2.transform.SetParent(hotbarSelectionPanel.transform);

                var img = hotbarSelectionPanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                hotbarSelectionPanelBackground2.AddComponent<GraphicRaycaster>();

                hotbarSelectionPanelBackground = new GameObject("FeatHotbarSelectionPanel_Background");
                hotbarSelectionPanelBackground.transform.SetParent(hotbarSelectionPanelBackground2.transform);

                img = hotbarSelectionPanelBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                hotbarSelectionPanelScrollUp = CreateBox(hotbarSelectionPanelBackground2, "FeatHotbarSelectionPanel_ScrollUp", "\u25B2", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                hotbarSelectionPanelScrollDown = CreateBox(hotbarSelectionPanelBackground2, "FeatHotbarSelectionPanel_ScrollDown", "\u25BC", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                hotbarSelectionPanel.SetActive(false);

                hotbarSelectionPanelHeaderRow = new SelectionRow();
                hotbarSelectionPanelHeaderRow.gIcon = new GameObject("FeatHotbarSelectionPanel_HeaderRow_Icon");
                hotbarSelectionPanelHeaderRow.gIcon.transform.SetParent(hotbarSelectionPanelBackground.transform);
                hotbarSelectionPanelHeaderRow.gIcon.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                hotbarSelectionPanelHeaderRow.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                hotbarSelectionPanelHeaderRow.gName = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_HeaderRow_Name", "", fontSize.Value, Color.black);
                hotbarSelectionPanelHeaderRow.gInventory = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_HeaderRow_Inventory", "", fontSize.Value, Color.black);
                hotbarSelectionPanelHeaderRow.gSelect = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_HeaderRow_Zero", "", fontSize.Value, Color.black);

                hotbarSelectionPanelEmpty = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_NoRows", SLoc.Get("FeatHotbar.NoBuildings"), fontSize.Value, Color.black);

                selectionRowsCache.Clear();
                int i = 0;
                foreach (var item in items)
                {
                    if (item.Value is not CItem_Content 
                        || item.Value.hiddenInItemBar
                        || item.Value is CItem_ContentCity
                        || item.Value is CItem_ContentCityDead
                        || (item.Value.uiGroup != null && (item.Value.uiGroup.isDebug || !item.Value.uiGroup.showInItemBar))
                        || excludeItems.Contains(item.Key)
                        )
                    {
                        continue;
                    }
                    var row = new SelectionRow();

                    row.item = item.Value;
                    row.codeName = item.Key;
                    row.name = SLoc.Get("ITEM_NAME_" + item.Key);
                    selectionRowsCache.Add(row);

                    row.gIcon = new GameObject("FeatHotbarSelectionPanel_Row_" + i + "_Icon");
                    row.gIcon.transform.SetParent(hotbarSelectionPanelBackground.transform);
                    img = row.gIcon.AddComponent<Image>();
                    img.sprite = row.item.icon.Asset;
                    img.color = row.item.colorItem;
                    row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                    row.gName = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_Row_" + i + "_Name", "<b>" + row.name + "</b>", fontSize.Value, Color.black);

                    row.gInventory = CreateText(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_Row_" + i + "_Inventory", "<b>" + string.Format("{0:#,##0}", row.item.nbOwned) + "</b>", fontSize.Value, Color.black);

                    row.gSelect = CreateBox(hotbarSelectionPanelBackground, "FeatHotbarSelectionPanel_Row_" + i + "_Select", SLoc.Get("FeatHotbar.Select"), fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                    i++;
                }
            }

            if (IsKeyDown(KeyCode.Escape))
            {
                targetSlot = -1;
                targetSubpanel = -1;
                hotbarSelectionPanel.SetActive(false);
                return;
            }
            if (!hotbarSelectionPanel.activeSelf)
            {
                return;
            }

            int[] maxWidths = new int[] { 0, 0, 0 };

            foreach (var sr in selectionRowsCache)
            {
                sr.gInventory.GetComponent<Text>().text = string.Format("{0:#,##0}", sr.item.nbOwned);

                ResizeBox(sr.gName, fontSize.Value * theScale);
                ResizeBox(sr.gInventory, fontSize.Value * theScale);
                ResizeBox(sr.gSelect, fontSize.Value * theScale);

                int col = 0;
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gName));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gInventory));
                MaxOf(ref maxWidths[col++], GetPreferredWidth(sr.gSelect));

                sr.SetActive(false);
            }

            Comparison<SelectionRow> comp = null;

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

            if (comp != null)
            {
                if (sortDesc)
                {
                    var oldComp = comp;
                    comp = (a, b) => oldComp(b, a);
                }

                selectionRowsCache.Sort(comp);
            }

            List<SelectionRow> rows = new();
            rows.AddRange(selectionRowsCache);

            // hide items not unlocked yet
            /*
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
            */

            var mp = GetMouseCanvasPos();
            if (Within(hotbarSelectionPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > 0)
                {
                    hotbarSelectionPanelOffset = Math.Max(0, hotbarSelectionPanelOffset - 1);
                }
                else
                if (scrollDelta < 0)
                {
                    hotbarSelectionPanelOffset = hotbarSelectionPanelOffset + 1;
                }
            }
            int maxNameWidth = 0;
            var vPadding = 10 * theScale;
            var hPadding = 30 * theScale;
            var hPaddingSmall = 10 * theScale;
            int border = 5;

            var maxLines = maxStatLines.Value;

            // adjust max lines depending on the available screen space
            var maxScreenSpace = Screen.height - theScale * (200 + panelBottom.Value + panelHeight.Value);
            var rowHeightAvg = iconSize + vPadding;
            var canShowLines = Math.Max(1, Mathf.FloorToInt(maxScreenSpace / rowHeightAvg));
            if (maxLines > canShowLines)
            {
                maxLines = canShowLines;
            }

            if (hotbarSelectionPanelOffset + maxLines > rows.Count)
            {
                hotbarSelectionPanelOffset = Math.Max(0, rows.Count - maxLines);
            }

            hotbarSelectionPanelScrollUp.SetActive(hotbarSelectionPanelOffset > 0);
            hotbarSelectionPanelScrollDown.SetActive(hotbarSelectionPanelOffset + maxLines < rows.Count);

            if (rows.Count == 0)
            {
                ResizeBox(hotbarSelectionPanelEmpty, fontSize.Value * theScale);
                maxNameWidth = GetPreferredWidth(hotbarSelectionPanelEmpty);
                SetLocalPosition(hotbarSelectionPanelEmpty, 0, 0);
                hotbarSelectionPanelEmpty.SetActive(true);
                hotbarSelectionPanelHeaderRow.SetActive(false);
            }
            else
            {
                rows.Insert(hotbarSelectionPanelOffset, hotbarSelectionPanelHeaderRow);
                hotbarSelectionPanelEmpty.SetActive(false);
                hotbarSelectionPanelHeaderRow.SetActive(true);

                hotbarSelectionPanelHeaderRow.gName.GetComponent<Text>().text = SLoc.Get("FeatProductionLimiter.Item") + GetSortIndicator(0);
                hotbarSelectionPanelHeaderRow.gInventory.GetComponent<Text>().text = SLoc.Get("FeatProductionLimiter.Inventory") + GetSortIndicator(1);

                ResizeBox(hotbarSelectionPanelHeaderRow.gName, fontSize.Value * theScale);
                ResizeBox(hotbarSelectionPanelHeaderRow.gInventory, fontSize.Value * theScale);

                ApplyPreferredSize(hotbarSelectionPanelHeaderRow.gName);
                ApplyPreferredSize(hotbarSelectionPanelHeaderRow.gInventory);

                MaxOf(ref maxWidths[0], GetPreferredWidth(hotbarSelectionPanelHeaderRow.gName));
                MaxOf(ref maxWidths[1], GetPreferredWidth(hotbarSelectionPanelHeaderRow.gInventory));

                maxLines++; // header
            }


            var bgHeight = maxLines * (iconSize + vPadding) + vPadding + 2 * border;
            var bgWidth = 2 * border + 2 * vPadding + 5 * hPadding + 6 * hPaddingSmall + iconSize + maxWidths.Sum();

            var rectBg2 = hotbarSelectionPanelBackground2.GetComponent<RectTransform>();
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
            rectBg2.localPosition = new Vector3(0, 75 * theScale); // do not overlap the top-center panel

            var rectBg = hotbarSelectionPanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * border * theScale, rectBg2.sizeDelta.y - 2 * border * theScale);

            ResizeBox(hotbarSelectionPanelScrollUp, fontSize.Value * theScale);
            ResizeBox(hotbarSelectionPanelScrollDown, fontSize.Value * theScale);

            hotbarSelectionPanelScrollUp.GetComponent<RectTransform>().localPosition = new Vector2(0, rectBg2.sizeDelta.y / 2 - 2);
            hotbarSelectionPanelScrollDown.GetComponent<RectTransform>().localPosition = new Vector2(0, -rectBg2.sizeDelta.y / 2 + 2);

            float dy = rectBg.sizeDelta.y / 2 - vPadding;
            for (int i = hotbarSelectionPanelOffset; i < rows.Count && i < hotbarSelectionPanelOffset + maxLines; i++)
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

                SetLocalPosition(row.gSelect, dx + GetPreferredWidth(row.gSelect) / 2, y);

                // --- next row

                dy -= iconSize + vPadding;

                row.SetActive(true);

                if (i != hotbarSelectionPanelOffset)
                {
                    var building = row.codeName;

                    Action onRowSelected = () =>
                    {
                        hotbarPanelSlotAssignments[targetSubpanel, targetSlot] = building;
                        targetSubpanel = -1;
                        targetSlot = -1;
                        hotbarSelectionPanel.SetActive(false);
                        PersistHotbar();
                    };

                    CheckRowButton(rectBg2, mp, row.gSelect, onRowSelected);
                }
            }

            if (hotbarSelectionPanelHeaderRow.gName.activeSelf)
            {
                CheckMouseSort(hotbarSelectionPanelHeaderRow.gName, 0);
                CheckMouseSort(hotbarSelectionPanelHeaderRow.gInventory, 1);
            }

            if (Within(rectBg2, mp) && Input.GetKeyDown(KeyCode.Mouse1))
            {
                targetSubpanel = -1;
                targetSlot = -1;
                hotbarSelectionPanel.SetActive(false);
            }
        }

        internal class SelectionRow
        {
            internal string codeName;
            internal string name;
            internal CItem item;

            internal GameObject gIcon;
            internal GameObject gName;
            internal GameObject gInventory;
            internal GameObject gSelect;

            internal void SetActive(bool active)
            {
                gIcon.SetActive(active);
                gName.SetActive(active);
                gInventory.SetActive(active);
                gSelect.SetActive(active);
            }
        }

        static void CheckRowButton(RectTransform rectBg2, Vector2 mp, GameObject button, Action onPress)
        {
            var img = button.GetComponent<Image>();
            if (Within(rectBg2, button.GetComponent<RectTransform>(), mp))
            {
                img.color = DEFAULT_BOX_COLOR_HOVER;
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    onPress();
                }
            }
            else
            {
                img.color = DEFAULT_BOX_COLOR;
            }
        }
        static void CheckMouseSort(GameObject go, int col)
        {
            if (Within(hotbarSelectionPanelBackground2.GetComponent<RectTransform>(), go.GetComponent<RectTransform>(), GetMouseCanvasPos()))
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
        static void MaxOf(ref int max, int amount)
        {
            max = Mathf.Max(max, amount);
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem), nameof(CItem.Init))]
        static void CItem_Init(CItem __instance)
        {
            items[__instance.codeName] = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatHotbar.Switch.Tooltip", "Switch between subpanels" },
                { "FeatHotbar.Button.Empty", "Empty slot" },
                { "FeatHotbar.Button.Build", "Build {0}" },
                { "FeatHotbar.Button.Tooltip", "<i>[Left Mouse]</i> Build\n<i>[Right Mouse]</i> Select building type\n<i>[Middle Mouse]</i> Clear slot" },
                { "FeatHotbar.Select", " <b>Select</b> " },
                { "FeatHotbar.NoBuildings", "<b>No buildings available.</b>" },
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatHotbar.Switch.Tooltip", "Részpanelek közötti váltás" },
                { "FeatHotbar.Button.Empty", "Üres hely" },
                { "FeatHotbar.Button.Build", "{0} építése" },
                { "FeatHotbar.Button.Tooltip", "<i>[Bal Egérgomb]</i> Építés\n<i>[Jobb Egérgomb]</i> Épülettípus kiválasztása\n<i>[Középső egérgomb]</i> Eltávolítás" },
                { "FeatHotbar.Select", " <b>Kiválaszt</b> " },
                { "FeatHotbar.NoBuildings", "<b>Nincs elérhető épület</b>" },
            });
        }

        // *****************************************************************************************

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentDepot), nameof(CItem_ContentDepot.Build))]
        static void CItem_ContentDepot_Build()
        {
            logger.LogInfo("GScene3D.duplicateCoords = " + GScene3D.duplicatedCoords);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SScene3D), nameof(SScene3D.Select))]
        static void SScene3D_Select()
        {
            logger.LogInfo(Environment.StackTrace);
        }
        */
    }
}