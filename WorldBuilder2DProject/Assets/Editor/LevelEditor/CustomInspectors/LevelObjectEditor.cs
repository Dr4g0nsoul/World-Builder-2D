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

        private LevelEditorSettings levelEditorSettings;
        private GUISkin levelEditorSkin;

        private bool styleInitialized = false;
        private GUIStyle textMiddle;
        private GUIStyle headerMiddle;

        private string[] toolbarOptions = new string[] {"General", "Categories", "Layers" };
        private int toolbarOptionSelected = 0;

        private readonly float optionLabelWidth = 150f;
        private GUIStyle optionLabel;
        private GUIStyle optionLabelActive;
        private GUIStyle imageMini;
        private GUIStyle imageText;

        private float categoryScrollPos = 0f;
        private int categoryHoveringPos = -1;
        private int categoryClickedPos = -1;
        private bool categoryClickedOnAssigned = false;

        private float layerScrollPos = 0f;
        private int layerHoveringPos = -1;
        private int layerClickedPos = -1;
        private bool layerClickedOnAssigned = false;


        private void OnEnable()
        {
            styleInitialized = false;
            levelEditorSettings = LevelEditorTool.GetLevelEditorSettings();
            levelEditorSkin = Resources.Load<GUISkin>("LevelEditor/Skin/LESkin");

            toolbarOptionSelected = 0;

            categoryHoveringPos = -1;
            categoryClickedPos = -1;

            layerHoveringPos = -1;
            layerClickedPos = -1;
        }

        public override void OnInspectorGUI()
        {

            if (levelEditorSettings != null)
            {
                if (!styleInitialized)
                {
                    textMiddle = new GUIStyle(EditorStyles.label);
                    textMiddle.alignment = TextAnchor.UpperCenter;

                    headerMiddle = new GUIStyle(EditorStyles.boldLabel);
                    headerMiddle.alignment = TextAnchor.UpperCenter;

                    optionLabel = new GUIStyle(EditorStyles.helpBox);
                    optionLabel.normal.background = levelEditorSkin.customStyles[6].normal.background;
                    optionLabel.onNormal.background = levelEditorSkin.customStyles[6].normal.background;
                    optionLabel.hover.background = levelEditorSkin.customStyles[6].hover.background;
                    optionLabel.onHover.background = levelEditorSkin.customStyles[6].hover.background;
                    optionLabel.border = levelEditorSkin.customStyles[6].border;
                    optionLabel.padding.left = 10;
                    optionLabelActive = new GUIStyle(EditorStyles.helpBox);
                    optionLabelActive.normal.background = levelEditorSkin.customStyles[6].active.background;
                    optionLabelActive.onNormal.background = levelEditorSkin.customStyles[6].active.background;
                    optionLabelActive.border = levelEditorSkin.customStyles[6].border;
                    optionLabelActive.padding.left = 10;

                    imageMini = new GUIStyle(EditorStyles.objectFieldThumb);
                    imageMini.alignment = TextAnchor.MiddleCenter;
                    imageMini.padding = new RectOffset(2, 2, 2, 2);
                    imageMini.margin = new RectOffset(2, 4, 2, 2);
                    imageMini.fixedHeight = 18f;
                    imageMini.fixedWidth = 18f;

                    imageText = new GUIStyle(EditorStyles.label);
                    imageText.fontSize = 14;
                    imageText.alignment = TextAnchor.MiddleLeft;

                    styleInitialized = true;
                }


                serializedObject.Update();

                //Draw General Info
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Level Object Properties", headerMiddle);
                EditorGUILayout.Space(10f);
                toolbarOptionSelected = GUILayout.Toolbar(toolbarOptionSelected, toolbarOptions);
                EditorGUILayout.Space(10f);

                switch (toolbarOptionSelected) {
                    case 0:
                        //General
                        DrawItemProperty(serializedObject.FindProperty("item"));
                        break;

                    case 1:
                        //Categories
                        DrawCategoryPicker();
                        EditorGUILayout.LabelField("Hover: " + categoryHoveringPos);
                        EditorGUILayout.LabelField("Click: " + categoryClickedPos);
                        break;

                    case 2:
                        //Layers
                        DrawLayerPicker();
                        EditorGUILayout.LabelField("Hover: " + layerHoveringPos);
                        EditorGUILayout.LabelField("Click: " + layerClickedPos);
                        break;

                }

                serializedObject.ApplyModifiedProperties();


                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("You need to setup the level editor first before you change level object properies!", MessageType.Warning);
            }
        }

        #region Draw Utils

        private void DrawItemProperty(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectPrefab"));
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


        #region Category

        private void DrawCategoryPicker()
        {

            //Get assigned categories id
            SerializedProperty mainCategory = serializedObject.FindProperty("mainCategory");
            int mainCategoryIndex = -1;
            SerializedProperty assignedCategoriesProperty = serializedObject.FindProperty("categories");
            List<string> assignedCategoriesGuid = new List<string>();

            for (int i = 0; i < assignedCategoriesProperty.arraySize; i++)
            {
                assignedCategoriesGuid.Add(assignedCategoriesProperty.GetArrayElementAtIndex(i).stringValue);
            }

            //Get assigned category names
            List<string> assignedCategoriesNames = new List<string>();

            //Get assigned and unassigned categories
            List<LevelObjectCategory> assignedCategories = new List<LevelObjectCategory>();
            List<LevelObjectCategory> unassignedCategories = new List<LevelObjectCategory>();
            int assignedIndex = 0;
            foreach(LevelObjectCategory cat in levelEditorSettings.levelObjectCategories)
            {
                if (assignedCategoriesGuid.Contains(cat.guid))
                {
                    if(cat.guid == serializedObject.FindProperty("mainCategory").stringValue)
                    {
                        mainCategoryIndex = assignedIndex;
                    }
                    assignedCategories.Add(cat);
                    assignedCategoriesNames.Add(cat.item.name);
                    assignedIndex++;
                }
                else
                {
                    unassignedCategories.Add(cat);
                }
            }


            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(3f);
            GUILayout.Label("Main Category", EditorStyles.boldLabel);
            GUILayout.Space(1f);
            if (assignedCategories.Count > 0)
            {
                int newMainCategoryIndex = EditorGUILayout.Popup(mainCategoryIndex, assignedCategoriesNames.ToArray(), EditorStyles.popup);
                if (mainCategoryIndex < 0)
                {
                    mainCategory.stringValue = assignedCategoriesGuid[0];
                }
                else if(newMainCategoryIndex != mainCategoryIndex || mainCategory.stringValue.Length < 1)
                {
                    mainCategory.stringValue = assignedCategories[newMainCategoryIndex].guid;
                }
                //GUILayout.Label(mainCategory.stringValue);
            }
            else
            {
                if (mainCategory.stringValue != null || mainCategory.stringValue.Length < 1)
                    mainCategory.stringValue = "";
                EditorGUILayout.HelpBox("Assign categories before choosing the main category", MessageType.Info, true);
            }
            GUILayout.Space(10f);


            GUILayout.Label("Assigned Categories", EditorStyles.boldLabel);
            if(assignedCategories.Count < 1)
                GUILayout.Label("No categories assigned yet", textMiddle);
            else
                CategoriesSelection(assignedCategories, true);

            GUILayout.Space(10f);

            GUILayout.Label("Unassigned Categories", EditorStyles.boldLabel);
            GUILayout.Space(1f);
            if(unassignedCategories.Count < 1)
                GUILayout.Label("All available categories were assigned", textMiddle);
            else
                CategoriesSelection(unassignedCategories, false);

            GUILayout.Space(3f);
            GUILayout.EndVertical();

            if (categoryClickedPos > -1)
                Repaint();

            if (Event.current.type == EventType.MouseUp 
                || (Event.current.type == EventType.MouseDrag && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)))
            {
                categoryClickedPos = -1;
            }
        }

        private void CategoriesSelection(List<LevelObjectCategory> categories, bool isAssigned)
        {

            //Draw Unassigned categories
            int objectsPerRow = (int)(EditorGUIUtility.currentViewWidth / (optionLabelWidth + 22));
            int objectCount = categories.Count;
            bool hoveringOverCategory = false;

            categoryScrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, categoryScrollPos), EditorStyles.helpBox, GUILayout.Height(Mathf.Min(10, Mathf.CeilToInt(objectCount / (float)objectsPerRow)) * 29.5f)).y;
            for (int i = 0; i < objectCount; i++)
            {
                if (i % objectsPerRow == 0)
                    GUILayout.BeginHorizontal();

                GUI.color = categories[i].item.accentColor;
                if (categoryClickedPos == i && categoryClickedOnAssigned == isAssigned)
                {
                    GUILayout.BeginHorizontal(optionLabelActive, GUILayout.Width(optionLabelWidth));
                    //Debug.Log($"clicking {i}");
                }
                else
                {
                    GUILayout.BeginHorizontal(optionLabel, GUILayout.Width(optionLabelWidth));
                }
                GUILayout.Label(categories[i].item.thumbnail, imageMini);
                GUI.color = Color.white;
                GUILayout.Label(categories[i].item.name, imageText);
                GUILayout.EndHorizontal();
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    categoryHoveringPos = i;
                    Repaint();
                    hoveringOverCategory = true;
                }

                if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
                {
                    GUILayout.EndHorizontal();
                    if (i < objectCount - 1)
                        GUILayout.Space(2f);
                }
            }

            EditorGUILayout.EndScrollView();
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {

                if (!hoveringOverCategory)
                {
                    categoryHoveringPos = -1;
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    categoryClickedPos = categoryHoveringPos;
                    categoryClickedOnAssigned = isAssigned;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (categoryHoveringPos >= 0)
                    {
                        //if (categoryHoveringPos != categoryClickedPos || clickedOnAssigned != isAssigned)
                            // Debug.Log("released on wrong button");
                        if(categoryClickedOnAssigned == isAssigned && categoryHoveringPos == categoryClickedPos)
                        {
                            if (isAssigned)
                            {
                                RemoveCategory(categories[categoryClickedPos].guid);
                            }
                            else
                            {
                                serializedObject.FindProperty("categories").arraySize += 1;
                                serializedObject.FindProperty("categories").GetArrayElementAtIndex(serializedObject.FindProperty("categories").arraySize - 1).stringValue = categories[categoryClickedPos].guid;
                            }
                            //Debug.Log($"selected {categoryHoveringPos}");
                        }
                    }
                    //else
                    //{
                    //    Debug.Log("selected none");
                    //}
                    categoryClickedPos = -1;
                }
            }
        }

        #endregion

        #region Layer

        private void DrawLayerPicker()
        {

            //Get assigned layer id
            SerializedProperty assignedLayerProperty = serializedObject.FindProperty("levelLayers");
            List<string> assignedLayersGuid = new List<string>();

            for (int i = 0; i < assignedLayerProperty.arraySize; i++)
                assignedLayersGuid.Add(assignedLayerProperty.GetArrayElementAtIndex(i).stringValue);



            //Get assigned and unassigned layers
            List<LevelLayer> assignedLayers = new List<LevelLayer>();
            List<LevelLayer> unassignedLayers = new List<LevelLayer>();
            foreach (LevelLayer layer in levelEditorSettings.levelLayers)
            {
                if (assignedLayersGuid.Contains(layer.guid))
                {
                    assignedLayers.Add(layer);
                }
                else
                {
                    unassignedLayers.Add(layer);
                }
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(3f);

            GUILayout.Label("Assigned Layers", EditorStyles.boldLabel);
            GUILayout.Space(1f);
            if (assignedLayers.Count < 1)
                GUILayout.Label("No layers assigned yet", textMiddle);
            else
                LayerSelection(assignedLayers, true);

            GUILayout.Space(10f);

            GUILayout.Label("Unassigned Layers", EditorStyles.boldLabel);
            GUILayout.Space(1f);
            if (unassignedLayers.Count < 1)
                GUILayout.Label("All layers were assigned", textMiddle);
            else
                LayerSelection(unassignedLayers, false);

            GUILayout.Space(3f);
            GUILayout.EndVertical();

            if (categoryClickedPos > -1)
                Repaint();

            if (Event.current.type == EventType.MouseUp
                || (Event.current.type == EventType.MouseDrag && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)))
            {
                categoryClickedPos = -1;
            }
        }

        private void LayerSelection(List<LevelLayer> layers, bool isAssigned)
        {

            //Draw Unassigned layers
            int objectsPerRow = (int)(EditorGUIUtility.currentViewWidth / (optionLabelWidth + 22));
            int objectCount = layers.Count;
            bool hoveringOverLayer = false;

            layerScrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, layerScrollPos), EditorStyles.helpBox, GUILayout.Height(Mathf.Min(10, Mathf.CeilToInt(objectCount / (float)objectsPerRow)) * 29.5f)).y;
            for (int i = 0; i < objectCount; i++)
            {
                if (i % objectsPerRow == 0)
                    GUILayout.BeginHorizontal();

                GUI.color = layers[i].item.accentColor;
                if (layerClickedPos == i && layerClickedOnAssigned == isAssigned)
                {
                    GUILayout.BeginHorizontal(optionLabelActive, GUILayout.Width(optionLabelWidth));
                }
                else
                {
                    GUILayout.BeginHorizontal(optionLabel, GUILayout.Width(optionLabelWidth));
                }
                GUILayout.Label(layers[i].item.thumbnail, imageMini);
                GUI.color = Color.white;
                GUILayout.Label(layers[i].item.name, imageText);
                GUILayout.EndHorizontal();
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    layerHoveringPos = i;
                    Repaint();
                    hoveringOverLayer = true;
                }

                if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
                {
                    GUILayout.EndHorizontal();
                    if (i < objectCount - 1)
                        GUILayout.Space(2f);
                }
            }

            EditorGUILayout.EndScrollView();
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {

                if (!hoveringOverLayer)
                {
                    layerHoveringPos = -1;
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    layerClickedPos = layerHoveringPos;
                    layerClickedOnAssigned = isAssigned;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (layerHoveringPos >= 0)
                    {
                        if (layerClickedOnAssigned == isAssigned && layerHoveringPos == layerClickedPos)
                        {
                            if (isAssigned)
                            {
                                RemoveLayer(layers[layerClickedPos].guid);
                            }
                            else
                            {
                                serializedObject.FindProperty("levelLayers").arraySize += 1;
                                serializedObject.FindProperty("levelLayers").GetArrayElementAtIndex(serializedObject.FindProperty("levelLayers").arraySize - 1).stringValue = layers[layerClickedPos].guid;
                            }
                        }
                    }

                    layerClickedPos = -1;
                }
            }
        }

        #endregion

        #endregion

        #region Remove Category/Layer

        private void RemoveCategory(string guid)
        {
            SerializedProperty assignedCategoriesProperty = serializedObject.FindProperty("categories");
            int indexToRemove = -1;

            for (int i = 0; i < assignedCategoriesProperty.arraySize; i++)
            {
                if(assignedCategoriesProperty.GetArrayElementAtIndex(i).stringValue == guid)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if(indexToRemove > -1)
            {
                assignedCategoriesProperty.DeleteArrayElementAtIndex(indexToRemove);
            }

        }

        private void RemoveLayer(string guid)
        {
            SerializedProperty assignedLayersProperty = serializedObject.FindProperty("levelLayers");
            int indexToRemove = -1;

            for (int i = 0; i < assignedLayersProperty.arraySize; i++)
            {
                if (assignedLayersProperty.GetArrayElementAtIndex(i).stringValue == guid)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove > -1)
            {
                assignedLayersProperty.DeleteArrayElementAtIndex(indexToRemove);
            }

        }

        #endregion
    }
}