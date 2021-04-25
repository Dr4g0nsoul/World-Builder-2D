using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Util
{

    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            else
                EditorGUI.PropertyField(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }

    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);
                SortingLayer[] sortingLayers = SortingLayer.layers;
                string[] sortingLayerNames = new string[sortingLayers.Length];
                int selectedIndex = 0;
                for (int i = 0; i < sortingLayers.Length; i++)
                {
                    if (sortingLayers[i].name == property.stringValue)
                    {
                        selectedIndex = i;
                    }
                    sortingLayerNames[i] = sortingLayers[i].name;
                }
                property.stringValue = sortingLayerNames[EditorGUI.Popup(position, label.text, selectedIndex, sortingLayerNames)];
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(PhysicsLayerAttribute))]
    public class PhysicsLayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(LevelPickerAttribute))]
    public class LevelPickerAttributeDrawer : PropertyDrawer
    {

        private string[] levelNames;
        private string[] levelGuids;

        public LevelPickerAttributeDrawer() : base()
        {
            LevelController levelController = LevelController.Instance;
            LevelNode[] levels = levelController.GetLevels();
            List<string> levelNames = new List<string>();
            List<string> levelGuids = new List<string>();
            foreach (LevelNode level in levels)
            {
                string worldName = levelController.GetWorldByLevel(level.guid)?.worldName;
                levelNames.Add(string.IsNullOrEmpty(worldName) ? level.levelName : $"{worldName} - {level.levelName}");
                levelGuids.Add(level.guid);
            }

            this.levelNames = levelNames.ToArray();
            this.levelGuids = levelGuids.ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String && levelGuids.Length > 0)
            {
                int selectedIndex = -1;

                for(int i = 0; i < levelGuids.Length; i++)
                {
                    if(levelGuids[i] == property.stringValue)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, levelNames);
                if(selectedIndex >= 0 && selectedIndex < levelGuids.Length)
                {
                    property.stringValue = levelGuids[selectedIndex];
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }


        }
    }
}
