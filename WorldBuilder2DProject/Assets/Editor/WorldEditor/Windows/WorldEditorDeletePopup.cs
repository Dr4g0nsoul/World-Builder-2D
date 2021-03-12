using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{
    public class WorldEditorDeletePopup : EditorWindow
    {
        public static WorldEditorDeletePopup current { get; private set; }
        public Node[] targets;

        private bool firstFrame = true;

        /// <summary> Show a delete popup for one or multiple nodes.
        public static WorldEditorDeletePopup Show(params Node[] targets)
        {
            WorldEditorDeletePopup window = null;
            if (targets.Length > 0) {
                string title = targets.Length == 1 ? $"Really remove {targets[0].name}?" : $"Really remove {targets.Length} elements?";

                window = EditorWindow.GetWindow<WorldEditorDeletePopup>(true, title, true);
                if (current != null) current.Close();
                current = window;
                window.targets = targets;
                window.minSize = new Vector2(350, 90);
                window.position = new Rect(0, 0, 350, 90);
                window.maxSize = new Vector2(350, 90);
                window.UpdatePositionToMouse();
            }
            return window;
        }

        private void UpdatePositionToMouse()
        {
            if (Event.current == null) return;
            Vector3 mousePoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Rect pos = position;
            pos.x = mousePoint.x - position.width * 0.5f;
            pos.y = mousePoint.y - 10;
            position = pos;
        }

        private void OnLostFocus()
        {
            // Make the popup close on lose focus
            Close();
        }

        private void OnGUI()
        {
            if (firstFrame)
            {
                UpdatePositionToMouse();
                firstFrame = false;
            }
            bool close = false;

            Event e = Event.current;
            string title = targets.Length == 1
                ? $"Do you really want to permanently delete {targets[0].name}?"
                : $"Do you really want to permanently delete {targets.Length} elements?";

            GUILayout.Space(15f);
            GUILayout.Label(title, LevelEditorStyles.TextCentered);
            GUILayout.Space(15f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            if(GUILayout.Button("Yes") || (e.isKey && e.keyCode == KeyCode.Return))
            {
                for(int i = targets.Length - 1; i >= 0; i--)
                {
                    NodeEditor editor = NodeEditor.GetEditor(targets[i], NodeEditorWindow.current);
                    if(editor.OnRemove())
                    {
                        NodeEditorWindow.current.graph.RemoveNode(targets[i]);
                        AssetDatabase.RemoveObjectFromAsset(targets[i]);
                    }

                }
                Undo.ClearAll();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                LevelController.Instance.LoadLevels();
                LevelController.Instance.LoadWorlds();
                close = true;
            }
            GUILayout.Space(30f);
            if(GUILayout.Button("No") || (e.isKey && e.keyCode == KeyCode.Escape))
            {
                close = true;
            }
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            GUILayout.Space(15f);

            if (close)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            EditorGUIUtility.editingTextField = false;
        }
    }
}
