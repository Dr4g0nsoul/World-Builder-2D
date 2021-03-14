using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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
                    Object.DestroyImmediate(tmr);
                }
                if (tm != null)
                {
                    Object.DestroyImmediate(tm);
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
							Object[] spriteObjects = AssetDatabase.LoadAllAssetsAtPath(pathToTexture);
							foreach (Object spriteObject in spriteObjects)
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
						// Needs to be called twice because reasons
						SetThumbnail(tilemap.tiles[selectedTile].sprite, tilemap, serializedObject);
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

		private void DrawTile(int index, Tile tile)
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
					SetThumbnail(sprite, tilemap, serializedObject);
                }
				Tile existingTile = AssetDatabase.LoadAssetAtPath<Tile>(TILES_FOLDER + "/" + sprite.name + ".asset");
				//Create tile asset
				if (existingTile == null)
				{
					Tile newTile = ScriptableObject.CreateInstance<Tile>();
					newTile.name = sprite.name;
					newTile.sprite = sprite;
					Util.EditorUtility.CreateAssetAndFolders(TILES_FOLDER, sprite.name, newTile);
					existingTile = AssetDatabase.LoadAssetAtPath<Tile>(TILES_FOLDER + "/" + newTile.name + ".asset");
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

		private void SetThumbnail(Sprite sprite, TilemapLevelObject tilemap, SerializedObject serializedObject)
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
			serializedObject.FindProperty("item").FindPropertyRelative("thumbnail").objectReferenceValue = thumbnail;
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
			GUILayout.BeginVertical(EditorStyles.helpBox);
			int rows = 5;
			int cols = 4;
			float sizePerButton = Mathf.Min(EditorGUIUtility.currentViewWidth / cols - 40, 250);
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

					Tile currentTile = tilemap.GetTile(group.autoTiles[x + y * cols]);


					if (selectedTile < 0 && !clearAutoTile)
						GUI.enabled = false;
					if (GUILayout.Button(GUIContent.none, LevelEditorStyles.Button, GUILayout.Width(sizePerButton), GUILayout.Height(sizePerButton)))
					{
						if (clearAutoTile)
						{
							serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
							.FindPropertyRelative("autoTiles").GetArrayElementAtIndex(x + y * cols).intValue = -1;
						}
						else
						{
							serializedObject.FindProperty("autoTileGroups").GetArrayElementAtIndex(selectedAutotileGroup)
								.FindPropertyRelative("autoTiles").GetArrayElementAtIndex(x + y * cols).intValue = selectedTile;
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
						Texture2D texture = tileTextures[group.autoTiles[x + y * cols]];
						GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleAndCrop, true, 0f, Color.white, 0f, 5f);
					}

					GUI.color = Color.white;

					GUIStyle symbolStyle = new GUIStyle(LevelEditorStyles.TextCentered);
					symbolStyle.fontSize = 60;
					symbolStyle.normal.textColor = new Color(1, 1, 1, 0.7f);
					symbolStyle.alignment = TextAnchor.MiddleCenter;
					GUI.Label(r, autoTileBackdrop[x + y * cols], symbolStyle);

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
			if (group.fallbackTile >= 0)
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
	}

    #endregion

}
