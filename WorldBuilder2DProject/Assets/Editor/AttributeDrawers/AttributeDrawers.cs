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

    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);
                SortingLayer[] sortingLayers = SortingLayer.layers;
                string[] sortingLayerNames = new string[sortingLayers.Length];
                int selectedIndex = 0;
                for(int i = 0; i<sortingLayers.Length; i++)
                {
                    if(sortingLayers[i].name == property.stringValue)
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
}
