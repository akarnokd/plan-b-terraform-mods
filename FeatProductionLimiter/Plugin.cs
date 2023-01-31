using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FeatProductionLimiter
{
    [BepInPlugin("akarnokd.planbterraformmods.featproductionlimiter", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

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
        static Dictionary<string, ConfigEntry<int>> limits = new();

        static ConfigEntry<bool> showAll;
        static ConfigEntry<KeyCode> toggleKey;
        static ConfigEntry<int> fontSize;
        static ConfigEntry<int> itemSize;
        static ConfigEntry<int> maxStatLines;
        static ConfigEntry<int> buttonLeft;
        static ConfigEntry<int> buttonSize;

        static ManualLogSource logger;

        static Sprite icon;

        static Color defaultPanelLightColor = new Color(231f / 255, 227f / 255, 243f / 255, 1f);
        static Color defaultBoxColor = new Color(121f / 255, 125f / 255, 245f / 255, 1f);
        static Color defaultBoxColorLight = new Color(161f / 255, 165f / 255, 245f / 255, 1f);

        static Dictionary<string, CItem> items = new();

        static GameObject limiterPanel;
        static GameObject limiterPanelBackground;
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

        static List<LimiterRow> limiterRowsCache = new();
        static LimiterRow statsPanelHeaderRow;
        static GameObject statsPanelEmpty;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");
            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            showAll = Config.Bind("General", "ShowAll", false, "Always show all products?");

            foreach (var ids in globalProducts)
            {
                limits.Add(ids, Config.Bind("General", ids, 500, "Limit the production of " + ids));
            }

            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.F4, "Key to toggle the limiter panel");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size in the panel");
            itemSize = Config.Bind("General", "ItemSize", 32, "The size of the item's icon in the list");
            buttonLeft = Config.Bind("General", "ButtonLeft", 175, "The button's position relative to the left of the screen");
            buttonSize = Config.Bind("General", "ButtonSize", 50, "The button's width and height");
            maxStatLines = Config.Bind("General", "MaxLines", 16, "How many lines of items to show");

            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            var iconPng = LoadPNG(Path.Combine(dir, "Icon.png"));
            icon = Sprite.Create(iconPng, new Rect(0, 0, iconPng.width, iconPng.height), new Vector2(0.5f, 0.5f));

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem_ContentFactory), "CheckStocks2")]
        static void CItem_ContentFactory_CheckStocks2(CRecipe recipe, ref bool __result)
        {
            if (modEnabled.Value)
            {
                foreach (var outp in recipe.outputs)
                {
                    if (limits.TryGetValue(outp.item.codeName, out var entry))
                    {
                        if (outp.item.nbOwned >= entry.Value)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
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
                if (limiterPanel != null)
                {
                    Destroy(limiterPanel);
                    limiterPanel = null;
                    limiterPanelBackground = null;
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
            Destroy(limiterPanel);
            limiterRowsCache.Clear();
        }

        static void UpdateButton()
        {
            if (statsButton == null)
            {
                statsButton = new GameObject("FeatProductionLimiterButton");
                var canvas = statsButton.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 52;

                statsButtonBackground2 = new GameObject("FeatProductionLimiterButton_BackgroundBorder");
                statsButtonBackground2.transform.SetParent(statsButton.transform);

                var img = statsButtonBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                statsButtonBackground = new GameObject("FeatProductionLimiterButton_Background");
                statsButtonBackground.transform.SetParent(statsButtonBackground2.transform);

                img = statsButtonBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                statsButtonIcon = new GameObject("FeatProductionLimiterButton_Icon");
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
                limiterPanel.SetActive(!limiterPanel.activeSelf);
            }
            if (Within(rectBg2, mp))
            {
                statsButtonBackground.GetComponent<Image>().color = Color.yellow;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    limiterPanel.SetActive(!limiterPanel.activeSelf);
                }
            }
            else
            {
                statsButtonBackground.GetComponent<Image>().color = defaultPanelLightColor;
            }
        }

        static void UpdatePanel()
        {
            int iconSize = itemSize.Value;

            if (limiterPanel == null)
            {
                limiterPanel = new GameObject("FeatProductionLimiterPanel");
                var canvas = limiterPanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                statsPanelBackground2 = new GameObject("FeatProductionLimiterPanel_BackgroundBorder");
                statsPanelBackground2.transform.SetParent(limiterPanel.transform);

                var img = statsPanelBackground2.AddComponent<Image>();
                img.color = new Color(121f / 255, 125f / 255, 245f / 255, 1f);

                limiterPanelBackground = new GameObject("FeatProductionLimiterPanel_Background");
                limiterPanelBackground.transform.SetParent(statsPanelBackground2.transform);

                img = limiterPanelBackground.AddComponent<Image>();
                img.color = defaultPanelLightColor;

                statsPanelScrollUp = CreateBox(statsPanelBackground2, "FeatProductionLimiterPanel_ScrollUp", "\u25B2");

                statsPanelScrollDown = CreateBox(statsPanelBackground2, "FeatProductionLimiterPanel_ScrollDown", "\u25BC");

                limiterPanel.SetActive(false);

                statsPanelHeaderRow = new LimiterRow();
                statsPanelHeaderRow.gIcon = new GameObject("FeatProductionLimiterPanel_HeaderRow_Icon");
                statsPanelHeaderRow.gIcon.transform.SetParent(limiterPanelBackground.transform);
                statsPanelHeaderRow.gIcon.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                statsPanelHeaderRow.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                statsPanelHeaderRow.gName = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Name", "");
                statsPanelHeaderRow.gZero = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Zero", "");
                statsPanelHeaderRow.gMinus100 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus100", "");
                statsPanelHeaderRow.gMinus10 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus10", "");
                statsPanelHeaderRow.gMinus1 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Minus1", "");
                statsPanelHeaderRow.gAmount = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Amount", "");
                statsPanelHeaderRow.gPlus1 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus1", "");
                statsPanelHeaderRow.gPlus10 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus10", "");
                statsPanelHeaderRow.gPlus100 = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Plus100", "");
                statsPanelHeaderRow.gUnlimited = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_HeaderRow_Unlimited", "");

                statsPanelEmpty = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_NoRows", "<b>No products available</b>");

                limiterRowsCache.Clear();
                int i = 0;
                foreach (var codeName in globalProducts)
                {
                    var row = new LimiterRow();

                    items.TryGetValue(codeName, out row.item);
                    row.codeName = codeName;
                    row.name = SLoc.Get("ITEM_NAME_" + codeName);
                    limits.TryGetValue(codeName, out row.limitConfig);
                    limiterRowsCache.Add(row);

                    row.gIcon = new GameObject("FeatProductionLimiterPanel_Row_" + i + "_Icon");
                    row.gIcon.transform.SetParent(limiterPanelBackground.transform);
                    img = row.gIcon.AddComponent<Image>();
                    img.sprite = row.item.icon.Sprite;
                    img.color = row.item.colorItem;
                    row.gIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);

                    row.gName = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Name", "<b>" + row.name + "</b>");

                    row.gZero = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Zero", "<b> Zero </b>");

                    row.gMinus100 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus100", "<b> -100 </b>");
                    row.gMinus10 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus10", "<b> -10 </b>");
                    row.gMinus1 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Minus1", "<b> -1 </b>");

                    row.gAmount = CreateText(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Amount", "");

                    row.gPlus1 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus1", "<b> +1 </b>");
                    row.gPlus10 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus10", "<b> +10 </b>");
                    row.gPlus100 = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Plus100", "<b> +100 </b>");

                    row.gUnlimited = CreateBox(limiterPanelBackground, "FeatProductionLimiterPanel_Row_" + i + "_Unlimited", "<b> ∞ </b>");

                    i++;
                }
            }

            if (!limiterPanel.activeSelf)
            {
                return;
            }

            int[] maxWidths = new int[] { 0, 0, 0, 0, 0, 100, 0, 0, 0, 0 };

            foreach (var sr in limiterRowsCache)
            {
                if (sr.limitConfig != null)
                {
                    sr.currentLimit = sr.limitConfig.Value;
                    sr.gAmount.GetComponent<Text>().text = "<b>" + sr.currentLimit + "</b>";
                }
                else
                {
                    sr.gAmount.GetComponent<Text>().text = "N/A";
                }

                MaxOf(ref maxWidths[0], GetPreferredWidth(sr.gName));
                MaxOf(ref maxWidths[1], GetPreferredWidth(sr.gZero));
                MaxOf(ref maxWidths[2], GetPreferredWidth(sr.gMinus100));
                MaxOf(ref maxWidths[3], GetPreferredWidth(sr.gMinus10));
                MaxOf(ref maxWidths[4], GetPreferredWidth(sr.gMinus1));
                MaxOf(ref maxWidths[5], GetPreferredWidth(sr.gAmount));
                MaxOf(ref maxWidths[6], GetPreferredWidth(sr.gPlus1));
                MaxOf(ref maxWidths[7], GetPreferredWidth(sr.gPlus10));
                MaxOf(ref maxWidths[8], GetPreferredWidth(sr.gPlus100));
                MaxOf(ref maxWidths[9], GetPreferredWidth(sr.gUnlimited));

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
                    var c = a.currentLimit.CompareTo(b.currentLimit);
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
            if (statsPanelOffset + maxLines > rows.Count)
            {
                statsPanelOffset = Math.Max(0, rows.Count - maxLines);
            }

            statsPanelScrollUp.SetActive(statsPanelOffset > 0);
            statsPanelScrollDown.SetActive(statsPanelOffset + maxLines < rows.Count);

            int maxNameWidth = 0;
            int vPadding = 10;
            int hPadding = 30;
            int hPaddingSmall = 10;
            int border = 5;

            if (rows.Count == 0)
            {
                maxNameWidth = GetPreferredWidth(statsPanelEmpty);
                SetLocalPosition(statsPanelEmpty, 0, 0);
                statsPanelEmpty.SetActive(true);
                statsPanelHeaderRow.SetActive(false);
            }
            else
            {
                rows.Insert(statsPanelOffset, statsPanelHeaderRow);
                statsPanelEmpty.SetActive(false);
                statsPanelHeaderRow.SetActive(true);

                statsPanelHeaderRow.gName.GetComponent<Text>().text = "<i>Item</i>" + GetSortIndicator(0);
                statsPanelHeaderRow.gAmount.GetComponent<Text>().text = "<i>Amount</i>" + GetSortIndicator(1);

                ApplyPreferredSize(statsPanelHeaderRow.gName);
                ApplyPreferredSize(statsPanelHeaderRow.gAmount);

                MaxOf(ref maxWidths[0], GetPreferredWidth(statsPanelHeaderRow.gName));
                MaxOf(ref maxWidths[5], GetPreferredWidth(statsPanelHeaderRow.gAmount));

                maxLines++; // header
            }


            int bgHeight = maxLines * (iconSize + vPadding) + vPadding + 2 * border;
            int bgWidth = 2 * border + 2 * vPadding + 4 * hPadding + 6 * hPaddingSmall + iconSize + maxWidths.Sum();

            var rectBg2 = statsPanelBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(Mathf.Max(bgWidth, rectBg2.sizeDelta.x), bgHeight);
            rectBg2.localPosition = new Vector3(0, 0);

            var rectBg = limiterPanelBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * border, rectBg2.sizeDelta.y - 2 * border);

            statsPanelScrollUp.GetComponent<RectTransform>().localPosition = new Vector2(0, rectBg2.sizeDelta.y / 2 - 2);
            statsPanelScrollDown.GetComponent<RectTransform>().localPosition = new Vector2(0, -rectBg2.sizeDelta.y / 2 + 2);

            float dy = rectBg.sizeDelta.y / 2 - vPadding;
            for (int i = statsPanelOffset; i < rows.Count && i < statsPanelOffset + maxLines; i++)
            {
                var row = rows[i];

                float y = dy - iconSize / 2;

                float dx = -rectBg.sizeDelta.x / 2 + vPadding;

                SetLocalPosition(row.gIcon, dx + iconSize / 2, y);

                dx += iconSize + hPadding;

                SetLocalPosition(row.gName, dx + GetPreferredWidth(row.gName) / 2, y);

                dx += maxWidths[0] + hPadding;

                SetLocalPosition(row.gZero, dx + GetPreferredWidth(row.gZero) / 2, y);

                dx += maxWidths[1] + hPaddingSmall;

                SetLocalPosition(row.gMinus100, dx + GetPreferredWidth(row.gMinus100) / 2, y);

                dx += maxWidths[2] + hPaddingSmall;

                SetLocalPosition(row.gMinus10, dx + GetPreferredWidth(row.gMinus10) / 2, y);

                dx += maxWidths[3] + hPaddingSmall;

                SetLocalPosition(row.gMinus1, dx + GetPreferredWidth(row.gMinus1) / 2, y);

                dx += maxWidths[4] + hPadding;

                SetLocalPosition(row.gAmount, dx + maxWidths[5] - GetPreferredWidth(row.gAmount) / 2, y);

                dx += maxWidths[5] + hPadding;

                SetLocalPosition(row.gPlus1, dx + GetPreferredWidth(row.gPlus1) / 2, y);

                dx += maxWidths[6] + hPaddingSmall;

                SetLocalPosition(row.gPlus10, dx + GetPreferredWidth(row.gPlus10) / 2, y);

                dx += maxWidths[7] + hPaddingSmall;

                SetLocalPosition(row.gPlus100, dx + GetPreferredWidth(row.gPlus100) / 2, y);

                dx += maxWidths[8] + hPaddingSmall;

                SetLocalPosition(row.gUnlimited, dx + GetPreferredWidth(row.gUnlimited) / 2, y);

                // --- next row

                dy -= iconSize + vPadding;

                row.SetActive(true);

                if (i != statsPanelOffset)
                {
                    CheckRowButton(rectBg2, mp, row.gZero, ChangeLimit(row, -int.MaxValue));
                    CheckRowButton(rectBg2, mp, row.gMinus100, ChangeLimit(row, -100, true));
                    CheckRowButton(rectBg2, mp, row.gMinus10, ChangeLimit(row, -10, true));
                    CheckRowButton(rectBg2, mp, row.gMinus1, ChangeLimit(row, -1, true));
                    CheckRowButton(rectBg2, mp, row.gPlus1, ChangeLimit(row, 1, true));
                    CheckRowButton(rectBg2, mp, row.gPlus10, ChangeLimit(row, 10, true));
                    CheckRowButton(rectBg2, mp, row.gPlus100, ChangeLimit(row, 100, true));
                    CheckRowButton(rectBg2, mp, row.gUnlimited, ChangeLimit(row, int.MaxValue));
                }
            }

            if (statsPanelHeaderRow.gName.activeSelf)
            {

                CheckMouseSort(statsPanelHeaderRow.gName, 0);
                CheckMouseSort(statsPanelHeaderRow.gAmount, 1);
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

        static Action ChangeLimit(LimiterRow row, int delta, bool shiftable = false)
        {
            if (shiftable)
            {
                return () =>
                {
                    var d = delta;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        d *= 10;
                        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        {
                            d *= 10;
                        }
                    }
                    row.limitConfig.Value = Mathf.Clamp(0, row.limitConfig.Value + d, int.MaxValue);
                };
            }
            return () => row.limitConfig.Value = Mathf.Clamp(0, row.limitConfig.Value + delta, int.MaxValue);
        }


        static void CheckRowButton(RectTransform rectBg2, Vector2 mp, GameObject button, Action onPress)
        {
            var img = button.GetComponent<Image>();
            if (Within(rectBg2, button.GetComponent<RectTransform>(), mp))
            {
                img.color = defaultBoxColorLight;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    onPress();
                }
            }
            else
            {
                img.color = defaultBoxColor;
            }
        }

        static void MaxOf(ref int max, int amount)
        {
            max = Mathf.Max(max, amount);
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
            internal int currentLimit;
            internal ConfigEntry<int> limitConfig;

            internal GameObject gIcon;
            internal GameObject gName;
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

        static bool Within(RectTransform parent, RectTransform rt, Vector2 vec)
        {
            var x = parent.localPosition.x + rt.localPosition.x - rt.sizeDelta.x / 2;
            var y = parent.localPosition.y + rt.localPosition.y - rt.sizeDelta.y / 2;
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
            img.color = defaultBoxColor;

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
            return Mathf.CeilToInt(go.GetComponentInChildren<Text>().preferredWidth);
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
            if (limiterPanel != null && limiterPanel.activeSelf
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
