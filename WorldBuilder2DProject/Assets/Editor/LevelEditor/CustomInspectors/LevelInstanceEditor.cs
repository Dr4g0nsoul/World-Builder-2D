using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CustomEditor(typeof(LevelInstance))]
    public class LevelInstanceEditor : Editor
    {

        private LevelNode level;
        private SerializedObject levelSearializedObject;

        private bool editSizeMode;
        private readonly Color editSizeFillColor = new Color(0.4f, 0.9f, .7f);
        private readonly Color editSizeBorderColor = new Color(0.298f, 0.847f, 1f);
        private Rect newBounds;

        private AnimBool showDebugFields;

        private void OnEnable()
        {
            //Initialize variables
            level = null;
            levelSearializedObject = null;

            editSizeMode = false;
            showDebugFields = new AnimBool(false);
            showDebugFields.valueChanged.AddListener(Repaint);

            LevelEditorStyles.RefreshStyles();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            string levelGUID = serializedObject.FindProperty("level").stringValue;
            level = LevelController.Instance.GetLevel(levelGUID);
            

            if (level == null)
            {
                EditorGUILayout.HelpBox("Wrong level id. Please open the level editor and re-initialize the level", MessageType.Warning);
            }
            else
            {
                if (levelSearializedObject == null)
                {
                    levelSearializedObject = new SerializedObject(level);
                }
                levelSearializedObject.Update();

                DrawGeneralInformation();

                levelSearializedObject.ApplyModifiedProperties();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Rect tempBounds = Util.EditorUtility.ResizeRect(newBounds, Handles.SphereHandleCap, Handles.DotHandleCap, editSizeBorderColor, editSizeFillColor, .4f, 0f, editSizeMode);
            if (tempBounds.size != Vector2.zero)
            {
                newBounds = tempBounds;
                newBounds.size = Vector2.Max(Vector2.one, newBounds.size);
            }
        }

        #region GUI

        private void DrawGeneralInformation()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Overview", LevelEditorStyles.TextCentered);


            LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(10, 10, 3, 15));
            //General information
            EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("levelName"));
            EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("levelDescription"));

            //World boundaries
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Level Bounds", LevelEditorStyles.TextCentered);
            EditorGUILayout.Space(3f);
            SerializedProperty levelBounds = levelSearializedObject.FindProperty("levelBoundaries");
            if(editSizeMode)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(levelBounds);
                GUI.enabled = true;
                if(GUILayout.Button("Save gizmo position", EditorStyles.miniButton))
                {
                    levelBounds.rectValue = newBounds;
                    editSizeMode = false;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(levelBounds);
                newBounds = levelBounds.rectValue;
                if (GUILayout.Button("Edit with gizmo", EditorStyles.miniButton))
                {
                    newBounds = levelSearializedObject.FindProperty("levelBoundaries").rectValue;
                    editSizeMode = true;
                }
            }

            //Debug information
            EditorGUILayout.Space(20f);
            showDebugFields.target = EditorGUILayout.ToggleLeft("Show debug fields", showDebugFields.target);
            if (EditorGUILayout.BeginFadeGroup(showDebugFields.faded))
            {
                EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("guid"));
                EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("assignedSceneName"));
                EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("assignedScenePath"));
            }
            EditorGUILayout.EndFadeGroup();


            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}