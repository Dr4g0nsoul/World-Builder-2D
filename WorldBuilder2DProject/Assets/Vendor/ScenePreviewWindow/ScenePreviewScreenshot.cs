using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene preview screenshot.
/// http://diegogiacomelli.com.br/unitytips-scene-preview-window
/// </summary>
public static class ScenePreviewScreenshot
{
    public static bool Capture(bool force = true)
    {
        var previewPath = ScenePreviewUtility.GetPreviewPath(SceneManager.GetActiveScene().name);

        if (force || !File.Exists(previewPath))
        {
            var cam = Camera.main;

            if (cam == null)
            {
                cam = UnityEngine.Object.FindObjectOfType<Camera>();

                if (cam == null)
                {
                    Debug.LogError("There is no Camera on the scene. Cannot take a screenshot.");
                    return false;
                }
            }

            var size = GetGameWindowSize();
            var width = (int)size.x;
            var height = (int)size.y;

            cam.targetTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture.active = cam.targetTexture;

            cam.Render();

            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            cam.targetTexture = null;
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            Debug.Log(previewPath);
            File.WriteAllBytes(previewPath, bytes);

            AssetDatabase.ImportAsset(previewPath);

            return true;
        }

        return false;
    }

    // http://answers.unity.com/answers/192818/view.html
    static Vector2 GetGameWindowSize()
    {
        Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
        MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
        return (Vector2)GetSizeOfMainGameView.Invoke(null, null);
    }
}