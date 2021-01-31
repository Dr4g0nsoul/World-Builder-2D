using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene preview utility.
/// http://diegogiacomelli.com.br/unitytips-scene-preview-window
/// </summary>
public static class ScenePreviewUtility
{
    public static ScenePreviewData[] Data { get; private set; } = new ScenePreviewData[0];
    public static bool ShowCaptureScreenshotButton { get; private set; }

    public static void RefreshTextures(ScenePreviewWindow window)
    {
        // Get the selected SceneAsset previews.
        var previewsPaths = Selection.objects
             .Where(asset => asset is SceneAsset)
             .Select(sceneAsset => new ScenePreviewData(AssetDatabase.GetAssetPath(sceneAsset), GetPreviewPath(((SceneAsset)sceneAsset).name)))
             .OrderBy(n => n.TexturePath)
             .ToList();

        if (previewsPaths.Count == 1)
            window.ShowTab();

        // If there no SceneAsset selected, then show the current scene preview.
        var activeScene = SceneManager.GetActiveScene();
        var activeScenePreviewPath = GetPreviewPath(activeScene.name);

        if (previewsPaths.Count == 0)
            previewsPaths.Add(new ScenePreviewData(activeScene.path, activeScenePreviewPath));

        ShowCaptureScreenshotButton = previewsPaths.Count == 1 && previewsPaths[0].TexturePath == activeScenePreviewPath;

        // Loads the previews textures.
        Data = previewsPaths
                .Distinct()
                .Where(item => item.Texture != null)
                .ToArray();

        window.Repaint();
    }

    public static string GetPreviewPath(string sceneName = null)
    {
        var folder = GetPreviewFolderRootedPath().Replace(Application.dataPath, string.Empty);
        folder = $"Assets{folder}";

        return String.IsNullOrEmpty(sceneName)
            ? folder
            : Path.Combine(folder, $"{sceneName}.png");

    }

    private static string GetPreviewFolderRootedPath([CallerFilePath] string callerFilePath = null)
    {
        var folder = Path.Combine(Path.GetDirectoryName(callerFilePath), "Previews");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return folder.Replace(@"\", "/");
    }
}