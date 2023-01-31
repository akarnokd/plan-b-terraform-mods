using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatProductionStats
{
    [BepInPlugin("akarnokd.planbterraformmods.featproductionstats", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<KeyCode> toggleKey;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> itemSize;
        static ConfigEntry<int> maxStatLines;
        static ConfigEntry<int> buttonLeft;
        static ConfigEntry<int> buttonSize;
        static ConfigEntry<int> historyLength;

        static readonly int2 dicoCoordinates1 = new int2 { x = -1_000_100_100, y = 0 };
        static readonly int2 dicoCoordinates2 = new int2 { x = -1_000_100_100, y = 1 };

        static ManualLogSource logger;

        static Sprite icon;

        static Dictionary<string, Sprite> itemSprites = new();
        static Dictionary<string, Color> itemColors = new();

        static GameObject statsPanel;
        static GameObject statsPanelBackground;
        static GameObject statsPanelBackground2;
        static GameObject statsPanelScrollUp;
        static GameObject statsPanelScrollDown;

        static int statsPanelOffset;
        static int sortByColumn;
        static bool sortDesc;

        static GameObject statsButton;
        static GameObject statsButtonBackground;
        static GameObject statsButtonBackground2;
        static GameObject statsButtonIcon;

        static Dictionary<int, Dictionary<string, int>> productionSamples = new();
        static Dictionary<int, Dictionary<string, int>> consumptionSamples = new();

        static List<StatsRow> statsRowsCache = new();
        static StatsRow statsPanelHeaderRow;
        static GameObject statsPanelEmpty;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.F3, "Key to press while the building is selected to toggle its enabled/disabled state");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size in the panel");
            itemSize = Config.Bind("General", "ItemSize", 32, "The size of the item's icon in the list");
            buttonLeft = Config.Bind("General", "ButtonLeft", 100, "The button's position relative to the left of the screen");
            buttonSize = Config.Bind("General", "ButtonSize", 50, "The button's width and height");
            maxStatLines = Config.Bind("General", "MaxLines", 16, "How many lines of items to show");
            historyLength = Config.Bind("General", "HistoryLength", 300, "How many days to keep as past production data?");

            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            var iconPng = LoadPNG(Path.Combine(dir, "Icon.png"));
            icon = Sprite.Create(iconPng, new Rect(0, 0, iconPng.width, iconPng.height), new Vector2(0.5f, 0.5f));
            
            Harmony.CreateAndPatchAll(typeof(Plugin));
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
                if (statsButton != null)
                {
                    Destroy(statsButton);
                    statsButton = null;
                    statsButtonBackground = null;
                    statsButtonBackground2 = null;
                    statsButtonIcon = null;
                }
                if (statsPanel != null)
                {
                    Destroy(statsPanel);
                    statsPanel = null;
                    statsPanelBackground = null;
                    statsPanelBackground2 = null;
                    statsPanelHeaderRow = null;
                    statsPanelEmpty = null;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnDeactivate")]
        static void SSceneHud_OnDeactivate()
        {
            statsPanel?.SetActive(false);
            statsRowsCache.Clear();
        }

        static void UpdateButton()
        {
            if (statsButton == null)
            {
                statsButton = new GameObject("FeatProductionStatsButton");
                var canvas = statsButton.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                statsButtonBackground2 = new GameObject("FeatProductionStatsButton_BackgroundBorder");
                statsButtonBackground2.transform.SetParent(statsButton.transform);

                var img = statsButtonBackground2.AddComponent<Image>();
                img.color = DEFAULT_PANEL_BORDER_COLOR;

                statsButtonBackground = new GameObject("FeatProductionStatsButton_Background");
                statsButtonBackground.transform.SetParent(statsButtonBackground2.transform);

                img = statsButtonBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                statsButtonIcon = new GameObject("FeatProductionStatsButton_Icon");
                statsButtonIcon.transform.SetParent(statsButtonBackground.transform);

                img = statsButtonIcon.AddComponent<Image>();
                img.color = Color.white;
                img.sprite = icon;

                statsButtonBackground2.AddComponent<GraphicRaycaster>();
                var tt = statsButtonBackground2.AddComponent<CTooltipTarget>();
                tt.text = SLoc.Get("FeatProductionStats.Tooltip");
                tt.textDesc = SLoc.Get("FeatProductionStats.TooltipDetails", toggleKey.Value);
            }

            var padding = 5;

            var rectBg2 = statsButtonBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(buttonSize.Value + 4 * padding, buttonSize.Value + 4 * padding);
            rectBg2.localPosition = new Vector3(-Screen.width / 2 + buttonLeft.Value + rectBg2.sizeDelta.x / 2, Screen.height / 2 - rectBg2.sizeDelta.y / 2);

            var rectBg = statsButtonBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * padding, rectBg2.sizeDelta.y - 2 * padding);

            var rectIcn = statsButtonIcon.GetComponent<RectTransform>();
            rectIcn.sizeDelta = new Vector2(buttonSize.Value, buttonSize.Value);

            var mp = GetMouseCanvasPos();

            if (IsKeyDown(toggleKey.Value))
            {
                statsPanel.SetActive(!statsPanel.activeSelf);
            }
            if (Within(rectBg2, mp))
            {
                statsButtonBackground.GetComponent<Image>().color = Color.yellow;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    statsPanel.SetActive(!statsPanel.activeSelf);
                }
            }
            else
            {
                statsButtonBackground.GetComponent<Image>().color = DEFAULT_PANEL_COLOR;
            }
        }

        static void UpdatePanel()
        {
            int iconSize = itemSize.Value;

            if (statsPanel == null)
            {
                statsPanel = new GameObject("FeatProductionStatsPanel");
                var canvas = statsPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                statsPanelBackground2 = new GameObject("FeatProductionStatsPanel_BackgroundBorder");
                statsPanelBackground2.transform.SetParent(statsPanel.transform);

                var img = statsPanelBackground2.AddComponent<Image>();
                img.color = DEFAULT_PANEL_BORDER_COLOR;

                statsPanelBackground = new GameObject("FeatProductionStatsPanel_Background");
                statsPanelBackground.transform.SetParent(statsPanelBackground2.transform);

                img = statsPanelBackground.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                statsPanelScrollUp = CreateBox(statsPanelBackground2, "FeatProductionStatsPanel_ScrollUp", "\u25B2", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                statsPanelScrollDown = CreateBox(statsPanelBackground2, "FeatProductionStatsPanel_ScrollDown", "\u25BC", fontSize.Value, DEFAULT_BOX_COLOR, Color.white);

                statsPanel.SetActive(false);

                statsPanelHeaderRow = new StatsRow();
                statsPanelHeaderRow.gIcon = new GameObject("FeatProductionStatsPanel_HeaderRow_Icon");
                statsPanelHeaderRow.gIcon.transform.SetParent(statsPanelBackground.transform);
                statsPanelHeaderRow.gIcon.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                statsPanelHeaderRow.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                statsPanelHeaderRow.gName = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Name", "", fontSize.Value, Color.black);
                statsPanelHeaderRow.gProduction = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Production", "", fontSize.Value, Color.black);
                statsPanelHeaderRow.gConsumption = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Consumption", "", fontSize.Value, Color.black);
                statsPanelHeaderRow.gRatio = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Ratio", "", fontSize.Value, Color.black);

                statsPanelEmpty = CreateText(statsPanelBackground, "FeatProductionStatsPanel_NoRows", "<b>No statistics available</b>", fontSize.Value, Color.black);
            }

            if (!statsPanel.activeSelf)
            {
                return;
            }

            Dictionary<string, StatsRow> statsRowsDict = new();

            int today = (int)GMain.simuPlanetTime;
            int horizonMax = 30;
            int horizon = 1;

            for (int t = today - horizonMax + 1; t <= today; t++)
            {
                if (productionSamples.ContainsKey(t) || consumptionSamples.ContainsKey(t))
                {
                    horizon = today - t + 1;
                    break;
                }
            }

            int lookback = today - horizon + 1; // TODO support larger horizons

            for (int t = lookback; t <= today; t++)
            {
                if (productionSamples.TryGetValue(t, out var daily))
                {
                    foreach (var kv in daily)
                    {
                        if (!statsRowsDict.TryGetValue(kv.Key, out var sr))
                        {
                            sr = new();
                            sr.codeName = kv.Key;
                            sr.name = SLoc.Get("ITEM_NAME_" + kv.Key);
                            itemSprites.TryGetValue(kv.Key, out sr.icon);
                            itemColors.TryGetValue(kv.Key, out sr.color);
                            statsRowsDict[kv.Key] = sr;
                        }

                        sr.sumProduction += kv.Value;
                    }
                }
                if (consumptionSamples.TryGetValue(t, out daily))
                {
                    foreach (var kv in daily)
                    {
                        if (!statsRowsDict.TryGetValue(kv.Key, out var sr))
                        {
                            sr = new();
                            sr.codeName = kv.Key;
                            sr.name = SLoc.Get("ITEM_NAME_" + kv.Key);
                            itemSprites.TryGetValue(kv.Key, out sr.icon);
                            itemColors.TryGetValue(kv.Key, out sr.color);
                            statsRowsDict[kv.Key] = sr;
                        }

                        sr.sumConsumption += kv.Value;
                    }
                }
            }

            List<StatsRow> allRows = new(statsRowsDict.Values);
            Comparison<StatsRow> comp = null;

            if (sortByColumn == 0)
            {
                comp = (a, b) => a.name.CompareTo(b.name);
            }
            if (sortByColumn == 1)
            {
                comp = (a, b) => a.sumProduction.CompareTo(b.sumProduction);
            }
            if (sortByColumn == 2)
            {
                comp = (a, b) => a.sumConsumption.CompareTo(b.sumConsumption);
            }
            if (sortByColumn == 3)
            {
                comp = (a, b) => 
                { 
                    if (a.sumConsumption == 0 && b.sumConsumption != 0)
                    {
                        return -1;
                    }
                    if (a.sumConsumption != 0 && b.sumConsumption == 0)
                    {
                        return 1;
                    }
                    if (a.sumConsumption == b.sumConsumption)
                    {
                        return 0;
                    }
                    var f1 = a.sumProduction / (float)a.sumConsumption;
                    var f2 = b.sumProduction / (float)b.sumConsumption;
                    return f1.CompareTo(f2);
                };
            }


            if (comp != null)
            {
                if (sortDesc)
                {
                    var oldComp = comp;
                    comp = (a, b) => oldComp(b, a);
                }

                allRows.Sort(comp);
            }

            var mp = GetMouseCanvasPos();
            if (Within(statsPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > 0)
                {
                    statsPanelOffset = Math.Max(0, statsPanelOffset - 1);
                }
                else
                if (scrollDelta < 0)
                {
                    statsPanelOffset = statsPanelOffset + 1;
                }
            }
            var maxLines = maxStatLines.Value;
            if (statsPanelOffset + maxLines > allRows.Count)
            {
                statsPanelOffset = Math.Max(0, allRows.Count - maxLines);
            }

            statsPanelScrollUp.SetActive(statsPanelOffset > 0);
            statsPanelScrollDown.SetActive(statsPanelOffset + maxLines < allRows.Count);

            int maxNameWidth = 0;
            int maxProductionWidth = 0;
            int maxConsumptionWidth = 0;
            int maxRatioWidth = 0;
            int vPadding = 10;
            int hPadding = 30;
            int border = 5;

            while (statsRowsCache.Count < allRows.Count)
            {
                int i = statsRowsCache.Count;

                var row = new StatsRow();
                statsRowsCache.Add(row);

                row.gIcon = new GameObject("FeatProductionStatsPanel_Row_" + i + "_Icon");
                row.gIcon.transform.SetParent(statsPanelBackground.transform);
                var img = row.gIcon.AddComponent<Image>();
                img.sprite = row.icon;
                img.color = row.color;
                row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                row.gName = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Name", "", fontSize.Value, Color.black);
                row.gProduction = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Production",
                        "", fontSize.Value, Color.black);
                row.gConsumption = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Consumption", "", fontSize.Value, Color.black);

                row.gRatio = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Ratio","", fontSize.Value, Color.black);
            }

            foreach (var sr in statsRowsCache)
            {
                sr.SetActive(false);
            }

            for (int i = 0; i < allRows.Count; i++)
            {
                var row = allRows[i];
                row.Use(statsRowsCache[i]);

                var img = row.gIcon.GetComponent<Image>();
                img.sprite = row.icon;
                img.color = row.color;
                row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                row.gName.GetComponent<Text>().text = "<b>" + row.name + "</b>";
                row.gProduction.GetComponent<Text>().text = 
                    string.Format("<b>{0:#,##0.000} / day</b>", row.sumProduction / (float)horizon);
                row.gConsumption.GetComponent<Text>().text = 
                    string.Format("<b>{0:#,##0.000} / day</b>", row.sumConsumption / (float)horizon);

                row.gRatio.GetComponent<Text>().text = 
                    row.sumConsumption > 0 ? (string.Format("<b>{0:#,##0.000}</b>", row.sumProduction / (float)row.sumConsumption)) : "N/A";

                maxNameWidth = Math.Max(maxNameWidth, GetPreferredWidth(row.gName));
                maxProductionWidth = Math.Max(maxProductionWidth, GetPreferredWidth(row.gProduction));
                maxConsumptionWidth = Math.Max(maxConsumptionWidth, GetPreferredWidth(row.gConsumption));
                maxRatioWidth = Math.Max(maxRatioWidth, GetPreferredWidth(row.gRatio));

                row.SetActive(false);
            }

            if (allRows.Count == 0)
            {
                maxNameWidth = GetPreferredWidth(statsPanelEmpty);
                SetLocalPosition(statsPanelEmpty, 0, 0);
                statsPanelEmpty.SetActive(true);
                statsPanelHeaderRow.SetActive(false);
            }
            else
            {
                allRows.Insert(statsPanelOffset, statsPanelHeaderRow);
                statsPanelEmpty.SetActive(false);
                statsPanelHeaderRow.SetActive(true);

                statsPanelHeaderRow.gName.GetComponent<Text>().text = "<i>Item</i>" + GetSortIndicator(0);
                statsPanelHeaderRow.gProduction.GetComponent<Text>().text = "<i>Production speed</i>" + GetSortIndicator(1);
                statsPanelHeaderRow.gConsumption.GetComponent<Text>().text = "<i>Consumption speed</i>" + GetSortIndicator(2);
                statsPanelHeaderRow.gRatio.GetComponent<Text>().text = "<i>Ratio</i>" + GetSortIndicator(3);

                ApplyPreferredSize(statsPanelHeaderRow.gName);
                ApplyPreferredSize(statsPanelHeaderRow.gProduction);
                ApplyPreferredSize(statsPanelHeaderRow.gConsumption);
                ApplyPreferredSize(statsPanelHeaderRow.gRatio);

                maxNameWidth = Math.Max(maxNameWidth, GetPreferredWidth(statsPanelHeaderRow.gName));
                maxProductionWidth = Math.Max(maxProductionWidth, GetPreferredWidth(statsPanelHeaderRow.gProduction));
                maxConsumptionWidth = Math.Max(maxConsumptionWidth, GetPreferredWidth(statsPanelHeaderRow.gConsumption));
                maxRatioWidth = Math.Max(maxRatioWidth, GetPreferredWidth(statsPanelHeaderRow.gRatio));

                maxLines++; // header
            }


            int bgHeight = maxLines * (iconSize + vPadding) + vPadding + 2 * border;
            int bgWidth = 2 * border + 2 * vPadding + 4 * hPadding + iconSize + maxNameWidth + maxProductionWidth + maxConsumptionWidth + maxRatioWidth;

            var rectBg2 = statsPanelBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(Mathf.Max(bgWidth, rectBg2.sizeDelta.x), bgHeight);
            rectBg2.localPosition = new Vector3(0, 0);

            var rectBg = statsPanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * border, rectBg2.sizeDelta.y - 2 * border);

            statsPanelScrollUp.GetComponent<RectTransform>().localPosition = new Vector2(0, rectBg2.sizeDelta.y / 2 - 2);
            statsPanelScrollDown.GetComponent<RectTransform>().localPosition = new Vector2(0, -rectBg2.sizeDelta.y / 2 + 2);

            float dy = rectBg.sizeDelta.y / 2 - vPadding;
            for (int i = statsPanelOffset; i < allRows.Count && i < statsPanelOffset + maxLines; i++)
            {
                var row = allRows[i];

                float y = dy - iconSize / 2;

                float dx = - rectBg.sizeDelta.x / 2 + vPadding;

                SetLocalPosition(row.gIcon, dx + iconSize / 2, y);

                dx += iconSize + hPadding;

                SetLocalPosition(row.gName, dx + GetPreferredWidth(row.gName) / 2, y);

                dx += maxNameWidth + hPadding;

                SetLocalPosition(row.gProduction, dx + maxProductionWidth - GetPreferredWidth(row.gProduction) / 2, y);

                dx += maxProductionWidth + hPadding;

                SetLocalPosition(row.gConsumption, dx + maxConsumptionWidth - GetPreferredWidth(row.gConsumption) / 2, y);

                dx += maxConsumptionWidth + hPadding;

                SetLocalPosition(row.gRatio, dx + maxRatioWidth - GetPreferredWidth(row.gRatio) / 2, y);

                dy -= iconSize + vPadding;

                row.SetActive(true);
            }

            if (statsPanelHeaderRow.gName.activeSelf) {

                CheckMouseSort(statsPanelHeaderRow.gName, 0);
                CheckMouseSort(statsPanelHeaderRow.gProduction, 1);
                CheckMouseSort(statsPanelHeaderRow.gConsumption, 2);
                CheckMouseSort(statsPanelHeaderRow.gRatio, 3);
            }
        }

        static void CheckMouseSort(GameObject go, int col)
        {
            if (Within(statsPanelBackground2.GetComponent<RectTransform>(), go.GetComponent<RectTransform>(), GetMouseCanvasPos()))
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
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            try
            {
                RestoreState();
            } 
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Save))]
        static void SGame_Save()
        {
            SaveState();
        }

        static void SaveState()
        {
            StringBuilder sb = new(512);
            AppendData(productionSamples, sb);
            GGame.dicoLandmarks[dicoCoordinates1] = sb.ToString();
            
            sb.Clear();
            AppendData(consumptionSamples, sb);
            GGame.dicoLandmarks[dicoCoordinates2] = sb.ToString();
        }

        static void AppendData(Dictionary<int, Dictionary<string, int>> data, StringBuilder sb)
        {
            int i = 0;
            foreach (var kv in data)
            {
                if (i++ > 0)
                {
                    sb.Append(';');
                }
                sb.Append(kv.Key).Append('=');
                int j = 0;
                foreach (var ev in kv.Value)
                {
                    if (j++ > 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(ev.Key).Append(':').Append(ev.Value);
                }
            }
        }

        static void ParseData(string str, Dictionary<int, Dictionary<string, int>> data)
        {
            if (str.Length != 0)
            {
                foreach (var days in str.Split(';'))
                {
                    var dayAndValues = days.Split('=');

                    int t = int.Parse(dayAndValues[0]);
                    var dict = new Dictionary<string, int>();
                    data[t] = dict;

                    foreach (var values in dayAndValues[1].Split(','))
                    {
                        var kv = values.Split(':');
                        dict[kv[0]] = int.Parse(kv[1]);
                    }
                }
            }
        }

        static void RestoreState()
        {
            // logger.LogInfo("RestoreState()");
            productionSamples.Clear();
            if (GGame.dicoLandmarks.TryGetValue(dicoCoordinates1, out var str))
            {
                // logger.LogInfo("  productionSamples\r\n  " + str);
                ParseData(str, productionSamples);
            }
            consumptionSamples.Clear();
            if (GGame.dicoLandmarks.TryGetValue(dicoCoordinates2, out str))
            {
                // logger.LogInfo("  consumptionSamples\r\n  " + str);
                ParseData(str, consumptionSamples);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem), nameof(CItem.Init))]
        static void CItem_Init(CItem __instance)
        {
            itemSprites[__instance.codeName] = __instance.icon.Sprite;
            itemColors[__instance.codeName] = __instance.colorItem;
        }

        static bool insideFactoryUpdate;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static void CItem_ContentFactory_Update01s_Pre(CItem_ContentFactory __instance,
            int2 coords,
            out List<CurrentStack> __state)
        {
            // logger.LogInfo("Factory Update Pre");
            insideFactoryUpdate = true;
            var recipe = __instance.GetRecipe(coords);
            if (recipe != null && !__instance.producesOutputContainers) {
                __state = new();

                foreach (var rec in recipe.outputs)
                {
                    __state.Add(new CurrentStack { item = rec.item, amount = rec.item.nbOwned });
                    // logger.LogInfo("  Factory Global Pre " + rec.item.codeName + " " + rec.item.nbOwned);
                }
            }
            else
            {
                __state = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static void CItem_ContentFactory_Update01s_Post(
            List<CurrentStack> __state)
        {
            insideFactoryUpdate = false;
            if (__state != null)
            {
                foreach (var cs in __state)
                {
                    var diff = cs.item.nbOwned - cs.amount;
                    if (diff != 0)
                    {
                        // logger.LogInfo("  Factory Global Post " + cs.item.codeName + " " + cs.item.nbOwned);
                        AddForToday(cs.item.codeName, 1, productionSamples);
                    }
                }
            }
            //logger.LogInfo("Factory Update Post");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.ProcessCityRecipeIFP))]
        static void CItem_ContentCityInOut_ProcessCityRecipeIFP_Pre()
        {
            // logger.LogInfo("City Update Pre");
            insideFactoryUpdate = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentCityInOut), nameof(CItem_ContentCityInOut.ProcessCityRecipeIFP))]
        static void CItem_ContentCityInOut_ProcessCityRecipeIFP_Post()
        {
            insideFactoryUpdate = false;
            //logger.LogInfo("City Update Post");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CStack), nameof(CStack.AddIFP))]
        static void CStack_AddIFP(ref CStack __instance, int count)
        {
            // logger.LogInfo("  Produced: " + __instance.item.codeName + " " + insideFactoryUpdate + " " + count);
            // logger.LogInfo(Environment.StackTrace);
            if (insideFactoryUpdate)
            {
                if (count > 0)
                {
                    AddForToday(__instance.item.codeName, 1, productionSamples);
                }
                else
                {
                    AddForToday(__instance.item.codeName, 1, consumptionSamples);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CStack), nameof(CStack.Add))]
        static void CStack_Add(ref CStack __instance, int count)
        {
            // logger.LogInfo("  Consumed: " + __instance.item.codeName + " " + insideFactoryUpdate + " " + count);
            if (insideFactoryUpdate && count < 0)
            {
                AddForToday(__instance.item.codeName, 1, consumptionSamples);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CItem_ContentExtractor_Update01s_Pre(
            CItem_ContentFactory __instance,
            int2 coords,
            out CurrentStack __state)
        {
            var stack = __instance.GetStack(coords, 0);

            __state = new CurrentStack { item = stack.item, amount = stack.nb };
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CItem_ContentExtractor_Update01s_Post(
            CItem_ContentFactory __instance,
            int2 coords,
            CurrentStack __state)
        {
            var stack = __instance.GetStack(coords);
            var diff = stack.nb - __state.amount;
            if (diff != 0)
            {
                if (__state.item != null)
                {
                    AddForToday(__state.item.codeName, diff, productionSamples);
                }
            }
        }

        static void AddForToday(string codeName, int amount, Dictionary<int, Dictionary<string, int>> data)
        {
            int today = (int)GMain.simuPlanetTime;
            if (!data.TryGetValue(today, out var daily))
            {
                daily = new();
                data[today] = daily;
                CleanupOldestDays(data, today);
            }
            daily.TryGetValue(codeName, out var n);
            daily[codeName] = n + amount;
            //logger.LogInfo("Item " + codeName + " = " + (n + amount) + " @ " + today);
        }

        static void CleanupOldestDays(Dictionary<int, Dictionary<string, int>> data, int today)
        {
            var limit = today - historyLength.Value;
            foreach (var k in new List<int>(data.Keys))
            {
                if (k <= limit)
                {
                    data.Remove(k);
                }
            }
        }

        internal class CurrentStack
        {
            internal CItem item;
            internal int amount;
        }

        internal class StatsRow
        {
            internal string codeName;
            internal string name;
            internal Sprite icon;
            internal Color color;
            internal int sumProduction;
            internal int sumConsumption;

            internal GameObject gIcon;
            internal GameObject gName;
            internal GameObject gProduction;
            internal GameObject gConsumption;
            internal GameObject gRatio;

            internal void SetActive(bool active)
            {
                gIcon.SetActive(active);
                gName.SetActive(active);
                gProduction.SetActive(active);
                gConsumption.SetActive(active);
                gRatio.SetActive(active);
            }

            internal void Use(StatsRow other)
            {
                gIcon = other.gIcon;
                gName = other.gName;
                gProduction = other.gProduction;
                gConsumption = other.gConsumption;
                gRatio = other.gRatio;
            }
        }

        // Prevent click-through the panel
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SMouse), nameof(SMouse.IsCursorOnGround))]
        static void SMouse_IsCursorOnGround(ref bool __result)
        {
            var mp = GetMouseCanvasPos();
            if (statsPanel != null && statsPanel.activeSelf
                && Within(statsPanelBackground2.GetComponent<RectTransform>(), mp))
            {
                __result = false;
            }
            if (statsButton != null && statsButton.activeSelf
                && Within(statsButtonBackground2.GetComponent<RectTransform>(), mp))
            {
                __result = false;
            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneTooltip), "OnUpdate")]
        static void SSceneTooltip_OnUpdate()
        {
            var hits = SSingleton<SScenesManager>.Inst.GetMouseRaycastResults();
            logger.LogInfo("SSceneTooltip: " + hits.Count);
            foreach (var hit in hits)
            {
                logger.LogInfo("  " + hit.gameObject.name);
            }
        }
        */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatProductionStats.Tooltip", "Toggle Statistics" },
                { "FeatProductionStats.TooltipDetails", "Toggle the Production and Consumption Statistics panel.\nHotkey: {0}.\n\n<i>FeatProductionStats mod</i>" }
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatProductionStats.Tooltip", "Statisztikák mutatása" },
                { "FeatProductionStats.TooltipDetails", "A gyártási és fogyasztási statisztikák képernyő megjelenítése vagy elrejtése.\nGyorsbillentyű: {0}.\n\n<i>FeatProductionStats mod</i>" }
            });
        }
    }
}
