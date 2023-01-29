using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FeatProductionStats
{
    [BepInPlugin("akarnokd.planbterraformmods.featproductionstats", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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

        static Color defaultPanelLightColor = new Color(231f / 255, 227f / 255, 243f / 255, 1f);

        static Dictionary<string, Sprite> itemSprites = new();
        static Dictionary<string, Color> itemColors = new();

        static GameObject statsPanel;
        static GameObject statsPanelBackground;
        static GameObject statsPanelBackground2;
        static GameObject statsPanelScrollUp;
        static GameObject statsPanelScrollDown;

        static int statsPanelOffset;

        static GameObject statsButton;
        static GameObject statsButtonBackground;
        static GameObject statsButtonBackground2;
        static GameObject statsButtonIcon;

        static Dictionary<int, Dictionary<string, int>> productionSamples = new();
        static Dictionary<int, Dictionary<string, int>> consumptionSamples = new();

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
                }
            }
        }

        static void UpdateButton()
        {
            if (statsButton == null)
            {
                statsButton = new GameObject("FeatProductionStatsButton");
                var canvas = statsButton.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                statsButtonBackground2 = new GameObject("FeatProductionStatsButton_BackgroundBorder");
                statsButtonBackground2.transform.SetParent(statsButton.transform);

                var img = statsButtonBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                statsButtonBackground = new GameObject("FeatProductionStatsButton_Background");
                statsButtonBackground.transform.SetParent(statsButtonBackground2.transform);

                img = statsButtonBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                statsButtonIcon = new GameObject("FeatProductionStatsButton_Icon");
                statsButtonIcon.transform.SetParent(statsButtonBackground.transform);

                img = statsButtonIcon.AddComponent<Image>();
                img.color = Color.white;
                img.sprite = icon;
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
                statsButtonBackground.GetComponent<Image>().color = defaultPanelLightColor;
            }
        }

        static void UpdatePanel()
        {
            if (statsPanel == null)
            {
                statsPanel = new GameObject("FeatProductionStatsPanel");
                var canvas = statsPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                statsPanelBackground2 = new GameObject("FeatProductionStatsPanel_BackgroundBorder");
                statsPanelBackground2.transform.SetParent(statsPanel.transform);

                var img = statsPanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                statsPanelBackground = new GameObject("FeatProductionStatsPanel_Background");
                statsPanelBackground.transform.SetParent(statsPanelBackground2.transform);

                img = statsPanelBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                statsPanelScrollUp = CreateBox(statsPanelBackground2, "FeatProductionStatsPanel_ScrollUp", "\u25B2");

                statsPanelScrollDown = CreateBox(statsPanelBackground2, "FeatProductionStatsPanel_ScrollDown", "\u25BC");

                statsPanel.SetActive(false);
            }

            for (int i = statsPanelBackground.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(statsPanelBackground.transform.GetChild(i).gameObject);
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
            allRows.Sort((a, b) => a.name.CompareTo(b.name));

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
            int vPadding = 10;
            int hPadding = 30;
            int border = 5;
            int iconSize = itemSize.Value;

            for (int i = statsPanelOffset; i < allRows.Count && i < statsPanelOffset + maxLines; i++)
            {
                var row = allRows[i];

                row.gIcon = new GameObject("FeatProductionStatsPanel_Row_" + i + "_Icon");
                row.gIcon.transform.SetParent(statsPanelBackground.transform);
                var img = row.gIcon.AddComponent<Image>();
                img.sprite = row.icon;
                img.color = row.color;
                row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                row.gName = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Name", "<b>" + row.name + "</b>");
                row.gProduction = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Production",
                        string.Format("<b>{0:#,##0.000} / day</b>", row.sumProduction / (float)horizon));
                row.gConsumption = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Consumption",
                        string.Format("<b>{0:#,##0.000} / day</b>", row.sumConsumption / (float)horizon));

                maxNameWidth = Math.Max(maxNameWidth, GetPreferredWidth(row.gName));
                maxProductionWidth = Math.Max(maxProductionWidth, GetPreferredWidth(row.gProduction));
                maxConsumptionWidth = Math.Max(maxConsumptionWidth, GetPreferredWidth(row.gConsumption));
            }

            if (allRows.Count == 0)
            {
                var empty = CreateText(statsPanelBackground, "FeatProductionStatsPanel_NoRows", "<b>No statistics available</b>");
                maxNameWidth = GetPreferredWidth(empty);
                SetLocalPosition(empty, 0, 0);
            }
            else
            {

                var headerRow = new StatsRow();
                headerRow.gIcon = new GameObject("FeatProductionStatsPanel_HeaderRow_Icon");
                headerRow.gIcon.transform.SetParent(statsPanelBackground.transform);
                headerRow.gIcon.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                headerRow.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                headerRow.gName = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Name", "<i>Item</i>");
                headerRow.gProduction = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Production", "<i>Production speed</i>");
                headerRow.gConsumption = CreateText(statsPanelBackground, "FeatProductionStatsPanel_HeaderRow_Consumption", "<i>Consumption speed</i>");

                maxNameWidth = Math.Max(maxNameWidth, GetPreferredWidth(headerRow.gName));
                maxProductionWidth = Math.Max(maxProductionWidth, GetPreferredWidth(headerRow.gProduction));
                maxConsumptionWidth = Math.Max(maxConsumptionWidth, GetPreferredWidth(headerRow.gConsumption));

                allRows.Insert(statsPanelOffset, headerRow);

                maxLines++; // header
            }

            int bgHeight = maxLines * (iconSize + vPadding) + vPadding + 2 * border;
            int bgWidth = 2 * border + 2 * vPadding + 3 * hPadding + iconSize + maxNameWidth + maxProductionWidth + maxConsumptionWidth;

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

                dy -= iconSize + vPadding;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            RestoreState();
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
        [HarmonyPatch(typeof(CStack), nameof(CStack.AddIFP))]
        static void CStack_AddIFP(ref CStack __instance, int count)
        {
            // logger.LogInfo("  Produced: " + __instance.item.codeName + " " + insideFactoryUpdate + " " + count);
            // logger.LogInfo(Environment.StackTrace);
            if (insideFactoryUpdate)
            {
                AddForToday(__instance.item.codeName, 1, productionSamples);
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
        }


        static bool IsKeyDown(KeyCode keyCode)
        {
            GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            return (currentSelectedGameObject == null || !currentSelectedGameObject.TryGetComponent<InputField>(out _))
                && Input.GetKeyDown(keyCode);
        }

        static Vector2 GetMouseCanvasPos()
        {
            var mousePos = Input.mousePosition;
            return new Vector2(-Screen.width / 2 + mousePos.x, -Screen.height / 2 + mousePos.y);
        }

        static bool Within(RectTransform rt, Vector2 vec)
        {
            var x = rt.localPosition.x - rt.sizeDelta.x / 2;
            var y = rt.localPosition.y - rt.sizeDelta.y / 2;
            var x2 = x + rt.sizeDelta.x;
            var y2 = y + rt.sizeDelta.y;
            return x <= vec.x && vec.x <= x2 && y <= vec.y && vec.y <= y2;
        }

        static Texture2D LoadPNG(string filename)
        {
            Texture2D tex = new Texture2D(100, 200);
            tex.LoadImage(File.ReadAllBytes(filename));

            return tex;
        }

        static GameObject CreateBox(GameObject parent, string name, string text)
        {
            var box = new GameObject(name);
            box.transform.SetParent(parent.transform);
            var img = box.AddComponent<Image>();
            img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

            var textGo = new GameObject(name + "_Text");
            textGo.transform.SetParent(box.transform);

            var txt = textGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = fontSize.Value;
            txt.color = Color.white;
            txt.resizeTextForBestFit = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = text;

            var rect = textGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);

            var rectbox = box.GetComponent<RectTransform>();
            rectbox.sizeDelta = new Vector2(rect.sizeDelta.x + 4, rect.sizeDelta.y + 4);

            return box;
        }

        static int GetPreferredWidth(GameObject go)
        {
            return Mathf.CeilToInt(go.GetComponent<Text>().preferredWidth);
        }

        static void SetLocalPosition(GameObject go, float x, float y)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.localPosition = new Vector2(x, y);
        }

        static GameObject CreateText(GameObject parent, string name, string text)
        {
            var textGo = new GameObject(name + "_Text");
            textGo.transform.SetParent(parent.transform);

            var txt = textGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = fontSize.Value;
            txt.color = Color.black;
            txt.resizeTextForBestFit = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = text;

            var rect = textGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(txt.preferredWidth, txt.preferredHeight);

            return textGo;
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
    }
}
