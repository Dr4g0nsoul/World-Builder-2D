using UnityEngine;
using UnityEditor;

/// <summary>
/// Scene preview data.
/// http://diegogiacomelli.com.br/unitytips-scene-preview-window
/// </summary>
public class ScenePreviewData
{
    Texture2D _texture;

    public ScenePreviewData(string scenePath, string texturePath)
    {
        ScenePath = scenePath;
        TexturePath = texturePath;
    }

    public string ScenePath { get; private set; }
    public string TexturePath { get; private set; }
    public Texture2D Texture
    {
        get
        {
            return _texture ?? (_texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));
        }
    }
}