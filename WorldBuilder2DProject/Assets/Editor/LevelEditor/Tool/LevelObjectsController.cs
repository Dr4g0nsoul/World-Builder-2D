using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{

    public class LevelObjectsController
    {
        public const string LEVEL_OBJECTS_PATH = "LevelEditor/LevelObjects";

        #region Singleton

        private static LevelObjectsController s_instance;
        public static LevelObjectsController Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new LevelObjectsController();
                }

                return s_instance;
            }
        }

        private LevelObjectsController()
        {
            LoadLevelObjects();
        }

        #endregion

        private SortedDictionary<string, LevelObject> levelObjects = new SortedDictionary<string, LevelObject>();

        private List<string> quickSelectBar = new List<string>();
        private SortedDictionary<string, List<string>> levelObjectsPerCategory = new SortedDictionary<string, List<string>>();
        private List<string> currentWorldFavorites = new List<string>();
        private List<string> currentLevelFavorites = new List<string>();

        #region Initialization

        public void LoadLevelObjects()
        {
            //Reset lists
            levelObjects = new SortedDictionary<string, LevelObject>();
            levelObjectsPerCategory = new SortedDictionary<string, List<string>>();
            currentWorldFavorites = new List<string>();
            currentLevelFavorites = new List<string>();

            //Load level objects
            LevelObject[] loadedLevelObjects = Resources.LoadAll<LevelObject>(LEVEL_OBJECTS_PATH);
            foreach(LevelObject lobj in loadedLevelObjects)
            {
                levelObjects.Add(lobj.guid, lobj);
            }

            //Update quick select bar
            if (quickSelectBar == null)
                quickSelectBar = new List<string>();
            else
            {
                for (int i = quickSelectBar.Count - 1; i >= 0; i--)
                {
                    if (!levelObjects.ContainsKey(quickSelectBar[i]))
                    {
                        quickSelectBar.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region Get Level Objects

        public LevelObject GetLevelObjectByGuid(string guid)
        {

            if (levelObjects.TryGetValue(guid, out LevelObject ret))
            {
                return ret;
            }

            return null;
        }

        public LevelObject GetLevelObjectByPrefab(GameObject prefab)
        {
            foreach(LevelObject lo in levelObjects.Values)
            {
                if (lo.objectPrefab == prefab)
                    return lo;
            }
            return null;
        }

        public SortedDictionary<string, LevelObject> GetAllLevelObjects()
        {
            return levelObjects;
        }

        public SortedDictionary<int, LevelObject> GetDummyLevelObjects(int amount)
        {
            int i = 0;
            SortedDictionary<int, LevelObject> result = new SortedDictionary<int, LevelObject>();
            while (i < amount)
            {
                foreach (LevelObject lobj in levelObjects.Values)
                {
                    result.Add(i, lobj);
                    i++;
                    if (i >= amount) break;
                }
            }
            return result;
        }

        #endregion

        #region Quick Select Bar

        public void AddToQuickSelectBar(string guid)
        {
            if(guid != null && levelObjects.ContainsKey(guid) && LevelEditorTool.GetLevelEditorSettings() != null)
            {
                if(quickSelectBar.Count < LevelEditorTool.GetLevelEditorSettings().quickSelectBarSize || quickSelectBar.Contains(guid))
                {
                    //If item already in quick select, move it to the front
                    if(quickSelectBar.Contains(guid))
                    {
                        quickSelectBar.Remove(guid);
                    }
                    quickSelectBar.Insert(0, guid);
                }
            }
        }

        public List<LevelObject> GetQuickSelectBar()
        {
            List<LevelObject> result = new List<LevelObject>();

            foreach(string guid in quickSelectBar)
            {
                if(levelObjects.TryGetValue(guid, out LevelObject item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        #endregion
    }
}