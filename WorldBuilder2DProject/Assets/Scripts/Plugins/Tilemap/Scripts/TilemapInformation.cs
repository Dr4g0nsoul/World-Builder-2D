using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.TilemapPlugin
{

    public class TilemapInformation : MonoBehaviour
    {

        public string[] serializedKeys;
        public TilemapCellProperties[] serializedValues;
        public Dictionary<string, TilemapCellProperties> tilemapInfo = new Dictionary<string, TilemapCellProperties>();


        public void LoadTilemapInfo()
        {
            tilemapInfo.Clear();
            for(int i = 0; i<serializedKeys.Length; i++)
            {
                tilemapInfo.Add(serializedKeys[i], serializedValues[i]);
            }
        }

        public TilemapCellProperties GetTileProperties(Vector3Int cellPos)
        {
            string pos = $"{cellPos.x}_{cellPos.y}";
            TilemapCellProperties properties;
            tilemapInfo.TryGetValue(pos, out properties);
            return properties;
        }

        public void SetTileProperties(Vector3Int cellPos, TilemapCellProperties properties)
        {
            string pos = $"{cellPos.x}_{cellPos.y}";
            if(tilemapInfo.ContainsKey(pos))
            {
                tilemapInfo[pos] = properties;
            }
            else
            {
                tilemapInfo.Add(pos, properties);
            }
        }

        public void ClearTileProperties(Vector3Int cellPos)
        {
            string pos = $"{cellPos.x}_{cellPos.y}";
            if (tilemapInfo.ContainsKey(pos))
            {
                tilemapInfo.Remove(pos);
            }
        }
    }

    [System.Serializable]
    public struct TilemapCellProperties
    {
        public TilemapCellProperties(int autoTileGroup)
        {
            isAutoTile = true;
            this.autoTileGroup = autoTileGroup;
        }

        public bool isAutoTile;
        public int autoTileGroup;
    }
}
