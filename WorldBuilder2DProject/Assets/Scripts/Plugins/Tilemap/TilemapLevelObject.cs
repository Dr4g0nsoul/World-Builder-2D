using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace dr4g0nsoul.WorldBuilder2D.TilemapPlugin
{

	public class TilemapLevelObject : LevelObject
	{

		/**
		 * Auto Tile id's
		 * -------------
		 *   0  1  2  3
		 *   4  5  6  7
		 *   8  9 10 11
		 *  12 13 14 15
		 *  16 17 18 19
		 * -------------
		 * Neighbour position values
		 * -------------------------
		 *   16   4  32
		 *    1   0   2
		 *   64   8 128
		 * -------------------------
		*/
		public AutoTileGroup[] autoTileGroups = new AutoTileGroup[0];
		private readonly int[] autoTileResultValue = new int[]
		{
			  8, 138, 203,  73,
			 12, 174, 255,  93,
			  4,  38,  55,  21,
			  0,   2,   3,   1,
			127, 191, 223, 239
		};
		private readonly Dictionary<int, int[]> altAutoTileResultValues = new Dictionary<int, int[]>()
		{
			{138, new int[]{2, 10, 130, 136, 170, 202, 42, 74, 234} }, //Top-left corner
			{203, new int[]{8, 3, 11, 67, 193, 131, 194, 75, 139, 195, 200, 219, 235, 91, 251} }, //Top-center
			{73, new int[]{1, 9, 65, 72, 89, 201, 25, 217} }, //Top-right corner
			{174, new int[]{12, 14, 140, 168, 44, 164, 142, 46, 172, 162, 190, 238, 156, 158, 254 } }, //Center-left
			{255, new int[]{} }, //Center
			{93, new int[]{13, 76, 88, 28, 84, 77, 29, 81, 92, 125, 221, 125, 253} }, //Center-right
			{38, new int[]{6, 34, 36, 166, 54, 134, 22, 182} }, //Bottom-left
			{55, new int[]{4, 7, 19, 49, 35, 11, 23, 39, 50, 51, 119, 183, 247} }, //Bottom-center
			{21, new int[]{5, 17, 20, 85, 53, 68, 117} } //Bottom-right
		};

		public LevelTile[] tiles = new LevelTile[0];

		public LevelTile GetTile(int selectedTileIndex)
		{
			if (selectedTileIndex >= 0 && selectedTileIndex < tiles.Length)
			{
				return tiles[selectedTileIndex];
			}
			return null;
		}

		public int GetTileIndex(LevelTile selectedTile)
		{
			if (selectedTile != null)
			{
				for (int i = 0; i < tiles.Length; i++)
				{
					if (tiles[i] == selectedTile)
					{
						return i;
					}

				}
			}
			return -1;
		}

		public LevelTile GetAutoTile(Tilemap tilemap, TilemapInformation tilemapInformation, int selectedGroupIndex, Vector2 worldPos, AutoTileMode mode = AutoTileMode.SameGroup)
		{
			if (tilemap != null && tilemapInformation != null)
			{
				return GetAutoTile(tilemap, tilemapInformation, selectedGroupIndex, tilemap.WorldToCell(worldPos), mode);
			}
			return null;
		}

		public LevelTile GetAutoTile(Tilemap tilemap, TilemapInformation tilemapInformation, int selectedGroupIndex, Vector3Int tilePosition, AutoTileMode mode = AutoTileMode.SameGroup)
		{
			if (selectedGroupIndex >= 0 && selectedGroupIndex < autoTileGroups.Length
				&& selectedGroupIndex >= 0 && tilemap != null && tilemapInformation != null)
			{
				Vector3Int[] boundaryPositions = new Vector3Int[] // Left right top bottom top-left top-right bottom-left bottom-right
                {
					new Vector3Int (tilePosition.x - 1, tilePosition.y, 0),
					new Vector3Int (tilePosition.x + 1, tilePosition.y, 0),
					new Vector3Int (tilePosition.x, tilePosition.y + 1, 0),
					new Vector3Int (tilePosition.x, tilePosition.y - 1, 0),
					new Vector3Int (tilePosition.x - 1, tilePosition.y + 1, 0),
					new Vector3Int (tilePosition.x + 1, tilePosition.y + 1, 0),
					new Vector3Int (tilePosition.x - 1, tilePosition.y - 1, 0),
					new Vector3Int (tilePosition.x + 1, tilePosition.y - 1, 0)
				};

				int score = 0;

				for (int i = 0; i < boundaryPositions.Length; i++)
				{
					if (IsValidAutotile(boundaryPositions[i], tilemapInformation, selectedGroupIndex, mode))
					{
						//2 to the power of ( i + 1 )
						score += 1 << i;
					}
				}

				int autoTilePosition = Array.IndexOf(autoTileResultValue, score);
				LevelTile resultingTile;
				//Check if result was found
				if (autoTilePosition >= 0)
				{
					resultingTile = GetTile(autoTileGroups[selectedGroupIndex].autoTiles[autoTilePosition]);
					if (resultingTile != null)
					{
						return resultingTile;
					}
				}

				//Check alternate results
				foreach (KeyValuePair<int, int[]> alternativeResult in altAutoTileResultValues)
				{
					foreach (int alternativeResultValue in alternativeResult.Value)
					{
						if (alternativeResultValue == score)
						{
							autoTilePosition = Array.IndexOf(autoTileResultValue, alternativeResult.Key);
							resultingTile = GetTile(autoTileGroups[selectedGroupIndex].autoTiles[autoTilePosition]);
							if (resultingTile != null)
							{
								//Debug.Log($"Found alternative result {alternativeResultValue} for result {alternativeResult.Key} with score {score}");
								return resultingTile;
							}
						}
					}
				}

				//Debug.Log("Use fallback Tile");
				return GetTile(autoTileGroups[selectedGroupIndex].fallbackTile);
			}
			return null;
		}

		public bool IsValidAutotile(Vector3Int position, TilemapInformation tilemapInformation, int selectedGroupIndex, AutoTileMode mode)
		{
			if (tilemapInformation != null) {

				TilemapCellProperties properties = tilemapInformation.GetTileProperties(position);

				if (selectedGroupIndex >= 0 && selectedGroupIndex < autoTileGroups.Length)
				{
					if (mode == AutoTileMode.All)
					{
						return properties.isAutoTile;
					}
					else if (mode == AutoTileMode.SameGroup)
					{
						return properties.isAutoTile && properties.autoTileGroup == selectedGroupIndex;
					}
				}
			}
			return false;
		}
	}

	[System.Serializable]
	public struct AutoTileGroup
	{
		public string autoTileGroupName;
		public int[] autoTiles;
		public int fallbackTile;
	}

	public enum AutoTileMode { All, SameGroup, None }

}