using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CustomEditor(typeof(LevelInstance))]
    public class LevelInstanceEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            string levelGUID = serializedObject.FindProperty("level").stringValue;
            LevelNode level = LevelController.Instance.GetLevel(levelGUID);

            if (string.IsNullOrEmpty(levelGUID) || level == null)
            {
                EditorGUILayout.HelpBox("Wrong level id. Please open the level editor and re-initialize the level", MessageType.Warning);
            }
        }
    }
}