using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;
using XNodeEditor;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{
    [CustomNodeGraphEditor(typeof(WorldEditorGraph))]
    public class WorldEditorGraphEditor : NodeGraphEditor
    {

        //General
        private GUISkin skin;
        private Texture2D gridTexture;

        //Selection Inspector
        public const float INSPECTOR_RECT_WIDTH = 360f;
        private INodeEditorInspector nodeEditorWithInspector;
        private readonly Vector2 inspectorRectMargin = new Vector2(10f, 20f);
        private bool blockMouse = false;


        public override Color GetPortColor(NodePort port)
        {
            return Color.blue;
        }

        public override Texture2D GetGridTexture()
        {
            if(gridTexture == null)
            {
                gridTexture = new Texture2D(0, 0);
            }
            return gridTexture;
        }

        public override Node CreateNode(Type type, Vector2 position)
        {
            Node createdNode = base.CreateNode(type, position);

            //Add GUID
            UniqueNode uNode = createdNode as UniqueNode;
            if (uNode != null)
            {
                uNode.GenerateGUID();
                createdNode = uNode;

                //Create Level Scene
                LevelNode lNode = createdNode as LevelNode;
                if (lNode != null)
                {
                    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    lNode.assignedSceneName = $"Level_{lNode.guid}";
                    lNode.assignedScenePath = $"{LevelController.LEVEL_SCENE_LOCATION}/{lNode.assignedSceneName}.unity";

                    Util.EditorUtility.CreateFolders(LevelController.LEVEL_SCENE_LOCATION);
                    EditorSceneManager.SaveScene(newScene, lNode.assignedScenePath);
                    EditorSceneManager.CloseScene(newScene, true);
                }
            }

            //Show rename popup
            RenamePopup.Show(createdNode);

            return createdNode;
        }

        public override void RemoveNode(Node node)
        {
            base.RemoveNode(node);
        }

        public override void OnGUI()
        {
            GUI.skin = skin;
            // Keep repainting the GUI of the active NodeEditorWindow
            NodeEditorWindow.current.Repaint();

            Vector2 windowSize = NodeEditorWindow.current.position.size;
            Rect inspectorRect = new Rect();
            inspectorRect.position = new Vector2(windowSize.x - inspectorRectMargin.x - INSPECTOR_RECT_WIDTH, inspectorRectMargin.y);
            inspectorRect.size = new Vector2(INSPECTOR_RECT_WIDTH, windowSize.y - inspectorRectMargin.y * 2f);


            if(Event.current.type == EventType.Layout)
            {
                nodeEditorWithInspector = null;
                if (Selection.objects.Length < 2)
                {
                    Node node = Selection.activeObject as Node;
                    if (node != null)
                    {
                        if (NodeEditor.GetEditor(node, NodeEditorWindow.current) is INodeEditorInspector nodeEditorWithInspector)
                        {
                            this.nodeEditorWithInspector = nodeEditorWithInspector;
                        }
                    }
                }
            }

            if(nodeEditorWithInspector != null)
            {
                GUILayout.BeginArea(inspectorRect, NodeEditorResources.styles.propertyBox);
                GUILayout.BeginVertical();
                nodeEditorWithInspector.OnNodeInspectorGUI();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                if (Event.current.type == EventType.Repaint)
                {
                    NodeEditorWindow.current.enableInput = !inspectorRect.Contains(Event.current.mousePosition);
                }

                if(blockMouse && Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                }
            }
            else
            {
                NodeEditorWindow.current.enableInput = true;
            }
        }

        public override void OnOpen()
        {
            skin = Resources.Load<GUISkin>("WorldEditor/Skin/WESkin");
            WorldEditorStyles.RefreshStyles();
        }

    }
}
