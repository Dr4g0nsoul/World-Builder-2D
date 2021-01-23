using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{
    [CustomNodeGraphEditor(typeof(WorldEditorGraph))]
    public class WorldEditorGraphEditor : NodeGraphEditor
    {

        private static readonly string s_LevelScenePath = "Assets/Scenes/Levels";


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
                    lNode.assignedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    lNode.assignedScene.name = $"Level_{lNode.guid}";
                    Util.EditorUtility.CreateFolders(s_LevelScenePath);
                    EditorSceneManager.SaveScene(lNode.assignedScene, $"{s_LevelScenePath}/{lNode.assignedScene.name}.unity");
                    EditorSceneManager.CloseScene(lNode.assignedScene, true);
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
    }
}
