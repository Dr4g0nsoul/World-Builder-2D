using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelEditorStyles
    {

        public const string ICON_ROOT = "LevelEditor/Icons/";

        //Background
        private static GUIStyle _editorContainer;
        private static GUIStyle _elementContainer;
        private static readonly Color backgroundColor = new Color(0f, 0f, 0f, .42f);
        private static readonly Color elementContainerBackgroundColor = new Color(0f, 0f, 0f, .4f);

        //Text style
        private static GUIStyle _textLeft;
        private static GUIStyle _textCentered;
        private static GUIStyle _headerLeft;
        private static GUIStyle _headerCentered;
        private static readonly int textSize;
        private static readonly Color textColor = new Color(1f, 1f, 1f);

        //Button style
        private static GUIStyle _button;
        private static readonly Color buttonColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color buttonHoverColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        //Message box preferences
        private static GUIStyle _messagebox;
        private static GUIStyle _messageboxHeader;
        private static GUIStyle _messageboxText;
        private static GUIStyle _messageboxButton;
        private static readonly Color messageBoxColor = new Color(0f, 0f, 0f, .42f);

        //Icon button preferences
        private static GUIStyle _menuElement;
        private static GUIStyle _menuElementImage;
        private static GUIStyle _menuElementText;
        private static GUIStyle _menuElementHeader;
        private static readonly Color menuElementColor = new Color(0f, 0f, 0f, .42f);
        private static readonly Color menuElementHoverColor = new Color(.2f, .2f, .2f, .42f);

        //Toolbar preferences
        private static GUIStyle _toolbar;
        private static GUIStyle _toolbarSelected;
        private static readonly Color toolbarUnselectedColor = new Color(.2f, .2f, .2f);
        private static readonly Color toolbarHoverColor = new Color(.3f, .3f, .3f);
        private static readonly Color toolbarSelectedColor = new Color(0f, .30f, .75f);
        private static readonly Color toolbarSelectedHoverColor = new Color(0f, .4f, .8f);
        private static readonly string[] toolbarIconPaths = new string[]
        {
        "Alphabetically",
        "RecentlyUsed",
        "MostUsedLevel",
        "MostUsedOverall"
        };
        public static Texture2D[] ToolbarIcons { get; private set; }

        //Search field preferences
        private static GUIStyle _searchField;

        #region Style Setup

        #region Style Properties

        #region Background

        public static GUIStyle EditorContainer
        {
            get
            {
                if (_editorContainer == null)
                {
                    _editorContainer = new GUIStyle(EditorStyles.helpBox);
                    _editorContainer.normal.background = GetBackgroundTexture(backgroundColor);
                }
                return _editorContainer;
            }
        }

        public static GUIStyle ElementContainer
        {
            get
            {
                if (_elementContainer == null)
                {
                    _elementContainer = new GUIStyle(EditorStyles.helpBox);
                    _elementContainer.normal.background = GetBackgroundTexture(elementContainerBackgroundColor);
                }
                return _elementContainer;
            }
        }

        #endregion

        #region Text

        public static GUIStyle TextLeft
        {
            get
            {
                if (_textLeft == null)
                {
                    _textLeft = new GUIStyle(EditorStyles.label);
                    _textLeft.normal.textColor = textColor;
                }
                return _textLeft;
            }
        }

        public static GUIStyle TextCentered
        {
            get
            {
                if (_textCentered == null)
                {
                    _textCentered = new GUIStyle(EditorStyles.label);
                    _textCentered.alignment = TextAnchor.UpperCenter;
                    _textCentered.normal.textColor = textColor;
                }
                return _textCentered;
            }
        }

        public static GUIStyle HeaderLeft
        {
            get
            {
                if (_headerLeft == null)
                {
                    _headerLeft = new GUIStyle(EditorStyles.boldLabel);
                    _headerLeft.normal.textColor = textColor;
                }
                return _headerLeft;
            }
        }

        public static GUIStyle HeaderCentered
        {
            get
            {
                if (_headerCentered == null)
                {
                    _headerCentered = new GUIStyle(EditorStyles.boldLabel);
                    _headerCentered.alignment = TextAnchor.UpperCenter;
                    _headerCentered.normal.textColor = textColor;
                }
                return _headerCentered;
            }
        }

        #endregion

        #region Button

        public static GUIStyle Button
        {
            get
            {
                if (_button == null)
                {
                    _button = new GUIStyle(EditorStyles.miniButton);
                    _button.normal.textColor = textColor;
                    _button.normal.background = GetBackgroundTexture(buttonColor);
                    _button.hover.textColor = textColor;
                    _button.hover.background = GetBackgroundTexture(buttonHoverColor);
                }
                return _button;
            }
        }

        #endregion

        #region Message Boxes

        public static GUIStyle Messagebox
        {
            get
            {
                if (_messagebox == null)
                {
                    _messagebox = new GUIStyle(EditorStyles.helpBox);
                    _messagebox.normal.background = GetBackgroundTexture(messageBoxColor);
                }
                return _messagebox;
            }
        }

        public static GUIStyle MessageboxText
        {
            get
            {
                if (_messageboxText == null)
                {
                    _messageboxText = new GUIStyle(EditorStyles.label);
                    _messageboxText.alignment = TextAnchor.MiddleCenter;
                    _messageboxText.normal.textColor = textColor;
                    _messageboxText.wordWrap = true;
                    _messageboxText.stretchHeight = true;
                }
                return _messageboxText;
            }
        }

        public static GUIStyle MessageboxHeader
        {
            get
            {
                if (_messageboxHeader == null)
                {
                    _messageboxHeader = new GUIStyle(EditorStyles.boldLabel);
                    _messageboxHeader.alignment = TextAnchor.UpperCenter;
                    _messageboxHeader.normal.textColor = textColor;
                    _messageboxHeader.stretchHeight = true;
                }
                return _messageboxHeader;
            }
        }

        public static GUIStyle MessageboxButton
        {
            get
            {
                if (_messageboxButton == null)
                {
                    _messageboxButton = new GUIStyle(EditorStyles.miniButton);
                    _messageboxButton.normal.textColor = textColor;
                    _messageboxButton.normal.background = GetBackgroundTexture(buttonColor);
                    _messageboxButton.hover.textColor = textColor;
                    _messageboxButton.hover.background = GetBackgroundTexture(buttonHoverColor);
                    _messageboxButton.alignment = TextAnchor.MiddleCenter;
                }
                return _messageboxButton;
            }
        }

        #endregion

        #region Menu Elements

        public static GUIStyle MenuElement
        {
            get
            {
                if (_menuElement == null)
                {
                    _menuElement = new GUIStyle(EditorStyles.helpBox);
                    _menuElement.normal.background = GetBackgroundTexture(menuElementColor);
                    _menuElement.hover.background = GetBackgroundTexture(menuElementHoverColor);
                }
                return _menuElement;
            }
        }

        public static GUIStyle MenuElementImage
        {
            get
            {
                if (_menuElementImage == null)
                {
                    _menuElementImage = new GUIStyle(EditorStyles.label);
                    _menuElementImage.alignment = TextAnchor.MiddleCenter;
                }
                return _menuElementImage;
            }
        }

        public static GUIStyle MenuElementText
        {
            get
            {
                if (_menuElementText == null)
                {
                    _menuElementText = new GUIStyle(EditorStyles.label);
                    _menuElementText.alignment = TextAnchor.MiddleCenter;
                    _menuElementText.normal.textColor = textColor;
                    _menuElementText.wordWrap = true;
                    _menuElementText.stretchHeight = true;
                }
                return _menuElementText;
            }
        }

        public static GUIStyle MenuElementHeaderText
        {
            get
            {
                if (_menuElementHeader == null)
                {
                    _menuElementHeader = new GUIStyle(EditorStyles.label);
                    _menuElementHeader.alignment = TextAnchor.UpperCenter;
                    _menuElementHeader.normal.textColor = textColor;
                    _menuElementHeader.wordWrap = true;
                    _menuElementHeader.stretchHeight = true;
                }
                return _menuElementHeader;
            }
        }

        #endregion

        #region Toolbar

        public static GUIStyle Toolbar
        {
            get
            {
                if (_toolbar == null)
                {
                    _toolbar = new GUIStyle(EditorStyles.label);
                    _toolbar.alignment = TextAnchor.MiddleCenter;
                    _toolbar.padding = new RectOffset(0, 0, 0, 0);
                    _toolbar.overflow = new RectOffset(2, 2, 0, 0);
                    _toolbar.border = new RectOffset(0, 0, 0, 0);
                    _toolbar.clipping = TextClipping.Clip;
                    _toolbar.contentOffset = Vector2.zero;
                    _toolbar.imagePosition = ImagePosition.ImageOnly;
                    _toolbar.normal.background = GetBackgroundTexture(toolbarUnselectedColor);
                    _toolbar.normal.textColor = Color.white;
                    _toolbar.onNormal.background = GetBackgroundTexture(toolbarSelectedColor);
                    _toolbar.onNormal.textColor = Color.white;
                    _toolbar.hover.background = GetBackgroundTexture(toolbarHoverColor);
                    _toolbar.hover.textColor = Color.white;
                }
                return _toolbar;
            }
        }

        public static GUIStyle ToolbarSelected
        {
            get
            {
                if (_toolbarSelected == null)
                {
                    _toolbarSelected = new GUIStyle(EditorStyles.label);
                    _toolbarSelected.alignment = TextAnchor.MiddleCenter;
                    _toolbarSelected.padding = new RectOffset(0, 0, 0, 0);
                    _toolbarSelected.overflow = new RectOffset(2, 2, 0, 0);
                    _toolbarSelected.border = new RectOffset(0, 0, 0, 0);
                    _toolbarSelected.clipping = TextClipping.Clip;
                    _toolbarSelected.contentOffset = Vector2.zero;
                    _toolbarSelected.imagePosition = ImagePosition.ImageOnly;
                    _toolbarSelected.normal.background = GetBackgroundTexture(toolbarSelectedColor);
                    _toolbarSelected.normal.textColor = Color.white;
                    _toolbarSelected.hover.background = GetBackgroundTexture(toolbarSelectedHoverColor);
                    _toolbarSelected.hover.textColor = Color.white;
                }
                return _toolbarSelected;
            }
        }

        #endregion

        #region Search Field

        public static GUIStyle SearchField
        {
            get
            {
                if (_searchField == null)
                {
                    _searchField = new GUIStyle(EditorStyles.toolbarSearchField);
                    _searchField.stretchHeight = true;
                    _searchField.fixedHeight = 0f;
                }
                return _searchField;
            }
        }

        #endregion

        #endregion

        #region Setup Functions

        public static void RefreshStyles()
        {
            //Background
            _editorContainer = null;
            _elementContainer = null;

            //Text style
            _textLeft = null;
            _textCentered = null;
            _headerLeft = null;
            _headerCentered = null;

            //Button style
            _button = null;

            //Message box preferences
            _messagebox = null;
            _messageboxHeader = null;
            _messageboxText = null;
            _messageboxButton = null;

            //Icon button preferences
            _menuElement = null;
            _menuElementImage = null;
            _menuElementText = null;
            _menuElementHeader = null;

            //Toolbar style
            _toolbar = null;
            _toolbarSelected = null;
            ToolbarIcons = new Texture2D[4];
            ToolbarIcons[0] = Resources.Load<Texture2D>($"{ICON_ROOT}{toolbarIconPaths[0]}");
            ToolbarIcons[1] = Resources.Load<Texture2D>($"{ICON_ROOT}{toolbarIconPaths[1]}");
            ToolbarIcons[2] = Resources.Load<Texture2D>($"{ICON_ROOT}{toolbarIconPaths[2]}");
            ToolbarIcons[3] = Resources.Load<Texture2D>($"{ICON_ROOT}{toolbarIconPaths[3]}");

            //Search field preferences
            _searchField = null;
        }

        #endregion

        #endregion

        #region Utility

        private static Texture2D GetBackgroundTexture(Color color)
        {

            var result = new Texture2D(1, 1);
            result.SetPixels(new Color[] { color });
            result.Apply();
            return result;
        }

        #endregion

    }
}