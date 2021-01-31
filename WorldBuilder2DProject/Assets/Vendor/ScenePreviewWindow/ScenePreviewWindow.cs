using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Scene preview window.
/// http://diegogiacomelli.com.br/unitytips-scene-preview-window
/// </summary>
public class ScenePreviewWindow : EditorWindow
{
    const float EditorMargin = 1;
    const float PreviewMargin = 1;
    const int Columns = 2;
    const int CaptureScreenshotButtonHeight = 30;
    const float SecondsToAutoCaptureScreenshot = 10f;

    static GUIStyle _buttonStyle = new GUIStyle();
    static ScenePreviewWindow _instance;

    [MenuItem("Window/Scene Preview")]
    public static void ShowWindow()
    {
        _instance = EditorWindow.GetWindow<ScenePreviewWindow>("Scene Preview");
    }

    [InitializeOnLoadMethod]
    static void Initialize()
    {
        Selection.selectionChanged += HandleSelectionChange;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        EditorSceneManager.sceneOpened += HandleEditorSceneManagerSceneOpened;
    }

    void OnEnable()
    {
        _instance = this;
        _buttonStyle.normal.background = null;
    }

    void OnDestroy()
    {
        Selection.selectionChanged -= HandleSelectionChange;
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorSceneManager.sceneOpened -= HandleEditorSceneManagerSceneOpened;
        _instance = null;
    }

    static void HandleSelectionChange()
    {
        if (_instance != null)
        {
            ScenePreviewUtility.RefreshTextures(_instance);
        }
    }

    static void HandlePlayModeStateChanged(PlayModeStateChange playMode)
    {
        if (playMode == PlayModeStateChange.EnteredPlayMode && _instance != null)
        {
            EditorApplication.update += HandleUpdate;
            HandleSelectionChange();
        }
    }

    static void HandleEditorSceneManagerSceneOpened(Scene scene, OpenSceneMode mode)
    {
        HandleSelectionChange();
    }

    static void HandleUpdate()
    {
        if (Time.timeSinceLevelLoad > SecondsToAutoCaptureScreenshot)
        {
            EditorApplication.update -= HandleUpdate;

            if (_instance != null && ScenePreviewScreenshot.Capture(false))
                ScenePreviewUtility.RefreshTextures(_instance);
        }
    }

    void OnGUI()
    {
        var rect = new Rect(EditorMargin, EditorMargin, Screen.width, position.height - EditorMargin * 2 - PreviewMargin - CaptureScreenshotButtonHeight - EditorMargin);
        var data = ScenePreviewUtility.Data;

        if (data.Length > 0)
        {
            var previewsCount = data.Length;
            var height = (position.height - EditorMargin * 2 - (PreviewMargin * previewsCount) - CaptureScreenshotButtonHeight) / previewsCount;
            var index = 0;

            foreach (var item in data)
            {
                rect = new Rect(EditorMargin, index * (height + PreviewMargin), position.width, height);

                if (GUI.Button(rect, string.Empty, _buttonStyle))
                {
                    EditorSceneManager.OpenScene(item.ScenePath);
                }

                GUI.DrawTexture(rect, item.Texture, ScaleMode.ScaleToFit);

                index++;
            }
        }

        if (ScenePreviewUtility.ShowCaptureScreenshotButton &&
         GUI.Button(new Rect(EditorMargin, rect.yMax + PreviewMargin, position.width - (EditorMargin * 2), CaptureScreenshotButtonHeight), "Capture screenshot"))
        {
            ScenePreviewScreenshot.Capture();
            ScenePreviewUtility.RefreshTextures(this);
        }
    }
}