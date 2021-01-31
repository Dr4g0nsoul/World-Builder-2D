using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(LevelNode))]
public class LevelNodeEditor : NodeEditor
{
    private static readonly int sizeMultiplier = 30;
    private static readonly int minWidth = 5;
    
    public override void OnCreate()
    {
        base.OnCreate();
        Debug.Log("HI");
        /*
        LevelNode lNode = target as LevelNode;
        if (lNode != null) {
            for (int i = 0; i < lNode.numberOfInputs; i++) 
            {
                lNode.AddDynamicInput(typeof(string), Node.ConnectionType.Override, Node.TypeConstraint.Strict, $"Input {i}");
            }
            for (int i = 0; i < lNode.numberOfOutputs; i++)
            {
                lNode.AddDynamicOutput(typeof(string), Node.ConnectionType.Override, Node.TypeConstraint.Strict, $"Output {i}");
            }
        }
        */
    }


    public override void AddContextMenuItems(GenericMenu menu)
    {
        // Add open level action
        if (Selection.objects.Length == 1 && Selection.activeObject is LevelNode)
        {
            XNode.Node node = Selection.activeObject as XNode.Node;
            menu.AddItem(new GUIContent("Open Level"), false, () => OpenCurrentLevel());
        }
        base.AddContextMenuItems(menu);
    }

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

            Texture2D thumbnail = LevelController.Instance.GetLevelThumbnail(lNode.guid);

            if (thumbnail != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(1f);
                GUILayout.BeginVertical(WorldEditorStyles.GetThumbnailStyle(LevelController.Instance.GetLevelThumbnail(lNode.guid), lNode.levelBoundaries.height * sizeMultiplier));
                GUILayout.Space(lNode.levelBoundaries.height * sizeMultiplier);
                GUILayout.EndVertical();
                GUILayout.Space(1f);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10f);
                GUILayout.Label("[No thumbnail found]", WorldEditorStyles.TextCentered);
                GUILayout.Space(10f);
            }
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

    public void OpenCurrentLevel()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode != null && lNode.assignedScenePath != null && lNode.assignedScenePath.Length > 0)
        {
            EditorSceneManager.OpenScene(lNode.assignedScenePath, OpenSceneMode.Additive);
            LevelEditorMenu.OpenLevelEditor();
        }
    }

    public override int GetWidth()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode == null || lNode.levelBoundaries == null || lNode.levelBoundaries.size.x < minWidth)
        {
            return minWidth * sizeMultiplier;
        }
        return Mathf.RoundToInt(lNode.levelBoundaries.width * sizeMultiplier);
    }

    public override void OnRename()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode != null)
        {
            lNode.levelName = lNode.name;
        }
    }
}
