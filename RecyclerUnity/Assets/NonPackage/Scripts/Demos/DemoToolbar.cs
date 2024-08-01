using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [SerializeField]
        private Text _demoTitle = null;

        [SerializeField]
        private Text _demoDescription = null;

        [SerializeField]
        private Text _buttonDesciptions = null;
        
        private static readonly Vector2 BotLeftRectValues = new(0f, 0f);
        private static readonly Vector2 TopLeftRectValues = new(0f, 1f);
        private static readonly Vector2 TopRightRectValues = new(1f, 1f);
        private static readonly Vector2 BotRightRectValues = new(1f, 0f);
        private static readonly Vector2 MiddleRectValues = new(0.5f, 0.5f);
        
        private ToolbarPosition _toolbarPosition;
        
        private void Awake()
        {
            HideHelpMenu();
            SetToolbarPosition(ToolbarPosition.BotLeft);
        }

        /// <summary>
        /// Within the help menu, sets the demo title
        /// </summary>
        public void SetHelpMenuDemoTitle(string title)
        {
            _demoTitle.text = title;
        }

        /// <summary>
        /// Within the help menu, sets the demo description
        /// </summary>
        public void SetHelpMenuDemoDescription(string description)
        {
            _demoDescription.text = description;
        }

        /// <summary>
        /// Within the help menu, sets the descriptions for the numbered buttons in the toolbar,
        /// the index corresponding to the number on the button.
        /// </summary>
        public void SetHelpMenuDemoButtonDescriptions(string[] buttonDescriptions)
        {
            _buttonDesciptions.text = string.Empty;
            for (int i = 0; i < buttonDescriptions.Length; i++)
            {
                _buttonDesciptions.text += $"{i}: {buttonDescriptions[i]}\n";
            }
        }

        /// <summary>
        /// Returns to the menu screen, where we can navigate to different demos
        /// </summary>
        public void ReturnToDemoMenu()
        {
            // Assume the menu screen is the first screen in our build index
            SceneManager.LoadScene(0);
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
