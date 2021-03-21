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
        private static readonly int smallFontSize = 12;
        private static readonly int bigFontSize = 28;

        //Editor Window
        private static GUIStyle _editorWindow;

        //Menu Bar
        private static GUIStyle _menuBar;
        private static GUIStyle _menuButtonCircle;
        private static GUIStyle _menuButtonCircleActive;
        private static GUIStyle _menuLabelCircle;
        private static GUIStyle _menuButtonSquare;

        //More Box
        private static GUIStyle _moreBox;

        //Buttons style
        public static readonly Color buttonHoverColor = new Color(0.298f, 0.847f, 1f);
        public static readonly Color buttonDangerColor = new Color(0.8739f, 0.0362f, 0.0798f);
        public static readonly Vector2 levelObjectPreviewImageOffset = new Vector2(8f, 8f);
        private static GUIStyle _button;
        private static GUIStyle _buttonActive;
        private static GUIStyle _buttonDangerActive;
        private static GUIStyle _levelObjectImage;
        private static GUIStyle _levelObjectPreviewImage;
        private static GUIStyle _levelObjectPreviewMiniImage;
        private static GUIStyle _levelObjectButton;
        private static GUIStyle _levelObjectButtonActive;

        //Search field preferences
        private static GUIStyle _searchFieldLeft;
        private static GUIStyle _searchFieldCenter;
        private static GUIStyle _searchFieldRigth;

        //Message box style
        private static readonly RectOffset messageBoxPadding = new RectOffset(10, 10, 10, 10);
        private static GUIStyle _messagebox;
        private static GUIStyle _messageboxHeader;
        private static GUIStyle _messageboxText;
        private static GUIStyle _messageboxButton;

        //Hover box style
        private static GUIStyle _hoverBox;
        private static GUIStyle _hoverBoxText;

        //Horizontal line
        private static GUIStyle horizontalLine;

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
                    _textLeft.alignment = TextAnchor.UpperLeft;
                    _textLeft.fontSize = smallFontSize;
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

        public static GUIStyle MenuButtonCircleActive
        {
            get
            {
                if (_menuButtonCircleActive == null)
                {
                    _menuButtonCircleActive = new GUIStyle(GUI.skin.customStyles[4]);
                    _menuButtonCircleActive.hover.background = _menuButtonCircleActive.active.background;
                    _menuButtonCircleActive.active.background = _menuButtonCircleActive.normal.background;
                    _menuButtonCircleActive.normal.background = _menuButtonCircleActive.hover.background;
                }
                return _menuButtonCircleActive;
            }

        }

        public static GUIStyle MenuLabelCircle
        {
            get
            {
                if (_menuLabelCircle == null)
                {
                    _menuLabelCircle = new GUIStyle(GUI.skin.customStyles[4]);
                    _menuLabelCircle.normal.background = null;
                    _menuLabelCircle.hover.background = null;
                    _menuLabelCircle.active.background = null;
                }
                return _menuLabelCircle;
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

        #region More Box

        public static GUIStyle MoreBox
        {
            get
            {
                if (_moreBox == null)
                {
                    _moreBox = new GUIStyle(GUI.skin.box);
                }
                return _moreBox;
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

        public static GUIStyle ButtonActive
        {
            get
            {
                if (_buttonActive == null)
                {
                    _buttonActive = new GUIStyle(GUI.skin.button);
                    _buttonActive.normal.textColor = buttonHoverColor;
                    _buttonActive.normal.background = TintTexture(buttonHoverColor, GUI.skin.button.active.background);
                    _buttonActive.hover.textColor = buttonHoverColor;
                    _buttonActive.hover.background = _buttonActive.normal.background;
                    _buttonActive.active.textColor = buttonHoverColor;
                    _buttonActive.active.background = _buttonActive.normal.background;
                }
                return _buttonActive;
            }
        }

        public static GUIStyle ButtonDangerActive
        {
            get
            {
                if (_buttonDangerActive == null)
                {
                    _buttonDangerActive = new GUIStyle(GUI.skin.button);
                    _buttonDangerActive.normal.textColor = buttonDangerColor;
                    _buttonDangerActive.normal.background = TintTexture(buttonDangerColor, GUI.skin.button.active.background);
                    _buttonDangerActive.hover.textColor = buttonDangerColor;
                    _buttonDangerActive.hover.background = TintTexture(buttonDangerColor, GUI.skin.button.hover.background);
                    _buttonDangerActive.active.textColor = buttonDangerColor;
                    _buttonDangerActive.active.background = TintTexture(buttonDangerColor, GUI.skin.button.normal.background);
                }
                return _buttonDangerActive;
            }
        }

        public static GUIStyle LevelObjectImage
        {
            get
            {
                if (_levelObjectImage == null)
                {
                    _levelObjectImage = new GUIStyle(GUI.skin.label);
                    _levelObjectImage.margin = new RectOffset(0, 0, 0, 0);
                    _levelObjectImage.padding = GUI.skin.customStyles[6].padding;
                    _levelObjectImage.alignment = TextAnchor.MiddleCenter;
                    _levelObjectImage.imagePosition = ImagePosition.ImageOnly;
                }
                return _levelObjectImage;
            }
        }

        public static GUIStyle LevelObjectPreviewImage
        {
            get
            {
                if (_levelObjectPreviewImage == null)
                {
                    _levelObjectPreviewImage = new GUIStyle(GUI.skin.label);
                    _levelObjectPreviewImage.margin = new RectOffset(0, 0, 0, 0);
                    _levelObjectPreviewImage.padding = new RectOffset(0, 0, 0, 0);
                    _levelObjectPreviewImage.alignment = TextAnchor.MiddleCenter;
                    _levelObjectPreviewImage.imagePosition = ImagePosition.ImageOnly;
                    _levelObjectPreviewImage.contentOffset = -levelObjectPreviewImageOffset;
                }
                return _levelObjectPreviewImage;
            }
        }

        public static GUIStyle LevelObjectPreviewMiniImage
        {
            get
            {
                if (_levelObjectPreviewMiniImage == null)
                {
                    _levelObjectPreviewMiniImage = new GUIStyle(EditorStyles.objectFieldThumb);
                    _levelObjectPreviewMiniImage.alignment = TextAnchor.MiddleCenter;
                    _levelObjectPreviewMiniImage.padding = new RectOffset(0, 0, 0, 0);
                    _levelObjectPreviewMiniImage.margin = new RectOffset(2, 4, 2, 2);
                    _levelObjectPreviewMiniImage.fixedHeight = 18f;
                    _levelObjectPreviewMiniImage.fixedWidth = 18f;
                    _levelObjectPreviewMiniImage.imagePosition = ImagePosition.ImageOnly;
                    //_levelObjectPreviewMiniImage.contentOffset = -levelObjectPreviewImageOffset;
                }
                return _levelObjectPreviewMiniImage;
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

        public static GUIStyle LevelObjectButtonActive
        {
            get
            {
                if (_levelObjectButtonActive == null)
                {
                    _levelObjectButtonActive = new GUIStyle(GUI.skin.customStyles[6]);
                    _levelObjectButtonActive.normal.background = _levelObjectButtonActive.active.background;
                    _levelObjectButtonActive.hover.background = _levelObjectButtonActive.active.background;
                }
                return _levelObjectButtonActive;
            }
        }

        #endregion

        #region Search Field

        public static GUIStyle SearchFieldLeft
        {
            get
            {
                if (_searchFieldLeft == null)
                {
                    _searchFieldLeft = GUI.skin.customStyles[9];
                }
                return _searchFieldLeft;
            }
        }

        public static GUIStyle SearchFieldRight
        {
            get
            {
                if (_searchFieldRigth == null)
                {
                    _searchFieldRigth = GUI.skin.customStyles[10];
                }
                return _searchFieldRigth;
            }
        }

        public static GUIStyle SearchFieldCenter
        {
            get
            {
                if (_searchFieldCenter == null)
                {
                    _searchFieldCenter = GUI.skin.customStyles[11];
                }
                return _searchFieldCenter;
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
                    _messageboxText.fontStyle = FontStyle.Normal;
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

        #region Hover Box

        
        public static GUIStyle HoverBox
        {
            get
            {
                if (_hoverBox == null)
                {
                    _hoverBox = new GUIStyle(GUI.skin.customStyles[12]);
                    _hoverBox.clipping = TextClipping.Overflow;
                }
                return _hoverBox;
            }
        }

        public static GUIStyle HoverBoxText
        {
            get
            {
                if (_hoverBoxText == null)
                {
                    _hoverBoxText = new GUIStyle(GUI.skin.label);
                    _hoverBoxText.alignment = TextAnchor.MiddleCenter;
                    _hoverBoxText.wordWrap = false;
                    _hoverBoxText.stretchWidth = true;
                    _hoverBoxText.stretchHeight = true;
                    _hoverBoxText.clipping = TextClipping.Overflow;
                }
                return _hoverBoxText;
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
            _menuButtonCircleActive = null;
            _menuLabelCircle = null;
            _menuButtonSquare = null;

            //More Box
            _moreBox = null;

            //Buttons style
            _button = null;
            _buttonActive = null;
            _buttonDangerActive = null;
            _levelObjectImage = null;
            _levelObjectPreviewImage = null;
            _levelObjectPreviewMiniImage = null;
            _levelObjectButton = null;
            _levelObjectButtonActive = null;

            //Search field preferences
            _searchFieldLeft = null;
            _searchFieldRigth = null;
            _searchFieldCenter = null;

            //Message box style
            _messagebox = null;
            _messageboxHeader = null;
            _messageboxText = null;
            _messageboxButton = null;

            //Hover box
            _hoverBox = null;
            _hoverBoxText = null;

            //Horizontal Line
            horizontalLine = null;
        }

        #endregion

        #endregion

        #region Utility

        public static Texture2D GetBackgroundTexture(Color color)
        {

            var result = new Texture2D(1, 1);
            result.SetPixels(new Color[] { color });
            result.Apply();
            return result;
        }

        public static Texture2D TintTexture(Color color, Texture2D srcTexture)
        {
            if (srcTexture != null)
            {
                Texture2D destTexture = new Texture2D(srcTexture.width, srcTexture.height, srcTexture.format, false);
                Color[] pixels = srcTexture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] *= color;
                }
                destTexture.SetPixels(pixels);
                destTexture.Apply();
                return destTexture;
            }
            return null;
        }

        public static void DrawHorizontalLine(Color color, RectOffset margin = null)
        {
            if(horizontalLine == null)
            {
                horizontalLine = new GUIStyle();
                horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
                horizontalLine.fixedHeight = 1;
                horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            }

            var c = GUI.color;
            GUI.color = color;
            if (margin != null)
            {
                GUIStyle customHorizontalLine = new GUIStyle(horizontalLine);
                customHorizontalLine.margin = margin;
                GUILayout.Box(GUIContent.none, customHorizontalLine);
            }
            else
            {
                GUILayout.Box(GUIContent.none, horizontalLine);
            }
            GUI.color = c;

            //horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        }

        public static Texture2D ResampleAndCrop(Texture2D source, int targetWidth, int targetHeight)
        {
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            float sourceAspect = (float)sourceWidth / sourceHeight;
            float targetAspect = (float)targetWidth / targetHeight;
            int xOffset = 0;
            int yOffset = 0;
            float factor = 1;
            if (sourceAspect > targetAspect)
            { // crop width
                factor = (float)targetHeight / sourceHeight;
                xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
            }
            else
            { // crop height
                factor = (float)targetWidth / sourceWidth;
                yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
            }
            Color32[] data = source.GetPixels32();
            Color32[] data2 = new Color32[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    var p = new Vector2(Mathf.Clamp(xOffset + x / factor, 0, sourceWidth - 1), Mathf.Clamp(yOffset + y / factor, 0, sourceHeight - 1));
                    // bilinear filtering
                    var c11 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                    var c12 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                    var c21 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                    var c22 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                    var f = new Vector2(Mathf.Repeat(p.x, 1f), Mathf.Repeat(p.y, 1f));
                    data2[x + y * targetWidth] = Color.Lerp(Color.Lerp(c11, c12, p.y), Color.Lerp(c21, c22, p.y), p.x);
                }
            }

            var tex = new Texture2D(targetWidth, targetHeight);
            tex.SetPixels32(data2);
            tex.Apply(true);
            return tex;
        }

        /*
        public static Texture2D CropTexture(Texture2D tex, Rect crop)
        {
            if (tex != null)
            {
                //Get readable copy of texture
                RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height);
                Graphics.Blit(tex, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D tmpTex = new Texture2D(tex.width, tex.height);
                tmpTex.ReadPixels(crop, 0, 0);
                tmpTex.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);



                Color[] pixels = tmpTex.GetPixels();
                Texture2D croppedTexture = new Texture2D((int)crop.width, (int)crop.height);
                croppedTexture.SetPixels(pixels);
                croppedTexture.Apply();
                return croppedTexture;
            }
            return null;
        }
        */

        public static Texture2D TextureFromSprite(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                //Get readable copy of texture
                RenderTexture tmp = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);
                tmp.filterMode = sprite.texture.filterMode;

                Graphics.Blit(sprite.texture, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D tmpTex = new Texture2D(sprite.texture.width, sprite.texture.height);
                tmpTex.ReadPixels(new Rect(0, 0, sprite.texture.width, sprite.texture.height), 0, 0);
                tmpTex.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                Color[] newColors = tmpTex.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.filterMode = sprite.texture.filterMode;
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }

        #endregion

    }
}