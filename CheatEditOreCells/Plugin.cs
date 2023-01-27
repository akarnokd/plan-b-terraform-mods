using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CheatEditOreCells
{
    [BepInPlugin("akarnokd.planbterraformmods.cheateditorecells", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> oreAmountChange;

        static KeyCode placeOreInput;
        static KeyCode removeOreInput;
        static KeyCode nextOreInput;
        static KeyCode prevOreInput;
        static KeyCode placementModeInput;

        static bool placementMode;
        static int currentOreIndex;
        static byte[] oreIndices = { 7, 6, 8, 9 };
        static string[] oreNames = { "sulfur", "iron", "aluminumOre", "fluoride" };

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loading!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled");
            oreAmountChange = Config.Bind("General", "AmountChange", 100, "How much ore to add or remove from the hex.");

            // To be configurable later
            placeOreInput = KeyCode.Mouse0;
            removeOreInput = KeyCode.Mouse1;
            nextOreInput = KeyCode.KeypadPlus;
            prevOreInput = KeyCode.KeypadMinus;
            placementModeInput = KeyCode.KeypadMultiply;

            Harmony.CreateAndPatchAll(typeof(Plugin));

            Logger.LogInfo($"Plugin is loaded!");
        }

        static bool IsKeyDown(KeyCode keyCode)
        {
            GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            return (currentSelectedGameObject == null || currentSelectedGameObject.TryGetComponent<InputField>(out _))
                && Input.GetKeyDown(keyCode);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud), "OnUpdate")]
        static bool SSceneHud_OnUpdate()
        {
            if (modEnabled.Value)
            {
                logger.LogInfo("SSceneHud_OnUpdate() start");
                try
                {
                    if (placementMode)
                    {
                        if (IsKeyDown(placementModeInput))
                        {
                            placementMode = false;
                            logger.LogInfo("Placement mode OFF");
                            FlashPanel(null);
                            return true;
                        }

                        if (IsKeyDown(nextOreInput))
                        {
                            currentOreIndex++;
                            if (currentOreIndex == oreIndices.Length)
                            {
                                currentOreIndex = 0;
                            }
                            FlashPanel();
                        }
                        else
                        if (IsKeyDown(prevOreInput))
                        {
                            currentOreIndex--;
                            if (currentOreIndex < 0)
                            {
                                currentOreIndex = oreIndices.Length - 1;
                            }
                            FlashPanel();
                        }

                        bool add = IsKeyDown(placeOreInput);
                        bool remove = IsKeyDown(removeOreInput);

                        if (add || remove)
                        {
                            int2 mouseoverCoords = GScene3D.mouseoverCoords;

                            if (mouseoverCoords.Negative)
                            {
                                return true;
                            }


                            var oreId = oreIndices[currentOreIndex];
                            var oreName = oreNames[currentOreIndex];
                            ushort amount = GHexes.groundData[mouseoverCoords.x, mouseoverCoords.y];

                            GHexes.groundId[mouseoverCoords.x, mouseoverCoords.y] = oreId;

                            if (add)
                            {
                                var newAmount = (ushort)Math.Min(ushort.MaxValue, amount + oreAmountChange.Value);
                                GHexes.groundData[mouseoverCoords.x, mouseoverCoords.y] = newAmount;
                                logger.LogInfo(" Changing " + oreName + "(" + oreId + ") at " + mouseoverCoords.x + "," + mouseoverCoords.y + " = " + newAmount);
                            }
                            else
                            {
                                var newAmount = (ushort)Math.Max(0, amount - oreAmountChange.Value);
                                GHexes.groundData[mouseoverCoords.x, mouseoverCoords.y] = newAmount;
                                if (newAmount == 0)
                                {
                                    GItems.itemDirt.Create(mouseoverCoords, true);
                                }
                                logger.LogInfo(" Changing " + oreName + "(" + oreId + ") at " + mouseoverCoords.x + "," + mouseoverCoords.y + " = " + newAmount);
                            }
                            SSingleton<SBlocks>.Inst.GetBlock(mouseoverCoords).itemsChanged = true;
                            GHexes.SetFlag(mouseoverCoords, GHexes.Flag.ItemsChanged, true);

                            return false;
                        }
                    }
                    else
                    {
                        if (IsKeyDown(placementModeInput))
                        {
                            placementMode = true;
                            logger.LogInfo("Placement mode ON");
                            FlashPanel();
                            return true;
                        }
                    }
                } 
                catch (Exception ex)
                {
                    logger.LogError(ex);
                }
            }
            return true;
        }


        static GameObject placementModePanel;
        static GameObject placementModePanelBackground;
        static GameObject placementModePanelText;
        static GameObject placementModePanelHint;

        static void FlashPanel()
        {
            FlashPanel("Place ore: " + oreNames[currentOreIndex] + " (±" + oreAmountChange.Value + ")");
        }
        static void FlashPanel(string title)
        {
            if (title == null)
            {
                Destroy(placementModePanel);
                Destroy(placementModePanelBackground);
                Destroy(placementModePanelText);
                Destroy(placementModePanelHint);

                placementModePanel = null;
                placementModePanelBackground = null;
                placementModePanelText = null;
                placementModePanelHint = null;
                return;
            }
            if (placementModePanel == null)
            {
                placementModePanel = new GameObject("CheatEditOreCellsInfo");
                var canvas = placementModePanel.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                placementModePanelBackground = new GameObject("CheatEditOreCellsInfo_Backgroud");
                placementModePanelBackground.transform.SetParent(placementModePanel.transform, false);
                var img = placementModePanelBackground.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.95f);

                placementModePanelText = new GameObject("CheatEditOreCellsInfo_Text");
                placementModePanelText.transform.SetParent(placementModePanel.transform, false);

                var textIn = placementModePanelText.AddComponent<Text>();
                textIn.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textIn.fontSize = 30;
                textIn.color = Color.white;
                textIn.resizeTextForBestFit = false;
                textIn.verticalOverflow = VerticalWrapMode.Overflow;
                textIn.horizontalOverflow = HorizontalWrapMode.Overflow;
                textIn.alignment = TextAnchor.MiddleCenter;

                placementModePanelHint = new GameObject("CheatEditOreCellsInfo_Hint");
                placementModePanelHint.transform.SetParent(placementModePanel.transform, false);
                textIn = placementModePanelHint.AddComponent<Text>();
                textIn.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textIn.fontSize = 20;
                textIn.color = Color.yellow;
                textIn.resizeTextForBestFit = false;
                textIn.verticalOverflow = VerticalWrapMode.Overflow;
                textIn.horizontalOverflow = HorizontalWrapMode.Overflow;
                textIn.alignment = TextAnchor.MiddleCenter;
            }
            var text = placementModePanelText.GetComponent<Text>();
            var textHint = placementModePanelHint.GetComponent<Text>();

            text.text = title;
            textHint.text = "Previous Ore: [Numpad -]. Next Ore: [Numpad +]. Toggle editing: [Numpad *]";

            var w = Mathf.Max(text.preferredWidth, textHint.preferredWidth) + 20;
            var h = text.preferredHeight + textHint.preferredHeight + 30;

            var newSizew = new Vector2(w, h);
            var x = 0;
            var y = -Screen.height / 2 + h + 20;

            var rectBg = placementModePanelBackground.GetComponent<RectTransform>();
            rectBg.localPosition = new Vector2(x, y);
            rectBg.sizeDelta = newSizew;

            var rect = text.GetComponent<RectTransform>();
            rect.localPosition = new Vector2(x, y + h / 2 - 15 - text.preferredHeight / 2);
            rect.sizeDelta = newSizew;

            var rectHint = textHint.GetComponent<RectTransform>();
            rectHint.localPosition = new Vector2(x, y - h / 2 + 15 + textHint.preferredHeight / 2);
            rectHint.sizeDelta = newSizew;
        }
    }
}
