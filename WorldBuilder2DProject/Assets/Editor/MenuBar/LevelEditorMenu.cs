using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor {
    public class LevelEditorMenu
    {
        [MenuItem("Level Editor/Scene View Toggle Fullscreen _F11")]
        public static void SceneViewToggleFullscreen()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.maximized = !SceneView.lastActiveSceneView.maximized;
            }
        }

        [MenuItem("Level Editor/Open Level Editor _SPACE")]
        public static void OpenLevelEditor()
        {
            if (EditorTools.activeToolType != typeof(LevelEditorTool))
            {
                EditorTools.SetActiveTool<LevelEditorTool>();
            }
            if (EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.Focus();
                LevelEditorTool.LevelEditorToolInstance.OpenLevelObjectDrawer();
            }
        }
    }
}
