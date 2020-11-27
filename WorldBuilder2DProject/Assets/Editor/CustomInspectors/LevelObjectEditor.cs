using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{

    [CustomEditor(typeof(LevelObject))]
    public class LevelObjectEditor : Editor
    {

        private bool styleInitialized = false;
        private GUIStyle textMiddle;
        private GUIStyle headerMiddle;

        private void OnEnable()
        {
            styleInitialized = false;
        }

        public override void OnInspectorGUI()
        {
            if (!styleInitialized)
            {
                textMiddle = new GUIStyle(EditorStyles.label);
                textMiddle.alignment = TextAnchor.UpperCenter;

                headerMiddle = new GUIStyle(EditorStyles.boldLabel);
                headerMiddle.alignment = TextAnchor.UpperCenter;
                
                styleInitialized = true;
            }


            serializedObject.Update();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Level Object Properties", headerMiddle);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectPrefab"));
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawItemProperty(serializedObject.FindProperty("item"));
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subCategory"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("parallaxLayers"));
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }


        #region Draw Utils

        private void DrawItemProperty(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("name"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Thumbnail");
                Object texToDisplay;
                
                if(property.FindPropertyRelative("thumbnail").objectReferenceValue == null)
                {
                    texToDisplay = AssetPreview.GetAssetPreview(serializedObject.FindProperty("objectPrefab").objectReferenceValue);
                }
                else
                {
                    texToDisplay = property.FindPropertyRelative("thumbnail").objectReferenceValue;
                }
                property.FindPropertyRelative("thumbnail").objectReferenceValue = EditorGUILayout.ObjectField(texToDisplay,
                    typeof(Texture2D), false, GUILayout.Width(70f), GUILayout.Height(70f));

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(property.FindPropertyRelative("description"), GUILayout.Height(100f));
                EditorGUILayout.EndVertical();
            }
        }

        #endregion
    }
}