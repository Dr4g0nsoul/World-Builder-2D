using System;
using System.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [CustomEditor(typeof(LevelEditorSettings))]
    public class LevelEditorSettingsEditor : Editor
    {
        private ReorderableList categoryList;
        private ReorderableList layerList;

        private int selectedListOption = 0;
        private readonly string[] listOptions = new string[]
        {
            "Categories",
            "Layers"
        };

        private GUIStyle textMiddle;
        private GUIStyle headerMiddle;

        private void OnEnable()
        {

            categoryList = new ReorderableList(serializedObject, serializedObject.FindProperty("levelObjectCategories"), true, true, true, true);

            categoryList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Categories");
            };

            categoryList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty item = serializedObject.FindProperty("levelObjectCategories").GetArrayElementAtIndex(index).FindPropertyRelative("item");
                EditorGUI.LabelField(rect, item.FindPropertyRelative("name").stringValue);
            };

            categoryList.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize += 1;
                SerializedProperty addedObj = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                addedObj.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();
                if (addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue.Length > 0)
                {
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue += " (Copy)";
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue =
                    list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 2).FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue;
                }
                else
                {
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue = "Unnamed Category";
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue = Color.white;
                }
            };

            layerList = new ReorderableList(serializedObject, serializedObject.FindProperty("levelLayers"), true, true, true, true);

            layerList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Layers");
            };

            layerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty item = serializedObject.FindProperty("levelLayers").GetArrayElementAtIndex(index).FindPropertyRelative("item");
                EditorGUI.LabelField(rect, item.FindPropertyRelative("name").stringValue);
            };

            layerList.onAddCallback = (ReorderableList list) =>
            {
                //Add a new object
                list.serializedProperty.arraySize += 1;
                SerializedProperty addedObj = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                addedObj.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();

                if (addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue.Length > 0)
                {
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue += " (Copy)";
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue =
                    list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 2).FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue;
                }
                else
                {
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("name").stringValue = "Unnamed Layer";
                    addedObj.FindPropertyRelative("item").FindPropertyRelative("accentColor").colorValue = Color.white;
                }


            };
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (textMiddle == null)
            {
                textMiddle = new GUIStyle(EditorStyles.label);
                textMiddle.alignment = TextAnchor.UpperCenter;

                headerMiddle = new GUIStyle(EditorStyles.boldLabel);
                headerMiddle.alignment = TextAnchor.UpperCenter;
            }


            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(20f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Categories and Layers", headerMiddle);

            serializedObject.Update();
            selectedListOption = GUILayout.Toolbar(selectedListOption, listOptions);

            EditorGUI.BeginChangeCheck();
            if (selectedListOption == 0)
            {
                categoryList.DoLayoutList();
                DrawCategoryProperty();
            }
            else if (selectedListOption == 1)
            {
                layerList.DoLayoutList();
                DrawLayerProperty();
            }
            if (EditorGUI.EndChangeCheck())
            {
                LevelObjectsController.Instance.ReloadSettingsCache();
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }


        private void DrawCategoryProperty()
        {
            if (categoryList.index >= 0 && categoryList.index < categoryList.serializedProperty.arraySize)
            {
                EditorGUILayout.LabelField("Category Properties", headerMiddle);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("levelObjectCategories").GetArrayElementAtIndex(categoryList.index).FindPropertyRelative("guid"));
                EditorGUILayout.EndVertical();
                
                DrawItemProperty(serializedObject.FindProperty("levelObjectCategories").GetArrayElementAtIndex(categoryList.index).FindPropertyRelative("item"));
            }
        }

        private void DrawLayerProperty()
        {
            if (layerList.index >= 0 && layerList.index < layerList.serializedProperty.arraySize)
            {
                EditorGUILayout.LabelField("Layer Properties", headerMiddle);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("levelLayers").GetArrayElementAtIndex(layerList.index).FindPropertyRelative("guid"));
                EditorGUILayout.EndVertical();
                DrawItemProperty(serializedObject.FindProperty("levelLayers").GetArrayElementAtIndex(layerList.index).FindPropertyRelative("item"));
            }
        }

        private void DrawItemProperty(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("name"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Thumbnail");
                property.FindPropertyRelative("thumbnail").objectReferenceValue = EditorGUILayout.ObjectField(property.FindPropertyRelative("thumbnail").objectReferenceValue,
                    typeof(Texture2D), false, GUILayout.Width(70f), GUILayout.Height(70f));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(property.FindPropertyRelative("accentColor"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("description"), GUILayout.Height(100f));
                EditorGUILayout.EndVertical();
            }
        }
    }
}