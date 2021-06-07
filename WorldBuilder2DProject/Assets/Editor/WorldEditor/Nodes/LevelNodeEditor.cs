using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;
using XNodeEditor;
using UnityEditor.AnimatedValues;
using System.Linq;

[CustomNodeEditor(typeof(LevelNode))]
public class LevelNodeEditor : NodeEditor, INodeEditorInspector
{
    //Node display
    private static readonly int sizeMultiplier = 15;
    private static readonly int minWidth = 5;

    private Rect thumbnailRect;

    //Node inspector
    private LevelEditorSettings levelEditorSettings;
    private LevelObjectsController levelObjectsController;
    //Tabs
    private int menuSelection;
    private readonly string[] menuLabels = new string[] { "Overview", "Level Favorites" };

    //Preferred items
    private readonly float optionLabelWidth = 150f;
    private GUIStyle optionLabel;
    private GUIStyle optionLabelActive;
    private GUIStyle imageMini;
    private GUIStyle imageText;
    private GUIStyle textCenter;

    private float categoryScrollPos = 0f;
    private int categoryHoveringPos = -1;
    private int categoryClickedPos = -1;
    private bool categoryClickedOnAssigned = false;

    private float levelObjectScrollPos = 0f;
    private int levelObjectHoveringPos = -1;
    private int levelObjectClickedPos = -1;
    private bool levelObjectClickedOnAssigned = false;
    private int filterCategoryIndex = 0;
    private LevelObjectCategory categoryToFilter;

    //Debug fields
    private AnimBool showDebugFields;



    #region Initialization

    public override void OnCreate()
    {
        base.OnCreate();
        RebuildLevelExitConnections();

        levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
        levelObjectsController = LevelObjectsController.Instance;

        //Inspector initialization
        menuSelection = 0;
        showDebugFields = new AnimBool(false);

        //Preferred Items
        categoryHoveringPos = -1;
        categoryClickedPos = -1;
        levelObjectHoveringPos = -1;
        levelObjectClickedPos = -1;
        filterCategoryIndex = 0;
        categoryToFilter = null;

        GUISkin levelEditorSkin = Resources.Load<GUISkin>("LevelEditor/Skin/LESkin");
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
        if (!EditorGUIUtility.isProSkin)
        {
            imageText.normal.textColor = Color.white;
        }

        textCenter = new GUIStyle(NodeEditorResources.styles.nodeHeader);
        if (!EditorGUIUtility.isProSkin)
        {
            textCenter.normal.textColor = Color.black;
        }
    }

    public override void AddContextMenuItems(GenericMenu menu)
    {
        bool canRemove = true;
        // Actions if only one node is selected
        if (Selection.objects.Length == 1 && Selection.activeObject is LevelNode)
        {
            XNode.Node node = Selection.activeObject as XNode.Node;
            // Add open level action
            menu.AddItem(new GUIContent("Open Level"), false, () => OpenCurrentLevel());
            // Add Rename
            menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);
            // Add Move to Top
            menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));

            canRemove = NodeGraphEditor.GetEditor(node.graph, NodeEditorWindow.current).CanRemove(node);
        }

        if (canRemove) menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
        else menu.AddItem(new GUIContent("Remove"), false, null);
    }

    #endregion

    #region GUI

    public override void OnHeaderGUI()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode == null || string.IsNullOrEmpty(lNode.levelName))
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        else
        {
            WorldNode world = LevelController.Instance.GetWorldByLevel(lNode.guid);
            if (world != null)
            {
                GUILayout.Label($"{world.worldName} - {lNode.levelName}", NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            }
            else
            {
                GUILayout.Label(lNode.levelName, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            }
        }

    }

    public override void OnBodyGUI()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode != null) {
            serializedObject.Update();

            //Thumbnail
            Texture2D thumbnail = LevelController.Instance.GetLevelThumbnail(lNode.guid);
            

            if (thumbnail != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(1f);
                GUILayout.BeginVertical(WorldEditorStyles.GetThumbnailStyle(LevelController.Instance.GetLevelThumbnail(lNode.guid), lNode.levelBoundaries.height * sizeMultiplier));
                GUILayout.Space(lNode.levelBoundaries.height * sizeMultiplier);
                GUILayout.EndVertical();
                if(Event.current.type == EventType.Repaint)
                {
                    thumbnailRect = GUILayoutUtility.GetLastRect();
                }
                GUILayout.Space(1f);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10f);
                GUILayout.Label("[No thumbnail found]", WorldEditorStyles.TextCentered);
                GUILayout.Space(10f);
            }

            //Level Exits
            if(Event.current.type == EventType.Layout && serializedObject.FindProperty("levelExitsUpdated").boolValue)
            {
                serializedObject.FindProperty("levelExitsUpdated").boolValue = false;
                RebuildLevelExitConnections();
            }
            //Draw Level Exits
            foreach (LevelExit exit in lNode.levelExits)
            {
                NodePort levelExitPort = lNode.GetPort(exit.guid);
                if(levelExitPort != null)
                    NodeEditorGUILayout.PortField(WorldToNodePosition(exit.entryPoint, lNode, thumbnailRect), levelExitPort);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    public override int GetWidth()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode == null || lNode.levelBoundaries == null || lNode.levelBoundaries.size.x < minWidth)
        {
            return minWidth * sizeMultiplier;
        }
        return Mathf.RoundToInt(lNode.levelBoundaries.width * sizeMultiplier);
    }

    #region Level Node Inspector

    public void OnNodeInspectorGUI()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode) {
            GUILayout.Label($"Level: {lNode.levelName}", GUILayout.ExpandHeight(false));
            LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(10, 10, 10, 10));

            serializedObject.Update();

            //Menu bar
            int lastMenuSelection = menuSelection;
            menuSelection = GUILayout.Toolbar(menuSelection, menuLabels, EditorStyles.toolbarButton);
            GUILayout.Space(10f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(menuLabels[menuSelection], textCenter);
            LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(10, 10, 3, 15));

            //Content
            switch (menuSelection)
            {
                case 0:
                    DrawGeneralInformation();
                    break;
                case 1:
                    DrawPreferredItemsSelection();
                    break;
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

    #region GUI

    private void DrawGeneralInformation()
    {
        //General information
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
        target.name = serializedObject.FindProperty("levelName").stringValue;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDescription"));

        //World boundaries
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Level Bounds", textCenter);
        EditorGUILayout.Space(3f);
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelBoundaries"));
        GUI.enabled = true;
        

        //Debug information
        EditorGUILayout.Space(20f);
        showDebugFields.target = EditorGUILayout.ToggleLeft("Show debug fields", showDebugFields.target);
        if (EditorGUILayout.BeginFadeGroup(showDebugFields.faded))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignedSceneName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignedScenePath"));
        }
        EditorGUILayout.EndFadeGroup();
    }

    private void DrawPreferredItemsSelection()
    {
        DrawCategoryPicker();
        DrawPreferredLevelObjectsPicker();
    }

    #endregion

    #region Preferred Items

    #region Category

    private void DrawCategoryPicker()
    {

        //Get assigned categories id
        SerializedProperty assignedCategoriesProperty = serializedObject.FindProperty("preferredItems").FindPropertyRelative("categories");
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
        foreach (LevelObjectCategory cat in levelEditorSettings.levelObjectCategories)
        {
            if (assignedCategoriesGuid.Contains(cat.guid))
            {
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


        GUILayout.Label("Assigned favorite categories", EditorStyles.boldLabel);
        if (assignedCategories.Count < 1)
            GUILayout.Label("No favorite categories assigned yet", textCenter);
        else
            CategoriesSelection(assignedCategories, true);

        GUILayout.Space(10f);

        GUILayout.Label("Unassigned Categories", EditorStyles.boldLabel);
        GUILayout.Space(1f);
        if (unassignedCategories.Count < 1)
            GUILayout.Label("All available categories were assigned", textCenter);
        else
            CategoriesSelection(unassignedCategories, false);

        GUILayout.Space(3f);
        GUILayout.EndVertical();

        if (Event.current.type == EventType.MouseUp
            || (Event.current.type == EventType.MouseDrag && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)))
        {
            categoryClickedPos = -1;
        }
    }

    private void CategoriesSelection(List<LevelObjectCategory> categories, bool isAssigned)
    {

        //Draw Unassigned categories
        int objectsPerRow = (int)(WorldEditorGraphEditor.INSPECTOR_RECT_WIDTH / (optionLabelWidth + 22));
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
                    if (categoryClickedOnAssigned == isAssigned && categoryHoveringPos == categoryClickedPos)
                    {
                        if (isAssigned)
                        {
                            RemoveCategory(categories[categoryClickedPos].guid);
                        }
                        else
                        {
                            serializedObject.FindProperty("preferredItems").FindPropertyRelative("categories").arraySize += 1;
                            serializedObject.FindProperty("preferredItems").FindPropertyRelative("categories").GetArrayElementAtIndex(serializedObject.FindProperty("preferredItems").FindPropertyRelative("categories").arraySize - 1).stringValue = categories[categoryClickedPos].guid;
                        }
                    }
                }
                categoryClickedPos = -1;
            }
        }
    }

    private void RemoveCategory(string guid)
    {
        SerializedProperty assignedCategoriesProperty = serializedObject.FindProperty("preferredItems").FindPropertyRelative("categories");
        int indexToRemove = -1;

        for (int i = 0; i < assignedCategoriesProperty.arraySize; i++)
        {
            if (assignedCategoriesProperty.GetArrayElementAtIndex(i).stringValue == guid)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove > -1)
        {
            assignedCategoriesProperty.DeleteArrayElementAtIndex(indexToRemove);
        }

    }

    #endregion

    #region Level Objects

    private void DrawPreferredLevelObjectsPicker()
    {
        //Get assigned level objects id
        SerializedProperty assignedLevelObjectsProperty = serializedObject.FindProperty("preferredItems").FindPropertyRelative("levelObjects");
        List<string> assignedLevelObjectsGuid = new List<string>();

        for (int i = 0; i < assignedLevelObjectsProperty.arraySize; i++)
        {
            assignedLevelObjectsGuid.Add(assignedLevelObjectsProperty.GetArrayElementAtIndex(i).stringValue);
        }

        //Get assigned level objects names
        List<string> assignedLevelObjectsNames = new List<string>();

        //Get assigned and unassigned level objects
        List<LevelObject> assignedLevelObjects = new List<LevelObject>();
        List<LevelObject> unassignedLevelObjects = new List<LevelObject>();
        int assignedIndex = 0;
        foreach (LevelObject obj in levelObjectsController.GetAllLevelObjects().Values)
        {
            if (assignedLevelObjectsGuid.Contains(obj.guid))
            {
                assignedLevelObjects.Add(obj);
                assignedLevelObjectsNames.Add(obj.item.name);
                assignedIndex++;
            }
            else
            {
                unassignedLevelObjects.Add(obj);
            }
        }


        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(3f);


        GUILayout.Label("Assigned favorite level objects", EditorStyles.boldLabel);
        if (assignedLevelObjects.Count < 1)
            GUILayout.Label("No favorite level objects assigned yet", textCenter);
        else
            LevelObjectsSelection(assignedLevelObjects, true);

        GUILayout.Space(10f);

        GUILayout.Label("Unassigned Level Objects", EditorStyles.boldLabel);
        GUILayout.Space(1f);
        if (unassignedLevelObjects.Count < 1)
            GUILayout.Label("All available level objects were assigned", textCenter);
        else
            LevelObjectsSelection(unassignedLevelObjects, false);

        GUILayout.Space(3f);
        GUILayout.EndVertical();

        if (Event.current.type == EventType.MouseUp
            || (Event.current.type == EventType.MouseDrag && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)))
        {
            levelObjectClickedPos = -1;
        }
    }


    private void LevelObjectsSelection(List<LevelObject> objects, bool isAssigned)
    {

        //Draw category filter
        if (!isAssigned)
        {
            List<string> filterCategoryNames = new List<string>();
            filterCategoryNames.Add("All");
            List<string> filterCategoryGuids = new List<string>();
            filterCategoryGuids.Add("");
            categoryToFilter = null;
            foreach (LevelObjectCategory cat in levelEditorSettings.levelObjectCategories)
            {
                filterCategoryGuids.Add(cat.guid);
                filterCategoryNames.Add(cat.item.name);
            }
            GUILayout.Space(3f);
            GUILayout.Label("Filter by category", EditorStyles.label);
            GUILayout.Space(1f);
            filterCategoryIndex = EditorGUILayout.Popup(filterCategoryIndex, filterCategoryNames.ToArray(), EditorStyles.popup);
            categoryToFilter = levelObjectsController.GetCategoryByGuid(filterCategoryGuids[filterCategoryIndex]);

            if (categoryToFilter != null)
            {
                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    if (!objects[i].categories.Contains(categoryToFilter.guid))
                    {
                        objects.RemoveAt(i);
                    }
                }
            }

            if (objects.Count < 1)
            {
                GUILayout.Label("Change filter option for more results", textCenter);
            }

        }

        //GUILayout.Label(mainCategory.stringValue);
        GUILayout.Space(10f);

        if (objects.Count > 0)
        {

            //Draw Unassigned categories
            int objectsPerRow = (int)(WorldEditorGraphEditor.INSPECTOR_RECT_WIDTH / (optionLabelWidth + 22));
            int objectCount = objects.Count;
            bool hoveringOverObject = false;

            levelObjectScrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, levelObjectScrollPos), EditorStyles.helpBox, GUILayout.Height(Mathf.Min(10, Mathf.CeilToInt(objectCount / (float)objectsPerRow)) * 29.5f)).y;
            for (int i = 0; i < objectCount; i++)
            {
                if (i % objectsPerRow == 0)
                    GUILayout.BeginHorizontal();

                Color levelObjectColor = Color.white;
                LevelObjectCategory objectMainCategory = levelObjectsController.GetCategoryByGuid(objects[i].mainCategory);
                if (objectMainCategory != null)
                {
                    levelObjectColor = objectMainCategory.item.accentColor;
                }

                GUI.color = levelObjectColor;
                if (levelObjectClickedPos == i && levelObjectClickedOnAssigned == isAssigned)
                {
                    GUILayout.BeginHorizontal(optionLabelActive, GUILayout.Width(optionLabelWidth));
                    //Debug.Log($"clicking {i}");
                }
                else
                {
                    GUILayout.BeginHorizontal(optionLabel, GUILayout.Width(optionLabelWidth));
                }

                GUI.color = Color.white;
                Texture2D thumbnail = objects[i].item.thumbnail;
                if (thumbnail == null)
                {
                    thumbnail = AssetPreview.GetAssetPreview(objects[i].objectPrefab);
                    if (thumbnail != null)
                        thumbnail = LevelEditorStyles.ResampleAndCrop(thumbnail, 24, 24);
                    GUILayout.Label(thumbnail, LevelEditorStyles.LevelObjectPreviewMiniImage);
                }
                else
                {
                    GUILayout.Label(thumbnail, imageMini);
                }
                GUILayout.Label(objects[i].item.name, imageText);
                GUILayout.EndHorizontal();
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    levelObjectHoveringPos = i;
                    hoveringOverObject = true;
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

                if (!hoveringOverObject)
                {
                    levelObjectHoveringPos = -1;
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    levelObjectClickedPos = levelObjectHoveringPos;
                    levelObjectClickedOnAssigned = isAssigned;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (levelObjectHoveringPos >= 0)
                    {
                        if (levelObjectClickedOnAssigned == isAssigned && levelObjectHoveringPos == levelObjectClickedPos)
                        {
                            if (isAssigned)
                            {
                                RemoveLevelObject(objects[levelObjectClickedPos].guid);
                            }
                            else
                            {
                                serializedObject.FindProperty("preferredItems").FindPropertyRelative("levelObjects").arraySize += 1;
                                serializedObject.FindProperty("preferredItems").FindPropertyRelative("levelObjects").GetArrayElementAtIndex(serializedObject.FindProperty("preferredItems").FindPropertyRelative("levelObjects").arraySize - 1).stringValue = objects[levelObjectClickedPos].guid;
                            }
                        }
                    }
                    levelObjectClickedPos = -1;
                }
            }
        }
    }

    private void RemoveLevelObject(string guid)
    {
        SerializedProperty assignedLevelObjectProperty = serializedObject.FindProperty("preferredItems").FindPropertyRelative("levelObjects");
        int indexToRemove = -1;

        for (int i = 0; i < assignedLevelObjectProperty.arraySize; i++)
        {
            if (assignedLevelObjectProperty.GetArrayElementAtIndex(i).stringValue == guid)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove > -1)
        {
            assignedLevelObjectProperty.DeleteArrayElementAtIndex(indexToRemove);
        }

    }

    #endregion

    #endregion

    #endregion

    #endregion

    #region Context Menu Actions

    public void OpenCurrentLevel()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode != null && lNode.assignedScenePath != null && lNode.assignedScenePath.Length > 0)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene currScene = EditorSceneManager.GetSceneAt(i);

                if (!LevelEditorTool.IsLevelEditorInitialized)
                {
                    LevelEditorMenu.OpenLevelEditor();
                }

                if (LevelEditorUtility.GetObjectGuid(currScene.name) != null)
                    EditorSceneManager.CloseScene(currScene, true);
            }
             
            Scene openedLevel = EditorSceneManager.OpenScene(lNode.assignedScenePath, OpenSceneMode.Additive);
            LevelEditorMenu.OpenLevelEditor();
        }
    }

    public override void OnRename()
    {
        LevelNode lNode = target as LevelNode;
        if (lNode != null)
        {
            lNode.levelName = lNode.name;
        }
    }

    public override bool OnRemove()
    {
        LevelNode lNode = target as LevelNode;
        if(lNode != null)
        {
            //Remove Thumbnail
            string thumbnailPath = AssetDatabase.GetAssetPath(LevelController.Instance.GetLevelThumbnail(lNode.guid));
            if (!string.IsNullOrEmpty(thumbnailPath))
            {
                FileUtil.DeleteFileOrDirectory(thumbnailPath);
                FileUtil.DeleteFileOrDirectory($"{thumbnailPath}.meta");
            }

            if (!string.IsNullOrEmpty(lNode.assignedSceneName) && !string.IsNullOrEmpty(lNode.assignedScenePath))
            {
                //Close scene
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    Scene scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.name == lNode.assignedSceneName)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }

                //Remove scene file
                FileUtil.DeleteFileOrDirectory(lNode.assignedScenePath);
                FileUtil.DeleteFileOrDirectory($"{lNode.assignedScenePath}.meta");
            }
            return true;
        }
        return false;
    }

    #endregion

    #region Level Exit Connections

    private void RebuildLevelExitConnections()
    {
        Debug.Log("Level Exits updated");
        LevelNode lNode = target as LevelNode;

        if (lNode != null)
        {
            //Clear all dynamic ports which have no Level Exit
            List<string> unusedNodePorts = new List<string>();

            foreach(NodePort port in lNode.DynamicPorts)
            {
                string portGuid = LevelEditorUtility.GetObjectGuid(port.fieldName);
                bool unusedPort = true;
                foreach(LevelExit exit in lNode.levelExits)
                {
                    if(exit.guid == portGuid)
                    {
                        unusedPort = false;
                        break;
                    }
                }
                if(unusedPort)
                {
                    unusedNodePorts.Add(port.fieldName);
                }
            }

            foreach(string portFieldName in unusedNodePorts)
            {
                lNode.RemoveDynamicPort(portFieldName);
            }


            //Add missing dynamic ports
            foreach (LevelExit exit in lNode.levelExits)
            {
                NodePort levelPort = lNode.GetPort(exit.guid);

                if(levelPort == null)
                {
                    lNode.AddDynamicBoth(typeof(LevelNode), Node.ConnectionType.Override, Node.TypeConstraint.Strict, exit.guid);
                }
            }
            

            //Verify node connections
            lNode.VerifyConnections();
        }
    }

    private Vector2 WorldToNodePosition(Vector2 worldPos, LevelNode lNode, Rect thumbnailRect)
    {
        if(lNode != null)
        {
            Vector2 boundsUpperLeftCorner = new Vector2(lNode.levelBoundaries.position.x - (lNode.levelBoundaries.width/2f), lNode.levelBoundaries.position.y + (lNode.levelBoundaries.height / 2f));
            Vector2 posRelativeToUpperLeftCorner = new Vector2(worldPos.x - boundsUpperLeftCorner.x, boundsUpperLeftCorner.y - worldPos.y) / lNode.levelBoundaries.size;
            Vector2 nodePosition = posRelativeToUpperLeftCorner * thumbnailRect.size;
            return nodePosition + thumbnailRect.position;
        }
        return worldPos;
    }

    #endregion
}
