using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CustomEditor(typeof(LevelInstance))]
    public class LevelInstanceEditor : Editor
    {

        private LevelNode level;
        private SerializedObject levelSearializedObject;

        //Tabs
        private int menuSelection;
        private readonly string[] menuLabels = new string[] { "Overview", "Level Exits" };

        //Rect Gizmo
        private bool editSizeMode;
        private readonly Color editSizeFillColor = new Color(0.4f, 0.9f, .7f);
        private readonly Color editSizeBorderColor = new Color(0.298f, 0.847f, 1f);
        private Rect newBounds;

        //Position Gizmo
        private readonly Color editPositionColor = new Color(0.486f, 0.227f, 0.929f);
        private Vector2 newPosition;

        //Overview Tab
        private AnimBool showDebugFields;

        //Level exit tab
        private ReorderableList levelExits;
        private int lastSelectedExit;

        private void OnEnable()
        {
            //Initialize variables
            level = null;
            levelSearializedObject = null;

            menuSelection = 0;

            editSizeMode = false;
            showDebugFields = new AnimBool(false);

            lastSelectedExit = -2;

            LevelEditorStyles.RefreshStyles();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void SetupLevelExitList()
        {
            levelExits = new ReorderableList(levelSearializedObject, levelSearializedObject.FindProperty("levelExits"), false, true, true, true);

            levelExits.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Level Exits");
            };

            levelExits.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty item = levelExits.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(rect, levelExits.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue);
            };

            levelExits.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize += 1;
                SerializedProperty addedObj = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                addedObj.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();
                addedObj.FindPropertyRelative("name").stringValue = $"Exit {list.serializedProperty.arraySize}";
                addedObj.FindPropertyRelative("active").boolValue = true;
                Vector2 pos = SceneView.lastActiveSceneView != null ? (Vector2)SceneView.lastActiveSceneView.camera.transform.position : Vector2.zero;
                addedObj.FindPropertyRelative("levelExitTrigger").rectValue = new Rect(pos, Vector2.one);
                addedObj.FindPropertyRelative("entryPoint").vector2Value = pos;
            };

            levelExits.onRemoveCallback = (ReorderableList list) =>
            {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            };

            levelExits.onSelectCallback = (ReorderableList list) =>
            {
                editSizeMode = false;
                if (lastSelectedExit == list.index)
                {
                    list.index = -1;
                    lastSelectedExit = -2;
                }
                else
                {
                    newPosition = levelExits.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("entryPoint").vector2Value;
                    lastSelectedExit = list.index;
                }
            };
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
                    SetupLevelExitList();
                }

                levelSearializedObject.Update();

                //Menu bar
                int lastMenuSelection = menuSelection;
                menuSelection = GUILayout.Toolbar(menuSelection, menuLabels);
                if(lastMenuSelection != menuSelection)
                {
                    editSizeMode = false;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(menuLabels[menuSelection], LevelEditorStyles.TextCentered);
                LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(10, 10, 3, 15));

                //Content
                if (menuSelection != 1 && lastSelectedExit > -2)
                {
                    levelExits.index = -1;
                    lastSelectedExit = -2;
                }
                switch(menuSelection)
                {
                    case 0:
                        DrawGeneralInformation();
                        break;
                    case 1:
                        DrawLevelExitEditor();
                        break;
                }

                EditorGUILayout.EndVertical();

                //Apply modifications
                levelSearializedObject.ApplyModifiedProperties();
                
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            switch(menuSelection)
            {
                case 0:
                    UpdateBoundsGizmo();
                    break;
                case 1:
                    //Draw all level exits
                    if(lastSelectedExit < -1)
                    {
                        for(int i = 0; i < levelExits.count; i++)
                        {
                            Util.EditorUtility.DrawRect(levelExits.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("levelExitTrigger").rectValue, editSizeBorderColor, editSizeFillColor);
                        }
                    }
                    else
                    {
                        UpdateBoundsGizmo();
                        Handles.color = editPositionColor;
                        newPosition = Handles.FreeMoveHandle(newPosition, Quaternion.identity, 0.7f, Vector2.zero, Handles.ConeHandleCap);
                        Handles.DrawDottedLine(newBounds.position, newPosition, 5f);
                    }
                    break;
            }
            Handles.color = Color.white;
            sceneView.Repaint();
        }

        #region GUI

        private void DrawGeneralInformation()
        {
            //General information
            EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("levelName"));
            EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("levelDescription"));

            //World boundaries
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Level Bounds", LevelEditorStyles.TextCentered);
            EditorGUILayout.Space(3f);
            SerializedProperty levelBounds = levelSearializedObject.FindProperty("levelBoundaries");
            EditBounds(levelBounds);
            

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
        }

        #region Level Exits

        private void DrawLevelExitEditor()
        {
            EditorGUI.BeginChangeCheck();

            //Level Exit selection
            levelExits.DoLayoutList();

            if(levelExits.index >= 0 && levelExits.index < levelExits.serializedProperty.arraySize)
            {
                SerializedProperty currLevelExit = levelExits.serializedProperty.GetArrayElementAtIndex(levelExits.index);
                DrawLevelExit(currLevelExit);
            }

            if(EditorGUI.EndChangeCheck())
            {
                levelSearializedObject.FindProperty("levelExitsUpdated").boolValue = true;
            }

            //Debug information
            EditorGUILayout.Space(20f);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(levelSearializedObject.FindProperty("levelExitsUpdated"));
            GUI.enabled = true;
        }

        private void DrawLevelExit(SerializedProperty currLevelExit)
        {
            //Level Entry
            EditorGUILayout.LabelField("Level Entry Properties", LevelEditorStyles.TextCentered);
            LevelEditorStyles.DrawHorizontalLine(Color.grey, new RectOffset(50, 50, 3, 15));
            //Level entry position
            SerializedProperty entryPoint = currLevelExit.FindPropertyRelative("entryPoint");
            if(entryPoint.vector2Value != newPosition)
            {
                levelSearializedObject.FindProperty("levelExitsUpdated").boolValue = true;
            }
            entryPoint.vector2Value = newPosition;
            EditorGUILayout.PropertyField(entryPoint);
            newPosition = entryPoint.vector2Value;


            //Level Exit
            EditorGUILayout.LabelField("Level Exit Properties", LevelEditorStyles.TextCentered);
            LevelEditorStyles.DrawHorizontalLine(Color.grey, new RectOffset(50, 50, 3, 15));

            EditorGUILayout.PropertyField(currLevelExit.FindPropertyRelative("guid"));
            EditorGUILayout.PropertyField(currLevelExit.FindPropertyRelative("active"));
            EditorGUILayout.PropertyField(currLevelExit.FindPropertyRelative("name"));

            if(EditBounds(currLevelExit.FindPropertyRelative("levelExitTrigger")))
            {
                levelSearializedObject.FindProperty("levelExitsUpdated").boolValue = true;
            }
        }

        #endregion

        #region GUI Utility

        /// <summary>
        /// Changes Bounds of an Object
        /// </summary>
        /// <param name="bounds">The Rect serialized property to change</param>
        /// <returns>Wheter bounds where saved or not</returns>
        private bool EditBounds(SerializedProperty bounds)
        {
            if (editSizeMode)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(bounds);
                GUI.enabled = true;
                if (GUILayout.Button("Save bounds", EditorStyles.miniButton))
                {
                    bounds.rectValue = newBounds;
                    editSizeMode = false;
                    EditorUtility.SetDirty(target);
                    return true;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(bounds);
                newBounds = bounds.rectValue;
                if (GUILayout.Button("Edit with gizmo", EditorStyles.miniButton))
                {
                    newBounds = bounds.rectValue;
                    newBounds.size = Vector2.Max(newBounds.size, Vector2.one);
                    editSizeMode = true;
                }
            }
            return false;
        }

        private void UpdateBoundsGizmo()
        {
            Rect tempBounds = Util.EditorUtility.ResizeRect(newBounds, Handles.SphereHandleCap, Handles.DotHandleCap, editSizeBorderColor, editSizeFillColor, .4f, 0f, editSizeMode);
            if (tempBounds.size != Vector2.zero)
            {
                newBounds = tempBounds;
                newBounds.size = Vector2.Max(Vector2.one, newBounds.size);
            }
        }

        #endregion

        #endregion
    }
}