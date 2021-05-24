using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.ParallaxBackgroundPlugin
{
    [CreateLevelObjectEditorExtension(typeof(ParallaxBackgroundLevelObject))]
    public class ParallaxBackgroundEditorExtenstion : LevelObjectEditorExtension
    {

        private ReorderableList backgroundLayers;
        private GUISkin prevSkin;
        private GUISkin levelEditorSkin;

        #region Setup

        public override GameObject OnLevelObjectCreate(GameObject currentPrefab, string prefabPath)
        {
            currentPrefab.transform.position = Vector3.zero;
            currentPrefab.transform.rotation = Quaternion.identity;
            currentPrefab.transform.localScale = Vector3.one;

            ParallaxBackground parallaxBackground = currentPrefab.GetComponent<ParallaxBackground>();

            if (parallaxBackground == null)
                parallaxBackground = currentPrefab.AddComponent<ParallaxBackground>();

            GameObject backgroundContainer = new GameObject();
            backgroundContainer.transform.position = Vector3.zero;
            backgroundContainer.transform.rotation = Quaternion.identity;
            backgroundContainer.transform.localScale = Vector3.one;
            backgroundContainer.transform.parent = currentPrefab.transform;
            backgroundContainer.name = "Background Container";

            parallaxBackground.parallaxBackgroundSettings = Target as ParallaxBackgroundLevelObject;
            parallaxBackground.backgroundContainer = backgroundContainer.transform;

            return currentPrefab;
        }

        #endregion

        #region Inspector

        public override string CustomInspectorTabName()
        {
            return "Background";
        }

        public override void OnCustomInspectorEnable(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject)
        {
            levelEditorSkin = Resources.Load<GUISkin>("LevelEditor/Skin/LESkin");
            LevelEditorStyles.RefreshStyles();

            backgroundLayers = new ReorderableList(serializedObject, serializedObject.FindProperty("parallaxBackgroundLayers"), true, true, true, true);

            backgroundLayers.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Background Layers");
            };

            backgroundLayers.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty item = backgroundLayers.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(rect, $"Layer {index}: {item.FindPropertyRelative("name").stringValue}");
            };
        }

        public override void OnCustomInspectorTabGUI(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject)
        {
            serializedObject.Update();

            prevSkin = GUI.skin;
            GUI.skin = levelEditorSkin;

            GUILayout.Label("Background Layers", LevelEditorStyles.HeaderCentered);
            GUI.skin = prevSkin;
            backgroundLayers.DoLayoutList();
            GUI.skin = levelEditorSkin;

            if (backgroundLayers.index >= 0)
            {
                SerializedProperty selectedLayer = backgroundLayers.serializedProperty.GetArrayElementAtIndex(backgroundLayers.index);
                if (selectedLayer != null) {
                    SerializedProperty selectedLayerName = selectedLayer.FindPropertyRelative("name");
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(string.IsNullOrEmpty(selectedLayerName.stringValue) ? "Unnamed Layer" : selectedLayerName.stringValue);
                    LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(30, 30, 0, 10));
                    EditorGUILayout.PropertyField(selectedLayerName);
                    EditorGUILayout.PropertyField(selectedLayer.FindPropertyRelative("image"));
                    EditorGUILayout.PropertyField(selectedLayer.FindPropertyRelative("parallaxSpeed"));
                    EditorGUILayout.PropertyField(selectedLayer.FindPropertyRelative("yOffset"));
                    GUILayout.EndVertical();
                }
            }

            GUI.skin = prevSkin;

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Placement

        public override bool UseTemporaryIndicator => false;

        public override GameObject SpawnObject(LevelObject obj, GameObject temporaryObject, Transform parentTransform, Vector2 worldPos, Vector2 mousePos)
        {
            if(parentTransform != null)
            {
                foreach (Transform child in parentTransform)
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject) && child.gameObject.name == obj.objectPrefab.name)
                    {
                        Selection.activeObject = child;
                        return child.gameObject;
                    }
                }
            }
            GameObject newObject = PrefabUtility.InstantiatePrefab(obj.objectPrefab) as GameObject;
            if (newObject != null)
            {
                newObject.transform.position = Vector2.zero;
                newObject.transform.rotation = Quaternion.identity;
                newObject.transform.parent = parentTransform;

                Undo.RegisterCreatedObjectUndo(newObject, $"LevelEditor: Placed {obj.item.name}");//Undo function
                Selection.activeObject = newObject;
                return newObject;
            }
            return null;
        }

        #endregion
    }
}
