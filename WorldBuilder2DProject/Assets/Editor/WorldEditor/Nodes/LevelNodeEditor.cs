using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(LevelNode))]
public class LevelNodeEditor : NodeEditor
{
    private static readonly int sizeMultiplier = 30;
    private static readonly int minWidth = 5;

    private Rect thumbnailRect;


    #region Initialization

    public override void OnCreate()
    {
        base.OnCreate();
        RebuildLevelExitConnections();
    }


    public override void AddContextMenuItems(GenericMenu menu)
    {
        bool canRemove = false;
        // Actions if only one node is selected
        if (Selection.objects.Length == 1 && Selection.activeObject is LevelNode)
        {
            XNode.Node node = Selection.activeObject as XNode.Node;
            // Add open level action
            menu.AddItem(new GUIContent("Open Level"), false, () => OpenCurrentLevel());
            // Add Rename
            menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);
            // Add Move to Top
            menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));

            canRemove = NodeGraphEditor.GetEditor(node.graph, NodeEditorWindow.current).CanRemove(node);
        }
    }

    #endregion

    #region GUI

    public override void OnHeaderGUI()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode == null || string.IsNullOrEmpty(lNode.levelName))
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        else
            GUILayout.Label(lNode.levelName, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));

    }

    public override void OnBodyGUI()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode != null) {
            serializedObject.Update();

            //Thumbnail
            Texture2D thumbnail = LevelController.Instance.GetLevelThumbnail(lNode.guid);
            

            if (thumbnail != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(1f);
                GUILayout.BeginVertical(WorldEditorStyles.GetThumbnailStyle(LevelController.Instance.GetLevelThumbnail(lNode.guid), lNode.levelBoundaries.height * sizeMultiplier));
                GUILayout.Space(lNode.levelBoundaries.height * sizeMultiplier);
                GUILayout.EndVertical();
                if(Event.current.type == EventType.Repaint)
                {
                    thumbnailRect = GUILayoutUtility.GetLastRect();
                }
                GUILayout.Space(1f);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10f);
                GUILayout.Label("[No thumbnail found]", WorldEditorStyles.TextCentered);
                GUILayout.Space(10f);
            }

            //Level Exits
            if(Event.current.type == EventType.Layout && serializedObject.FindProperty("levelExitsUpdated").boolValue)
            {
                serializedObject.FindProperty("levelExitsUpdated").boolValue = false;
                RebuildLevelExitConnections();
            }
            //Draw Level Exits
            foreach (LevelExit exit in lNode.levelExits)
            {
                NodePort levelExitPort = lNode.GetPort(exit.guid);
                if(levelExitPort != null)
                    NodeEditorGUILayout.PortField(WorldToNodePosition(exit.entryPoint, lNode, thumbnailRect), levelExitPort);
            }

            serializedObject.ApplyModifiedProperties();
        }
        /*
        LevelNode lNode = target as LevelNode;
        Rect r = new Rect()
        {
            position = Vector2.zero,
            size = new Vector2(16f, 16f)
        };
        NodePort port1 = null;
        foreach (NodePort port in lNode.DynamicPorts) {
            port1 = port;
            break;
        }
        NodeEditorGUILayout.PortField(new Vector2(50f, 50f), port1);
        */
    }

    public override int GetWidth()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode == null || lNode.levelBoundaries == null || lNode.levelBoundaries.size.x < minWidth)
        {
            return minWidth * sizeMultiplier;
        }
        return Mathf.RoundToInt(lNode.levelBoundaries.width * sizeMultiplier);
    }

    #endregion

    #region Context Menu Actions

    public void OpenCurrentLevel()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode != null && lNode.assignedScenePath != null && lNode.assignedScenePath.Length > 0)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene currScene = EditorSceneManager.GetSceneAt(i);

                if (!LevelEditorTool.IsLevelEditorInitialized)
                {
                    LevelEditorMenu.OpenLevelEditor();
                }

                if (LevelEditorUtility.GetObjectGuid(currScene.name) != null)
                    EditorSceneManager.CloseScene(currScene, true);
            }
             
            EditorSceneManager.OpenScene(lNode.assignedScenePath, OpenSceneMode.Additive);
            LevelEditorMenu.OpenLevelEditor();
        }
    }

    public override void OnRename()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode != null)
        {
            lNode.levelName = lNode.name;
        }
    }

    #endregion

    #region Level Exit Connections

    private void RebuildLevelExitConnections()
    {
        LevelNode lNode = target as LevelNode;

        if (lNode != null)
        {
            //Clear all dynamic ports which have no Level Exit
            List<string> unusedNodePorts = new List<string>();

            foreach(NodePort port in lNode.DynamicPorts)
            {
                string portGuid = LevelEditorUtility.GetObjectGuid(port.fieldName);
                bool unusedPort = true;
                foreach(LevelExit exit in lNode.levelExits)
                {
                    if(exit.guid == portGuid)
                    {
                        unusedPort = false;
                        break;
                    }
                }
                if(unusedPort)
                {
                    unusedNodePorts.Add(port.fieldName);
                }
            }

            foreach(string portFieldName in unusedNodePorts)
            {
                lNode.RemoveDynamicPort(portFieldName);
            }


            //Add missing dynamic ports
            foreach (LevelExit exit in lNode.levelExits)
            {
                NodePort levelPort = lNode.GetPort(exit.guid);

                if(levelPort == null)
                {
                    lNode.AddDynamicBoth(typeof(LevelNode), Node.ConnectionType.Override, Node.TypeConstraint.Strict, exit.guid);
                }
            }
            

            //Verify node connections
            lNode.VerifyConnections();
        }
    }

    private Vector2 WorldToNodePosition(Vector2 worldPos, LevelNode lNode, Rect thumbnailRect)
    {
        if(lNode != null)
        {
            Vector2 boundsUpperLeftCorner = new Vector2(lNode.levelBoundaries.position.x - (lNode.levelBoundaries.width/2f), lNode.levelBoundaries.position.y + (lNode.levelBoundaries.height / 2f));
            Vector2 posRelativeToUpperLeftCorner = new Vector2(worldPos.x - boundsUpperLeftCorner.x, boundsUpperLeftCorner.y - worldPos.y) / lNode.levelBoundaries.size;
            Vector2 nodePosition = posRelativeToUpperLeftCorner * thumbnailRect.size;
            return nodePosition + thumbnailRect.position;
        }
        return worldPos;
    }

    #endregion
}
