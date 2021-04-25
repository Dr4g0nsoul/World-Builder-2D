using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace dr4g0nsoul.WorldBuilder2D.Game.Editors
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {

        private readonly string[] toolbarLabels = new string[] { "Level Exit Events", "Level Loading Events" };
        private int selectedToolbarIndex;

        private void OnEnable()
        {
            selectedToolbarIndex = 0;
        }

        public override void OnInspectorGUI()
        {
            LevelManager levelManager = target as LevelManager;
            if (levelManager == null) return;


            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerCollider"));

            if(levelManager.mainCamera == null || levelManager.playerCollider == null)
            {
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.HelpBox("Add all necessary references in order to proceed!", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("startLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipStartLevelLoading"));

            EditorGUILayout.Space(15f);
            selectedToolbarIndex = GUILayout.Toolbar(selectedToolbarIndex, toolbarLabels);
            EditorGUILayout.Space(7f);

            switch(selectedToolbarIndex)
            {
                case 0:
                    DrawLevelExitEvents();
                    break;
                case 1:
                    DrawLevelLoadingEvents();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLevelExitEvents()
        {
            EditorGUILayout.HelpBox("Events only triggered during level exit transitions. " +
                "Make sure to use functions with the same parameter list as the event to take advantage of dynamic parameters", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onLevelExitTriggered"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onLevelEntered"));
        }

        private void DrawLevelLoadingEvents()
        {
            EditorGUILayout.HelpBox("These events are triggered during loading/unloading of levels", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPreviousLevelUnloadStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPreviousLevelUnloading"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPreviousLevelUnloadComplete"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewLevelLoadStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewLevelLoading"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewLevelLoadComplete"));
        }
    }
}
