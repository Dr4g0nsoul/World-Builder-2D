using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor {
    public class LevelEditorMenu
    {
        [MenuItem("Tools/Level Editor/Scene View Toggle Fullscreen _F11")]
        public static void SceneViewToggleFullscreen()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.maximized = !SceneView.lastActiveSceneView.maximized;
            }
        }

        [MenuItem("Tools/Level Editor/Open Level Editor _F10")]
        public static void OpenLevelEditor()
        {
            if (!Application.isPlaying)
            {
                if (ToolManager.activeToolType != typeof(LevelEditorTool))
                {
                    ToolManager.SetActiveTool<LevelEditorTool>();
                }
                if (EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
                {
                    SceneView.lastActiveSceneView.Focus();
                    LevelEditorTool.LevelEditorToolInstance.OpenLevelObjectDrawer();
                }
            }
        }
    }
}
