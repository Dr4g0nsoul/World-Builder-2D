using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelEditorStyles
    {

        public const string ICON_ROOT = "LevelEditor/Skin/Icons/";

        //Text style
        private static GUIStyle _textLeft;
        private static GUIStyle _textCentered;
        private static GUIStyle _headerLeft;
        private static GUIStyle _headerCentered;
        private static GUIStyle _headerCenteredBig;
        private static readonly int bigFontSize = 28;

        //Editor Window
        private static GUIStyle _editorWindow;

        //Menu Bar
        private static GUIStyle _menuBar;
        private static GUIStyle _menuButtonCircle;
        private static GUIStyle _menuButtonSquare;

        //Buttons style
        public static readonly Color buttonHoverColor = new Color(0.298f, 0.847f, 1f);
        private static GUIStyle _button;
        private static GUIStyle _levelObjectButton;

        //Search field preferences
        private static GUIStyle _searchField;

        //Message box style
        private static readonly RectOffset messageBoxPadding = new RectOffset(10, 10, 10, 10);
        private static GUIStyle _messagebox;
        private static GUIStyle _messageboxHeader;
        private static GUIStyle _messageboxText;
        private static GUIStyle _messageboxButton;

        #region Style Setup

        #region Style Properties

        #region Text

        public static GUIStyle TextLeft
        {
            get
            {
                if (_textLeft == null)
                {
                    _textLeft = new GUIStyle(GUI.skin.label);
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
                    _textCentered = new GUIStyle(GUI.skin.label);
                    _textCentered.alignment = TextAnchor.UpperCenter;
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
                    _headerLeft = new GUIStyle(GUI.skin.label);
                    _headerLeft.fontStyle = FontStyle.Bold;
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
                    _headerCentered = new GUIStyle(GUI.skin.label);
                    _headerCentered.alignment = TextAnchor.UpperCenter;
                    _headerCentered.fontStyle = FontStyle.Bold;
                }
                return _headerCentered;
            }
        }

        public static GUIStyle HeaderCenteredBig
        {
            get
            {
                if (_headerCenteredBig == null)
                {
                    _headerCenteredBig = new GUIStyle(GUI.skin.label);
                    _headerCenteredBig.alignment = TextAnchor.UpperCenter;
                    _headerCenteredBig.fontStyle = FontStyle.Bold;
                    _headerCenteredBig.fontSize = bigFontSize;
                }
                return _headerCenteredBig;
            }
        }

        #endregion

        #region Scrollbar
        #endregion

        #region Editor Window

        public static GUIStyle EditorContainer
        {
            get
            {
                if (_editorWindow == null)
                {
                    _editorWindow = new GUIStyle(GUI.skin.window);
                }
                return _editorWindow;
            }
        }

        #endregion

        #region Menu Bar

        public static GUIStyle MenuBar
        {
            get
            {
                if (_menuBar == null)
                {
                    _menuBar = new GUIStyle(GUI.skin.customStyles[3]);
                }
                return _menuBar;
            }
            
        }

        public static GUIStyle MenuButtonCircle
        {
            get
            {
                if (_menuButtonCircle == null)
                {
                    _menuButtonCircle = new GUIStyle(GUI.skin.customStyles[4]);
                }
                return _menuButtonCircle;
            }

        }

        public static GUIStyle MenuButtonSquare
        {
            get
            {
                if (_menuButtonSquare == null)
                {
                    _menuButtonSquare = new GUIStyle(GUI.skin.customStyles[5]);
                }
                return _menuButtonSquare;
            }

        }

        #endregion

        #region Buttons

        public static GUIStyle Button
        {
            get
            {
                if (_button == null)
                {
                    _button = new GUIStyle(GUI.skin.button);
                    _button.hover.textColor = buttonHoverColor;
                    _button.hover.background = TintTexture(buttonHoverColor, GUI.skin.button.hover.background);
                    _button.active.textColor = buttonHoverColor;
                    _button.active.background = TintTexture(buttonHoverColor, GUI.skin.button.active.background);
                }
                return _button;
            }
        }

        public static GUIStyle LevelObjectButton
        {
            get
            {
                if (_levelObjectButton == null)
                {
                    _levelObjectButton = new GUIStyle(GUI.skin.customStyles[6]);
                }
                return _levelObjectButton;
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
                    _searchField = null;
                    //_searchField.stretchHeight = true;
                    //_searchField.fixedHeight = 0f;
                }
                return _searchField;
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
                    _messagebox = new GUIStyle(GUI.skin.box);
                    _messagebox.padding = messageBoxPadding;
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
                    _messageboxText = new GUIStyle(GUI.skin.label);
                    _messageboxText.alignment = TextAnchor.UpperCenter;
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
                    _messageboxHeader = new GUIStyle(GUI.skin.label);
                    _messageboxHeader.alignment = TextAnchor.MiddleCenter;
                    _messageboxHeader.stretchHeight = true;
                    _messageboxHeader.fontStyle = FontStyle.Bold;
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
                    _messageboxButton = new GUIStyle(Button);
                    _messageboxButton.alignment = TextAnchor.LowerCenter;
                }
                return _messageboxButton;
            }
        }

        #endregion

        #endregion

        #region Setup Functions

        public static void RefreshStyles()
        {
            //Text style
            _textLeft = null;
            _textCentered = null;
            _headerLeft = null;
            _headerCentered = null;
            _headerCenteredBig = null;

            //Editor Window
            _editorWindow = null;

            //Menu Bar
            _menuBar = null;
            _menuButtonCircle = null;
            _menuButtonSquare = null;

            //Buttons style
            _button = null;
            _levelObjectButton = null;

            //Search field preferences
            _searchField = null;

            //Message box style
            _messagebox = null;
            _messageboxHeader = null;
            _messageboxText = null;
            _messageboxButton = null;
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

        private static Texture2D TintTexture(Color color, Texture2D srcTexture)
        {
            Texture2D destTexture = new Texture2D(srcTexture.width, srcTexture.height, srcTexture.format, false);
            Color[] pixels = srcTexture.GetPixels();
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] *= color;
            }
            destTexture.SetPixels(pixels);
            destTexture.Apply();
            return destTexture;
        }

        #endregion

    }
}