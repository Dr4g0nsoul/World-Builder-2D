using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelObject : UniqueScriptableObject
    {

        //Prefab that will be spawned
        [ReadOnly] public GameObject objectPrefab;

        //Item display properties
        public LevelEditorItem item;

        //Main category (Color displayed in the level editor)
        public string mainCategory = null;
        //Item category (e.g. Hazards, Semisolids, Enemies, Bosses, ...)
        public string[] categories = null;
        //In which layers this item is available
        public string[] levelLayers = null;


    }

}
