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
		private int[] autoTileResultValue = new int[]
		{
			  0, 138, 203,  73,
			  8, 174, 255,  93,
			 12,  38,  55,  21,
			  4,   2,   3,   1,
			127, 191, 223, 239
		};

		public Tile[] tiles = new Tile[0];

		public enum AutoTileMode { All, SameGroup, None }

		
		public Tile GetTile(int selectedTileIndex)
        {
			if(selectedTileIndex >= 0 && selectedTileIndex < tiles.Length)
            {
				return tiles[selectedTileIndex];
            }
			return null;
        }

		public int GetTileIndex(Tile selectedTile)
        {
			if(selectedTile != null)
            {
				for (int i = 0; i<tiles.Length; i++)
                {
					if(tiles[i] == selectedTile)
                    {
						return i;
                    }

                }
            }
			return -1;
        }

		public Tile GetAutoTile(Grid tilemapGrid, Tilemap tilemap, int selectedGroupIndex, int selectedTileIndex, Vector2 worldPos, AutoTileMode mode = AutoTileMode.SameGroup)
		{
			if (selectedGroupIndex >= 0 && selectedGroupIndex < autoTileGroups.Length 
				&& selectedGroupIndex >= 0 && selectedTileIndex < autoTileGroups[selectedGroupIndex].autoTiles.Length
				&& tilemapGrid != null && tilemap != null)
			{
				Vector2Int tilePosition = (Vector2Int)tilemapGrid.WorldToCell(worldPos);

				Tile[] boundaryTiles = new Tile[] // Left right top bottom top-left top-right bottom-left bottom-right
                {
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x - 1, tilePosition.y, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x + 1, tilePosition.y, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x, tilePosition.y + 1, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x, tilePosition.y - 1, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x - 1, tilePosition.y + 1, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x + 1, tilePosition.y + 1, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x - 1, tilePosition.y - 1, 0)),
					tilemap.GetTile<Tile>(new Vector3Int (tilePosition.x + 1, tilePosition.y - 1, 0))
				};

				int score = 0;

				for(int i = 0; i < boundaryTiles.Length; i++)
                {
					if(boundaryTiles[i] != null && IsValidAutotile(boundaryTiles[i], selectedGroupIndex, mode))
                    {
						//2 to the power of ( i + 1 )
						score += 1 << (i + 1);
					}
                }

				int autoTilePosition = Array.IndexOf(autoTileGroups[selectedGroupIndex].autoTiles, score);
				if(autoTilePosition > 0)
                {
					return GetTile(autoTileGroups[selectedGroupIndex].autoTiles[autoTilePosition]);
                }
			}
			return null;
		}

		private bool IsValidAutotile(Tile tile, int selectedGroupIndex, AutoTileMode mode)
        {
			if(tile != null)
            {
				int tileIndex = Array.IndexOf(tiles, tile);
				if (tileIndex >= 0)
				{
					if (mode == AutoTileMode.All)
					{
						foreach (AutoTileGroup autotileGroup in autoTileGroups)
						{
							if (autotileGroup.autoTiles.Contains(tileIndex))
								return true;
						}
					}
					else if (mode == AutoTileMode.SameGroup)
					{
						AutoTileGroup currentAutoTileGroup = autoTileGroups[selectedGroupIndex];
						if (currentAutoTileGroup.autoTiles.Contains(tileIndex))
							return true;
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
}