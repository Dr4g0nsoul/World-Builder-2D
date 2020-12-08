using dr4g0nsoul.WorldBuilder2D.LevelEditor;
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

    [CustomPropertyDrawer(typeof(LevelLayerAttribute))]
    public class ParallaxLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                LevelEditorSettings levelEditorSettings = LevelEditorTool.GetLevelEditorSettings();

                string debugText = label.text + $" [{property.intValue}]";
                label.text = debugText;

                EditorGUI.BeginProperty(position, label, property);

                SortedDictionary<int, string> layerList = new SortedDictionary<int, string>();

                foreach(LevelLayer layer in levelEditorSettings.levelLayers)
                {
                    layerList.Add(layer.id, layer.item.name);
                }

                property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, layerList.Values.ToArray());

                EditorGUI.EndProperty();
            }
        }
    }

    [CustomPropertyDrawer(typeof(LevelObjectCategoryAttribute))]
    public class LevelObjectCategoryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /*
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                LevelEditorSettings levelEditorSettings = LevelEditorTool.GetLevelEditorSettings();

                string debugText = label.text + $" [{property.intValue}]";
                label.text = debugText;

                EditorGUI.BeginProperty(position, label, property);

                Dictionary<string, string> categoryList = new Dictionary<string, string>();

                categoryList.Add(System.Guid.Empty.ToString(), "None");

                foreach (LevelObjectCategory category in levelEditorSettings.levelObjectCategories)
                {
                    categoryList.Add(category.guid, category.item.name);
                }
                EditorGUI.Popup
                property.intValue = EditorGUI.IntPopup(position, label.text, property.intValue, categoryList.Values.ToArray(), categoryList.Keys.ToArray());

                EditorGUI.EndProperty();
            }
            */
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
