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

    [CustomPropertyDrawer(typeof(ParallaxLayerAttribute))]
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

                foreach(ParallaxLayer layer in levelEditorSettings.levelEditorLayers)
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

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                LevelEditorSettings levelEditorSettings = LevelEditorTool.GetLevelEditorSettings();

                string debugText = label.text + $" [{property.intValue}]";
                label.text = debugText;

                EditorGUI.BeginProperty(position, label, property);

                Dictionary<int, string> categoryList = new Dictionary<int, string>();

                categoryList.Add(-1, "None");

                foreach (LevelEditorCategory category in levelEditorSettings.levelEditorCategories)
                {
                    categoryList.Add(category.id, category.item.name);
                }

                property.intValue = EditorGUI.IntPopup(position, label.text, property.intValue, categoryList.Values.ToArray(), categoryList.Keys.ToArray());

                EditorGUI.EndProperty();
            }
        }
    }

    [CustomPropertyDrawer(typeof(LevelObjectSubCategoryAttribute))]
    public class LevelObjectSubCategoryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (property.propertyType == SerializedPropertyType.Integer)
            {

                //Get current category
                SerializedProperty categoryProperty = property.serializedObject.FindProperty("category");
                int categoryId = -1;
                if (categoryProperty != null)
                {
                    categoryId = categoryProperty.intValue;
                }
                else
                {
                    //Category property not found
                    EditorGUI.LabelField(position, "--- Sub-category works only in Level Object SO ---", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                //Get all subcategories of givven category
                if (categoryId >= 0)
                {
                    LevelEditorSettings levelEditorSettings = LevelEditorTool.GetLevelEditorSettings();

                    LevelEditorCategory category = null;

                    foreach (LevelEditorCategory currCategory in levelEditorSettings.levelEditorCategories)
                    {
                        if (currCategory.id == categoryId)
                        {
                            category = currCategory;
                            break;
                        }
                    }

                    if (category != null && category.subCategories.Length > 0)
                    {
                        //Load subcategories
                        string debugText = label.text + $" [{property.intValue}]";
                        label.text = debugText;

                        EditorGUI.BeginProperty(position, label, property);

                        Dictionary<int, string> subCategoryList = new Dictionary<int, string>();

                        subCategoryList.Add(-1, "None");

                        foreach (LevelEditorSubCategory subCategory in category.subCategories)
                        {
                            subCategoryList.Add(subCategory.id, subCategory.item.name);
                        }

                        property.intValue = EditorGUI.IntPopup(position, label.text, property.intValue, subCategoryList.Values.ToArray(), subCategoryList.Keys.ToArray());

                        EditorGUI.EndProperty();

                        return;
                    }
                    else
                    {
                        //If no subcategories found
                        EditorGUI.LabelField(position, "--- Sub-categories empty ---", EditorStyles.centeredGreyMiniLabel);
                    }
                }
                else
                {
                    //If none is selected
                    EditorGUI.LabelField(position, "--- No category selected ---", EditorStyles.centeredGreyMiniLabel);
                }

            }
            else
            {
                
                //Not an integer property
                EditorGUI.LabelField(position, "--- Invalid property type (not int) ---", EditorStyles.centeredGreyMiniLabel);
            }


        }
        
    }
}
