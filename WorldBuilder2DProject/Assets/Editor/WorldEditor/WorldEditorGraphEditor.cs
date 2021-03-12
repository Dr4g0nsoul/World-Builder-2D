using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public const float INSPECTOR_RECT_WIDTH = 370f;
        private INodeEditorInspector nodeEditorWithInspector;
        public INodeEditorInspector worldInspector = null;
        private readonly Vector2 inspectorRectMargin = new Vector2(10f, 20f);
        private float inspectorScrollPos = 0f;
        private bool blockMouse = false;
        public bool IsMouseOverInspector { get => _isMouseOverInspector; }
        private bool _isMouseOverInspector = false;
        public bool resizingWorldBox = false;
        private bool mouseUp = false;


        public override Color GetPortColor(NodePort port)
        {
            Color col = NodeEditorPreferences.GetTypeColor();

            //Check wheter it is a level node
            LevelNode lNode = port.node as LevelNode;
            if(lNode != null)
            {
                WorldNode world = LevelController.Instance.GetWorldByLevel(lNode.guid);
                if (world != null)
                    return new Color(world.accentColor.r, world.accentColor.g, world.accentColor.b);
            }

            return col;
        }

        public override Color GetTypeColor(Type type)
        {
            return Color.white;
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

        /// <summary>
        /// Disables remove
        /// </summary>
        /// <param name="node">Doesn't matter</param>
        public override void RemoveNode(Node node)
        {
            List<Node> nodesToDelete = new List<Node>();
            UnityEngine.Object[] selection = Selection.objects;
            foreach(UnityEngine.Object obj in selection)
            {
                Node selectedNode = obj as Node;
                if(selectedNode != null)
                {
                    nodesToDelete.Add(selectedNode);
                }
            }
            if(!nodesToDelete.Contains(node))
            {
                nodesToDelete.Add(node);
            }

            WorldEditorDeletePopup.Show(nodesToDelete.ToArray());
        }

        public override bool CanRemove(Node node)
        {
            return true;
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

            //Mouse up and down
            if (Event.current.type == EventType.MouseUp)
            {
                mouseUp = true;
            }
            else if (Event.current.type == EventType.Used || Event.current.type == EventType.MouseDown)
            {
                if (!inspectorRect.Contains(Event.current.mousePosition))
                {
                    worldInspector = null;
                    mouseUp = false;
                }
            }

            if(Event.current.type == EventType.Layout)
            {
                nodeEditorWithInspector = null;
                if (mouseUp)
                {
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
                    else if (worldInspector != null)
                    {
                        nodeEditorWithInspector = worldInspector;
                    }
                }
            }

            if(nodeEditorWithInspector != null)
            {
                GUILayout.BeginArea(inspectorRect, NodeEditorResources.styles.propertyBox);
                inspectorScrollPos = GUILayout.BeginScrollView(new Vector2(0f, inspectorScrollPos), false, false).y;
                GUILayout.BeginVertical();
                nodeEditorWithInspector.OnNodeInspectorGUI();
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                if (Event.current.type == EventType.Repaint)
                {
                    _isMouseOverInspector = inspectorRect.Contains(Event.current.mousePosition);
                    NodeEditorWindow.current.enableInput = !_isMouseOverInspector;
                }

                if(blockMouse && Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                }
            }
            else if(!resizingWorldBox)
            {
                NodeEditorWindow.current.enableInput = true;
                _isMouseOverInspector = false;
                inspectorScrollPos = 0f;
            }
        }

        public override void OnOpen()
        {
            skin = Resources.Load<GUISkin>("WorldEditor/Skin/WESkin");
            WorldEditorStyles.RefreshStyles();
        }

    }
}
