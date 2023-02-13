// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using UnityEngine;
using UnityEngine.UI;
using static LibCommon.GUITools;

namespace LibCommon
{
    /// <summary>
    /// A button to be displayed and animated on the top-left of the ingame screen.
    /// </summary>
    internal class ToolbarTopButton
    {
        GameObject buttonCanvas;
        GameObject buttonBackground;
        GameObject buttonBackground2;
        GameObject buttonIcon;

        internal Action onClick;

        public void Create(string name, Action onClick)
        {
            if (buttonCanvas != null)
            {
                return;
            }

            buttonCanvas = new GameObject(name);
            var canvas = buttonCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            buttonBackground2 = new GameObject(name + "_BackgroundBorder");
            buttonBackground2.transform.SetParent(buttonCanvas.transform);

            var img = buttonBackground2.AddComponent<Image>();
            img.color = DEFAULT_PANEL_BORDER_COLOR;

            buttonBackground = new GameObject(name + "_Background");
            buttonBackground.transform.SetParent(buttonBackground2.transform);

            img = buttonBackground.AddComponent<Image>();
            img.color = DEFAULT_PANEL_COLOR;

            buttonIcon = new GameObject(name + "_Icon");
            buttonIcon.transform.SetParent(buttonBackground.transform);

            img = buttonIcon.AddComponent<Image>();
            img.color = Color.white;

            buttonBackground2.AddComponent<GraphicRaycaster>();
            buttonBackground2.AddComponent<CTooltipTarget>();

            this.onClick = onClick;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(buttonCanvas);
            buttonCanvas = null;
            buttonBackground = null;
            buttonBackground2 = null;
            buttonIcon = null;
            onClick = null;
        }

        public bool IsAvailable()
        {
            return buttonCanvas != null;
        }

        public RectTransform GetRectTransform()
        {
            return buttonBackground2?.GetComponent<RectTransform>();
        }

        public void SetIcon(Sprite icon)
        {
            buttonIcon.GetComponent<Image>().sprite = icon;
        }

        public void SetTooltip(string tooltipTitle, string tooltipDesc)
        {
            var tt = buttonBackground2.GetComponent<CTooltipTarget>();
            tt.text = tooltipTitle;
            tt.textDesc = tooltipDesc;
        }

        public void Update(int buttonLeft, int buttonSize, bool autoScale)
        {
            float theScale = autoScale ? GUIScalingSupport.currentScale : 1f;

            var padding = 5;

            var rectBg2 = buttonBackground2.GetComponent<RectTransform>();
            rectBg2.sizeDelta = new Vector2(buttonSize + 4 * padding, buttonSize + 4 * padding) * theScale;
            rectBg2.localPosition = new Vector3(-Screen.width / 2 + buttonLeft * theScale + rectBg2.sizeDelta.x / 2, Screen.height / 2 - rectBg2.sizeDelta.y / 2);

            var rectBg = buttonBackground.GetComponent<RectTransform>();
            rectBg.sizeDelta = new Vector2(rectBg2.sizeDelta.x - 2 * padding * theScale, rectBg2.sizeDelta.y - 2 * padding * theScale);

            var rectIcn = buttonIcon.GetComponent<RectTransform>();
            rectIcn.sizeDelta = new Vector2(buttonSize, buttonSize) * theScale;

            var mp = GetMouseCanvasPos();

            if (Within(rectBg2, mp))
            {
                buttonBackground.GetComponent<Image>().color = Color.yellow;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    onClick?.Invoke();
                }
            }
            else
            {
                buttonBackground.GetComponent<Image>().color = DEFAULT_PANEL_COLOR;
            }
        }

        public void SetVisible(bool visible)
        {
            buttonCanvas?.SetActive(visible);
        }
    }
}
