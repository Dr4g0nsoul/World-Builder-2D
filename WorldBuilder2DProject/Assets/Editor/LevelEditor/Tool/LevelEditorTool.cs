﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [EditorTool("Level Editor")]
    public class LevelEditorTool : EditorTool
    {
        #region Settings Singleton

        private static readonly string s_prefLocation = "LevelEditor/LevelEditorSettings";
        private static LevelEditorSettings s_levelEditorSettings;

        public static LevelEditorSettings GetLevelEditorSettings()
        {
            if (s_levelEditorSettings == null)
            {
                s_levelEditorSettings = Resources.Load<LevelEditorSettings>(s_prefLocation);
            }
            return s_levelEditorSettings;
        }

        #endregion


        // Serialize this value to set a default value in the Inspector.
        [SerializeField]
        private Texture2D m_ToolIcon;
        private GUIContent m_IconContent;

        // Level editor main references
        private LevelEditorSettings levelEditorSettings;
        private Transform levelEditorRoot;
        private Transform levelRoot;

        //Mouse Block
        private bool blockMouse = false;
        private bool _hoveringButton = false;
        private bool HoveringButton
        {
            get => _hoveringButton;
            set
            {
                if (value != _hoveringButton && SceneView.currentDrawingSceneView != null)
                    SceneView.currentDrawingSceneView.Repaint();
                _hoveringButton = value;
            }
        }

        //Window preferences
        private GUISkin guiSkin;

        //Messagebox preferences
        private Vector2 messageboxSize = new Vector2(400f, 140f);

        //Menu Bar
        private float menuBarHeight = 50f;

        //Object Picker
        private float objectPickerVerticalOffset;
        private float objectPickerVerticalSizeReduction;
        private RectOffset objectPickerMargin;

        //Icon button preferences
        private readonly float menuElementImageToHeaderRatio = .65f;

        //Toolbar preferences
        private readonly float toolbarHeight = 30f;
        private readonly float toolbarMargin = 5f;

        //Sorting values
        private int selectedSortingAlgorithm = 0;
        private string sortingSearchString = "";

        //Level object items preferences
        private readonly Vector2 loiContainerSizeReduction = new Vector2(8f, 10f); //Right, Down
        private Vector2 loiContainerScrollPos;
        private readonly float loiContainerTopOffset = 3f;
        private readonly float loiItemMargin = 15f;
        private readonly int loiNumberPerRow = 4;
        private Rect loiLastValidContainer;
        private LevelEditorMenuState currLevelEditorState = LevelEditorMenuState.None;

        //Add Prefab Dialog
        //-->Margin from bottom right corner, size is absolute
        private readonly Vector4 prefabDialogBounds = new Vector4(15f, 10f, 300f, 80f);

        #region Initialization / DeInitialization
        void OnEnable()
        {
            //Disable mouse
            SceneView.beforeSceneGui += BlockMouse;

            //Setup Tool icon
            m_ToolIcon = Resources.Load<Texture2D>($"{LevelEditorStyles.ICON_ROOT}LevelEditorIcon");
            m_IconContent = new GUIContent()
            {
                image = m_ToolIcon,
                text = "Level Editor",
                tooltip = "Custom Level Editor"
            };

            //Grab GUI skin
            guiSkin = Resources.Load<GUISkin>("LevelEditor/Skin/LESkin");
            if (guiSkin == null)
            {
                Debug.LogError("LevelEditorTool: GUI Skin not found!");
            }

            LevelEditorStyles.RefreshStyles();
            levelEditorSettings = GetLevelEditorSettings();

            HoveringButton = false;
            blockMouse = false;

            if (currLevelEditorState == LevelEditorMenuState.None)
                currLevelEditorState = LevelEditorMenuState.SelectCategory;

            SetupVariables();
        }

        private void SetupVariables()
        {
            //Messagebox preferences
            messageboxSize = new Vector2(400f, 140f);

            //Menu Bar
            menuBarHeight = 50f;

            //Object Picker
            objectPickerVerticalOffset = 0f;
            objectPickerVerticalSizeReduction = 0f;
            objectPickerMargin = new RectOffset(10, 10, 10, 10);

        }

        private void OnDisable()
        {
            //Disable mouse
            SceneView.beforeSceneGui -= BlockMouse;
        }

        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        #region Mouse Block

        private void BlockMouse(SceneView view)
        {
            if (Tools.current == Tool.Custom)
            {
                Event e = Event.current;
                if (e.type == EventType.MouseUp || e.type == EventType.MouseDown)
                {
                    if (blockMouse)
                    {
                        e.Use();
                    }

                }
                else if (e.type == EventType.Repaint)
                {
                    HoveringButton = false;
                }
                else if (view == SceneView.currentDrawingSceneView && e.type == EventType.KeyUp)
                {
                    if (e.keyCode == KeyCode.F10)
                        view.maximized = !view.maximized;
                    else if (e.keyCode == KeyCode.F11)
                        LevelEditorStyles.RefreshStyles();
                }
            }
        }

        #endregion

        // This is called for each window that your tool is active in. Put the functionality of your tool here.s
        public override void OnToolGUI(EditorWindow window)
        {
            blockMouse = false;

            //Fix GUI breaking randomly
            GUI.skin = guiSkin;

            //Get scene camera
            GameObject sceneCam = GameObject.Find("SceneCamera");
            if (sceneCam != null)
            {

                //Compute bounds
                Rect cameraBounds = sceneCam.GetComponent<Camera>().pixelRect;

                //Debug: Draw Window
                //ShowMessage(cameraBounds, "Test Window", "Test message asdklfjsdfkl asdkjasld woqpie c msldj asopid  klsa di asijklas jkaldkl asdkljaskjdkas klas djklasd as kdjaskl dald dklasdj aklsdj");
                //ShowButtonMessage(cameraBounds, "Test Window", "aaskljdh ASDqwiom askldj aö da spoi wqop sa opasiod", 
                //    "Button text", null);

                PrefabStage currPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                //Show add item menu
                if (currPrefabStage != null)
                {
                    //DrawAddNewPrefabDialog(cameraBounds, currPrefabStage);
                }
                //--- Checks before drawing GUI ---
                else if (levelEditorSettings == null)
                {
                    ShowButtonMessage(cameraBounds, "No Level Editor setting found!", "Please add one at Resources/LevelEditor with the name \"LevelEditorSettings\".\nOr just press the button below:", "Create Level Editor Settings", () => CreateLevelEditorSettings());
                }
                else if (levelEditorSettings.levelEditorRootTag == null || levelEditorSettings.levelEditorRootTag.Length <= 0
                    || levelEditorSettings.levelEditorRootTag == "Untagged")
                {
                    ShowMessage(cameraBounds, "Level Root Tag missing!", "Set the tag for the game object who acts as root for the level strucure.");
                }
                else
                {
                    //--- Setup Level Editor Root ---
                    if (levelEditorRoot == null)
                    {
                        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(levelEditorSettings.levelEditorRootTag);
                        if (gameObjects.Length > 1)
                        {
                            ShowMessage(cameraBounds, "Too many level editor roots!", $"There are multiple objects that share the root tag \"{levelEditorSettings.levelEditorRootTag}\"");
                        }
                        else if (gameObjects.Length < 1)
                        {
                            ShowButtonMessage(cameraBounds, "Almost ready!", "Press to setup the level editor", "Start Level Editor", () => InstantiateLevelEditor());
                        }
                        else
                        {
                            levelEditorRoot = gameObjects[0].transform;
                        }
                    }
                    else
                    {

                        //TODO: --- Level Select ---


                        //--- Draw GUI ---
                        DrawGui(cameraBounds);

                        /*
                        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(levelEditorSettings.levelObjectsRootTag);
                        if (gameObjects.Length > 1)
                        {
                            ShowMessage(cameraBounds, "Too many level roots!", $"There are multiple objects that share the root tag \"{levelEditorSettings.levelObjectsRootTag}\"");
                        }
                        else if (gameObjects.Length < 1)
                        {
                            ShowButtonMessage(cameraBounds, "Almost ready!", "Press to instantiate level", () => InstantiateLevel());
                        }
                        else
                        {
                            //--- Draw Level Editor ---
                            DrawGUI(cameraBounds);
                        }
                        */
                    }
                }
                //}


                //Does the mouse input need to be blocked
                if (HoveringButton)
                {
                    blockMouse = false;
                    SceneView.currentDrawingSceneView.Repaint();
                }

            }
        }

        #endregion

        #region GUI

        #region General

        public void DrawGui(Rect screenRect)
        {
            //Compute Element Rectangles

            //-Get objectPicker rect
            Rect objectPickerRect = new Rect()
            {
                position = screenRect.position - Vector2.up * objectPickerVerticalOffset,
                size = screenRect.size - Vector2.up * objectPickerVerticalSizeReduction
            };
            //-Reduce by margin
            //--Vertical
            objectPickerRect.position += Vector2.up * (objectPickerMargin.top + menuBarHeight);
            objectPickerRect.size -= Vector2.up * ((objectPickerMargin.top + menuBarHeight) + objectPickerMargin.bottom);
            //--Horizontal
            objectPickerRect.position += Vector2.right * objectPickerMargin.right;
            objectPickerRect.size -= Vector2.right * (objectPickerMargin.right + objectPickerMargin.left);

            //-Get menuBarRect rect
            Rect menuBarRect = new Rect()
            {
                position = objectPickerRect.position - Vector2.up * menuBarHeight,
                size = new Vector2(objectPickerRect.size.x, menuBarHeight)
            };


            //Draw GUI Elements
            Handles.BeginGUI();

            DrawMenuBar(menuBarRect);
            DrawObjectPicker(objectPickerRect);

            Handles.EndGUI();
        }

        #endregion

        #region Menu Bar

        #region General

        public void DrawMenuBar(Rect menuBarRect)
        {
            GUILayout.BeginArea(menuBarRect, LevelEditorStyles.MenuBar);
            GUILayout.BeginHorizontal();
            DrawMenuCategories();
            GUILayout.Space(menuBarRect.size.x * 0.075f);
            DrawMenuPreferredItems();
            GUILayout.Space(menuBarRect.size.x * 0.09f);
            DrawMenuLayers();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            float optionsRectWidth = 2f * (LevelEditorStyles.MenuButtonCircle.margin.left 
                + LevelEditorStyles.MenuButtonCircle.margin.right 
                + LevelEditorStyles.MenuButtonCircle.fixedWidth);

            Rect optionButtonsRect = new Rect()
            {
                position = new Vector2(menuBarRect.x + menuBarRect.width - LevelEditorStyles.MenuBar.padding.right - optionsRectWidth, menuBarRect.y + LevelEditorStyles.MenuButtonCircle.margin.top),
                size = new Vector2(optionsRectWidth, menuBarRect.height)
            };
            GUILayout.BeginArea(optionButtonsRect);
            GUILayout.BeginHorizontal();
            DrawMenuOther();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        #endregion

        #region Categories

        private void DrawMenuCategories()
        {
            GUILayout.Button("C1", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("C2", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("C3", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("C4", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("C5", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("C6", LevelEditorStyles.MenuButtonCircle);
        }

        #endregion

        #region Preferred Items

        private void DrawMenuPreferredItems()
        {
            GUILayout.Button("PI1", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("PI2", LevelEditorStyles.MenuButtonCircle);
        }

        #endregion

        #region Layers

        private void DrawMenuLayers()
        {
            GUILayout.Button("L1", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("L2", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("L3", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("L4", LevelEditorStyles.MenuButtonCircle);
        }

        #endregion

        #region Search/Settings

        private void DrawMenuOther()
        {
            GUILayout.Button("O1", LevelEditorStyles.MenuButtonCircle);
            GUILayout.Button("O2", LevelEditorStyles.MenuButtonCircle);
        }

        #endregion

        #endregion

        #region Object Picker

        #region General

        public void DrawObjectPicker(Rect objectPickerRect)
        {
            GUILayout.BeginArea(objectPickerRect, LevelEditorStyles.EditorContainer);
            GUILayout.EndArea();
        }

        #endregion

        #region Quick Select Bar
        #endregion

        #region All Objects
        #endregion

        #endregion

        #endregion

        /*
        #region GUI Old

        private void DrawGUI(Rect windowRect)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(windowRect, LevelEditorStyles.EditorContainer);
            GUILayout.BeginVertical();

            Rect headerRect = DrawHeader(windowRect);

            //--- Item Pane ---
            //Compute space to work with
            Rect itemPaneContainer = new Rect()
            {
                position = new Vector2(headerRect.position.x, headerRect.position.y + headerRect.height),
                size = new Vector3(windowRect.size.x, windowRect.size.y - headerRect.size.y)
            };
            //Fixing weird bug with rect sometimes being none
            if (headerRect.height > 2f) //Valid header rect
            {
                loiLastValidContainer = itemPaneContainer;
            }
            SetItemGUIState(DrawItemPane(loiLastValidContainer, GetCurrentEditorItems()));



            //isTestSelected = DrawLevelObjectButton(windowRect.size / 2f, windowRect.width, new LevelEditorItem()
            //{
            //    name = "Test name",
            //    thumbnail = m_ToolIcon,
            //    description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam fringilla pellentesque porttitor. In dignissim, ante ac faucibus varius, enim dui molestie justo, sit amet suscipit nibh nibh nec lorem. Curabitur."
            //});

            GUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();

        }

        #region Header

        private Rect DrawHeader(Rect windowRect)
        {
            GUILayout.BeginVertical(LevelEditorStyles.ElementContainer);
            GUILayout.Space(10f);

            //Draw Sorting Toolbar
            selectedSortingAlgorithm = DrawSortToolbar(windowRect.width, selectedSortingAlgorithm);

            GUILayout.Space(10f);

            sortingSearchString = GUILayout.TextField(sortingSearchString, LevelEditorStyles.SearchField, GUILayout.Height(30f));
            EnableMouse();

            GUILayout.Space(15f);

            GUILayout.EndVertical();
            return GUILayoutUtility.GetLastRect();
        }

        private int DrawSortToolbar(float width, int selected)
        {
            width -= toolbarMargin * 2f;


            GUILayout.BeginHorizontal();
            GUILayout.Space(1f);


            //Toolbar buttons
            if (selected == 0)
            {
                GUILayout.Button(LevelEditorStyles.ToolbarIcons[0], LevelEditorStyles.ToolbarSelected,
                    GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight));
            }
            else if (GUILayout.Button(LevelEditorStyles.ToolbarIcons[0], LevelEditorStyles.Toolbar,
                GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight)))
            {
                selected = 0;
            }

            if (selected == 1)
            {
                GUILayout.Button(LevelEditorStyles.ToolbarIcons[1], LevelEditorStyles.ToolbarSelected,
                    GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight));
            }
            else if (GUILayout.Button(LevelEditorStyles.ToolbarIcons[1], LevelEditorStyles.Toolbar,
                GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight)))
            {
                selected = 1;
            }

            if (selected == 2)
            {
                GUILayout.Button(LevelEditorStyles.ToolbarIcons[2], LevelEditorStyles.ToolbarSelected,
                    GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight));
            }
            else if (GUILayout.Button(LevelEditorStyles.ToolbarIcons[2], LevelEditorStyles.Toolbar,
                GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight)))
            {
                selected = 2;
            }

            if (selected == 3)
            {
                GUILayout.Button(LevelEditorStyles.ToolbarIcons[3], LevelEditorStyles.ToolbarSelected,
                    GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight));
            }
            else if (GUILayout.Button(LevelEditorStyles.ToolbarIcons[3], LevelEditorStyles.Toolbar,
                GUILayout.Width(width / 4f - 4f), GUILayout.Height(toolbarHeight)))
            {
                selected = 3;
            }

            GUILayout.EndHorizontal();

            //Check if mouse is on any of the buttons
            EnableMouse();


            return selected;
        }

        #endregion

        #region Items

        private int DrawItemPane(Rect itemPaneRect, List<LevelEditorItemWithId> items)
        {
            int ret = -1;
            if (items != null && items.Count > 0)
            {
                //Adjust panel size
                itemPaneRect.height -= loiContainerSizeReduction.y;
                itemPaneRect.width -= loiContainerSizeReduction.x;
                itemPaneRect.position += Vector2.up * loiContainerTopOffset;
                //itemPaneRect.position += Vector2.left * 5f;
                GUILayout.BeginArea(itemPaneRect, LevelEditorStyles.ElementContainer);
                loiContainerScrollPos = GUILayout.BeginScrollView(loiContainerScrollPos, GUILayout.Width(itemPaneRect.width - 5f), GUILayout.Height(itemPaneRect.height));
                GUILayout.BeginVertical();
                //Draw Elements
                //Vertical Positions
                float vertPos = 0f;
                float horPos = -10f;
                float itemSize = itemPaneRect.width / loiNumberPerRow;
                int i = 0;
                while (i < items.Count)
                {
                    for (int j = 0; j < loiNumberPerRow; j++)
                    {
                        Rect itemRect = new Rect()
                        {
                            position = new Vector2(horPos, vertPos) + Vector2.one * (loiItemMargin / 2f),
                            size = Vector2.one * itemSize - Vector2.one * loiItemMargin
                        };
                        if (DrawLevelObjectButton(itemRect, items[i].item))
                            ret = items[i].id;
                        horPos += itemSize;
                        i++;
                        if (i >= items.Count)
                            break;
                    }
                    GUILayout.Label("", GUILayout.Height(itemSize));
                    vertPos += itemSize;
                    horPos = -10f;
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                EnableMouse();
                GUILayout.EndArea();
            }
            return ret;
        }

        private bool DrawLevelObjectButton(Rect itemRect, LevelEditorItem levelEditorItem)
        {

            bool clicked = false;

            //Create Button
            GUILayout.BeginArea(itemRect);
            if (levelEditorItem.description != null && levelEditorItem.description.Length > 0)
                GUILayout.BeginVertical(new GUIContent("", levelEditorItem.description), LevelEditorStyles.MenuElement);
            else
                GUILayout.BeginVertical(LevelEditorStyles.MenuElement);

            //Draw button info
            if (levelEditorItem.thumbnail != null)
            {
                GUILayout.Label(levelEditorItem.thumbnail, LevelEditorStyles.MenuElementImage,
                    GUILayout.Height(itemRect.height * menuElementImageToHeaderRatio));
            }
            else
            {
                GUILayout.Label(m_ToolIcon, LevelEditorStyles.MenuElementImage,
                    GUILayout.Height(itemRect.height * menuElementImageToHeaderRatio));
            }

            if (levelEditorItem.name == null || levelEditorItem.name.Length < 1)
            {
                GUILayout.Label("Unnamed Item", LevelEditorStyles.MenuElementText);
            }
            else
            {
                GUILayout.Label(levelEditorItem.name, LevelEditorStyles.MenuElementText);
            }
            GUILayout.EndVertical();
            clicked = CreateInvisibleButton(GUILayoutUtility.GetLastRect());
            EnableMouse();
            GUILayout.EndArea();
            return clicked;
        }

        #endregion

        #endregion

        */
        #region Utility

        #region Message Boxes

        private void ShowMessage(Rect cameraBounds, string header, string message)
        {
            Rect messageRect = new Rect()
            {
                position = new Vector2(cameraBounds.center.x - messageboxSize.x / 2f, cameraBounds.center.y - messageboxSize.y / 2f),
                size = messageboxSize
            };

            Handles.BeginGUI();
            GUILayout.BeginArea(messageRect);
            GUILayout.BeginVertical(LevelEditorStyles.Messagebox);
            GUILayout.Label(header, LevelEditorStyles.MessageboxHeader);
            GUILayout.Label(message, LevelEditorStyles.MessageboxText);
            GUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();

            BlockMouseInArea(messageRect);
        }

        private void ShowButtonMessage(Rect cameraBounds, string header, string text, string buttonText, Action callback)
        {
            Rect messageRect = new Rect()
            {
                position = new Vector2(cameraBounds.center.x - messageboxSize.x / 2f, cameraBounds.center.y - messageboxSize.y / 2f),
                size = messageboxSize
            };

            DrawButtonMessage(messageRect, header, text, buttonText, callback);
        }

        private void DrawButtonMessage(Rect messageRect, string header, string text, string buttonText, Action callback)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(messageRect);
            GUILayout.BeginVertical(LevelEditorStyles.Messagebox);
            GUILayout.Label(header, LevelEditorStyles.MessageboxHeader);
            GUILayout.Label(text, LevelEditorStyles.MessageboxText);
            if (GUILayout.Button(buttonText, LevelEditorStyles.MessageboxButton) && callback != null)
            {
                callback.Invoke();
            }
            EnableMouse();
            GUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        #endregion

        #region Mouse and Buttons

        private void EnableMouse()
        {
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                HoveringButton = true;
            }
        }

        private void EnableMouse(Rect containingRect)
        {
            if (containingRect.Contains(Event.current.mousePosition))
            {
                HoveringButton = true;
            }
        }

        private void BlockMouseInArea(Rect containingRect)
        {
            //Debug.Log(containingRect);
            if (containingRect.Contains(Event.current.mousePosition))
            {
                blockMouse = true;
            }
        }

        private bool CreateInvisibleButton(Rect rect)
        {
            GUI.color = Color.clear;
            bool ret = GUI.Button(rect, "");
            GUI.color = Color.white;
            return ret;
        }

        #endregion

        #region Button Functions

        #region Initialization

        private void CreateLevelEditorSettings()
        {
            levelEditorSettings = GetLevelEditorSettings();

            if (levelEditorSettings == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/", "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/Resources/LevelEditor"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources/", "LevelEditor");
                }

                AssetDatabase.CreateAsset(new LevelEditorSettings(), "Assets/Resources/LevelEditor/LevelEditorSettings.asset");
                levelEditorSettings = GetLevelEditorSettings();
            }
        }

        private void InstantiateLevelEditor()
        {
            GameObject obj = new GameObject();
            obj.name = "Level Container";
            obj.tag = levelEditorSettings.levelEditorRootTag;
            Selection.activeTransform = obj.transform;
        }

        #endregion

        #endregion

        #endregion
        /*

        #region Level Editor

        private void InstantiateLevel()
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(levelEditorSettings.levelPrefab, Selection.activeTransform);
            Selection.activeTransform = obj.transform;
        }

        private List<LevelEditorItemWithId> GetCurrentEditorItems()
        {
            List<LevelEditorItemWithId> items = new List<LevelEditorItemWithId>();

            switch (currLevelEditorState)
            {
                case LevelEditorMenuState.SelectCategory:
                    foreach(LevelEditorCategory category in levelEditorSettings.levelEditorCategories)
                    {
                        items.Add(new LevelEditorItemWithId()
                        {
                            id = category.id,
                            item = category.item
                        });
                    }
                    break;
                case LevelEditorMenuState.SelectSubCategory:
                    break;
                case LevelEditorMenuState.SelectLevelObject:
                    break;
            }

            return items;
        }

        private void SetItemGUIState(int itemSelectionResult)
        {
            if(itemSelectionResult >= 0)
            {
                Debug.Log($"Item with id {itemSelectionResult} was selected");
            }
        }

        #endregion

        #region Add Prefab To Level Objects

        private void DrawAddNewPrefabDialog(Rect windowRect, PrefabStage currPrefabStage)
        {
            Vector2 bottomRight = new Vector2(windowRect.position.x + windowRect.width,
                windowRect.position.y + windowRect.height);
            Rect dialogRect = new Rect
            {
                position = new Vector2(bottomRight.x - prefabDialogBounds.z - prefabDialogBounds.x,
                    bottomRight.y - prefabDialogBounds.w - prefabDialogBounds.y),
                size = new Vector2(prefabDialogBounds.z, prefabDialogBounds.w)
            };
            DrawButtonMessage(dialogRect, "Add this prefab to the List of prefabs!", "Add Prefab", () => CreateNewLevelObject(currPrefabStage));

        }

        private void CreateNewLevelObject(PrefabStage currPrefabStage)
        {
            LevelObject lvlObj = CreateInstance<LevelObject>();
            lvlObj.objectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(currPrefabStage.prefabAssetPath);
            lvlObj.name = lvlObj.objectPrefab.name;
            lvlObj.item = new LevelEditorItem();
            lvlObj.item.name = lvlObj.name;

            //while (AssetPreview.IsLoadingAssetPreviews())
            //{
            //    ;
            //}
            //Texture2D thumbnail = AssetPreview.GetAssetPreview(lvlObj.objectPrefab);

            //lvlObj.levelEditorItem.thumbnail = new Texture2D(thumbnail.width, thumbnail.height);

            //lvlObj.levelEditorItem.thumbnail.SetPixels(thumbnail.GetPixels());
            //lvlObj.levelEditorItem.thumbnail.SetPixels32(thumbnail.GetPixels32());

            string rootPath = AssetDatabase.GetAssetPath(levelEditorSettings);
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));
            if (!AssetDatabase.IsValidFolder(rootPath + "/LevelObjects"))
                AssetDatabase.CreateFolder(rootPath, "LevelObjects");
            AssetDatabase.CreateAsset(lvlObj, rootPath + "/LevelObjects/" + lvlObj.name + ".asset");
        }

        #endregion
        */
    }

    public enum LevelEditorMenuState { None = 0, SelectCategory, SelectSubCategory, SelectLevelObject }

    public class LevelEditorItemWithId
    {
        public int id;
        public LevelEditorItem item;
    }
}