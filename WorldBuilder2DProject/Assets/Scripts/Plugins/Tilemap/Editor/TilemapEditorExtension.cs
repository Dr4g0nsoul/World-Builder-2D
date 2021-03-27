using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace dr4g0nsoul.WorldBuilder2D.TilemapPlugin
{

	[CreateLevelObjectEditorExtension(typeof(TilemapLevelObject))]
	public class TilemapEditorExtension : LevelObjectEditorExtension
	{

		public const string TILES_FOLDER = "Assets/Resources/LevelEditorTiles";
		public const string THUMBNAIL_FOLDER = "Assets/Editor/Resources/LevelEditorTilesThumbnails";
		public const string THUMBNAIL_PREFIX = "thumb";
		public const int TILES_AMOUNT_PER_GROUP = 20;

		//For custom inspector - start
		private string[] autoTileBackdrop = new string[]
		{
			"╻", "┏", "┳", "┓",
			"┃", "┣", "╋", "┫",
			"╹", "┗", "┻", "┛",
			"▪", "╺", "━", "╸",
			"╭", "╮", "╰", "╯"
		};

		private float tilesOverviewScrollpos = 0f;
		private float tileSize = 50f;
		private List<Texture2D> tileTextures = new List<Texture2D>();//
		private int tilesetMenu = 0;
		private int selectedTile = -1;
		private bool tileRemoved;
		private readonly string[] tilesetMenuItems = new string[] {"Tiles", "Auto Tiles"};
		private string newGroupName;
		private int selectedAutotileGroup;
		private bool clearAutoTile;
		private GUISkin levelEditorSkin;
		private GUISkin prevSkin;
		//For custom inspector - end

		//For Level Editor Tool - start
		private Tilemap t_currentTilemap;
		private TilemapInformation t_currentTilemapInformation;
		private TilemapRenderer t_currentTilemapRenderer;
		private int t_menu;
		private readonly string[] t_menuItems = new string[] { "Tiles", "Auto Tiles" };
		private Rect t_scrollRect;
		private int t_selectedTile;
		private float t_tilesScrollpos;
		private int t_selectedAutoTileGroup;
		private List<Texture2D> t_tileTextures;
		private float t_autoTilesScrollpos;
		private bool t_inDeletionMode;
		private Vector3Int t_lastDraggedTilePos;
		private Vector3Int t_lastPreviewTilePos;
		private AutoTileMode t_autoTileMode;
		//For Level Editor Tool - end

		#region Level Object Create

		public override GameObject OnLevelObjectCreate(GameObject currentPrefab, string prefabPath)
        {
            currentPrefab.transform.position = Vector3.zero;
            currentPrefab.transform.rotation = Quaternion.identity;
            if (currentPrefab.GetComponent<Grid>() == null)
                currentPrefab.AddComponent<Grid>();

            Transform previousTilemap = null;

            foreach (Transform child in currentPrefab.transform)
            {
                Tilemap tm = child.GetComponent<Tilemap>();
                TilemapRenderer tmr = child.GetComponent<TilemapRenderer>();

                if (tmr != null)
                {
                    UnityEngine.Object.DestroyImmediate(tmr);
                }
                if (tm != null)
                {
					UnityEngine.Object.DestroyImmediate(tm);
                    if (child.parent == currentPrefab.transform)
                    {
                        previousTilemap = child;
                    }
                }
            }


            GameObject tilemap = previousTilemap == null ? new GameObject() : previousTilemap.gameObject;

            tilemap.name = "Tilemap";
            tilemap.AddComponent<Tilemap>();
            tilemap.AddComponent<TilemapRenderer>();
			tilemap.AddComponent<TilemapInformation>();
			Rigidbody2D rb = tilemap.AddComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Static;
			tilemap.AddComponent<CompositeCollider2D>();
			TilemapCollider2D col = tilemap.AddComponent<TilemapCollider2D>();
			col.usedByComposite = true;
            tilemap.transform.parent = currentPrefab.transform;
            tilemap.transform.position = Vector3.zero;
            tilemap.transform.rotation = Quaternion.identity;


            return currentPrefab;
        }

        #endregion

        #region Custom inspector

        public override string CustomInspectorTabName() => "Tilemap";

		public override void OnCustomInspectorEnable()
		{
			tileTextures = new List<Texture2D>();
			levelEditorSkin = Resources.Load<GUISkin>("LevelEditor/Skin/LESkin");
			tileRemoved = false;
			newGroupName = "";
			selectedAutotileGroup = -1;
			clearAutoTile = false;
		}

		public override void OnCustomInspectorTabGUI(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject)
        {
			prevSkin = GUI.skin;
			GUI.skin = levelEditorSkin;
			LevelEditorStyles.RefreshStyles();
			
			TilemapLevelObject tilemap = Target as TilemapLevelObject;

			if (tilemap != null)
			{

				serializedObject.Update();

				GUI.skin = prevSkin;
				tilesetMenu = GUILayout.Toolbar(tilesetMenu, tilesetMenuItems);
				GUI.skin = levelEditorSkin;

				GUILayout.BeginVertical(EditorStyles.helpBox);

				switch (tilesetMenu) {
					case 0:
						TilesInspectorGUI(tilemap, serializedObject);
						break;

					case 1:
						AutoTilesInspectorGUI(tilemap, serializedObject);
						break;
				}

				GUILayout.EndVertical();
				serializedObject.ApplyModifiedProperties();

				levelObjectEditor.Repaint();
			}
			GUI.skin = prevSkin;
		}

		private void TilesInspectorGUI(TilemapLevelObject tilemap, SerializedObject serializedObject)
        {
			//Add new sprites to tilemap
			if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
			{
				if (EditorGUIUtility.GetObjectPickerObject().GetType() == typeof(Texture2D))
				{
					Texture2D tex = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
					if (tex != null)
					{
						string pathToTexture = AssetDatabase.GetAssetPath(tex);
						TextureImporter textureImporter = AssetImporter.GetAtPath(pathToTexture) as TextureImporter;
						if (textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
						{
							UnityEngine.Object[] spriteObjects = AssetDatabase.LoadAllAssetsAtPath(pathToTexture);
							foreach (UnityEngine.Object spriteObject in spriteObjects)
							{
								Sprite sprite = spriteObject as Sprite;
								AddTile(sprite, tilemap, serializedObject);
							}
						}
					}
				}
				else if (EditorGUIUtility.GetObjectPickerObject().GetType() == typeof(Sprite))
				{
					Sprite sprite = EditorGUIUtility.GetObjectPickerObject() as Sprite;
					if (sprite != null)
					{
						AddTile(sprite, tilemap, serializedObject);
					}
				}
			}


			//Add sprites gui
			GUILayout.Space(10f);
			GUILayout.Label("Add sprites to tilemap", LevelEditorStyles.HeaderCentered);
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal();
			GUI.skin = prevSkin;
			if (GUILayout.Button("Add sprite"))
			{
				EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", 0);
			}
			if (GUILayout.Button("Add sprite image"))
			{
				EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", 1);
			}
			GUI.skin = levelEditorSkin;
			GUILayout.EndHorizontal();

			//Sprite overview GUI
			GUILayout.Space(15f);
			ShowAllTiles(tilemap, serializedObject, true);
			GUILayout.Space(10f);
		}

		private void ShowAllTiles(TilemapLevelObject tilemap, SerializedObject serializedObject, bool withRemovalOption)
        {
			GUILayout.Label("All tiles", LevelEditorStyles.HeaderCentered);
			GUILayout.Space(5f);
			if (withRemovalOption)
			{
				GUI.skin = prevSkin;
				if (Event.current.type == EventType.Layout && tileRemoved)
				{
					tileTextures = new List<Texture2D>();
					tileRemoved = false;
				}
				if (selectedTile >= 0 && selectedTile < tilemap.tiles.Length)
				{
					
					if (GUILayout.Button("Remove selected tile"))
					{
						// Needs to be called twice because reasons
						serializedObject.FindProperty("tiles").DeleteArrayElementAtIndex(selectedTile);
						serializedObject.FindProperty("tiles").DeleteArrayElementAtIndex(selectedTile);
						tileRemoved = true;
					}
					
				}
				GUILayout.Space(5f);
				if (selectedTile >= 0 && selectedTile < tilemap.tiles.Length)
				{
					GUI.skin = prevSkin;
					if (GUILayout.Button("Set as thumbnail"))
					{
						SetThumbnail(tilemap, serializedObject);
						selectedTile = -1;
					}
					GUI.skin = levelEditorSkin;
				}
				GUILayout.Space(5f);
				GUI.skin = levelEditorSkin;
			}
			GUILayout.BeginVertical(EditorStyles.helpBox);


			int objectsPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / tileSize - 2f);
			int objectCount = tilemap.tiles.Length;
			tilesOverviewScrollpos = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(tilesOverviewScrollpos, 0f)), false, false, GUILayout.MaxHeight(300f)).y;

			GUILayout.BeginVertical();
			for (int i = 0; i<tilemap.tiles.Length; i++)
			{
				if (i % objectsPerRow == 0)
					GUILayout.BeginHorizontal();

				DrawTile(i, tilemap.tiles[i]);

				if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
				{
					GUILayout.EndHorizontal();
					if (i < objectCount - 1)
						GUILayout.Space(Mathf.Max(LevelEditorStyles.LevelObjectButton.margin.right, LevelEditorStyles.LevelObjectButton.margin.left));
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();



			GUILayout.EndVertical();
		}

		private void DrawTile(int index, LevelTile tile)
        {
			GUIStyle buttonStyle = selectedTile == index ? LevelEditorStyles.ButtonActive : LevelEditorStyles.Button;
			if (GUILayout.Button(GUIContent.none, buttonStyle, GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
			{
				if (selectedTile == index)
					selectedTile = -1;
				else
					selectedTile = index;

				clearAutoTile = false;
			}
			Rect r = GUILayoutUtility.GetLastRect();
			r.position = new Vector2(r.position.x + 5, r.position.y + 5);
			r.size = new Vector2(r.width - 10, r.height - 10);

			if(index == tileTextures.Count)
            {
				if(tile != null)
					tileTextures.Add(LevelEditorStyles.TextureFromSprite(tile.sprite));
				else
					tileTextures.Add(null);
            }
			if(tile != null && index < tileTextures.Count && tileTextures[index] != null) {
				GUI.DrawTexture(r, tileTextures[index], ScaleMode.ScaleAndCrop, true, 0, Color.white, 0f, 5f);
			}
			else
            {
				GUIStyle xStyle = new GUIStyle(LevelEditorStyles.HeaderCenteredBig);
				xStyle.alignment = TextAnchor.MiddleCenter;
				GUI.Label(r, "X", xStyle);
            }
		}

		private void AddTile(Sprite sprite, TilemapLevelObject tilemap, SerializedObject serializedObject)
        {

			if (sprite != null)
			{
				if(tilemap.item.thumbnail == null)
                {
					SetThumbnail(tilemap, serializedObject);
                }
				LevelTile existingTile = AssetDatabase.LoadAssetAtPath<LevelTile>(TILES_FOLDER + "/" + sprite.name + ".asset");
				//Create tile asset
				if (existingTile == null)
				{
					LevelTile newTile = ScriptableObject.CreateInstance<LevelTile>();
					newTile.name = sprite.name;
					newTile.sprite = sprite;
					Util.EditorUtility.CreateAssetAndFolders(TILES_FOLDER, sprite.name, newTile);
					existingTile = AssetDatabase.LoadAssetAtPath<LevelTile>(TILES_FOLDER + "/" + newTile.name + ".asset");
					if(existingTile != null)
                    {
						serializedObject.FindProperty("tiles").arraySize += 1;
						serializedObject.FindProperty("tiles").GetArrayElementAtIndex(serializedObject.FindProperty("tiles").arraySize - 1).objectReferenceValue = existingTile;
						Debug.Log($"Created tile {existingTile.name} at {(TILES_FOLDER + "/" + sprite.name)}");
                    }
					else
                    {
						Debug.LogError($"Unable to create {existingTile.name} at {(TILES_FOLDER + " / " + sprite.name)}");
                    }
				}
				//Update tile asset
				else
                {
					existingTile.sprite = sprite;
					if(tilemap != null) {
						if(!tilemap.tiles.Contains(existingTile))
                        {
							serializedObject.FindProperty("tiles").arraySize += 1;
							serializedObject.FindProperty("tiles").GetArrayElementAtIndex(serializedObject.FindProperty("tiles").arraySize - 1).objectReferenceValue = existingTile;
							Debug.Log($"Added tile {existingTile.name}");
						}
                    }
					else
                    {
						Debug.LogError($"Target is null");
                    }
					Debug.Log($"Updated tile {existingTile.name} at {(TILES_FOLDER + "/" + sprite.name)}");
				}

				tileTextures = new List<Texture2D>();
			}
		}

		private void SetThumbnail(TilemapLevelObject tilemap, SerializedObject serializedObject)
        {
			if (selectedTile >= 0 && selectedTile < tilemap.tiles.Length)
			{
				if (!AssetDatabase.IsValidFolder(THUMBNAIL_FOLDER))
				{
					Util.EditorUtility.CreateFolders(THUMBNAIL_FOLDER);
				}
				Texture2D thumbnail = LevelEditorStyles.TextureFromSprite(tilemap.tiles[selectedTile].sprite);
				thumbnail.filterMode = FilterMode.Point;
				File.WriteAllBytes($"{THUMBNAIL_FOLDER}/{THUMBNAIL_PREFIX}_{tilemap.guid}.png", thumbnail.EncodeToPNG());
				AssetDatabase.ImportAsset($"{THUMBNAIL_FOLDER}/{THUMBNAIL_PREFIX}_{tilemap.guid}.png");
				AssetDatabase.Refresh();
				thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>($"{THUMBNAIL_FOLDER}/{THUMBNAIL_PREFIX}_{tilemap.guid}.png");
				thumbnail.filterMode = FilterMode.Point;
				serializedObject.FindProperty("item").FindPropertyRelative("thumbnail").objectReferenceValue = thumbnail;
			}
		}

		private void AutoTilesInspectorGUI(TilemapLevelObject tilemap, SerializedObject serializedObject)
		{

			//Autotile groups
			//Add new group
			GUILayout.Space(10f);
			GUILayout.Label("Autotile groups", LevelEditorStyles.HeaderCentered);
			GUILayout.Space(5f);
			newGroupName = EditorGUILayout.TextField("New group name: ", newGroupName);
			GUILayout.Space(5f);
			GUI.skin = prevSkin;
			if (string.IsNullOrWhiteSpace(newGroupName) || tilemap.autoTileGroups.Where((group) => group.autoTileGroupName == newGroupName).ToArray().Length > 0)
				GUI.enabled = false;
			if (GUILayout.Button("Add new group"))
            {
				serializedObject.FindProperty("autoTileGroups").arraySize += 1;
				SerializedProperty newAutoTileGroup = serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(serializedObject.FindProperty("autoTileGroups").arraySize - 1);
				newAutoTileGroup.FindPropertyRelative("autoTileGroupName").stringValue = newGroupName;
				newAutoTileGroup.FindPropertyRelative("autoTiles").arraySize = TILES_AMOUNT_PER_GROUP;
				for(int i = 0; i < TILES_AMOUNT_PER_GROUP; i++)
                {
					newAutoTileGroup.FindPropertyRelative("autoTiles").GetArrayElementAtIndex(i).intValue = -1;
				}
				EditorGUI.FocusTextInControl(null);
				newGroupName = "";
			}
			GUI.enabled = true;
			GUI.skin = levelEditorSkin;
			GUILayout.Space(10f);
			//Select group
			List<string> autoTileGroupNames = new List<string>();
			foreach(AutoTileGroup group in tilemap.autoTileGroups)
            {
				autoTileGroupNames.Add(group.autoTileGroupName);
            }
			if(autoTileGroupNames.Count > 0)
            {
				selectedAutotileGroup = EditorGUILayout.Popup("Select autotile group:", selectedAutotileGroup, autoTileGroupNames.ToArray());
            }
			GUILayout.Space(5f);

			//Draw auto tiles
			if(selectedAutotileGroup >= 0)
            {
				GUI.skin = prevSkin;
				GUI.enabled = clearAutoTile;
				if (GUILayout.Button("Remove autotile group"))
				{
					serializedObject.FindProperty("autoTileGroups").DeleteArrayElementAtIndex(selectedAutotileGroup);
					selectedAutotileGroup = -1;
					clearAutoTile = false;
				}
				else
				{
					GUI.enabled = true;
					GUI.skin = levelEditorSkin;
					GUILayout.Space(15f);
					DrawAutoTilePicker(tilemap.autoTileGroups[selectedAutotileGroup], tilemap, serializedObject);
				}
				GUI.enabled = true;
				GUI.skin = levelEditorSkin;
			}




			//Sprite overview GUI
			GUILayout.Space(15f);
			ShowAllTiles(tilemap, serializedObject, false);
			GUILayout.Space(10f);
		}

		private void DrawAutoTilePicker(AutoTileGroup group, TilemapLevelObject tilemap, SerializedObject serializedObject)
        {
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(150));
			int rows = 5;
			int cols = 4;
			float sizePerButton = Mathf.Min(EditorGUIUtility.currentViewWidth / cols - 40, 150);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10f);
			for (int x = 0; x < cols; x++)
			{
				GUILayout.BeginVertical();
				for (int y = 0; y < rows; y++)
				{
					GUILayout.Space(5f);
					GUILayout.BeginHorizontal();
					GUILayout.Space(5f);

					LevelTile currentTile = null;
					int index = x + y * cols;
					if (index >= 0 && index < group.autoTiles.Length) {
						currentTile = tilemap.GetTile(group.autoTiles[index]);
					


						if (selectedTile < 0 && !clearAutoTile)
							GUI.enabled = false;
						if (GUILayout.Button(GUIContent.none, LevelEditorStyles.Button, GUILayout.Width(sizePerButton), GUILayout.Height(sizePerButton)))
						{
							if (clearAutoTile)
							{
								serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
								.FindPropertyRelative("autoTiles").GetArrayElementAtIndex(index).intValue = -1;
							}
							else
							{
								serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
									.FindPropertyRelative("autoTiles").GetArrayElementAtIndex(index).intValue = selectedTile;
							}
						}
						Rect r = GUILayoutUtility.GetLastRect();
						GUI.enabled = true;

						if (currentTile != null)
						{
							Rect imageRect = new Rect()
							{
								position = new Vector2(r.position.x + 5, r.position.y + 5),
								size = new Vector2(r.width - 10, r.height - 10)
							};
							Texture2D texture = tileTextures[group.autoTiles[index]];
							GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleAndCrop, true, 0f, Color.white, 0f, 5f);
						}

						GUI.color = Color.white;

						GUIStyle symbolStyle = new GUIStyle(LevelEditorStyles.TextCentered);
						symbolStyle.fontSize = 60;
						symbolStyle.normal.textColor = new Color(1, 1, 1, 0.7f);
						symbolStyle.alignment = TextAnchor.MiddleCenter;
						GUI.Label(r, autoTileBackdrop[index], symbolStyle);

					}

					GUILayout.Space(5f);
					GUILayout.EndHorizontal();
					GUILayout.Space(5f);
				}
				GUILayout.EndVertical();
			}
			GUILayout.Space(10f);
			GUILayout.EndHorizontal();
			GUILayout.Space(15f);

			//Fallback tile
			GUILayout.BeginHorizontal();
			GUILayout.Label("Default Tile:", LevelEditorStyles.TextLeft, GUILayout.ExpandWidth(false));
			if (selectedTile < 0 && !clearAutoTile)
				GUI.enabled = false;
			if (GUILayout.Button(GUIContent.none, LevelEditorStyles.Button, GUILayout.Width(sizePerButton), GUILayout.Height(sizePerButton)))
			{
				if (clearAutoTile)
				{
					serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
						.FindPropertyRelative("fallbackTile").intValue = -1;
				}
				else
				{
					serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
						.FindPropertyRelative("fallbackTile").intValue = selectedTile;
				}
			}
			Rect rFallback = GUILayoutUtility.GetLastRect();
			GUI.enabled = true;
			if (group.fallbackTile >= 0 && group.fallbackTile < tileTextures.Count)
			{
				Rect imageRect = new Rect()
				{
					position = new Vector2(rFallback.position.x + 5, rFallback.position.y + 5),
					size = new Vector2(rFallback.width - 10, rFallback.height - 10)
				};
				Texture2D texture = tileTextures[group.fallbackTile];
				GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleAndCrop, true, 0f, Color.white, 0f, 5f);
			}
			GUIStyle symbolStyleFallback = new GUIStyle(LevelEditorStyles.TextCentered);
			symbolStyleFallback.fontSize = 60;
			symbolStyleFallback.normal.textColor = new Color(1, 1, 1, 0.7f);
			symbolStyleFallback.alignment = TextAnchor.UpperCenter;
			GUI.Label(rFallback, "*", symbolStyleFallback);
			GUILayout.EndHorizontal();

			//Clearing auto tiles
			GUILayout.Space(10f);

			GUI.skin = prevSkin;
			if (GUILayout.Button(clearAutoTile ? "Stop clearing autotiles" : "Enter Clear Autotile Mode"))
			{
				selectedTile = -1;
				clearAutoTile = !clearAutoTile;
			}
			GUI.skin = levelEditorSkin;
			GUILayout.EndVertical();
		}

        #endregion

        #region Custom Tool

        public override void OnApplySortingLayers(LevelObject obj, GameObject spawningObject, string sortingLayerName)
        {
			TilemapRenderer tilemapRenderer = t_currentTilemapRenderer;
			if (tilemapRenderer == null && spawningObject.transform.childCount > 0)
            {
				tilemapRenderer = spawningObject.transform.GetChild(0).GetComponent<TilemapRenderer>();
            }
			if(tilemapRenderer != null)
            {
				tilemapRenderer.sortingLayerName = sortingLayerName;
            }
        }

        public override void OnApplyPhysicsLayers(LevelObject obj, GameObject spawningObject, int targetLayer, bool onlyRootObject, LayerMask layersNotToOverride, bool removePhysicsComponents)
        {
			Tilemap tilemap = t_currentTilemap;
			if (tilemap == null && spawningObject.transform.childCount > 0)
			{
				tilemap = spawningObject.transform.GetChild(0).GetComponent<Tilemap>();
			}

			if (tilemap != null && (layersNotToOverride & 1 << tilemap.gameObject.layer) == 0)
            {
				tilemap.transform.parent.gameObject.layer = targetLayer;
				if(!onlyRootObject)
                {
					tilemap.gameObject.layer = targetLayer;
                }
				if (removePhysicsComponents)
				{
					Collider2D[] colliders = tilemap.gameObject.GetComponents<Collider2D>();
					for (int i = colliders.Length - 1; i >= 0; i--)
					{
						GameObject.DestroyImmediate(colliders[i]);
					}
					Rigidbody2D[] rigidbodies = tilemap.gameObject.GetComponents<Rigidbody2D>();
					for (int i = rigidbodies.Length - 1; i >= 0; i--)
					{
						GameObject.DestroyImmediate(rigidbodies[i]);
					}
				}
			}
        }

        public override bool UseTemporaryIndicator => false;

        public override GameObject SpawnObject(LevelObject obj, GameObject temporaryObject, Transform parentTransform, Vector2 worldPos, Vector2 mousePos)
        {
			if (t_currentTilemap == null)
			{
				//Check if Tilemap already spawned
				GameObject tilemap = null;
				foreach (Transform child in parentTransform)
				{
					if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject) && child.gameObject.name == obj.objectPrefab.name)
					{
						tilemap = child.gameObject;
						break;
					}
				}

				if (tilemap == null)
				{
					tilemap = base.SpawnObject(obj, temporaryObject, parentTransform, worldPos, mousePos);
				}

				tilemap.transform.position = Vector2.zero;

				Selection.activeGameObject = tilemap;
				Selection.activeTransform = tilemap.transform;
				if (Event.current.button == 0)
				{
					Event.current.Use();
				}

				return tilemap;
			}

			//Spawn tile
			SpawnTile(obj, worldPos);
			Selection.activeGameObject = t_currentTilemap.transform.parent.gameObject;
			Selection.activeTransform = t_currentTilemap.transform.parent;
			if (Event.current.button == 0)
			{
				Event.current.Use();
			}

			return t_currentTilemap.transform.parent.gameObject;

        }

        public override void OnLevelObjectSelected(LevelObject obj)
        {
			t_currentTilemap = null;
        }

		public override void OnLevelObjectDeSelected(LevelObject obj)
		{
			if(t_currentTilemap != null)
            {
				SaveTilemapInfo();
				t_currentTilemap.ClearAllEditorPreviewTiles();
            }
			t_currentTilemap = null;
			Selection.activeGameObject = null;
			Selection.activeTransform = null;
		}

		public override void HoverObject(LevelObject obj, GameObject temporaryObject, Vector2 worldPos, Vector2 mousePos)
        {
			if(t_currentTilemap != null && t_currentTilemapInformation != null && obj is TilemapLevelObject tilemapSettings)
            {
				LevelTile selectedTile = null;
				if (t_selectedTile >= 0)
				{
					selectedTile = tilemapSettings.GetTile(t_selectedTile);
				}
				else if(t_selectedAutoTileGroup >= 0)
				{
					selectedTile = GetPrimaryAutoTileGroupCoverTile(tilemapSettings, tilemapSettings.autoTileGroups[t_selectedAutoTileGroup]);
                }
				//Draw active cell box
				Vector3Int cell = t_currentTilemap.WorldToCell(worldPos);
				DrawTileBorder(cell, t_currentTilemapInformation.GetTileProperties(cell));

				//Set Preview Tile
				if (!t_inDeletionMode && selectedTile != null && cell != t_lastPreviewTilePos)
				{
					t_currentTilemap.SetEditorPreviewTile(t_lastPreviewTilePos, null);
					t_currentTilemap.SetEditorPreviewTile(cell, selectedTile);
					t_lastPreviewTilePos = cell;
				}
				SceneView.currentDrawingSceneView.Repaint();
			}
        }

        public override bool EnableDragging => true;

        public override void OnMouseDrag(LevelObject obj, GameObject temporaryObject, Vector2 worldPos, Vector2 mousePos)
        {
			if(t_currentTilemap != null)
            {
				Vector3Int currentDraggedTilePos = t_currentTilemap.WorldToCell(worldPos);

				if (currentDraggedTilePos != t_lastDraggedTilePos)
                {
					t_lastDraggedTilePos = currentDraggedTilePos;
					SpawnObject(obj, temporaryObject, t_currentTilemap.transform.parent.parent, worldPos, mousePos);
                }
				SceneView.currentDrawingSceneView.Repaint();
				Event.current.Use();
            }
        }

        private void DrawTileBorder(Vector3Int cell, TilemapCellProperties info)
		{
			if(t_inDeletionMode)
            {
				if(t_currentTilemap.GetTile(cell) != null)
                {
					DrawTileBorder(cell, LevelEditorStyles.buttonDangerColor, info);
                }
				else
                {
					DrawTileBorder(cell, Color.white, info);
				}
            }
			else if(t_currentTilemap.GetTile(cell) != null)
            {
				DrawTileBorder(cell, LevelEditorStyles.buttonHoverColor, info);
				
			}
			else
            {
				DrawTileBorder(cell, Color.white, info);
			}
		}

		private void DrawTileBorder(Vector3Int cell, Color color, TilemapCellProperties info)
        {
			Bounds cellBounds = t_currentTilemap.GetBoundsLocal(cell);
			Vector2 cellCenter = t_currentTilemap.GetCellCenterWorld(cell);
			Vector2 cellCenterLeft = new Vector2(cellCenter.x - cellBounds.extents.x, cellCenter.y);
			Vector2 cellCenterRight = new Vector2(cellCenter.x + cellBounds.extents.x, cellCenter.y);
			Vector2 cellCenterLeftScreen = Util.EditorUtility.WorldToSceneViewPos(cellCenterLeft);
			Vector2 cellCenterRightScreen = Util.EditorUtility.WorldToSceneViewPos(cellCenterRight);
			float cellScreenSize = cellCenterRightScreen.x - cellCenterLeftScreen.x;


			Rect drawRect = new Rect()
			{
				position = new Vector2(cellCenterLeftScreen.x, cellCenterLeftScreen.y - cellScreenSize / 2f),
				size = new Vector2(cellScreenSize, cellScreenSize)
			};

			GUI.color = color;
			GUI.Box(drawRect, " ");
			LevelTile myTile = t_currentTilemap.GetTile<LevelTile>(cell);
			if(myTile != null && info.isAutoTile)
            {
				GUIStyle labelMiddle = new GUIStyle(GUI.skin.label);
				labelMiddle.alignment = TextAnchor.MiddleCenter;
				GUI.Label(drawRect, info.autoTileGroup+"", labelMiddle);
            }
			GUI.color = Color.white;
		}

		private void SpawnTile(LevelObject obj, Vector2 worldPos)
		{
			if (t_currentTilemap != null && t_currentTilemapInformation != null && obj is TilemapLevelObject tilemapSettings)
			{
				Vector3Int cell = t_currentTilemap.WorldToCell(worldPos);
				
				if (t_inDeletionMode)
				{
					t_currentTilemap.SetTile(cell, null);

					if(t_menu == 1)
                    {
						TilemapCellProperties tileProperties = t_currentTilemapInformation.GetTileProperties(cell);
						//Update neighbor tiles
						UpdateNeighbourTiles(tilemapSettings, cell, AutoTileMode.All, true);
					}

					t_currentTilemapInformation.ClearTileProperties(cell);
				}
				else
				{
					if (t_menu == 0)
					{
						LevelTile selectedTile = tilemapSettings.GetTile(t_selectedTile);
						if (selectedTile != null)
						{
							t_currentTilemap.SetTile(cell, selectedTile);
							t_currentTilemapInformation.ClearTileProperties(cell);
						}
					}
					else if(t_menu == 1)
                    {
						TilemapCellProperties tileProperties = t_currentTilemapInformation.GetTileProperties(cell);
						LevelTile selectedTile = tilemapSettings.GetAutoTile(t_currentTilemap, t_currentTilemapInformation, t_selectedAutoTileGroup, worldPos, t_autoTileMode);
						if(selectedTile != null)
                        {
							t_currentTilemap.SetTile(cell, selectedTile);
							t_currentTilemapInformation.SetTileProperties(cell, new TilemapCellProperties(t_selectedAutoTileGroup));
							UnityEditor.EditorUtility.SetDirty(t_currentTilemapInformation.gameObject);
						}

						//Update neighbor tiles
						UpdateNeighbourTiles(tilemapSettings, cell, t_autoTileMode);
					}
				}
				SceneView.currentDrawingSceneView.Repaint();
				EditorSceneManager.MarkSceneDirty(t_currentTilemap.gameObject.scene);
			}
		}

		private void UpdateNeighbourTiles(TilemapLevelObject tilemapSettings, Vector3Int cell, AutoTileMode mode, bool forceDefaultGroup = false)
        {
			//Update neighbor tiles
			Vector3Int[] boundaryTilePositions = new Vector3Int[] // Left right top bottom top-left top-right bottom-left bottom-right
			{
				new Vector3Int (cell.x - 1, cell.y,     0),
				new Vector3Int (cell.x + 1, cell.y,     0),
				new Vector3Int (cell.x,     cell.y + 1, 0),
				new Vector3Int (cell.x,     cell.y - 1, 0),
				new Vector3Int (cell.x - 1, cell.y + 1, 0),
				new Vector3Int (cell.x + 1, cell.y + 1, 0),
				new Vector3Int (cell.x - 1, cell.y - 1, 0),
				new Vector3Int (cell.x + 1, cell.y - 1, 0)
			};
			int group = forceDefaultGroup ? 0 : t_selectedAutoTileGroup;
			foreach (Vector3Int boundaryTilePosition in boundaryTilePositions)
			{
				if (tilemapSettings.IsValidAutotile(boundaryTilePosition, t_currentTilemapInformation, group, mode))
				{
					LevelTile selectedBoundaryTile = tilemapSettings.GetAutoTile(t_currentTilemap, t_currentTilemapInformation, group, boundaryTilePosition, mode);
					if (selectedBoundaryTile != null)
					{
						t_currentTilemap.SetTile(boundaryTilePosition, selectedBoundaryTile);
						t_currentTilemapInformation.SetTileProperties(boundaryTilePosition, new TilemapCellProperties(group));
						UnityEditor.EditorUtility.SetDirty(t_currentTilemapInformation.gameObject);
					}
				}
			}
		}

		public override bool UseLevelEditorToolInspector()
        {
			return true;
        }

        #region Level Editor Tool Inspector

        public override void OnLevelEditorToolInspectorGUI(LevelObject obj)
        {
			if(Selection.activeTransform != null 
				&& PrefabUtility.IsAnyPrefabInstanceRoot(Selection.activeTransform.gameObject) 
				&& Selection.activeTransform.gameObject.name == obj.objectPrefab.name)
            {
				if(t_currentTilemap == null)
                {
					if (obj is TilemapLevelObject tilemapSettings)
					{
						InitializeTilemapTool(tilemapSettings);
					}
                }

				if(t_inDeletionMode && GUILayout.Button("Exit Deletion Mode", LevelEditorStyles.ButtonDangerActive)) {
					t_inDeletionMode = false;
                }
				else if (!t_inDeletionMode && GUILayout.Button("Enter Deletion Mode", LevelEditorStyles.Button)){
					t_inDeletionMode = true;
					t_selectedTile = -1;
					t_selectedAutoTileGroup = -1;
					t_currentTilemap.ClearAllEditorPreviewTiles();
                }

				GUISkin prevSkin = GUI.skin;
				GUI.skin = null;
				t_menu = GUILayout.Toolbar(t_menu, t_menuItems);
				GUI.skin = prevSkin;

				switch(t_menu)
                {
					case 0:
						ToolTileGUI(obj);
						break;
					case 1:
						ToolAutoTileGUI(obj);
						break;
                }

            }
			else
            {
				GUILayout.Label("Click on the Scene to spawn/select tilemap");
				base.OnLevelEditorToolInspectorGUI(obj);
				t_selectedTile = -1;
				t_selectedAutoTileGroup = -1;
				t_inDeletionMode = false;
            }
        }

		private void ToolTileGUI(LevelObject obj)
        {
			if (obj is TilemapLevelObject tilemapSettings)
			{
				ToolDrawTilePicker(tilemapSettings);
			}
		}

		private void ToolAutoTileGUI(LevelObject obj)
        {
			if (obj is TilemapLevelObject tilemapSettings)
			{
				ToolDrawAutoGroupPicker(tilemapSettings);
			}
		}

		private void InitializeTilemapTool(TilemapLevelObject obj)
        {
			
			foreach (Transform child in Selection.activeTransform)
			{
				t_currentTilemap = child.GetComponent<Tilemap>();
				if (t_currentTilemap != null)
				{
					t_currentTilemapInformation = null;
					t_currentTilemapRenderer = null;
					t_currentTilemapInformation = t_currentTilemap.transform.GetComponent<TilemapInformation>();
					t_currentTilemapRenderer = t_currentTilemap.transform.GetComponent<TilemapRenderer>();
					if (t_currentTilemapInformation != null && t_currentTilemapRenderer != null && t_currentTilemapInformation != null)
					{
						t_currentTilemapInformation.LoadTilemapInfo();
						break;
					}
					else
                    {
						t_currentTilemap = null;
                    }
				}
			}

			t_menu = 0;
			t_selectedTile = -1;
			t_tilesScrollpos = 0f;
			t_selectedAutoTileGroup = -1;
			t_autoTilesScrollpos = 0f;
			t_inDeletionMode = false;
			t_autoTileMode = AutoTileMode.SameGroup;

			GenerateTileTextures(obj);
        }

		private void GenerateTileTextures(TilemapLevelObject obj)
        {
			t_tileTextures = new List<Texture2D>();

			foreach(LevelTile tile in obj.tiles)
            {
				t_tileTextures.Add(LevelEditorStyles.TextureFromSprite(tile.sprite));
			}
        }

		#region Tile Picker

		private void ToolDrawTilePicker(TilemapLevelObject tilemapSettings)
        {
			GUILayout.Label("All Tiles", LevelEditorStyles.HeaderCentered);
			GUILayout.Space(5f);

			int objectsPerRow = Mathf.FloorToInt(LevelEditorTool.ObjectInspectorWidth / tileSize - 2f);
			int objectCount = tilemapSettings.tiles.Length;
			float containerHeight = Mathf.CeilToInt((float)objectCount / objectsPerRow) * (tileSize + 15f);
			containerHeight = Mathf.Min(containerHeight, 250f);

			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(containerHeight));

			t_tilesScrollpos = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(t_tilesScrollpos, 0f)), false, false, GUILayout.MaxHeight(300f)).y;

			GUILayout.BeginVertical();
			for (int i = 0; i < objectCount; i++)
			{
				if (i % objectsPerRow == 0)
					GUILayout.BeginHorizontal();

				ToolDrawTile(i, tilemapSettings.tiles[i], t_selectedTile == i, (selected) => {
					t_selectedTile = selected ? -1 : i;
					t_selectedAutoTileGroup = -1;
				});

				if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
				{
					GUILayout.EndHorizontal();
					if (i < objectCount - 1)
						GUILayout.Space(Mathf.Max(LevelEditorStyles.LevelObjectButton.margin.right, LevelEditorStyles.LevelObjectButton.margin.left));
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			if(Event.current.type == EventType.Repaint)
            {
				t_scrollRect = GUILayoutUtility.GetLastRect();
				t_scrollRect.position += LevelEditorTool.GetInspectorRect().position;
            }
			if (t_scrollRect.Contains(Event.current.mousePosition))
			{
				t_tilesScrollpos += LevelEditorTool.GetScroll();
			}

			GUILayout.EndVertical();
		}

		#endregion

		#region Auto Tile Picker

		private void ToolDrawAutoGroupPicker(TilemapLevelObject tilemapSettings)
		{

			GUILayout.Label("Auto Tiles", LevelEditorStyles.HeaderCentered);
			GUILayout.Space(5f);

			t_autoTileMode = (AutoTileMode)EditorGUILayout.EnumPopup(t_autoTileMode);
			
			int objectsPerRow = Mathf.FloorToInt(LevelEditorTool.ObjectInspectorWidth / tileSize - 2f);
			int objectCount = tilemapSettings.autoTileGroups.Length;
			float containerHeight = Mathf.CeilToInt((float)objectCount / objectsPerRow) * (tileSize + 15f);
			containerHeight = Mathf.Min(containerHeight, 250f);

			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(containerHeight));

			t_autoTilesScrollpos = GUILayout.BeginScrollView(new Vector2(0, Mathf.Max(t_autoTilesScrollpos, 0f)), false, false, GUILayout.MaxHeight(300f)).y;

			GUILayout.BeginVertical();
			for (int i = 0; i < objectCount; i++)
			{
				if (i % objectsPerRow == 0)
					GUILayout.BeginHorizontal();

				ToolDrawAutoTileGroup(tilemapSettings, i, tilemapSettings.autoTileGroups[i]);

				if ((i + 1) % objectsPerRow == 0 || i == objectCount - 1)
				{
					GUILayout.EndHorizontal();
					if (i < objectCount - 1)
						GUILayout.Space(Mathf.Max(LevelEditorStyles.LevelObjectButton.margin.right, LevelEditorStyles.LevelObjectButton.margin.left));
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			if (Event.current.type == EventType.Repaint)
			{
				t_scrollRect = GUILayoutUtility.GetLastRect();
				t_scrollRect.position += LevelEditorTool.GetInspectorRect().position;
			}
			if (t_scrollRect.Contains(Event.current.mousePosition))
			{
				t_tilesScrollpos += LevelEditorTool.GetScroll();
			}

			GUILayout.EndVertical();
		}

		private void ToolDrawAutoTileGroup(TilemapLevelObject tilemapSettings, int index, AutoTileGroup group)
        {
			LevelTile currentTile = GetPrimaryAutoTileGroupCoverTile(tilemapSettings, group);

			int currentTileIndex = tilemapSettings.GetTileIndex(currentTile);

			ToolDrawTile(currentTileIndex, currentTile, index == t_selectedAutoTileGroup, (selected) =>
			{
				t_selectedAutoTileGroup = selected ? -1 : index;
				t_selectedTile = -1;
			}, group.autoTileGroupName);
		}

		private LevelTile GetPrimaryAutoTileGroupCoverTile(TilemapLevelObject tilemapSettings, AutoTileGroup group)
        {
			LevelTile coverTile = tilemapSettings.GetTile(group.autoTiles[12]);
			if (coverTile == null)
			{
				coverTile = tilemapSettings.GetTile(group.fallbackTile);
				if (coverTile == null)
				{
					coverTile = tilemapSettings.GetTile(group.autoTiles.First((tile) =>
					{
						return tile >= 0;
					}));
				}
			}
			return coverTile;
		}

		#endregion

		#region Util

		private void ToolDrawTile(int index, LevelTile tile, bool selected, Action<bool> clickAction, string hovertext = "")
		{
			GUIStyle buttonStyle = selected ? LevelEditorStyles.ButtonActive : LevelEditorStyles.Button;
			if (GUILayout.Button(GUIContent.none, buttonStyle, GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
			{
				t_inDeletionMode = false;
				clickAction.Invoke(selected);
			}
			Rect r = GUILayoutUtility.GetLastRect();
			r.position = new Vector2(r.position.x + 5, r.position.y + 5);
			r.size = new Vector2(r.width - 10, r.height - 10);

			LevelEditorTool.DrawHoverText(r, hovertext, new Vector2(50f, -25f));

			if (tile != null && index < t_tileTextures.Count && t_tileTextures[index] != null)
			{
				GUI.DrawTexture(r, t_tileTextures[index], ScaleMode.ScaleAndCrop, true, 0, Color.white, 0f, 5f);
			}
			else
			{
				GUIStyle xStyle = new GUIStyle(LevelEditorStyles.HeaderCenteredBig);
				xStyle.alignment = TextAnchor.MiddleCenter;
				GUI.Label(r, "X", xStyle);
			}
		}

		private void SaveTilemapInfo()
        {
			if(t_currentTilemapInformation != null)
            {

				SerializedObject tilemapInfoSO = new SerializedObject(t_currentTilemapInformation);
				tilemapInfoSO.Update();
                SerializedProperty keys = tilemapInfoSO.FindProperty("serializedKeys");
				SerializedProperty values = tilemapInfoSO.FindProperty("serializedValues");
				keys.arraySize = 0;
				values.arraySize = 0;

				int i = 0;
				foreach (KeyValuePair<string, TilemapCellProperties> properties in t_currentTilemapInformation.tilemapInfo)
				{
					keys.arraySize += 1;
					keys.GetArrayElementAtIndex(i).stringValue = properties.Key;
					values.arraySize += 1;
					values.GetArrayElementAtIndex(i).FindPropertyRelative("isAutoTile").boolValue = properties.Value.isAutoTile;
					values.GetArrayElementAtIndex(i).FindPropertyRelative("autoTileGroup").intValue = properties.Value.autoTileGroup;
					i++;
				}

				tilemapInfoSO.ApplyModifiedProperties();
			}
        }

		#endregion

		#endregion

		#endregion

	}

}
