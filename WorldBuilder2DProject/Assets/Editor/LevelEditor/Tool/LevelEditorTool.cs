using System;
using System.Collections.Generic;
using Unglide;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

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
                Debug.LogWarning("LevelEditorToolInstance: Level Editor Tool not found!");
                return null;
            }
        }
        #endregion

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
        private LevelObjectsController levelObjectsController;
        private Transform levelEditorRoot;
        private Transform levelRoot;
        private Camera sceneCam;

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

        //Messagebox preferences
        private Vector2 messageboxSize = new Vector2(400f, 140f);

        //Menu Bar
        private float menuBarHeight = 50f;
        private LevelEditorItem[] systemMenuBarItems;
        private bool mouseHoveringMenuBar;

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

        //Object placement
        private bool inObjectPlacementMode;
        private LevelObject objectToPlace;
        private GameObject temporaryObject;
        private bool canObjectBePlaced;

        //Add Prefab Dialog
        //-->Margin from bottom right corner, size is absolute
        private readonly Vector4 prefabDialogBounds = new Vector4(15f, 10f, 300f, 120f);

        #region Initialization / DeInitialization
        void OnEnable()
        {
            s_levelEditorTool = this;

            //Events
            SceneView.beforeSceneGui += ToolInput;
            SceneView.beforeSceneGui += WindowFocus;

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
            forgetNextSpacePress = false;

            SetupVariables();
        }

        private void SetupVariables()
        {
            //Main references
            levelObjectsController = LevelObjectsController.Instance;

            //Animation
            tweener = new Tweener();

            //Messagebox preferences
            messageboxSize = new Vector2(400f, 140f);

            //Menu Bar
            menuBarHeight = 50f;
            SetupMenuBarIcons();
            
            //Object Picker
            objectPickerVerticalOffset = 0f;
            objectPickerVerticalSizeReduction = 50f;
            objectPickerMargin = new RectOffset(10, 10, 10, 10);
            objectDrawerHidden = true;
            objectDrawerHiddenComplete = true;
            objectDrawerHeight = 0f;

            //Object placement
            objectToPlace = null;
            canObjectBePlaced = false;

            //Start ressource reloader
            tweener.Timer(0f).OnComplete(() => RefreshVariables());
        }

        private void RefreshVariables()
        {
            LevelEditorStyles.RefreshStyles();
            levelObjectsController.LoadLevelObjects();
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
            systemMenuBarItems = new LevelEditorItem[6];
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
        }


        private void OnDisable()
        {
            s_levelEditorTool = null;

            //Events
            SceneView.beforeSceneGui -= ToolInput;
            SceneView.beforeSceneGui -= WindowFocus;
        }

        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        #region Events

        private void ToolInput(SceneView view)
        {
            Event e = Event.current;
            //Locked to current tool
            if (Tools.current == Tool.Custom)
            {
                if (e.type == EventType.MouseDown)
                {
                    if (blockMouse)
                    {
                        e.Use();
                    }

                }
                else if (e.type == EventType.MouseUp)
                {
                    if (e.button == 0)
                    {
                        if (inObjectPlacementMode && objectToPlace != null && canObjectBePlaced)
                        {
                            objectToPlace.SpawnObject(temporaryObject, null, Util.EditorUtility.SceneViewToWorldPos(view), Event.current.mousePosition);
                            
                            //Instantiate(objectToPlace.objectPrefab, Util.EditorUtility.SceneViewToWorldPos(view), Quaternion.identity);
                        }
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
            //Lost Focus
            if (EditorWindow.focusedWindow != view)
            {
                DeselectCurrentlySelectedObject();
            }
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
                        try
                        {
                            DrawGui(cameraBounds);
                        }
                        catch (ArgumentException ex) 
                        {
                            //Remove harmless error Message
                            if (!ex.Message.Contains("Getting control 1's position in a group with only 1 controls when doing repaint"))
                                Debug.LogError(ex.Message);
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

        public void DrawGui(Rect screenRect)
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

            DrawMenuBar(menuBarRect);
            DrawObjectPicker(objectPickerRect);

            Handles.EndGUI();
            if (Util.EditorUtility.IsMouseInsideSceneView(SceneView.currentDrawingSceneView) && !mouseHoveringMenuBar && !objectPickerRect.Contains(Event.current.mousePosition))
            {
                canObjectBePlaced = true;
            }
            else
            {
                canObjectBePlaced = false;
            }

            if(canObjectBePlaced)
            {
                //Temporary gameobject spawn
                if(objectToPlace != null && objectToPlace.UseTemporaryIndicator)
                {
                    if(temporaryObject == null)
                    {
                        temporaryObject = Instantiate(objectToPlace.objectPrefab);
                        temporaryObject.name = $"temp [{objectToPlace.guid}]";
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

        private void DrawMenubarButton(LevelEditorItem item, Action action)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(LevelEditorStyles.MenuButtonCircle.fixedWidth, LevelEditorStyles.MenuButtonCircle.fixedHeight, LevelEditorStyles.MenuButtonCircle);

            //Hover tinting
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                mouseHoveringMenuBar = true;
                if (item.accentColor.a > 0)
                {
                    GUI.color = item.accentColor;
                }
                else
                {
                    GUI.color = LevelEditorStyles.buttonHoverColor;
                }
            }

            if(GUI.Button(buttonRect, item.thumbnail, LevelEditorStyles.MenuButtonCircle) && action != null)
            {
                action.Invoke();
            }

            GUI.color = Color.white;
        }

        #endregion

        #region Categories

        private void DrawMenuCategories()
        {
            DrawMenubarButton(systemMenuBarItems[0], null);
            DrawMenubarButton(systemMenuBarItems[0], null);
            DrawMenubarButton(systemMenuBarItems[0], null);
            DrawMenubarButton(systemMenuBarItems[0], null);
            DrawMenubarButton(systemMenuBarItems[0], null);
            DrawMenubarButton(systemMenuBarItems[0], null);
        }

        #endregion

        #region Preferred Items

        private void DrawMenuPreferredItems()
        {
            DrawMenubarButton(systemMenuBarItems[1], null);
            DrawMenubarButton(systemMenuBarItems[2], null);
        }

        #endregion

        #region Layers

        private void DrawMenuLayers()
        {
            DrawMenubarButton(systemMenuBarItems[3], null);
            DrawMenubarButton(systemMenuBarItems[3], null);
            DrawMenubarButton(systemMenuBarItems[3], null);
            DrawMenubarButton(systemMenuBarItems[3], null);
        }

        #endregion

        #region Search/Settings

        private void DrawMenuOther()
        {
            DrawMenubarButton(systemMenuBarItems[4], null);
            DrawMenubarButton(systemMenuBarItems[5], null);
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

            SortedDictionary<string, LevelObject> levelObjects = levelObjectsController.GetAllLevelObjects();
            int objectCount = levelObjects.Values.Count;
            objectDrawerScrollPosition = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(objectDrawerScrollPosition, 0f)), false, false, GUILayout.Height(remainingHeight)).y;

            GUILayout.BeginVertical();
            int i = 0;
            foreach (LevelObject obj in levelObjects.Values)
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
            GUI.color = obj.item.accentColor;
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                EnableMouse();
            }
            GUIStyle buttonActive = (objectToPlace == null || obj.guid != objectToPlace.guid) ? LevelEditorStyles.LevelObjectButton : LevelEditorStyles.LevelObjectButtonActive;
            if(GUI.Button(buttonRect, "", buttonActive))
            {
                if(!objectDrawerHidden)
                    ToggleObjectDrawer();
                inObjectPlacementMode = true;
                objectToPlace = obj;
                levelObjectsController.AddToQuickSelectBar(obj.guid);

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
            obj.name = "Level Editor Container";
            obj.tag = levelEditorSettings.levelEditorRootTag;
            Selection.activeTransform = obj.transform;
        }

        #endregion

        #region MenuBar

        

        #endregion

        #endregion

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

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(currPrefabStage.prefabAssetPath);
            if (prefab != null)
            {
                LevelObject currObject = levelObjectsController.GetLevelObjectByPrefab(prefab);
                if(currObject != null)
                {
                    DrawButtonMessage(dialogRect, "Level Object Found", "Press the button below to access its properties!", "Open Level Object Properties", () => ProjectWindowUtil.ShowCreatedAsset(currObject));
                }
                else
                {
                    DrawButtonMessage(dialogRect, "Add Prefab", "Add this prefab to the Level Editor!", "Create Level Object", () => CreateNewLevelObject(prefab));
                }
            }
            else
            {
                ShowMessage(windowRect, "Level Editor Error", "Invalid prefab path");
            }

        }

        private void CreateNewLevelObject(GameObject currPrefab)
        {
            LevelObject lvlObj = CreateInstance<LevelObject>();
            lvlObj.objectPrefab = currPrefab;
            lvlObj.GenerateGUID();
            lvlObj.name = lvlObj.objectPrefab.name;
            lvlObj.item = new LevelEditorItem();
            lvlObj.item.name = lvlObj.name;
            lvlObj.item.accentColor = Color.white;
            lvlObj.item.thumbnail = AssetPreview.GetAssetPreview(lvlObj.objectPrefab);

            string rootPath = "Assets/Resources/"+LevelObjectsController.LEVEL_OBJECTS_PATH;
            Util.EditorUtility.CreateAssetAndFolders(rootPath, lvlObj.name, lvlObj);
            levelObjectsController.LoadLevelObjects();
            
        }


        #endregion

        #region Public Methods

        #region General

        public void OpenLevelObjectDrawer()
        {
            forgetNextSpacePress = true;
        }

        #endregion

        #region Level Object Drawer

        public void DeselectCurrentlySelectedObject()
        {
            inObjectPlacementMode = false;
            objectToPlace = null;
            if(temporaryObject != null)
            {
                DestroyImmediate(temporaryObject);
                temporaryObject = null;
            }
        }

        #endregion

        #endregion
    }

}