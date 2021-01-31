using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldEditorStyles
{
    public const string ICON_ROOT = "LevelEditor/Skin/Icons/";

    //Node style
    private static GUIStyle _nodeTitle;
    private static GUIStyle _nodeBody;

    //Text style
    private static GUIStyle _textLeft;
    private static GUIStyle _textCentered;
    private static GUIStyle _headerLeft;
    private static GUIStyle _headerCentered;
    private static GUIStyle _headerCenteredBig;
    private static readonly int bigFontSize = 28;

    //Thumbnail
    private static GUIStyle _thumbnail;

    //Horizontal line
    private static GUIStyle horizontalLine;

    #region Style Setup

    #region Style Properties

    #region Node Style

    public static GUIStyle NodeTitle
    {
        get
        {
            if (_nodeTitle == null)
            {
                _nodeTitle = new GUIStyle(GUI.skin.label);
            }
            return _nodeTitle;
        }
    }

    public static GUIStyle NodeBody
    {
        get
        {
            if (_nodeBody == null)
            {
                _nodeBody = new GUIStyle(GUI.skin.label);
            }
            return _nodeBody;
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

    #region Thumbnail

    private static GUIStyle Thumbnail
    {
        get
        {
            if (_thumbnail == null)
            {
                _thumbnail = new GUIStyle(GUI.skin.label);
                _thumbnail.alignment = TextAnchor.MiddleCenter;
                _thumbnail.imagePosition = ImagePosition.ImageOnly;
                _thumbnail.margin = new RectOffset(0, 0, 0, 0);
                _thumbnail.border = new RectOffset();
                _thumbnail.padding = new RectOffset(0,0,0,0);
                _thumbnail.stretchWidth = true;
            }
            return _thumbnail;
        }
    }

    #endregion

    #endregion

    #region Setup Functions

    public static void RefreshStyles()
    {
        //Node style
        _nodeTitle = null;
        _nodeBody = null;

        //Text style
        _textLeft = null;
        _textCentered = null;
        _headerLeft = null;
        _headerCentered = null;
        _headerCenteredBig = null;

        //Thumbnail
        _thumbnail = null;

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

    public static void DrawHorizontalLine(Color color, RectOffset margin = null)
    {
        if (horizontalLine == null)
        {
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.fixedHeight = 1;
        }

        horizontalLine.margin = margin ?? new RectOffset(0, 0, 4, 4);

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;

        //horizontalLine.margin = new RectOffset(0, 0, 4, 4);
    }

    public static GUIStyle GetThumbnailStyle(Texture2D texture, float height = 100)
    {
        GUIStyle style = Thumbnail;
        if(style != null)
        {
            style.normal.background = texture;
            style.fixedHeight = height;
        }
        return style;
    }

    #endregion

}