using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace FeatDepotPriority
{
    [BepInPlugin("akarnokd.planbterraformmods.featdepotpriority", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("akarnokd.planbterraformmods.uitranslationhungarian", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;
        static ConfigEntry<int> panelSize;
        static ConfigEntry<int> panelBottom;
        static ConfigEntry<int> panelLeft;
        static ConfigEntry<bool> autoScale;
        static ConfigEntry<int> overlayFontScale;

        static readonly Dictionary<int2, int> priorityDictionary = new();

        static readonly int2 dicoCoordinates = new() { x = -1_000_300_000, y = 0 };

        static ManualLogSource logger;

        static GameObject panelCanvas;
        static GameObject panelBody;
        static GameObject panelBorder;
        static GameObject panelIncrease;
        static GameObject panelDecrease;
        static GameObject panelValue;
        static GameObject panelOverlay;

        static int cachedScreenWidth;
        static int cachedScreenHeight;

        static readonly Dictionary<int2, GameObject> overlayIcons = new();

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            panelSize = Config.Bind("General", "PanelSize", 75, "The panel size (width and height)");
            panelBottom = Config.Bind("General", "PanelBottom", 35, "The panel position from the bottom of the screen");
            panelLeft = Config.Bind("General", "PanelLeft", 150, "The panel position from the left of the screen"); // we allow overlap with disable building for now
            autoScale = Config.Bind("General", "AutoScale", true, "Scale the position and size of the button with the UI scale of the game?");
            overlayFontScale = Config.Bind("General", "OverlayFontScale", 15, "The font scaling percent when zooming in on a priority depot.");

            var h = Harmony.CreateAndPatchAll(typeof(Plugin));
            GUIScalingSupport.TryEnable(h);
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

                if (selCoords.Positive && selBuilding is CItem_ContentDepot)
                {
                    UpdatePanel(selCoords);
                    panelBorder.SetActive(true);
                }
                else
                {
                    panelBorder.SetActive(false);
                }

            }
            else
            {
                if (panelCanvas != null)
                {
                    Destroy(panelCanvas);
                    panelCanvas = null;
                }
            }
        }

        static void EnsurePanel()
        {
            if (cachedScreenWidth != 0 && cachedScreenHeight != 0 && (cachedScreenWidth != Screen.width || cachedScreenHeight != Screen.height))
            {
                Destroy(panelCanvas);
                panelCanvas = null;
            }
            if (panelCanvas == null)
            {
                panelCanvas = new GameObject("FeatDepotPriority");
                var canvas = panelCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;

                panelOverlay = new GameObject("FeatDepotPriority_Overlay");
                panelOverlay.transform.SetParent(panelCanvas.transform);

                panelBorder = new GameObject("FeatDepotPriority_PanelBorder");
                panelBorder.transform.SetParent(panelCanvas.transform);

                panelBorder.AddComponent<GraphicRaycaster>();
                panelBorder.AddComponent<CTooltipTarget>();

                var img = panelBorder.AddComponent<Image>();
                img.color = DEFAULT_PANEL_BORDER_COLOR;

                panelBody = new GameObject("FeatDepotPriority_PanelBody");
                panelBody.transform.SetParent(panelBorder.transform);

                img = panelBody.AddComponent<Image>();
                img.color = DEFAULT_PANEL_COLOR;

                panelIncrease = CreateBox(panelBody, "FeatDepotPriority_PanelIncrease", "\u25B2", panelSize.Value / 3, DEFAULT_BOX_COLOR, Color.white);
                panelIncrease.AddComponent<GraphicRaycaster>();
                panelDecrease = CreateBox(panelBody, "FeatDepotPriority_PanelDecrease", "\u25BC", panelSize.Value / 3, DEFAULT_BOX_COLOR, Color.white);
                panelDecrease.AddComponent<GraphicRaycaster>();

                panelValue = CreateText(panelBody, "FeatDepotPriority_PanelValue", "", panelSize.Value / 3, Color.black);

                cachedScreenWidth = Screen.width;
                cachedScreenHeight = Screen.height;

                logger.LogInfo("Panel created");
            }
        }

        static void UpdatePanel(int2 selCoords)
        {
            var theScale = autoScale.Value ? GUIScalingSupport.currentScale : 1f;

            int border = 5;

            var bgRect = panelBorder.GetComponent<RectTransform>();

            bgRect.sizeDelta = new Vector2(panelSize.Value + 4 * border, panelSize.Value + 4 * border) * theScale;
            bgRect.localPosition = new Vector2(-Screen.width / 2 + panelLeft.Value * theScale, -Screen.height / 2 + panelBottom.Value * theScale)
                + bgRect.sizeDelta / 2;

            var tt = panelBorder.GetComponent<CTooltipTarget>();
            tt.text = SLoc.Get("FeatDepotPriority.Tooltip");
            tt.textDesc = SLoc.Get("FeatDepotPriority.TooltipDetails");

            var bodyRect = panelBody.GetComponent<RectTransform>();
            bodyRect.sizeDelta = bgRect.sizeDelta - new Vector2(2 * border, 2 * border);

            var fs = (bodyRect.sizeDelta.y - 8 - 4 * border) / 3;

            ResizeBox(panelIncrease, fs);
            ResizeBox(panelDecrease, fs);
            panelValue.GetComponent<Text>().fontSize = (int)fs;


            var incRect = panelIncrease.GetComponent<RectTransform>();
            incRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x - 2 * border, incRect.sizeDelta.y);
            incRect.localPosition = new Vector2(0, bodyRect.sizeDelta.y / 2 - border - incRect.sizeDelta.y / 2);

            var decRect = panelDecrease.GetComponent<RectTransform>();
            decRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x - 2 * border, decRect.sizeDelta.y);
            decRect.localPosition = new Vector2(0, - bodyRect.sizeDelta.y / 2 + border + decRect.sizeDelta.y / 2);

            priorityDictionary.TryGetValue(selCoords, out var n);
            var txt = panelValue.GetComponent<Text>();
            if (n == 0)
            {
                txt.text = SLoc.Get("FeatDepotPriority.None");
            }
            else
            {
                txt.text = SLoc.Get("FeatDepotPriority.Some", n);
            }

            panelIncrease.GetComponent<Image>().color = DEFAULT_BOX_COLOR;
            panelDecrease.GetComponent<Image>().color = DEFAULT_BOX_COLOR;


            List<RaycastResult> mouseRaycastResults = SSingleton<SScenesManager>.Inst.GetMouseRaycastResults();
            foreach (var rcr in mouseRaycastResults)
            {
                Action<int> onClick = v =>
                            {
                                priorityDictionary.TryGetValue(selCoords, out var m);
                                var u = m + v;
                                if (u != 0)
                                {
                                    priorityDictionary[selCoords] = u;
                                }
                                else
                                {
                                    priorityDictionary.Remove(selCoords);
                                }
                                SaveState();
                            };
                HandleButton(rcr, panelIncrease, 1, onClick);
                HandleButton(rcr, panelDecrease, -1, onClick);
                if (rcr.gameObject == panelBorder)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse2))
                    {
                        priorityDictionary.Remove(selCoords);
                    }
                }
            }
        }

        void Update()
        {
            // lags otherwise?
            UpdateOverlay();
        }

        static void UpdateOverlay()
        {
            if (panelOverlay != null)
            {
                foreach (var kv in priorityDictionary)
                {
                    var coords = kv.Key;
                    if (!overlayIcons.TryGetValue(coords, out var icon) || icon == null)
                    {
                        icon = CreateBox(panelOverlay, "Priority_At_" + coords.x + "_" + coords.y, "", 10, new Color(0.9f, 0.9f, 0.9f, 1f), Color.black);

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

                    var diff = Vector2.Distance(posCanvas, posCanvasNeighbor);
                    float scaler = diff * overlayFontScale.Value / 100f;

                    rect.localPosition = new Vector2(posCanvas.x, posCanvas.y - diff / 2 + scaler);

                    icon.GetComponentInChildren<Text>().text = SLoc.Get("FeatDepotPriority.Overlay", kv.Value);

                    ResizeBox(icon, scaler);
                }

                foreach (var coords in new List<int2>(overlayIcons.Keys))
                {
                    if (!priorityDictionary.ContainsKey(coords))
                    {
                        Destroy(overlayIcons[coords]);
                        overlayIcons.Remove(coords);
                    }
                }
            }
        }

        static void HandleButton(RaycastResult rcr, GameObject go, int delta, Action<int> onClick)
        {
            if (rcr.gameObject == go)
            {
                go.GetComponent<Image>().color = DEFAULT_BOX_COLOR_HOVER;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        delta *= 10;
                    }
                    onClick(delta);
                }
            }
        }

        static void SaveState()
        {
            StringBuilder sb = new(512);
            
            foreach (var kv in priorityDictionary)
            {
                if (sb.Length != 0)
                {
                    sb.Append(';');
                }
                sb.Append(kv.Key.x).Append(',').Append(kv.Key.y).Append(',').Append(kv.Value);
            }

            GGame.dicoLandmarks[dicoCoordinates] = sb.ToString();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SGame), nameof(SGame.Load))]
        static void SGame_Load()
        {
            RestoreState();
            logger.LogInfo("priorityDictionary.Count = " + priorityDictionary.Count);
        }

        static void RestoreState()
        {
            priorityDictionary.Clear();
            if (GGame.dicoLandmarks.TryGetValue(dicoCoordinates, out var str))
            {
                if (str.Length != 0)
                {
                    var coords = str.Split(';');
                    foreach (var coord in coords)
                    {
                        var nums = coord.Split(',');
                        if (nums.Length == 3)
                        {
                            var c = new int2(int.Parse(nums[0]), int.Parse(nums[1]));
                            priorityDictionary[c] = int.Parse(nums[2]);
                        }
                    }
                }
            }
        }

        static bool priorityTransfer;
        static int takeFromAmountSave;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CDrone), "SearchStacks")]
        static bool CDrone_SearchStacks(
            in int2 ___depotCoords, 
            CItem ___depotItem, 
            int2 coords, 
            ref CDrone.TransportStep ___takeFrom,
            ref CDrone.TransportStep ___giveTo,
            CStacks stacks, 
            CVehicle vehicle, 
            ref bool found)
        {
            // already picked a priority transfer? do nothing
            if (priorityTransfer)
            {
                return false;
            }
            if (modEnabled.Value)
            {
                if (vehicle == null && ___depotCoords != coords)
                {
                    var ourStacks = GHexes.stacks[___depotCoords.x, ___depotCoords.y];
                    ref var ourStack0 = ref ourStacks.stacks[0];

                    var areWeDepot = ourStack0.IsDepot;
                    var areTheyDepot = stacks.stacks != null ? stacks.stacks[0].IsDepot : false;

                    // logger.LogDebug("CDrone_SearchStacks. " + areWeDepot + " -> " + areTheyDepot);
                    if (areWeDepot && areTheyDepot)
                    {

                        priorityDictionary.TryGetValue(___depotCoords, out var depotPrio);
                        priorityDictionary.TryGetValue(coords, out var targetPrio);

                        // logger.LogDebug("CDrone_SearchStacks: " + depotPrio + " -> " + targetPrio);

                        // if both are part of the priority system
                        if (depotPrio != 0 && targetPrio != 0)
                        {
                            // but have different priority
                            if (depotPrio != targetPrio)
                            {
                                var isEmpty = ourStack0.nb <= 0;
                                var isFull = ourStack0.nb + ourStack0.nbBooked >= ourStack0.nbMax;

                                for (int i = stacks.stacks.Length - 1; i >= 0; i--)
                                {
                                    ref var targetStack = ref stacks.stacks[i];
                                    // has the same item type as we, is a depot
                                    if (targetStack.item == ___depotItem)
                                    {
                                        // logger.LogDebug("CDrone_SearchStacks: we are (" + isEmpty + ", " + isFull + ") they are (" 
                                        //    + targetStack.Empty + ", " + targetStack.Full + ")");
                                        if (depotPrio < targetPrio && !isEmpty && !targetStack.Full)
                                        {
                                            ___takeFrom.stacks = ourStacks;
                                            ___takeFrom.stackId = 0;
                                            ___takeFrom.stackContainerId = ourStack0.nb - 1;
                                            ___takeFrom.coords = ___depotCoords;
                                            ___takeFrom.vehicle = null;

                                            ___giveTo.stacks = stacks;
                                            ___giveTo.stackId = i;
                                            ___giveTo.stackContainerId = targetStack.nb;
                                            ___giveTo.coords = coords;
                                            ___giveTo.vehicle = null;

                                            priorityTransfer = true;
                                            takeFromAmountSave = ourStack0.nb;
                                            // we need to pass the if check for depot balancing between each other,
                                            // which only happens if the source depot has 2+ items more than the target depot
                                            // we'll have to temporarily bump the from stack and restore it later
                                            ourStack0.nb = targetStack.nb + 2;

                                            found = true;
                                            logger.LogInfo("CDrone_SearchStacks: us -> them");
                                            break;
                                        }
                                        else
                                        if (depotPrio > targetPrio && !isFull && !targetStack.Empty)
                                        {
                                            ___takeFrom.stacks = stacks;
                                            ___takeFrom.stackId = i;
                                            ___takeFrom.stackContainerId = targetStack.nb - 1;
                                            ___takeFrom.coords = coords;
                                            ___takeFrom.vehicle = null;

                                            ___giveTo.stacks = ourStacks;
                                            ___giveTo.stackId = 0;
                                            ___giveTo.stackContainerId = ourStack0.nb;
                                            ___giveTo.coords = ___depotCoords;
                                            ___giveTo.vehicle = null;

                                            priorityTransfer = true;
                                            takeFromAmountSave = targetStack.nb;
                                            // we need to pass the if check for depot balancing between each other,
                                            // which only happens if the source depot has 2+ items more than the target depot
                                            // we'll have to temporarily bump the from stack and restore it later
                                            targetStack.nb = ourStack0.nb + 2;

                                            found = true;
                                            logger.LogInfo("CDrone_SearchStacks: us <- them");
                                            break;
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                        if (depotPrio != 0 || targetPrio != 0)
                        {
                            // logger.LogDebug("CDrone_SearchStacks: no interaction allowed");
                            return false;
                        }
                    }
                }
            }
            // logger.LogDebug("Running original CDrone_SearchStacks");
            return true;
        }

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CDrone), "Search")]
        static void CDrone_SearchStacks()
        {
            if (priorityTransfer)
            {
                logger.LogError("Unexpected priority transfer");
            }
        }
        */

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CDrone), "ChangeState_Taking")]
        static void CDrone_ChangeState_Taking(
            ref CDrone.TransportStep ___takeFrom)
        {
            if (priorityTransfer)
            {
                //logger.LogDebug("ChangeState_Taking: Restoring stack amount: " + ___takeFrom.Stack.nb + " <- " + takeFromAmountSave);
                priorityTransfer = false;
                ___takeFrom.Stack.nb = takeFromAmountSave;
                takeFromAmountSave = 0;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CItem_Content), "Destroy")]
        static void CItem_Content_Destroy_Pre(int2 coords)
        {
            priorityDictionary.Remove(coords);
            SaveState();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            LibCommon.Translation.UpdateTranslations("English", new()
            {
                { "FeatDepotPriority.Tooltip", "Depot Priority" },
                { "FeatDepotPriority.TooltipDetails", "Drones will try to move containers from lower priority depots to higher ones. Numbers indicate relative priority.\n\nReset priority with [Middle click].\n\n<i>FeatDepotPriority mod</i>" },
                { "FeatDepotPriority.None", "<b>None</b>" },
                { "FeatDepotPriority.Some", "<b>{0}</b>" },
                { "FeatDepotPriority.Overlay", "\u25B2 <b>{0}</b> \u25B2" },
            });

            LibCommon.Translation.UpdateTranslations("Hungarian", new()
            {
                { "FeatDepotPriority.Tooltip", "Raktár prioritása" },
                { "FeatDepotPriority.TooltipDetails", "A drónok megpróbálják az alacsonyabb prioritású raktárakból a magasabb prioritású raktárakba szállítani a konténereket. A számok relatív prioritást jelentenek.\n\nPrioritás törlése [Középső egérgombbal].\n\n<i>FeatDepotPriority mod</i>" },
                { "FeatDepotPriority.None", "<b>Nincs</b>" },
                { "FeatDepotPriority.Some", "<b>{0}</b>" },
                { "FeatDepotPriority.Overlay", "\u25B2 <b>{0}</b> \u25B2" },
            });
        }
    }
}
