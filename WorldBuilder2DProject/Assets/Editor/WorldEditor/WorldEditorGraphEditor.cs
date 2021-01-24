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

        private static readonly string s_LevelScenePath = "Assets/Scenes/Levels";
        private Texture2D gridTexture;


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
                    lNode.assignedScenePath = $"{s_LevelScenePath}/{lNode.assignedSceneName}.unity";

                    Util.EditorUtility.CreateFolders(s_LevelScenePath);
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
    }
}
