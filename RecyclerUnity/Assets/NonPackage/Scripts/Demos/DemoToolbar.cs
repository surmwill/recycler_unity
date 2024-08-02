using System;
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
        private RectTransform _toolbarRootRectTransform = null;

        [SerializeField]
        private GameObject[] _enableSecondRow = null;

        [SerializeField]
        private GameObject _helpMenu = null;

        [SerializeField]
        private Text _demoTitle = null;

        [SerializeField]
        private Text _demoDescription = null;

        [SerializeField]
        private Text _buttonDesciptions = null;

        [SerializeField]
        private bool _shouldEnableSecondRow = false;

        [SerializeField]
        private ButtonWithPointerDown[] _buttons = null;

        private bool[] _isButtonDown = null;

        private static readonly Vector2 BotLeftRectValues = new(0f, 0f);
        private static readonly Vector2 TopLeftRectValues = new(0f, 1f);
        private static readonly Vector2 TopRightRectValues = new(1f, 1f);
        private static readonly Vector2 BotRightRectValues = new(1f, 0f);
        private static readonly Vector2 MiddleRectValues = new(0.5f, 0.5f);
        
        private ToolbarPosition _toolbarPosition;
        
        private void Awake()
        {
            _isButtonDown = new bool[_buttons.Length];
            HideHelpMenu();
            SetToolbarPosition(ToolbarPosition.BotLeft);
        }

        private void Start()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                int buttonIndex = i;
                _buttons[i].OnPointerDownEvent.AddListener(() => SetButtonDown(buttonIndex, true));
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _isButtonDown.Length; i++)
            {
                SetButtonDown(i, false);
            }
        }

        private void OnDestroy()
        {
            Array.ForEach(_buttons, b => b.OnPointerDownEvent.RemoveAllListeners());
        }

        private void SetButtonDown(int buttonIndex, bool isDown)
        {
            _isButtonDown[buttonIndex] = isDown;
        }

        /// <summary>
        /// Returns true if the button with the given number was pressed down this frame, similar to Input.GetKeyDown.
        /// </summary>
        public bool GetButtonDown(int buttonNum)
        {
            return _isButtonDown[buttonNum];
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
            for (int i = 0; i < buttonDescriptions?.Length; i++)
            {
                _buttonDesciptions.text += $"{buttonDescriptions[i]}\n";
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
                (_toolbarRootRectTransform.anchorMin, _toolbarRootRectTransform.anchorMax) = (values, values);
                _toolbarRootRectTransform.pivot = values;
            }
        }

        private void OnValidate()
        {
            Array.ForEach(_enableSecondRow ?? Array.Empty<GameObject>(), go =>
            {
                if (go != null)
                {
                    go.SetActive(_shouldEnableSecondRow);   
                }
            });
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
