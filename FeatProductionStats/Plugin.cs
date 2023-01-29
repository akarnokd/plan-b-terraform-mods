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
    [BepInDependency("akarnokd.planbterraformmods.cheatprogressspeed", BepInDependency.DependencyFlags.SoftDependency)]
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

        static readonly int2 dicoCoordinates = new int2 { x = -1_000_100_100, y = 0 };

        static ManualLogSource logger;

        static Sprite icon;

        static Color defaultPanelLightColor = new Color(231f / 255, 227f / 255, 243f / 255, 1f);

        static Dictionary<string, Sprite> itemSprites = new();

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
            toggleKey = Config.Bind("General", "ToggleKey", KeyCode.F1, "Key to press while the building is selected to toggle its enabled/disabled state");
            fontSize = Config.Bind("General", "FontSize", 15, "The font size in the panel");
            itemSize = Config.Bind("General", "ItemSize", 32, "The size of the item's icon in the list");
            buttonLeft = Config.Bind("General", "ButtonLeft", 50, "The button's position relative to the left of the screen");
            buttonSize = Config.Bind("General", "ButtonSize", 50, "The button's width and height");
            maxStatLines = Config.Bind("General", "MaxLines", 20, "How many lines of items to show");
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

            if (Within(rectBg2, mp))
            {
                statsButtonIcon.GetComponent<Image>().color = Color.yellow;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    statsPanel.SetActive(!statsPanel.activeSelf);
                }
            }
            else
            {
                statsButtonIcon.GetComponent<Image>().color = Color.white;
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
            int horizon = 1;
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
            int padding = 10;
            int border = 5;
            int iconSize = itemSize.Value;

            for (int i = statsPanelOffset; i < allRows.Count && i < statsPanelOffset + maxLines; i++)
            {
                var row = allRows[i];

                row.gIcon = new GameObject("FeatProductionStatsPanel_Row_" + i + "_Icon");
                row.gIcon.transform.SetParent(statsPanelBackground.transform);
                row.gIcon.AddComponent<Image>().sprite = row.icon;

                row.gName = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Name", row.name);
                row.gProduction = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Production",
                        string.Format("{0:#.##0.0}", row.sumProduction / (float)horizon));
                row.gConsumption = CreateText(statsPanelBackground, "FeatProductionStatsPanel_Row_" + i + "_Consumption",
                        string.Format("{0:#,##0.0}", row.sumConsumption / (float)horizon));

                maxNameWidth = Math.Max(maxNameWidth, GetPreferredWidth(row.gName));
                maxProductionWidth = Math.Max(maxProductionWidth, GetPreferredWidth(row.gProduction));
                maxConsumptionWidth = Math.Max(maxConsumptionWidth, GetPreferredWidth(row.gConsumption));
            }

            int bgHeight = maxLines * (iconSize + padding) + padding + 2 * border;
            int bgWidth = 2 * border + 5 * padding + iconSize + maxNameWidth + maxProductionWidth + maxConsumptionWidth;

            var rectBg2 = statsPanelBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(Mathf.Max(bgWidth, rectBg2.sizeDelta.x), bgHeight);

            var rectBg = statsPanelBackground.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * border, rectBg2.sizeDelta.y - 2 * border);

            statsPanelScrollUp.GetComponent<RectTransform>().localPosition = new Vector2(0, rectBg2.sizeDelta.y / 2 - 2);
            statsPanelScrollDown.GetComponent<RectTransform>().localPosition = new Vector2(0, -rectBg2.sizeDelta.y / 2 + 2);

            float dy = rectBg.sizeDelta.y / 2 + padding;
            for (int i = statsPanelOffset; i < allRows.Count && i < statsPanelOffset + maxLines; i++)
            {
                var row = allRows[i];

                float y = dy - iconSize / 2;

                float dx = rectBg.sizeDelta.x / 2 + padding;

                SetLocalPosition(row.gIcon, dx + iconSize / 2, y);

                dx += iconSize;

                SetLocalPosition(row.gName, dx + GetPreferredWidth(row.gName) / 2, y);

                dx += maxNameWidth;

                SetLocalPosition(row.gProduction, dx + maxProductionWidth - GetPreferredWidth(row.gProduction) / 2, y);

                dx += maxProductionWidth;

                SetLocalPosition(row.gConsumption, dx + maxConsumptionWidth - GetPreferredWidth(row.gConsumption) / 2, y);

                dy -= iconSize - padding;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            RestoreState();
        }

        static void SaveState()
        {
            StringBuilder sb = new(512);
            // TODO
            GGame.dicoLandmarks[dicoCoordinates] = sb.ToString();
        }


        static void RestoreState()
        {
            if (GGame.dicoLandmarks.TryGetValue(dicoCoordinates, out var str))
            {
            // TODO
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CItem), nameof(CItem.Init))]
        static void CItem_Init(CItem __instance)
        {
            itemSprites[__instance.codeName] = __instance.icon.Sprite;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static void CItem_ContentFactory_Update01s_Pre(
            CItem_ContentFactory __instance, 
            int2 coords, 
            out List<CurrentStack> __state)
        {
            var stacks = __instance.GetStacks(coords);

            var save = new List<CurrentStack>();

            foreach (var stack in stacks.stacks)
            {
                save.Add(new CurrentStack { codeName = stack.item.codeName, amount = stack.nb });
            }

            __state = save;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentFactory), nameof(CItem_ContentFactory.Update01s))]
        static void CItem_ContentFactory_Update01s_Post(
            CItem_ContentFactory __instance,
            int2 coords,
            List<CurrentStack> __state)
        {
            var stacks = __instance.GetStacks(coords);
            var recipe = __instance.GetRecipe(coords);
            int numInputs = recipe.inputs.Count;

            for (int i = 0; i < stacks.stacks.Length; i++)
            {
                CStack stack = stacks.stacks[i];

                var curr = __state[i];
                var diff = curr.amount - stack.nb;
                if (diff != 0)
                {
                    if (i < numInputs)
                    {
                        AddForToday(curr.codeName, -diff, consumptionSamples);
                    }
                    else
                    {
                        AddForToday(curr.codeName, diff, productionSamples);
                    }
                }
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

            __state = new CurrentStack { codeName = stack.item.codeName, amount = stack.nb };
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_ContentExtractor), nameof(CItem_ContentExtractor.Update01s))]
        static void CItem_ContentExtractor_Update01s_Post(
            CItem_ContentFactory __instance,
            int2 coords,
            CurrentStack __state)
        {
            var stack = __instance.GetStack(coords);
            var diff = __state.amount - stack.nb;
            if (diff != 0)
            {
                AddForToday(__state.codeName, diff, productionSamples);
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
            internal string codeName;
            internal int amount;
        }

        internal class StatsRow
        {
            internal string codeName;
            internal string name;
            internal Sprite icon;
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
            txt.color = Color.white;
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
