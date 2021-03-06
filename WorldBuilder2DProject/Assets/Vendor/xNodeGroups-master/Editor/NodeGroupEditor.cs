using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNode.NodeGroups;

namespace XNodeEditor.NodeGroups
{
	[CustomNodeEditor(typeof(NodeGroup))]
	public class NodeGroupEditor : NodeEditor, INodeEditorInspector
	{
		private NodeGroup group { get { return _group != null ? _group : _group = target as NodeGroup; } }
		private NodeGroup _group;
		public static Texture2D corner { get { return _corner != null ? _corner : _corner = Resources.Load<Texture2D>("xnode_corner"); } }
		private static Texture2D _corner;
		private bool isDragging;
		private Vector2 size;
		private bool hoveringOverCorner = false;

		//Node inspector
		private LevelEditorSettings levelEditorSettings;
		private LevelObjectsController levelObjectsController;
		private LevelController levelController;
		//Tabs
		private int menuSelection;
		private readonly string[] menuLabels = new string[] { "Overview", "World Favorites" };

        //Levels
        private ReorderableList levels;

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


		public override void OnCreate()
        {
            base.OnCreate();

            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            levelObjectsController = LevelObjectsController.Instance;
            levelController = LevelController.Instance;

            //Inspector initialization
            menuSelection = 0;
            showDebugFields = new AnimBool(false);

            //Levels
            levels = new ReorderableList(serializedObject, serializedObject.FindProperty("levels"), false, true, false, false);
            levels.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Levels");
            };
            levels.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                string levelGuid = serializedObject.FindProperty("levels").GetArrayElementAtIndex(index).stringValue;
                LevelNode lNode = levelController.GetLevel(levelGuid);
                string levelName = "Unknown";
                if (lNode != null)
                    levelName = lNode.levelName;

                EditorGUI.LabelField(rect, levelName);
            };

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

        public override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
        }

        public override void OnBodyGUI()
		{

			Event e = Event.current;
			Rect lowerRight = new Rect();

			if (e.type == EventType.Repaint && NodeEditorWindow.current.nodeSizes.TryGetValue(target, out size))
			{
				// Mouse position checking is in node local space
				lowerRight = new Rect(size.x - 34, size.y - 34, 30, 30);
				if (lowerRight.Contains(e.mousePosition))
				{
					hoveringOverCorner = true;
				}
				else if(Event.current.type != EventType.Used)
				{
					hoveringOverCorner = false;
				}
			}

			WorldEditorGraphEditor nodeGrpahEditor = NodeGraphEditor.GetEditor(LevelController.Instance.GetWorldEditorGraph(), NodeEditorWindow.current) as WorldEditorGraphEditor;
			if(nodeGrpahEditor != null)
            {
				nodeGrpahEditor.resizingWorldBox = hoveringOverCorner;
            }
			if (!nodeGrpahEditor.IsMouseOverInspector)
			{
				NodeEditorWindow.current.enableInput = !hoveringOverCorner;
			}

			switch (e.type)
			{
				case EventType.MouseDrag:
					if (isDragging)
					{
						group.width = Mathf.Max(200, (int)e.mousePosition.x + 16);
						group.height = Mathf.Max(100, (int)e.mousePosition.y - 34);
						NodeEditorWindow.current.Repaint();
					}
					break;
				case EventType.MouseDown:
					// Ignore everything except left clicks
					if (e.button != 0) return;
					// Mouse position checking is in node local space
					if (hoveringOverCorner)
					{
						isDragging = true;
					}
					break;
				case EventType.MouseUp:
                    if (Selection.Contains(target))
                    {
                        Selection.objects = new Object[1] { group };
                    }
                    if (isDragging)
                        Selection.activeObject = group;
					isDragging = false;
					// Select nodes inside the group
					if (Selection.Contains(target))
					{
						List<Object> selection = Selection.objects.ToList();
						// Select Nodes
						selection.AddRange(group.GetNodes());
						// Select Reroutes
						foreach (Node node in target.graph.nodes)
						{
							foreach (NodePort port in node.Ports)
							{
								for (int i = 0; i < port.ConnectionCount; i++)
								{
									List<Vector2> reroutes = port.GetReroutePoints(i);
									for (int k = 0; k < reroutes.Count; k++)
									{
										Vector2 p = reroutes[k];
										if (p.x < group.position.x) continue;
										if (p.y < group.position.y) continue;
										if (p.x > group.position.x + group.width) continue;
										if (p.y > group.position.y + group.height + 30) continue;
										if (NodeEditorWindow.current.selectedReroutes.Any(x => x.port == port && x.connectionIndex == i && x.pointIndex == k)) continue;
										NodeEditorWindow.current.selectedReroutes.Add(
											new Internal.RerouteReference(port, i, k)
										);
									}
								}
							}
						}

                        Object[] selectedObjects = selection.Distinct().ToArray();
						Selection.objects = selectedObjects;

                        //Add levels to the world
                        serializedObject.Update();
                        SerializedProperty levels = serializedObject.FindProperty("levels");
                        levels.ClearArray();
                        for(int i = 0; i<selectedObjects.Length; i++)
                        {
                            LevelNode lNode = selectedObjects[i] as LevelNode;
                            if(lNode != null)
                            {
                                levels.arraySize += 1;
                                levels.GetArrayElementAtIndex(levels.arraySize - 1).stringValue = lNode.guid;
                            }
                        }
                        serializedObject.ApplyModifiedProperties();

                        //Open Inspector for this object
                        nodeGrpahEditor.worldInspector = this;
					}
					break;
				case EventType.Repaint:
					// Move to bottom
					if (target.graph.nodes.IndexOf(target) != 0)
					{
						target.graph.nodes.Remove(target);
						target.graph.nodes.Insert(0, target);
					}
					// Add scale cursors
					if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out size))
					{
						Rect lowerRight2 = new Rect(target.position, new Vector2(30, 30));
						lowerRight2.y += size.y - 34;
						lowerRight2.x += size.x - 34;
						lowerRight2 = NodeEditorWindow.current.GridToWindowRect(lowerRight2);
						NodeEditorWindow.current.onLateGUI += () => AddMouseRect(lowerRight2);
					}
					break;
			}

			// Control height of node
			GUILayout.Space(group.height);

			GUI.DrawTexture(new Rect(group.width - 34, group.height + 16, 24, 24), corner);
		}

		public override int GetWidth()
		{
			return group.width;
		}

		public override Color GetTint()
		{
			return group.accentColor;
		}

		public override void OnRename()
		{
			if (group != null)
			{
				group.worldName = group.name;
			}
		}

		public static void AddMouseRect(Rect rect)
		{
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeUpLeft);
		}

        #region World Node Inspector

        public void OnNodeInspectorGUI()
        {
            if (group)
            {
                GUILayout.Label($"World: {group.worldName}", GUILayout.ExpandHeight(false));
                LevelEditorStyles.DrawHorizontalLine(Color.white, new RectOffset(10, 10, 10, 10));

                serializedObject.Update();

                //Menu bar
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldName"));
            if(group.worldName != group.name)
            {
                group.name = group.worldName;
            }
            GUISkin prevSkin = GUI.skin;
            GUI.skin = null;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accentColor"));
            GUI.skin = prevSkin;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldDescription"));

            //Levels
            GUI.skin = null;
            levels.DoLayoutList();
            GUI.skin = prevSkin;
            if(levels.index >= 0 && GUILayout.Button("Open level inspector"))
            {
                LevelNode lNode = levelController.GetLevel(group.levels[levels.index]);
                if(lNode != null)
                {
                    Selection.activeObject = lNode;
                }
            }

            //Debug information
            EditorGUILayout.Space(20f);
            showDebugFields.target = EditorGUILayout.ToggleLeft("Show debug fields", showDebugFields.target);
            if (EditorGUILayout.BeginFadeGroup(showDebugFields.faded))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
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
    }
}