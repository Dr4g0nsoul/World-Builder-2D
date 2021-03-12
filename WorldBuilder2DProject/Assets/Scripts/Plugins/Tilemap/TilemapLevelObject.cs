using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{

    public class TilemapLevelObject : LevelObject
    {

        public override GameObject OnLevelObjectCreate(GameObject currentPrefab, string prefabPath)
        {
            currentPrefab.transform.position = Vector3.zero;
            currentPrefab.transform.rotation = Quaternion.identity;
            if (currentPrefab.GetComponent<Grid>() == null)
                currentPrefab.AddComponent<Grid>();

            Transform previousTilemap = null;

            foreach(Transform child in currentPrefab.transform)
            {
                Tilemap tm = child.GetComponent<Tilemap>();
                TilemapRenderer tmr = child.GetComponent<TilemapRenderer>();

                if(tmr != null)
                {
                    DestroyImmediate(tmr);
                }
                if(tm != null)
                {
                    DestroyImmediate(tm);
                    if(child.parent == currentPrefab.transform)
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
    }

}
