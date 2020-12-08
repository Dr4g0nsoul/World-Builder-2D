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

        public bool useCustomThumbnail;

        //Item display properties
        public LevelEditorItem item;

        //In which layers this item is available
        [LevelLayer] public int levelLayers;
        //Item category (e.g. Hazards, Semisolids, Enemies, Bosses, ...)
        [LevelObjectCategory] public int category = -1;
        



        /////////////
        // Methods //
        /////////////

        /// <summary>
        /// Puts all ids of the layers this level object is in into returnList
        /// </summary>
        /// <param name="returnList">An empty array that gets filled with ids (is empty if return value is not 0)</param>
        /// <returns>
        ///     0 if returnList was correctly filled
        ///     1 if no parallax Layer was assigned
        ///     2 if all parallax layers were assigned
        /// </returns>
        public int GetParallaxLayerIds(out List<int> returnList)
        {
            returnList = new List<int>();
            if (levelLayers == 0)
                return 1;
            else if (levelLayers < 0)
                return 2;
            
            for(int i = 0; i<LevelEditorSettings.MAX_LAYER_SIZE; i++)
            {
                if(HasParallaxLayer(i))
                {
                    returnList.Add(i);
                }
            }
            return 0;
        }

        public bool HasParallaxLayer(int layerID)
        {
            return (levelLayers & (1 << layerID)) != 0;
        }
    }

}
