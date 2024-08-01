using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Toolbar with buttons, allowing us to test various ScrollRect behaviours on device.
    /// </summary>
    public class DemoToolbar : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _toolbarRectTransform = null;

        [SerializeField]
        private GameObject _helpMenu = null;
        
        private static readonly Vector2 BotLeftRectValues = new(0f, 0f);
        private static readonly Vector2 TopLeftRectValues = new(0f, 1f);
        private static readonly Vector2 TopRightRectValues = new(1f, 1f);
        private static readonly Vector2 BotRightRectValues = new(1f, 0f);
        private static readonly Vector2 MiddleRectValues = new(0.5f, 0.5f);
        
        private ToolbarPosition _toolbarPosition;
        
        private void Awake()
        {
            SetToolbarPosition(ToolbarPosition.BotLeft);
        }

        /// <summary>
        /// Shows the help menu.
        /// </summary>
        public void ShowHelpMenu()
        {
            _helpMenu.SetActive(true);
        }

        /// <summary>
        /// Hides the help menu.
        /// </summary>
        public void HideHelpMenu()
        {
            _helpMenu.SetActive(false);
        }
        
        /// <summary>
        /// Rotates the toolbar position clockwise to the next corner.
        /// </summary>
        public void RotateToolbarPositionClockwise()
        {
            switch (_toolbarPosition)
            {
                case ToolbarPosition.BotLeft:
                    SetToolbarPosition(ToolbarPosition.TopLeft);
                    break;
                
                case ToolbarPosition.TopLeft:
                    SetToolbarPosition(ToolbarPosition.TopRight);
                    break;
                
                case ToolbarPosition.TopRight:
                    SetToolbarPosition(ToolbarPosition.BotRight);
                    break;
                
                case ToolbarPosition.BotRight:
                    SetToolbarPosition(ToolbarPosition.Middle);
                    break;
                
                case ToolbarPosition.Middle:
                    SetToolbarPosition(ToolbarPosition.BotLeft);
                    break;
            }
        }

        private void SetToolbarPosition(ToolbarPosition position)
        {
            _toolbarPosition = position;
            switch (position)
            {
                case ToolbarPosition.BotLeft:
                    SetRectTransformValues(BotLeftRectValues);
                    break;
                
                case ToolbarPosition.TopLeft:
                    SetRectTransformValues(TopLeftRectValues);
                    break;
                
                case ToolbarPosition.TopRight:
                    SetRectTransformValues(TopRightRectValues);
                    break;
                
                case ToolbarPosition.BotRight:
                    SetRectTransformValues(BotRightRectValues);
                    break;
                
                case ToolbarPosition.Middle:
                    SetRectTransformValues(MiddleRectValues);
                    break;
            }

            void SetRectTransformValues(Vector2 values)
            {
                (_toolbarRectTransform.anchorMin, _toolbarRectTransform.anchorMax) = (values, values);
                _toolbarRectTransform.pivot = values;
            }
        }

        private enum ToolbarPosition
        {
            BotLeft = 0,
            TopLeft = 1,
            TopRight = 2,
            BotRight = 3,
            Middle = 4,
        }
    }
}
