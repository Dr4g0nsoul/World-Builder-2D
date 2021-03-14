using System;
using System.Collections.Generic;
using Unglide;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using UnityEditor.SceneManagement;
using XNodeEditor;
using UnityEngine.SceneManagement;
using System.IO;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [EditorTool("Level Editor")]
    public class LevelEditorTool : EditorTool
    {
        #region Tool Singleton
        private static LevelEditorTool s_levelEditorTool;
        public static LevelEditorTool LevelEditorToolInstance
        {
            get
            {
                if (s_levelEditorTool != null)
                    return s_levelEditorTool;
                Debug.LogWarning("LevelEditorTool::LevelEditorToolInstance: Level Editor Tool not found!");
                return null;
            }
        }

        public static bool IsLevelEditorInitialized => s_levelEditorTool != null;

        #endregion


        // Serialize this value to set a default value in the Inspector.
        [SerializeField]
        private Texture2D m_ToolIcon;
        private GUIContent m_IconContent;

        // Level editor main references
        private LevelEditorSettings levelEditorSettings;
        private LevelObjectsController levelObjectsController;
        private WorldEditorGraph worldEditorGraph;
        private Camera sceneCam;

        //Initialization
        private int levelCount;
        private bool allLevelsInitialized;
        private bool openWorldEditor;

        //Timing
        private double lastTimeSinceStartup = 0f;
        private double deltaTime = 0f;
        private readonly double timeUntilBlockingReset = 0.3;
        private readonly float timeUntilRefreshVariables = 10f;
        private Tweener tweener;

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

        //Scrolling and scroll block
        private bool blockScroll = false;
        private double currScrollBlockingTime = 0;
        private float lastScrollDelta = 0;
        private readonly float scrollMultiplier = 10f;
        private bool scrollDone = false;

        //Other input
        private bool forgetNextSpacePress = false;

        //Window preferences
        private GUISkin guiSkin;
        private readonly float minWindowHeight = 550f;
        private bool drawGUI;

        //Responsive View
        private float currentResponsiveWidth;

        //Messagebox preferences
        private Vector2 messageboxSize = new Vector2(400f, 140f);

        //Menu Bar
        private float menuBarHeight = 50f;
        private LevelEditorItem[] systemMenuBarItems;
        private bool mouseHoveringMenuBar;
        //private readonly Vector3Int responsiveViewWidth = new Vector3Int(1020, 900, 700);
        private readonly Vector3Int responsiveViewWidth = new Vector3Int(1120, 1000, 800);

        //Categories
        private readonly Vector3Int menuBarCategoriesAmount = new Vector3Int(5, 3, 1);
        private List<string> selectedCategories;
        private float categoriesMoreBoxScrollPos;
        private Tween showCategoryMoreBoxAnimation;
        private float showCategoryMoreBoxAnimationValue;

        //Preferred items
        private LevelObjectsController.PreferredItemsFilterMode preferredItemsFilterMode;

        //Layers
        private readonly Vector3Int menuBarLayersAmount = new Vector3Int(3, 3, 1);
        private string selectedLayer;
        private readonly Vector3 layerMoreBoxXPos = new Vector3(465f, 380f, 350f);
        private float layersMoreBoxScrollPos;
        private Tween showLayerMoreBoxAnimation;
        private float showLayerMoreBoxAnimationValue;

        //More Box
        private readonly Vector2 moreBoxSize = new Vector2(330f, 200f);
        private readonly float moreBoxAnimationTime = .2f;

        //Search Bar
        private string searchString;
        private readonly int searchMaxChars = 25;
        private readonly float searchBarWidth = 250f;
        private Tween searchBarAnimation;
        private float searchBarAnimationValue;
        private readonly float searchBarAnimationTime = .2f;
        private bool searchFocused;

        //Settings button
        private bool openLevelEditorSettings = false;

        //Object Picker
        private float objectPickerVerticalOffset;
        private float objectPickerVerticalSizeReduction;
        private RectOffset objectPickerMargin;

        //Quick Select Bar
        private readonly float quickSelectBarHeight = 70f;
        private float quickSelectBarScrollPosition = 0f;
        private Tween objectDrawerToggleAnimation;

        //Object Drawer
        private float objectDrawerScrollPosition = 0f;
        private float objectDrawerHeight;
        private bool objectDrawerHidden = true;
        private bool objectDrawerHiddenComplete = true;
        private SortedDictionary<string, LevelObject> drawerLevelObjects;

        //Hover Box
        private readonly Vector2 hbOffsetFromMouse = new Vector2(0, 0);
        private string _hbText;
        private string hbGUIText;
        private string HoverText
        {
            get
            {
                return _hbText;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    if (hbShowing) {
                        hbShowing = false;
                        HideHoverBox();
                    }
                }
                else
                {
                    if(!hbShowing)
                    {
                        hbShowing = true;
                        ShowHoverBox();
                    }
                    _hbText = value;
                }
            }
        }
        private bool hbShowing;
        private Rect hbRect;
        private Rect hbTextRect;
        private float hbOpacity;
        private Tween hbAnimation;
        private readonly float hbFadeDuration = 0.7f;
        private readonly float hbShowDelay = 1.5f;
        private readonly float hbHideDelay = .5f;

        //Object placement
        private bool inObjectPlacementMode;
        private ObjectToPlace objectToPlace;
        private GameObject temporaryObject;
        private bool canObjectBePlaced;
        private Transform levelRootTransform;

        //Add Prefab Dialog
        //-->Margin from bottom right corner, size is absolute
        private readonly Vector4 prefabDialogBounds = new Vector4(15f, 10f, 300f, 120f);
        private readonly Vector4 prefabDialogSelectBounds = new Vector4(15f, 10f, 300f, 150f);
        private Type[] levelObjectTypes;
        private string[] levelObjectTypeNames;
        private int levelObjectTypeSelection;

        #region Initialization / DeInitialization
        void OnEnable()
        {
            s_levelEditorTool = this;

            //Events
            SceneView.beforeSceneGui += ToolInput;
            SceneView.beforeSceneGui += WindowFocus;
            EditorSceneManager.sceneSaved += SceneSaved;

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
            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            worldEditorGraph = LevelController.Instance.GetWorldEditorGraph();

            //Initialization
            levelCount = 0;
            allLevelsInitialized = false;
            openWorldEditor = false;

            HoveringButton = false;
            blockMouse = false;
            forgetNextSpacePress = false;

            SetupVariables();
        }

        private void SetupVariables()
        {
            drawGUI = false;

            //Main references
            levelObjectsController = LevelObjectsController.Instance;

            //Responsive View
            currentResponsiveWidth = 0f;

            //Animation
            tweener = new Tweener();

            //Messagebox preferences
            messageboxSize = new Vector2(400f, 140f);

            //Menu Bar
            menuBarHeight = 50f;
            SetupMenuBarIcons();

            //Categories
            selectedCategories = new List<string>();
            categoriesMoreBoxScrollPos = 0f;
            showCategoryMoreBoxAnimationValue = 0f;

            //Preferred Items
            preferredItemsFilterMode = LevelObjectsController.PreferredItemsFilterMode.None;

            //Layers
            selectedLayer = null;
            layersMoreBoxScrollPos = 0f;
            showLayerMoreBoxAnimationValue = 0f;

            //Search
            searchString = "";
            searchBarAnimationValue = 0f;
            searchFocused = false;

            //Settings button
            openLevelEditorSettings = false;

            //Object Picker
            objectPickerVerticalOffset = 0f;
            objectPickerVerticalSizeReduction = 50f;
            objectPickerMargin = new RectOffset(10, 10, 10, 10);
            objectDrawerHidden = true;
            objectDrawerHiddenComplete = true;
            objectDrawerHeight = 0f;
            drawerLevelObjects = new SortedDictionary<string, LevelObject>();

            //Object placement
            objectToPlace.Unset();
            canObjectBePlaced = false;
            levelRootTransform = null;
            SetLevelRoot(true);

            //Hover Box
            _hbText = "";
            hbShowing = false;

            //Add Prefab Dialog
            levelObjectTypes = LevelEditorReflection.GetLevelObjectTypes();
            levelObjectTypeNames = LevelEditorReflection.GetFormattedTypeNames(levelObjectTypes);
            levelObjectTypeSelection = 0;

            //Start ressource reloader
            tweener.Timer(0f).OnComplete(() => RefreshVariables());
        }

        private void RefreshVariables()
        {
            LevelEditorStyles.RefreshStyles();
            levelObjectsController.LoadLevelObjects();
            if (levelRootTransform != null)
            {
                ReloadFilters();
            }
            tweener.Timer(timeUntilRefreshVariables).OnComplete(() => RefreshVariables());

            //Object Picker
            objectDrawerHeight = objectPickerVerticalSizeReduction
            + LevelEditorStyles.MenuButtonCircle.margin.top
            + LevelEditorStyles.EditorContainer.padding.top
            + LevelEditorStyles.EditorContainer.padding.bottom
            + quickSelectBarHeight
            + menuBarHeight;
        }

        private void SetupMenuBarIcons()
        {
            systemMenuBarItems = new LevelEditorItem[7];
            systemMenuBarItems[0] = new LevelEditorItem
            {
                name = "All Categories",
                description = "Show all categories",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/category"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[1] = new LevelEditorItem
            {
                name = "Level favorites",
                description = "Filter for objects specific to this level",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/extension"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[2] = new LevelEditorItem
            {
                name = "World favorites",
                description = "Filter for objects specific to this world",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/language"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[3] = new LevelEditorItem
            {
                name = "All layers",
                description = "Show all layers",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/layers"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[4] = new LevelEditorItem
            {
                name = "Search",
                description = "Press to show search bar",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/search"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[5] = new LevelEditorItem
            {
                name = "Options",
                description = "Display Level Editor Settings",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/miscellaneous_services"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
            systemMenuBarItems[6] = new LevelEditorItem
            {
                name = "Clear Category Filter",
                thumbnail = Resources.Load<Texture2D>(LevelEditorStyles.ICON_ROOT + "MDI/delete_forever"),
                accentColor = LevelEditorStyles.buttonHoverColor
            };
        }


        private void OnDisable()
        {
            s_levelEditorTool = null;

            //Events
            SceneView.beforeSceneGui -= ToolInput;
            SceneView.beforeSceneGui -= WindowFocus;
            //EditorSceneManager.sceneSaved -= SceneSaved;
        }

        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        #region Events

        private void ToolInput(SceneView view)
        {
            Event e = Event.current;

            //Block Keyboard input when focusing search bar
            if (GUI.GetNameOfFocusedControl() == "Search")
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    TextEditor te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                    if (te != null)
                    {
                        if (Event.current.keyCode == KeyCode.RightArrow)
                        {
                            te.cursorIndex += 1;
                            te.selectIndex = te.cursorIndex;
                            Event.current.Use();
                        }
                        else if (Event.current.keyCode == KeyCode.LeftArrow)
                        {
                            te.cursorIndex -= 1;
                            te.selectIndex = te.cursorIndex;
                            Event.current.Use();
                        }
                        else if (Event.current.keyCode == KeyCode.Return)
                        {
                            GUI.FocusControl(null);
                            ToggleSearchMenu();
                        }
                        else if (Event.current.keyCode == KeyCode.Escape)
                        {
                            searchString = "";
                            ToggleSearchMenu();
                        }
                    }
                }
            }
            //Locked to current tool
            else if (Tools.current == Tool.Custom)
            {
                if (e.type == EventType.MouseDown)
                {
                    if (blockMouse)
                    {
                        DeselectCurrentlySelectedObject();
                        e.Use();
                    }

                }
                else if (e.type == EventType.MouseUp)
                {
                    if (e.button == 0)
                    {
                        if (inObjectPlacementMode && objectToPlace.IsValid() && canObjectBePlaced)
                        {
                            objectToPlace.levelObjectEditor.SpawnObject(temporaryObject, GetLevelObjectParentTransform(objectToPlace.levelObject), Util.EditorUtility.SceneViewToWorldPos(view), Event.current.mousePosition);
                            
                            //Instantiate(objectToPlace.objectPrefab, Util.EditorUtility.SceneViewToWorldPos(view), Quaternion.identity);
                        }
                    }

                    //Toggle search menu when clicking out of it
                    if(searchFocused)
                    {
                        ToggleSearchMenu();
                    }
                }
                else if (e.type == EventType.KeyUp)
                {
                    if (e.keyCode == KeyCode.Space)
                    {
                        if (forgetNextSpacePress)
                            forgetNextSpacePress = false;
                        else
                            ToggleObjectDrawer();
                    }
                    else if(e.keyCode == KeyCode.Escape)
                    {
                        DeselectCurrentlySelectedObject();
                    }
                }
                else if (Event.current.isScrollWheel)
                {
                    lastScrollDelta = Event.current.delta.y * scrollMultiplier;
                    if (blockScroll)
                    {
                        Event.current.Use();
                    }
                }
                else if (e.type == EventType.Repaint)
                {
                    HoveringButton = false;
                }

                
            }
        }

        private void WindowFocus(SceneView view)
        {
            //Clicked Open Level Editor Settings
            if(openLevelEditorSettings)
            {
                openLevelEditorSettings = false;

                EditorUtility.FocusProjectWindow();
                ProjectWindowUtil.ShowCreatedAsset(levelEditorSettings);
                //EditorGUIUtility.PingObject(levelEditorSettings);
            }
            //Clicked on Open World Editor Window
            else if(openWorldEditor)
            {
                openWorldEditor = false;
                LevelController.Instance.OpenWorldEditor();
                
            }
            //Lost Focus
            if (EditorWindow.focusedWindow != view)
            {
                DeselectCurrentlySelectedObject();
            }
        }

        private void SceneSaved(Scene scene)
        {
            DeselectCurrentlySelectedObject();
            GenerateThumbnail(scene);
        }

        #endregion

        // This is called for each window that your tool is active in. Put the functionality of your tool here.s
        public override void OnToolGUI(EditorWindow window)
        {
            //Apply GUI Skin
            GUI.skin = guiSkin;

            deltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
            lastTimeSinceStartup = EditorApplication.timeSinceStartup;

            tweener.Update((float)deltaTime);

            if (blockScroll)
            {
                currScrollBlockingTime -= deltaTime;
                if (currScrollBlockingTime < 0)
                    blockScroll = false;
            }
            blockMouse = false;

            //Get scene camera
            GameObject sceneCamObj = GameObject.Find("SceneCamera");
            if (sceneCam == null && sceneCamObj != null)
                sceneCam = sceneCamObj.GetComponent<Camera>();
            if (sceneCam != null)
            {

                //Compute bounds
                Rect cameraBounds = sceneCam.pixelRect;

                PrefabStage currPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                //Show add item menu
                if (currPrefabStage != null)
                {
                    DrawAddNewPrefabDialog(cameraBounds, currPrefabStage);
                }
                //--- Checks before drawing GUI ---
                else if (levelEditorSettings == null || worldEditorGraph == null)
                {
                    ShowButtonMessage(cameraBounds, "Uninitialized Level Editor!", "Please press the button below to generate necessary editor assets", "Initialize Level Editor", () => CreateLevelEditorAssets());
                }
                else if (levelEditorSettings.levelEditorRootTag == null || levelEditorSettings.levelEditorRootTag.Length <= 0
                    || levelEditorSettings.levelEditorRootTag == "Untagged")
                {
                    ShowMessage(cameraBounds, "Level Root Tag missing!", "Set the tag for the game object who acts as root for the level strucure.");
                }
                else
                {
                    //--- Setup Levels ---
                    if (GetLoadedLevelsCount() < 2)
                    {
                        ShowButtonMessage(cameraBounds, "Open Level From World Editor!", "Right click on an empty space in the World Graph to create a level", "Open World Editor", () => OpenWorldEditor());
                        /*
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
                        */
                    }
                    else if (!CheckLevelsInitialized())
                    {
                        ShowButtonMessage(cameraBounds, "Initialize Levels!", "Some levels appear to not be initialized. Use the button below to Initialize a level scene. (Contents of that scene will be wiped!)", "Initialize Level Scenes", () => InitializeLevelScenes());
                    }
                    else
                    {

                        //Set level root
                        SetLevelRoot();

                        if (cameraBounds.width < responsiveViewWidth.z || cameraBounds.height < minWindowHeight)
                        {
                            ShowMessage(cameraBounds, "Window too small!", "Resize window to show level editor.");
                        }
                        else
                        {

                            //Fix Layout <-> Draw bug
                            if(Event.current.type == EventType.Layout)
                            {
                                drawGUI = true;
                                currentResponsiveWidth = cameraBounds.width;
                            }

                            //--- Draw GUI ---
                            if (drawGUI)
                            {
                                DrawGui(cameraBounds);
                            }
                            /*
                            try
                            {
                            }
                            catch (ArgumentException ex)
                            {
                                //Remove harmless error Message
                                if (!ex.Message.Contains("Getting control 1's position in a group with only 1 controls when doing repaint"))
                                    Debug.LogError(ex.Message);
                            }
                            */
                        }
                    }
                }

                //Does the mouse input need to be blocked
                if (HoveringButton)
                {
                    blockMouse = false;
                    SceneView.currentDrawingSceneView.Repaint();
                }

                //Reset scroll delta
                if (scrollDone || !blockScroll)
                    lastScrollDelta = 0f;

            }
        }

        #endregion

        #region GUI

        #region General

        private void DrawGui(Rect screenRect)
        {
            //Compute Element Rectangles
            //- If objectPicker hidden set offset according to screen height
            if (objectDrawerHiddenComplete)
            {
                objectPickerVerticalOffset = objectDrawerHeight - sceneCam.pixelHeight;
            }
            //-Get objectPicker rect
            Rect objectPickerRect = new Rect()
            {
                position = screenRect.position - Vector2.up * (objectPickerVerticalOffset - objectPickerVerticalSizeReduction),
                size = screenRect.size - Vector2.up * objectPickerVerticalSizeReduction
            };
            //-Reduce by margin
            //--Vertical
            objectPickerRect.position += Vector2.up * (objectPickerMargin.top + menuBarHeight);
            objectPickerRect.size -= Vector2.up * ((objectPickerMargin.top + menuBarHeight) + objectPickerMargin.bottom);
            //--Horizontal
            objectPickerRect.position += Vector2.right * objectPickerMargin.right;
            objectPickerRect.size -= Vector2.right * (objectPickerMargin.right + objectPickerMargin.left);
            //--Clamp by morebox height
            objectPickerRect.position = new Vector2(objectPickerRect.position.x, Mathf.Max(objectPickerRect.position.y, GetMoreBoxHeight()));
            objectPickerRect.size = new Vector2(objectPickerRect.size.x, Mathf.Min(objectPickerRect.size.y, sceneCam.pixelHeight - GetMoreBoxHeight() - objectPickerMargin.bottom));

            //-Get menuBarRect rect
            Rect menuBarRect = new Rect()
            {
                position = objectPickerRect.position - Vector2.up * menuBarHeight,
                size = new Vector2(objectPickerRect.size.x, menuBarHeight)
            };

            //-Reset Menu bar hovering
            if(Event.current.type == EventType.Repaint)
                mouseHoveringMenuBar = false;

            //Block mouse
            BlockMouseInArea(objectPickerRect);

            //Draw GUI Elements
            Handles.BeginGUI();

            //Reset hover box gui text
            if(Event.current.type == EventType.Repaint)
                hbGUIText = "";

            ObjectPlacement(objectPickerRect);
            DrawMenuBar(menuBarRect);
            DrawObjectPicker(objectPickerRect);

            

            HoverText = hbGUIText;
            DrawHoverBox(screenRect);

            Handles.EndGUI();

        }

        #endregion

        #region Object Placement

        private void ObjectPlacement(Rect objectPickerRect)
        {
            //Check wether Mouse is hovering over gui element or not and if a layer was selected
            if (!string.IsNullOrEmpty(selectedLayer) && Util.EditorUtility.IsMouseInsideSceneView(SceneView.currentDrawingSceneView) && !mouseHoveringMenuBar && !objectPickerRect.Contains(Event.current.mousePosition))
            {
                canObjectBePlaced = true;
            }
            else
            {
                canObjectBePlaced = false;
            }

            if (canObjectBePlaced)
            {
                //Temporary gameobject spawn
                if (objectToPlace.IsValid() && objectToPlace.levelObjectEditor.UseTemporaryIndicator)
                {
                    if (temporaryObject == null)
                    {
                        temporaryObject = Instantiate(objectToPlace.levelObject.objectPrefab);
                        temporaryObject.name = $"temp [{objectToPlace.levelObject.guid}]";
                    }
                    temporaryObject.transform.position = Util.EditorUtility.SceneViewToWorldPos(SceneView.currentDrawingSceneView);
                }
            }
            else if (temporaryObject != null)
            {
                //Destroy temporary object
                DestroyImmediate(temporaryObject);
                temporaryObject = null;
            }

            if(inObjectPlacementMode && string.IsNullOrEmpty(selectedLayer))
            {
                hbGUIText = "Select Layer to place object";
            }
        }

        #endregion

        #region Menu Bar

        #region General

        public void DrawMenuBar(Rect menuBarRect)
        {
            //Get Level layers based on selected object
            List<LevelLayer> levelLayers = levelObjectsController.GetLevelObjectLayers(objectToPlace.levelObject);

            GUILayout.BeginArea(menuBarRect, LevelEditorStyles.MenuBar);
            GUILayout.BeginHorizontal();
            DrawMenuCategories();
            GUILayout.Space(menuBarRect.size.x * 0.075f);
            DrawMenuPreferredItems();
            GUILayout.Space(menuBarRect.size.x * 0.09f);
            DrawMenuLayers(levelLayers);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            float optionsRectWidth = 2f * (LevelEditorStyles.MenuButtonCircle.margin.left 
                + LevelEditorStyles.MenuButtonCircle.margin.right 
                + LevelEditorStyles.MenuButtonCircle.fixedWidth);

            Rect optionButtonsRect = new Rect()
            {
                position = new Vector2(menuBarRect.x + menuBarRect.width - LevelEditorStyles.MenuBar.padding.right - optionsRectWidth - (searchBarWidth * searchBarAnimationValue), menuBarRect.y + LevelEditorStyles.MenuButtonCircle.margin.top),
                size = new Vector2(optionsRectWidth + (searchBarWidth * searchBarAnimationValue), menuBarRect.height)
            };
            GUILayout.BeginArea(optionButtonsRect);
            GUILayout.BeginHorizontal();
            DrawMenuOther();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //Menu Boxes
            categoriesMoreBoxScrollPos = DrawMoreBox(menuBarRect.position + Vector2.down * LevelEditorStyles.MoreBox.margin.bottom, 
                (index) => DrawCategoryButton(levelEditorSettings.levelObjectCategories[index + GetResponsiveAmount(menuBarCategoriesAmount)]), levelEditorSettings.levelObjectCategories.Length - GetResponsiveAmount(menuBarCategoriesAmount), categoriesMoreBoxScrollPos, showCategoryMoreBoxAnimationValue,
                systemMenuBarItems[6], () => ClearCategoryFilter());
            layersMoreBoxScrollPos = DrawMoreBox(new Vector2(menuBarRect.position.x + GetResponsiveAmount(layerMoreBoxXPos), menuBarRect.position.y - LevelEditorStyles.MoreBox.margin.bottom),
                (index) => DrawLayerButton(levelLayers[index + GetResponsiveAmount(menuBarLayersAmount)]), levelLayers.Count - GetResponsiveAmount(menuBarLayersAmount), layersMoreBoxScrollPos, showLayerMoreBoxAnimationValue);

        }

        private void DrawMenuBarButton(LevelEditorItem item, Action action, bool selected = false)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(LevelEditorStyles.MenuButtonCircle.fixedWidth, LevelEditorStyles.MenuButtonCircle.fixedHeight, LevelEditorStyles.MenuButtonCircle);

            //Hover tinting
            if (buttonRect.Contains(Event.current.mousePosition) || selected)
            {
                if (buttonRect.Contains(Event.current.mousePosition))
                {
                    mouseHoveringMenuBar = true;
                    hbGUIText = item.name;
                }

                if (item.accentColor.a > 0)
                {
                    GUI.color = item.accentColor;
                }
                else
                {
                    GUI.color = LevelEditorStyles.buttonHoverColor;
                }

            }

            GUIStyle btnStyle = selected ? LevelEditorStyles.MenuButtonCircleActive : LevelEditorStyles.MenuButtonCircle;
                

            if(GUI.Button(buttonRect, item.thumbnail, btnStyle) && action != null)
            {
                action.Invoke();
            }

            GUI.color = Color.white;
        }

        private void DrawCategoryButton(LevelObjectCategory cat)
        {
            if (cat != null)
            {
                DrawMenuBarButton(cat.item, () => ToggleCategory(cat.guid), selectedCategories.Contains(cat.guid));
            }
        }

        private void DrawLayerButton(LevelLayer layer)
        {
            if (layer != null)
            {
                DrawMenuBarButton(layer.item, () => ToggleLayer(layer.guid), selectedLayer == layer.guid);
            }
        }

        private int GetResponsiveAmount(Vector3Int amounts)
        {
            if (currentResponsiveWidth < responsiveViewWidth.x)
            {
                if (currentResponsiveWidth < responsiveViewWidth.y)
                    return amounts.z;
                else
                    return amounts.y;
            }
            return amounts.x;
        }

        private float GetResponsiveAmount(Vector3 amounts)
        {
            if (currentResponsiveWidth < responsiveViewWidth.x)
            {
                if (currentResponsiveWidth < responsiveViewWidth.y)
                    return amounts.z;
                else
                    return amounts.y;
            }
            return amounts.x;
        }

        #endregion

        #region Categories

        private void DrawMenuCategories()
        {
            for(int i = 0; i<Mathf.Min(GetResponsiveAmount(menuBarCategoriesAmount), levelEditorSettings.levelObjectCategories.Length); i++)
            {
                DrawCategoryButton(levelEditorSettings.levelObjectCategories[i]);
            }

            if (GetResponsiveAmount(menuBarCategoriesAmount) < levelEditorSettings.levelObjectCategories.Length)
            {
                DrawMenuBarButton(systemMenuBarItems[0], () => ToggleCategoryMoreBox(), showCategoryMoreBoxAnimationValue > 0f);
            }
            else if (showCategoryMoreBoxAnimationValue > 0f)
            {
                showCategoryMoreBoxAnimationValue = 0f;
            }
        }

        #endregion

        #region Preferred Items

        private void DrawMenuPreferredItems()
        {
            DrawMenuBarButton(systemMenuBarItems[1], () => ToggleLevelPreferredItems(), preferredItemsFilterMode == LevelObjectsController.PreferredItemsFilterMode.Level);
            DrawMenuBarButton(systemMenuBarItems[2], () => ToggleWorldPreferredItems(), preferredItemsFilterMode == LevelObjectsController.PreferredItemsFilterMode.World);
        }

        private void ToggleLevelPreferredItems()
        {
            if (preferredItemsFilterMode == LevelObjectsController.PreferredItemsFilterMode.Level)
            {
                preferredItemsFilterMode = LevelObjectsController.PreferredItemsFilterMode.None;
            }
            else
            {
                preferredItemsFilterMode = LevelObjectsController.PreferredItemsFilterMode.Level;
            }
            ReloadFilters();
        }

        private void ToggleWorldPreferredItems()
        {
            if (preferredItemsFilterMode == LevelObjectsController.PreferredItemsFilterMode.World)
            {
                preferredItemsFilterMode = LevelObjectsController.PreferredItemsFilterMode.None;
            }
            else
            {
                preferredItemsFilterMode = LevelObjectsController.PreferredItemsFilterMode.World;
            }
            ReloadFilters();
        }

        #endregion

        #region Layers

        private void DrawMenuLayers(List<LevelLayer> levelLayers)
        {
            
            for (int i = 0; i < Mathf.Min(GetResponsiveAmount(menuBarLayersAmount), levelLayers.Count); i++)
            {
                DrawLayerButton(levelLayers[i]);
            }

            if (GetResponsiveAmount(menuBarLayersAmount) < levelLayers.Count)
            {
                DrawMenuBarButton(systemMenuBarItems[3], () => ToggleLayerMoreBox(), showLayerMoreBoxAnimationValue > 0f);
            }
            else if (showLayerMoreBoxAnimationValue > 0f)
            {
                showLayerMoreBoxAnimationValue = 0f;
            }
        }

        #endregion

        #region Search/Settings

        private void DrawMenuOther()
        {
            if (searchBarAnimationValue <= 0f)
                DrawMenuBarButton(systemMenuBarItems[4], () => ToggleSearchMenu());
            else
                DrawSearchBar();
            DrawMenuBarButton(systemMenuBarItems[5], () => OpenLevelEditorSettings());
        }

        private void DrawSearchBar()
        {
            GUILayout.Label("", LevelEditorStyles.SearchFieldLeft);
            GUI.SetNextControlName("Search");
            searchString = GUILayout.TextField(searchString, searchMaxChars, LevelEditorStyles.SearchFieldCenter, GUILayout.Width(searchBarWidth * searchBarAnimationValue));

            if (!searchFocused)
            {
                GUI.FocusControl("Search");
                searchFocused = true;
            }
            GUILayout.Label("", LevelEditorStyles.SearchFieldRight);

            Rect searchLabel = GUILayoutUtility.GetLastRect();
            searchLabel.position = new Vector2(searchLabel.position.x - searchLabel.size.x, searchLabel.position.y);
            GUI.Label(searchLabel, systemMenuBarItems[4].thumbnail, LevelEditorStyles.MenuLabelCircle);
        }

        #endregion

        #region MoreBox

        private float DrawMoreBox(Vector2 bottomLeft, Action<int> contentDrawFunction, int itemCount, float scrollPos, float animationValue, LevelEditorItem bottomButton = null, Action bottomClickAction = null)
        {
            float newScrollPos = scrollPos;

            if (animationValue > 0f)
            {
                if (contentDrawFunction != null)
                {
                    Rect moreBoxRect = new Rect()
                    {
                        position = bottomLeft + Vector2.down * moreBoxSize.y * animationValue,
                        size = new Vector2(moreBoxSize.x, moreBoxSize.y * animationValue)
                    };

                    if (Event.current.type == EventType.Layout)
                    {
                        BlockMouseInArea(moreBoxRect);
                    }

                    if(moreBoxRect.Contains(Event.current.mousePosition))
                    {
                        mouseHoveringMenuBar = true;
                    }

                    GUILayout.BeginArea(moreBoxRect, LevelEditorStyles.MoreBox);
                    int objectsPerRow = Mathf.FloorToInt(moreBoxSize.x / (LevelEditorStyles.MenuButtonCircle.fixedWidth + LevelEditorStyles.MenuButtonCircle.margin.right + LevelEditorStyles.MenuButtonCircle.margin.left) + 1);

                    newScrollPos = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(scrollPos, 0f)), false, false).y;

                    GUILayout.BeginVertical();
                    for (int i = 0; i < itemCount; i++)
                    {
                        if (i % objectsPerRow == 0)
                            GUILayout.BeginHorizontal();

                        contentDrawFunction.Invoke(i);

                        EnableMouse();

                        if ((i + 1) % objectsPerRow == 0 || i == itemCount - 1)
                        {
                            GUILayout.EndHorizontal();
                        }
                    }

                    if (bottomButton != null && bottomClickAction != null)
                    {
                        GUILayout.BeginHorizontal(GUI.skin.GetStyle("HorizontalRule"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Clear Category Filter", LevelEditorStyles.MenuButtonSquare))
                            bottomClickAction.Invoke();
                        EnableMouse();
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();

                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        newScrollPos += lastScrollDelta;
                        scrollDone = lastScrollDelta != 0;
                    }
                    DisableScroll();
                    GUILayout.EndArea();



                    
                }
            }

            return newScrollPos;
        }

        private float GetMoreBoxHeight()
        {
            return (moreBoxSize.y + LevelEditorStyles.MoreBox.margin.bottom + LevelEditorStyles.MoreBox.margin.top + menuBarHeight) * Math.Max(showCategoryMoreBoxAnimationValue, showLayerMoreBoxAnimationValue);
        }

        #endregion

        #endregion

        #region Object Picker

        #region General

        private void DrawObjectPicker(Rect objectPickerRect)
        {
            GUILayout.BeginArea(objectPickerRect, LevelEditorStyles.EditorContainer);

            GUILayout.BeginVertical();
            GUILayout.BeginVertical(GUILayout.Height(quickSelectBarHeight));
            DrawQuickSelectBar();
            GUILayout.EndVertical();
            GUILayout.Label("All Items", LevelEditorStyles.HeaderCenteredBig);
            DrawObjectDrawer(objectPickerRect.width, objectPickerRect.height
                - LevelEditorStyles.EditorContainer.margin.top
                - LevelEditorStyles.EditorContainer.margin.bottom
                - quickSelectBarHeight
                - LevelEditorStyles.HeaderCenteredBig.fontSize
                - 50);
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        #endregion

        #region Quick Select Bar

        private void DrawQuickSelectBar() 
        {
            List<LevelObject> quickSelectItems = levelObjectsController.GetQuickSelectBar();
            if(quickSelectItems.Count > 0 || !objectDrawerHidden)
            {
                quickSelectBarScrollPosition = GUILayout.BeginScrollView(new Vector2(Mathf.Max(quickSelectBarScrollPosition, 0f), 0f), false, false).x;
                GUILayout.BeginHorizontal();
                for (int i = 0; i < levelEditorSettings.quickSelectBarSize; i++)
                {
                    if (i < quickSelectItems.Count)
                        DrawLevelObjectButton(quickSelectItems[i]);
                    else
                    {
                        Rect buttonRect = GUILayoutUtility.GetRect(LevelEditorStyles.LevelObjectButton.fixedWidth + LevelEditorStyles.LevelObjectButton.margin.left + LevelEditorStyles.LevelObjectButton.margin.right,
                            LevelEditorStyles.LevelObjectButton.fixedHeight + LevelEditorStyles.LevelObjectButton.margin.top + LevelEditorStyles.LevelObjectButton.margin.bottom,
                            GUILayout.ExpandWidth(false));
                        GUI.color = Color.grey;
                        GUI.Label(buttonRect, "", LevelEditorStyles.LevelObjectButton);
                        GUI.color = Color.white;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    quickSelectBarScrollPosition += lastScrollDelta;
                    scrollDone = lastScrollDelta != 0;
                }
                DisableScroll();
            }
            else
            {
                if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
                {
                    GUILayout.Space(10f);
                    GUILayout.Label("[ PRESS SPACE TO ACCESS ALL OBJECTS ]", LevelEditorStyles.HeaderCenteredBig);
                }
            }
        }

        #endregion

        #region All Objects

        private void DrawObjectDrawer(float containerWidth, float remainingHeight)
        {
            int objectsPerRow = Mathf.FloorToInt(containerWidth / 
                (LevelEditorStyles.LevelObjectButton.fixedWidth + LevelEditorStyles.LevelObjectButton.margin.right + LevelEditorStyles.LevelObjectButton.margin.left) + 0.23f) - 1;

            //Load level objects for drawer
            if (Event.current.type == EventType.Layout)
            {
                if (string.IsNullOrEmpty(selectedLayer) && selectedCategories.Count < 1 && preferredItemsFilterMode == LevelObjectsController.PreferredItemsFilterMode.None)
                {
                    if(string.IsNullOrEmpty(searchString))
                        drawerLevelObjects = levelObjectsController.GetAllLevelObjects();
                    else
                        drawerLevelObjects = levelObjectsController.GetFilteredLevelObjects(searchString, true);
                }
                else
                {
                    drawerLevelObjects = levelObjectsController.GetFilteredLevelObjects(searchString);
                }
            }

            int objectCount = drawerLevelObjects.Values.Count;
            objectDrawerScrollPosition = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(objectDrawerScrollPosition, 0f)), false, false, GUILayout.Height(remainingHeight)).y;

            GUILayout.BeginVertical();
            int i = 0;
            foreach (LevelObject obj in drawerLevelObjects.Values)
            {
                if (i % objectsPerRow == 0)
                    GUILayout.BeginHorizontal();

                DrawLevelObjectButton(obj);
                
                if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
                {
                    GUILayout.EndHorizontal();
                    if (i < objectCount - 1)
                        GUILayout.Space(Mathf.Max(LevelEditorStyles.LevelObjectButton.margin.right,LevelEditorStyles.LevelObjectButton.margin.left));
                }

                i++;
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                objectDrawerScrollPosition += lastScrollDelta;
                scrollDone = lastScrollDelta != 0;
            }
            DisableScroll();
        }

        private void DrawLevelObjectButton(LevelObject obj)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(LevelEditorStyles.LevelObjectButton.fixedWidth + LevelEditorStyles.LevelObjectButton.margin.left + LevelEditorStyles.LevelObjectButton.margin.right,
                    LevelEditorStyles.LevelObjectButton.fixedHeight + LevelEditorStyles.LevelObjectButton.margin.top + LevelEditorStyles.LevelObjectButton.margin.bottom,
                    GUILayout.ExpandWidth(false));
            GUI.color = levelObjectsController.GetAccentColor(obj);
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                EnableMouse();
                hbGUIText = obj.name;
            }
            GUIStyle buttonActive = (!objectToPlace.IsValid() || obj.guid != objectToPlace.levelObject.guid) ? LevelEditorStyles.LevelObjectButton : LevelEditorStyles.LevelObjectButtonActive;
            if(GUI.Button(buttonRect, "", buttonActive))
            {
                if(!objectDrawerHidden)
                    ToggleObjectDrawer();
                inObjectPlacementMode = true;
                objectToPlace.Unset();
                objectToPlace.levelObject = obj;
                objectToPlace.levelObjectEditor = LevelObjectEditorExtension.GetBaseLevelObjectEditorExtension(obj);
                levelObjectsController.AddToQuickSelectBar(obj.guid);
                List<LevelLayer> availableLayers = levelObjectsController.GetLevelObjectLayers(obj);
                if(availableLayers != null && availableLayers.Count == 1)
                {
                    selectedLayer = availableLayers[0].guid;
                }

                //Temporary Object
                if (temporaryObject != null)
                {
                    DestroyImmediate(temporaryObject);
                    temporaryObject = null;
                }
            }
            GUI.color = Color.white;
            Texture2D thumbnail = obj.item.thumbnail;
            if (thumbnail == null)
            {
                Rect cutoffRect = new Rect()
                {
                    position = new Vector2(buttonRect.position.x + LevelEditorStyles.levelObjectPreviewImageOffset.x - LevelEditorStyles.LevelObjectButton.padding.left / 2f, buttonRect.position.y + LevelEditorStyles.levelObjectPreviewImageOffset.y - LevelEditorStyles.LevelObjectButton.padding.top / 2f),
                    size = new Vector2(buttonRect.size.x - LevelEditorStyles.levelObjectPreviewImageOffset.x * 2f, buttonRect.size.y - LevelEditorStyles.levelObjectPreviewImageOffset.y * 2f)
                };
                GUI.BeginGroup(cutoffRect);
                thumbnail = AssetPreview.GetAssetPreview(obj.objectPrefab);
                buttonRect.position = Vector2.zero;
                buttonRect.size = cutoffRect.size * 1.5f;
                GUI.Label(buttonRect,
                    thumbnail, LevelEditorStyles.LevelObjectPreviewImage);
                GUI.EndGroup();
            }
            else
            {
                GUI.Label(buttonRect,
                    thumbnail, LevelEditorStyles.LevelObjectImage);
            }
        }

        private void ToggleObjectDrawer()
        {
            if(objectDrawerToggleAnimation != null)
                objectDrawerToggleAnimation.CancelAndComplete();

            if (objectDrawerHidden)
            {
                objectDrawerToggleAnimation = tweener.Tween(this, null, .2f).Ease(Ease.SineIn).OnUpdate((value) => ShowObjectDrawer(value)).OnComplete(() => ShowObjectDrawer(1f));
                objectDrawerHidden = false;
                objectDrawerHiddenComplete = false;
            }
            else
            {
                objectDrawerToggleAnimation = tweener.Tween(this, null, .2f).Ease(Ease.SineIn).OnUpdate((value) => HideObjectDrawer(value)).OnComplete(() => HideObjectDrawer(1f));
                objectDrawerHidden = true;
            }
        }

        #endregion

        #endregion

        #region Hover Box

        private void DrawHoverBox(Rect screenRect)
        {
            

            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            {
                hbRect = new Rect()
                {
                    position = Vector2.zero,
                    size = Vector2.zero
                };

                //Rect newHbTextRect = GUILayoutUtility.GetRect(new GUIContent(hbText), LevelEditorStyles.TextCentered);
                //if (newHbTextRect.size != Vector2.one)
                //    hbTextRect = newHbTextRect;
                if (LevelEditorStyles.HoverBoxText != null)
                    hbTextRect.size = LevelEditorStyles.HoverBoxText.CalcSize(new GUIContent(HoverText));

                hbRect.size = hbTextRect.size + new Vector2((LevelEditorStyles.HoverBox.padding.left + LevelEditorStyles.HoverBox.padding.right) * 2f, 
                    (LevelEditorStyles.HoverBox.padding.top + LevelEditorStyles.HoverBox.padding.bottom) * 2f);

                hbRect.position = Event.current.mousePosition 
                    - new Vector2(hbTextRect.width / 2f, hbTextRect.height) 
                    - new Vector2((LevelEditorStyles.HoverBox.padding.left + LevelEditorStyles.HoverBox.padding.right),
                        (LevelEditorStyles.HoverBox.padding.top + LevelEditorStyles.HoverBox.padding.bottom))
                    - hbOffsetFromMouse;

                if (!screenRect.Contains(Event.current.mousePosition))
                {
                    HoverText = "";
                }
            }


            //Debug.Log(hbRect.size);

            GUI.color = new Color(1f, 1f, 1f, hbOpacity);
            GUI.Box(hbRect, HoverText, LevelEditorStyles.HoverBox);
            GUI.color = Color.white;
        }

        private void HideHoverBox()
        {
            if(hbAnimation != null)
            {
                hbAnimation.CancelAndComplete();
            }
            if (hbOpacity > 0)
            {
                hbAnimation = tweener.Tween(this, null, hbFadeDuration, hbHideDelay)
                    .Ease(Ease.SineIn)
                    .OnUpdate(val => HideHoverBoxAnimation(val))
                    .OnComplete(() => HideHoverBoxAnimation(1f));
            }
            else
            {
                HideHoverBoxAnimation(1f);
            }
        }

        private void ShowHoverBox()
        {
            if (hbAnimation != null)
            {
                hbAnimation.CancelAndComplete();
            }
            if (hbOpacity < 1)
            {
                hbAnimation = tweener.Tween(this, null, hbFadeDuration, hbShowDelay)
                    .Ease(Ease.SineIn)
                    .OnUpdate(val => ShowHoverBoxAnimation(val))
                    .OnComplete(() => ShowHoverBoxAnimation(1f));
            }
        }

        #endregion

        #region Animations

        private void HideObjectDrawer(float value)
        {
            objectPickerVerticalOffset = Mathf.Lerp(0f, -sceneCam.pixelHeight 
                + objectDrawerHeight, value);

            if (value >= 1f)
                objectDrawerHiddenComplete = true;

            SceneView.currentDrawingSceneView.Repaint();
        }

        private void ShowObjectDrawer(float value)
        {
            objectPickerVerticalOffset = Mathf.Lerp(-sceneCam.pixelHeight
                + objectDrawerHeight, 0f, value);
            SceneView.currentDrawingSceneView.Repaint();
        }

        private void HideHoverBoxAnimation(float value)
        {
            hbOpacity = Mathf.Lerp(1f, 0f, value);
            if(value == 1f)
            {
                _hbText = "";
            }
        }

        private void ShowHoverBoxAnimation(float value)
        {
            hbOpacity = Mathf.Lerp(0f, 1f, value);
        }

        #endregion

        #endregion

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

        private void DrawAddLevelObjectMessage(Rect messageRect, string header, string text, string dropdownLabel, string buttonText, Action<Type> callback)
        {
            Type selectedType = typeof(LevelObject);


            Handles.BeginGUI();
            GUILayout.BeginArea(messageRect);
            GUILayout.BeginVertical(LevelEditorStyles.Messagebox);
            GUILayout.Label(header, LevelEditorStyles.MessageboxHeader);
            GUILayout.Label(text, LevelEditorStyles.MessageboxText);
            GUILayout.BeginHorizontal();
            GUILayout.Label(dropdownLabel, LevelEditorStyles.TextLeft);
            levelObjectTypeSelection = EditorGUILayout.Popup(levelObjectTypeSelection, levelObjectTypeNames);
            GUILayout.EndHorizontal();
            if (GUILayout.Button(buttonText, LevelEditorStyles.MessageboxButton) && callback != null)
            {
                callback.Invoke(levelObjectTypes[levelObjectTypeSelection]);
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
            if (containingRect.Contains(Event.current.mousePosition))
            {
                blockMouse = true;
            }
        }

        private void DisableScroll()
        {
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                blockScroll = true;
                currScrollBlockingTime = timeUntilBlockingReset;
            }
        }

        #endregion

        #region Button Functions

        #region Initialization

        private void CreateLevelEditorAssets()
        {

            //Level Editor Settings
            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();

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
                levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            }

            //World Editor Graph
            worldEditorGraph = LevelController.Instance.GetWorldEditorGraph();
            
            if(worldEditorGraph == null)
            {
                Util.EditorUtility.CreateAssetAndFolders("Assets/Resources/" + LevelController.WORLD_EDITOR_GRAPH_LOCATION, LevelController.WORLD_EDITOR_GRAPH_FILE_NAME, new WorldEditorGraph());
                worldEditorGraph = LevelController.Instance.GetWorldEditorGraph();
            }
        }

        /* Not used anmyore because levels are split into multiple scenes
        private void InstantiateLevelEditor()
        {
            GameObject obj = new GameObject();
            obj.name = "Level Editor Container";
            obj.tag = levelEditorSettings.levelEditorRootTag;
            Selection.activeTransform = obj.transform;
        }
        */

        private int GetLoadedLevelsCount()
        {
            if(Event.current.type == EventType.Layout)
            {
                levelCount = EditorSceneManager.loadedSceneCount;
            }
            return levelCount;
        }

        private bool CheckLevelsInitialized()
        {
            if(Event.current.type == EventType.Layout)
            {
                int countLoaded = SceneManager.sceneCount;
                if (countLoaded > 1)
                {
                    for (int i = 1; i < countLoaded; i++)
                    {
                        GameObject[] objs = SceneManager.GetSceneAt(i).GetRootGameObjects();
                        if (objs.Length < 1 || !LevelController.Instance.IsValidLevel(objs[0]))
                        {
                            allLevelsInitialized = false;
                            return allLevelsInitialized;
                        }
                    }

                    allLevelsInitialized = true;
                }
            }
            return allLevelsInitialized;
        }

        private void InitializeLevelScenes()
        {
            LevelController.Instance.LoadLevels();
            LevelController.Instance.LoadWorlds();

            //Get all loaded scenes
            int countLoaded = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[countLoaded];

            for (int i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            //Initialize all loaded scenes
            for (int i = 1; i < countLoaded; i++)
            {
                GameObject[] objs = loadedScenes[i].GetRootGameObjects();

                //Incorrectly initialized - The first root object has to be a valid Level Root
                if(objs.Length < 1 || !LevelController.Instance.IsValidLevel(objs[0]))
                {
                    //Initialize scene
                    /* Do not delete already present gameobjects */
                    EditorSceneManager.SetActiveScene(loadedScenes[i]);
                    GameObject levelRoot = new GameObject();
                    levelRoot.name = "Level";
                    LevelInstance levelInstance = levelRoot.AddComponent<LevelInstance>();
                    levelInstance.level = Util.LevelEditorUtility.GetObjectGuid(loadedScenes[i].name);
                    if(!LevelController.Instance.IsValidLevel(levelInstance.level))
                    {
                        Debug.LogError("LevelEditorTool::InitializeLevelScenes: Invalid level scene found");
                    }
                    levelRoot.transform.SetAsFirstSibling();

                    EditorSceneManager.SaveOpenScenes();
                }
            }

            if(loadedScenes.Length > 1)
            {
                GenerateThumbnail(loadedScenes[1]);
            }
        }

        private void OpenWorldEditor()
        {
            openWorldEditor = true;
        }


        #endregion

        #region MenuBar

        #region Categories

        private void ToggleCategory(string guid)
        {
            if (selectedCategories.Contains(guid))
            {
                selectedCategories.Remove(guid);
                ReloadFilters();
            }
            else
            {
                selectedCategories.Add(guid);
                if (selectedCategories.Count > 1)
                    AddCategoryFilter(guid);
                else
                    ReloadFilters();
            }
        }

        private void ClearCategoryFilter()
        {
            selectedCategories.Clear();
            ToggleCategoryMoreBox();
            ReloadFilters();
        }

        private void ToggleCategoryMoreBox()
        {
            if (showCategoryMoreBoxAnimation == null || showCategoryMoreBoxAnimation.Paused)
            {
                if (showCategoryMoreBoxAnimationValue < 0.5f)
                {
                    showCategoryMoreBoxAnimation = tweener.Tween(this, null, moreBoxAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => showCategoryMoreBoxAnimationValue = val)
                        .OnComplete(() =>
                        {
                            showCategoryMoreBoxAnimationValue = Mathf.Round(showCategoryMoreBoxAnimationValue);
                            showCategoryMoreBoxAnimation = null;
                        });
                }
                else
                {
                    showCategoryMoreBoxAnimation = tweener.Tween(this, null, moreBoxAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => showCategoryMoreBoxAnimationValue = 1f - val)
                        .OnComplete(() =>
                        {
                            showCategoryMoreBoxAnimationValue = Mathf.Round(showCategoryMoreBoxAnimationValue);
                            showCategoryMoreBoxAnimation = null;
                        });
                }
            }
        }

        #endregion

        #region Layers

        private void ToggleLayer(string guid)
        {
            if (selectedLayer == guid)
                selectedLayer = null;
            else
                selectedLayer = guid;

            ReloadFilters();
        }

        private void ToggleLayerMoreBox()
        {
            if (showLayerMoreBoxAnimation == null || showLayerMoreBoxAnimation.Paused)
            {
                if (showLayerMoreBoxAnimationValue < 0.5f)
                {
                    showLayerMoreBoxAnimation = tweener.Tween(this, null, moreBoxAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => showLayerMoreBoxAnimationValue = val)
                        .OnComplete(() =>
                        {
                            showLayerMoreBoxAnimationValue = Mathf.Round(showLayerMoreBoxAnimationValue);
                            showLayerMoreBoxAnimation = null;
                        });
                }
                else
                {
                    showLayerMoreBoxAnimation = tweener.Tween(this, null, moreBoxAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => showLayerMoreBoxAnimationValue = 1f - val)
                        .OnComplete(() =>
                        {
                            showLayerMoreBoxAnimationValue = Mathf.Round(showLayerMoreBoxAnimationValue);
                            showLayerMoreBoxAnimation = null;
                        });
                }
            }
        }

        #endregion

        #region Search/Settings

        private void ToggleSearchMenu(bool force = false)
        {
            if (searchBarAnimation == null || searchBarAnimation.Paused)
            {
                if (searchBarAnimationValue < 0.5f)
                {
                    searchBarAnimation = tweener.Tween(this, null, searchBarAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => searchBarAnimationValue = val)
                        .OnComplete(() =>
                        {
                            searchBarAnimationValue = Mathf.Round(searchBarAnimationValue);
                            searchBarAnimation = null;
                        });
                }
                else if(force || searchString == null || searchString.Length <= 0)
                {
                    GUI.FocusControl(null);
                    searchString = "";
                    searchBarAnimation = tweener.Tween(this, null, searchBarAnimationTime).Ease(Ease.SineIn).OnUpdate((val) => searchBarAnimationValue = 1f - val)
                        .OnComplete(() =>
                        {
                            searchFocused = false;
                            searchBarAnimationValue = Mathf.Round(searchBarAnimationValue);
                            searchBarAnimation = null;
                        });
                }
            }
        }

        private void OpenLevelEditorSettings()
        {
            openLevelEditorSettings = true;
        }

        #endregion

        #endregion

        #endregion

        #region Filter Utility

        private void ReloadFilters()
        {
            //Get level for preferred items
            LevelInstance levelInstance = levelRootTransform.GetComponent<LevelInstance>();
            string levelGuid = null;
            if (levelInstance != null)
                levelGuid = levelInstance.level;
            else
            {
                Debug.LogError("asdasd");
            }

            //Reload both
            if (selectedCategories.Count > 0 && !string.IsNullOrEmpty(selectedLayer))
            {
                levelObjectsController.ApplyFilters(selectedCategories, selectedLayer, preferredItemsFilterMode, levelGuid);
            }
            //Reload with Categories
            else if (selectedCategories.Count > 0)
            {
                levelObjectsController.ApplyFilters(selectedCategories, preferredItemsFilterMode, levelGuid);
            }
            //Reload with Layer
            else if (!string.IsNullOrEmpty(selectedLayer))
            {
                levelObjectsController.ApplyFilters(selectedLayer, preferredItemsFilterMode, levelGuid);
            }
            //Reload with only preferred items
            else
            {
                levelObjectsController.ApplyFilters(preferredItemsFilterMode, levelGuid);
            }
        }

        private void AddCategoryFilter(string guid)
        {
            levelObjectsController.AddCategoryFilter(guid);
        }

        #endregion

        #region Level Utility

        private bool GenerateThumbnail(Scene levelScene)
        {
            string guid = Util.LevelEditorUtility.GetObjectGuid(levelScene.name);
            if (LevelController.Instance.IsValidLevel(guid))
            {
                GameObject level = levelScene.GetRootGameObjects()[0];
                if (level != null)
                {
                    //Set default scene as active scene
                    EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneAt(0));

                    //Setup camera
                    GameObject cameraObject = new GameObject();
                    Camera thumbnailCam = cameraObject.AddComponent<Camera>();
                    thumbnailCam.enabled = false;

                    //Load/Setup texture
                    /*
                    RenderTexture thumbnail = Resources.Load<RenderTexture>($"{LevelController.LEVEL_THUMBNAIL_RESOURCE_LOCATION}/{LevelController.LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX}_{guid}");
                    
                    if(thumbnail == null)
                    {
                        thumbnail = new RenderTexture(LevelController.LEVEL_THUMBNAIL_RESOLUTION, LevelController.LEVEL_THUMBNAIL_RESOLUTION, 32);
                        Util.EditorUtility.CreateAssetAndFolders(LevelController.LEVEL_THUMBNAIL_LOCATION, $"{LevelController.LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX}_{guid}", thumbnail, ".renderTexture");
                    }
                    */

                    

                    //Disable all other levels
                    //-Get all loaded scenes
                    int countLoaded = SceneManager.sceneCount;
                    Scene[] loadedScenes = new Scene[countLoaded];

                    for (int i = 0; i < countLoaded; i++)
                    {
                        loadedScenes[i] = SceneManager.GetSceneAt(i);
                    }

                    //Disable all level scenes first object that are not the current level
                    for (int i = 1; i < loadedScenes.Length; i++)
                    {
                        string otherGuid = Util.LevelEditorUtility.GetObjectGuid(loadedScenes[i].name);
                        //If other level is a valid level different from the current one which is saved
                        if (otherGuid != null && otherGuid != guid && LevelController.Instance.IsValidLevel(otherGuid))
                        {
                            loadedScenes[i].GetRootGameObjects()[0].gameObject.SetActive(false);
                        }
                    }

                    //Position camera
                    Rect levelRect = LevelController.Instance.GetLevel(guid).levelBoundaries;
                    thumbnailCam.orthographic = true;
                    cameraObject.transform.position = new Vector3(levelRect.position.x, levelRect.position.y, -10f);

                    //Resize camera to match rect size
                    //- height = 2 * Camera.main.orthographicSize;
                    thumbnailCam.orthographicSize = levelRect.height / 2f;
                    //- width = height * Camera.main.aspect;
                    thumbnailCam.aspect = levelRect.width / levelRect.height;

                    //Setup camera render target
                    thumbnailCam.targetTexture = RenderTexture.GetTemporary(thumbnailCam.pixelWidth, thumbnailCam.pixelHeight, 24, RenderTextureFormat.ARGB32);
                    RenderTexture.active = thumbnailCam.targetTexture;

                    //Render scene
                    thumbnailCam.Render();

                    Texture2D previewImage = new Texture2D(thumbnailCam.pixelWidth, thumbnailCam.pixelHeight, TextureFormat.RGB24, false);
                    previewImage.ReadPixels(new Rect(0, 0, thumbnailCam.pixelWidth, thumbnailCam.pixelHeight), 0, 0);
                    thumbnailCam.targetTexture = null;
                    previewImage.Apply();

                    byte[] bytes = previewImage.EncodeToPNG();
                    File.WriteAllBytes($"{LevelController.LEVEL_THUMBNAIL_LOCATION}/{LevelController.LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX}_{guid}.png", bytes);

                    AssetDatabase.ImportAsset($"{LevelController.LEVEL_THUMBNAIL_LOCATION}/{LevelController.LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX}_{guid}.png");

                    //Reenable other levels
                    for (int i = 1; i < loadedScenes.Length; i++)
                    {
                        string otherGuid = Util.LevelEditorUtility.GetObjectGuid(loadedScenes[i].name);
                        //If other level is a valid level different from the current one which is saved
                        if (otherGuid != null && otherGuid != guid && LevelController.Instance.IsValidLevel(otherGuid))
                        {
                            loadedScenes[i].GetRootGameObjects()[0].gameObject.SetActive(true);
                        }
                    }

                    DestroyImmediate(cameraObject);
                    LevelController.Instance.EmptyLevelThumbnailsCache();

                    //AssetDatabase.SaveAssets();

                    return true;
                }

            }
            return false;
        }

        private void SetLevelRoot(bool force = false)
        {
            if(levelRootTransform == null || force)
            {
                if(EditorSceneManager.sceneCount > 1)
                {
                    GameObject[] levelRootGameObjects = EditorSceneManager.GetSceneAt(1).GetRootGameObjects();
                    if(levelRootGameObjects.Length > 0 && levelRootGameObjects[0].GetComponent<LevelInstance>() != null)
                    {
                        levelRootTransform = levelRootGameObjects[0].transform;
                    }
                }
            }
        }

        private Transform GetLevelObjectParentTransform(LevelObject obj)
        {
            if(levelRootTransform != null)
            {
                LevelInstance levelInstance = levelRootTransform.GetComponent<LevelInstance>();
                if(levelInstance != null)
                    return levelInstance.FindParentTransform(obj, selectedLayer, LevelEditorSettingsController.Instance.GetLevelEditorSettings());
            }
            return null;
        }

        #endregion

        #endregion

        #region Add Prefab To Level Objects

        private void DrawAddNewPrefabDialog(Rect windowRect, PrefabStage currPrefabStage)
        {
            Vector2 bottomRight = new Vector2(windowRect.position.x + windowRect.width,
                windowRect.position.y + windowRect.height);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(currPrefabStage.prefabAssetPath);
            if (prefab != null)
            {
                LevelObject currObject = levelObjectsController.GetLevelObjectByPrefab(prefab);
                if(currObject != null)
                {
                    Rect dialogRect = new Rect
                    {
                        position = new Vector2(bottomRight.x - prefabDialogBounds.z - prefabDialogBounds.x,
                            bottomRight.y - prefabDialogBounds.w - prefabDialogBounds.y),
                        size = new Vector2(prefabDialogBounds.z, prefabDialogBounds.w)
                    };

                    DrawButtonMessage(dialogRect, "Level Object Found", "Press the button below to access its properties!", "Open Level Object Properties", () => ProjectWindowUtil.ShowCreatedAsset(currObject));
                }
                else
                {
                    Rect dialogRect = new Rect
                    {
                        position = new Vector2(bottomRight.x - prefabDialogSelectBounds.z - prefabDialogSelectBounds.x,
                            bottomRight.y - prefabDialogSelectBounds.w - prefabDialogSelectBounds.y),
                        size = new Vector2(prefabDialogSelectBounds.z, prefabDialogSelectBounds.w)
                    };

                    DrawAddLevelObjectMessage(dialogRect, "Add Prefab", "Add this prefab to the Level Editor!", "Object Type:", 
                        "Create Level Object", (type) => CreateNewLevelObject(prefab, type));
                }
            }
            else
            {
                ShowMessage(windowRect, "Level Editor Error", "Invalid prefab path");
            }

        }

        private void CreateNewLevelObject(GameObject currPrefab, Type levelObjectType)
        {
            LevelObject lvlObj = CreateInstance(levelObjectType) as LevelObject;
            if (lvlObj != null)
            {
                lvlObj.objectPrefab = currPrefab;
                lvlObj.GenerateGUID();
                lvlObj.name = lvlObj.objectPrefab.name;
                lvlObj.item = new LevelEditorItem();
                lvlObj.item.name = lvlObj.name;
                lvlObj.item.accentColor = Color.white;
                lvlObj.item.thumbnail = AssetPreview.GetAssetPreview(lvlObj.objectPrefab);
                
                string rootPath = "Assets/Resources/" + LevelObjectsController.LEVEL_OBJECTS_PATH;
                Util.EditorUtility.CreateAssetAndFolders(rootPath, lvlObj.name, lvlObj);

                LevelObjectEditorExtension lvlObjEditor = LevelObjectEditorExtension.GetBaseLevelObjectEditorExtension(lvlObj);

                if (lvlObjEditor != null)
                {
                    GameObject prefabToChange = Instantiate(currPrefab);

                    GameObject changedPrefab = lvlObjEditor.OnLevelObjectCreate(prefabToChange, rootPath);

                    if (changedPrefab != null)
                    {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(changedPrefab, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(currPrefab), InteractionMode.AutomatedAction);
                    }

                    DestroyImmediate(prefabToChange);
                }

                levelObjectsController.LoadLevelObjects();
                levelObjectTypeSelection = 0;
            }
            
        }


        #endregion

        #region Public Methods

        #region Level Object Drawer

        public void OpenLevelObjectDrawer()
        {
            forgetNextSpacePress = true;
        }

        public void DeselectCurrentlySelectedObject()
        {
            inObjectPlacementMode = false;
            objectToPlace.Unset();
            if(temporaryObject != null)
            {
                DestroyImmediate(temporaryObject);
                temporaryObject = null;
            }
        }

        #endregion

        #endregion

        #region Structures
        private struct ObjectToPlace
        {
            public LevelObject levelObject;
            public LevelObjectEditorExtension levelObjectEditor;

            public bool IsValid() => levelObject != null && levelObjectEditor != null;

            public void Unset()
            {
                levelObject = null;
                levelObjectEditor = null;
            }
        }

        #endregion
    }


}