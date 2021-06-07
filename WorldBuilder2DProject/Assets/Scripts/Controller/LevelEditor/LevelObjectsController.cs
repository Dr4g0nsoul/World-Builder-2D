using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            ReloadSettingsCache();
            LoadLevelObjects();
        }

        #endregion

        private Dictionary<string, LevelObject> levelObjects = new Dictionary<string, LevelObject>();

        //Filtering
        private Dictionary<string, LevelObject> filteredLevelObjects = new Dictionary<string, LevelObject>();
        private string lastSearch = "";
        private bool lastSearchAll = false;
        private Dictionary<string, LevelObject> searchFilteredLevelObjects = new Dictionary<string, LevelObject>();
        public enum PreferredItemsFilterMode { None, Level, World }

        //Level Object data
        private List<string> quickSelectBar = new List<string>();
        private List<string> currentWorldFavorites = new List<string>();
        private List<string> currentLevelFavorites = new List<string>();

        //Cached Level Editor Settings Data
        private Dictionary<string, LevelObjectCategory> levelObjectCategoryCache;
        private Dictionary<string, LevelLayer> levelLayerCache;

        #region Initialization

        public void LoadLevelObjects()
        {

            //Reset lists
            levelObjects.Clear();
            currentWorldFavorites.Clear();
            currentLevelFavorites.Clear();
            filteredLevelObjects.Clear();
            searchFilteredLevelObjects.Clear();


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

        public void ReloadSettingsCache()
        {
            LevelEditorSettings settings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            if(settings != null)
            {

                //Build category cache
                if (levelObjectCategoryCache == null)
                    levelObjectCategoryCache = new Dictionary<string, LevelObjectCategory>();
                
                levelObjectCategoryCache.Clear();

                foreach(LevelObjectCategory cat in settings.levelObjectCategories)
                {
                    levelObjectCategoryCache.Add(cat.guid, cat);
                }

                //Build level layer cache
                if (levelLayerCache == null)
                    levelLayerCache = new Dictionary<string, LevelLayer>();

                levelLayerCache.Clear();

                foreach (LevelLayer layer in settings.levelLayers)
                {
                    levelLayerCache.Add(layer.guid, layer);
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

        public Dictionary<string, LevelObject> GetAllLevelObjects()
        {
            return levelObjects;
        }

        public Dictionary<int, LevelObject> GetDummyLevelObjects(int amount)
        {
            int i = 0;
            Dictionary<int, LevelObject> result = new Dictionary<int, LevelObject>();
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

        #region Filtering level objects

        public Dictionary<string, LevelObject> GetFilteredLevelObjects(string search = null, bool searchAll = false)
        {
            if (string.IsNullOrEmpty(search))
            {
                return filteredLevelObjects;
            }
            else
            {
                if (search != lastSearch || lastSearchAll != searchAll)
                {
                    ApplySearchFilter(search, searchAll);
                }
                lastSearch = search;
                lastSearchAll = searchAll;

                return searchFilteredLevelObjects;
            }
        }

        public Dictionary<string, LevelObject> GetPreferredLevelObjects(PreferredItemsFilterMode preferredItemsFilterMode, string levelGuid)
        {
            Dictionary<string, LevelObject> preferredLevelObjects = new Dictionary<string, LevelObject>();

            LevelNode lNode = LevelController.Instance.GetLevel(levelGuid);

            if (preferredItemsFilterMode == PreferredItemsFilterMode.None || lNode == null)
            {
                preferredLevelObjects = new Dictionary<string, LevelObject>(levelObjects);
            }
            else
            {
                //Get Preferred items
                bool prefItemsFound = false;
                PreferredItems prefItems = new PreferredItems();
                if (preferredItemsFilterMode == PreferredItemsFilterMode.Level)
                {
                    prefItems = lNode.preferredItems;
                    prefItemsFound = true;
                }
                else if (preferredItemsFilterMode == PreferredItemsFilterMode.World)
                {
                    WorldNode world = LevelController.Instance.GetWorldByLevel(lNode.guid);
                    if (world != null)
                    {
                        prefItems = world.preferredItems;
                        prefItemsFound = true;
                    }
                }

                //Filter by preferred items
                if (prefItemsFound)
                {
                    foreach (LevelObject obj in levelObjects.Values)
                    {
                        //Check if level object id in level favorite objects
                        if (prefItems.levelObjects.Contains(obj.guid))
                        {
                            preferredLevelObjects.Add(obj.guid, obj);
                        }
                        //Check if any level category contained in level favorite categories
                        else
                        {
                            foreach (string category in obj.categories)
                            {
                                if (prefItems.categories.Contains(category))
                                {
                                    preferredLevelObjects.Add(obj.guid, obj);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return preferredLevelObjects;
        }

        public void ApplyFilters(PreferredItemsFilterMode preferredItemsFilterMode, string levelGuid)
        {
            filteredLevelObjects.Clear();

            filteredLevelObjects = GetPreferredLevelObjects(preferredItemsFilterMode, levelGuid);

        }

        public void ApplyFilters(List<string> categories, PreferredItemsFilterMode preferredItemsFilterMode = PreferredItemsFilterMode.None, string levelGuid = null)
        {
            filteredLevelObjects.Clear();

            Dictionary<string, LevelObject> preferredLevelObjects = GetPreferredLevelObjects(preferredItemsFilterMode, levelGuid);

            foreach (LevelObject obj in preferredLevelObjects.Values)
            {
                //Check if level object contains all categories
                if (obj != null && !categories.Except(obj.categories.ToList()).Any())
                    filteredLevelObjects.Add(obj.guid, obj);
            }
            lastSearch = "";
        }

        public void ApplyFilters(string layer, PreferredItemsFilterMode preferredItemsFilterMode = PreferredItemsFilterMode.None, string levelGuid = null)
        {
            filteredLevelObjects.Clear();

            Dictionary<string, LevelObject> preferredLevelObjects = GetPreferredLevelObjects(preferredItemsFilterMode, levelGuid);

            foreach (LevelObject obj in preferredLevelObjects.Values)
            {
                //Check if level object contains layer
                if (obj != null && obj.levelLayers != null && Array.Exists(obj.levelLayers, (val) => val == layer))
                {
                    filteredLevelObjects.Add(obj.guid, obj);
                }
            }
            lastSearch = "";
        }

        public void ApplyFilters(List<string> categories, string layer, PreferredItemsFilterMode preferredItemsFilterMode = PreferredItemsFilterMode.None, string levelGuid = null)
        {
            filteredLevelObjects.Clear();

            Dictionary<string, LevelObject> preferredLevelObjects = GetPreferredLevelObjects(preferredItemsFilterMode, levelGuid);

            //Category and layer filters
            foreach (LevelObject obj in preferredLevelObjects.Values)
            {
                //Check if level object contains layer
                if(obj != null && obj.levelLayers != null && Array.Exists(obj.levelLayers, (val) => val == layer))
                {
                    //Check if level object contains all categories
                    if (!categories.Except(obj.categories.ToList()).Any())
                        filteredLevelObjects.Add(obj.guid, obj);
                }
            }
            lastSearch = "";
        }

        public void AddCategoryFilter(string category)
        {
            foreach (LevelObject obj in filteredLevelObjects.Values.ToList())
            {
                if(obj != null && !Array.Exists(obj.categories, (val) => val == category))
                    filteredLevelObjects.Remove(obj.guid);
            }
            lastSearch = "";
        }

        public void ClearFilters()
        {
            filteredLevelObjects.Clear();
            lastSearch = "";
        }

        private void ApplySearchFilter(string search, bool searchAll = false)
        {
            searchFilteredLevelObjects.Clear();
            var objectsToSearchThrough = filteredLevelObjects.Values;
            if(searchAll)
            {
                objectsToSearchThrough = levelObjects.Values;
            }
            foreach(LevelObject obj in objectsToSearchThrough)
            {
                if(obj != null && obj.item.name.ToLower().Contains(search.ToLower()))
                {
                    searchFilteredLevelObjects.Add(obj.guid, obj);
                }
            }
        }

        #endregion

        #region Quick Select Bar

        public void AddToQuickSelectBar(string guid)
        {
            if(guid != null && levelObjects.ContainsKey(guid) && LevelEditorSettingsController.Instance.GetLevelEditorSettings() != null)
            {
                if(quickSelectBar.Count < LevelEditorSettingsController.Instance.GetLevelEditorSettings().quickSelectBarSize || quickSelectBar.Contains(guid))
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

        #region Level Object Category

        public LevelObjectCategory GetCategoryByGuid(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && levelObjectCategoryCache.TryGetValue(guid, out LevelObjectCategory ret))
            {
                return ret;
            }

            return null;
        }

        #endregion

        #region Level Object Layers

        public LevelLayer GetLevelLayerByGuid(string guid)
        {

            if (levelLayerCache.TryGetValue(guid, out LevelLayer ret))
            {
                return ret;
            }

            return null;
        }

        public List<LevelLayer> GetLevelObjectLayers(LevelObject obj)
        {
            LevelEditorSettings settings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            if(settings != null)
            {
                if (obj != null)
                {
                    List<LevelLayer> objectLayers = new List<LevelLayer>();
                    foreach(LevelLayer layer in settings.levelLayers)
                    {
                        if (obj.levelLayers.Contains(layer.guid))
                            objectLayers.Add(layer);
                    }
                    return objectLayers;
                }

                return settings.levelLayers.ToList();
            }
            return null;
        }

        #endregion

        #region Utility

        public Color GetAccentColor(string guid)
        {
            if (levelObjects.TryGetValue(guid, out LevelObject obj)) {
                return GetAccentColor(obj);
            }
            return Color.white;
        }

        public Color GetAccentColor(LevelObject obj)
        {
            if (obj.mainCategory != null && obj.mainCategory.Length > 0 && levelObjectCategoryCache.TryGetValue(obj.mainCategory, out LevelObjectCategory cat))
            {
                if (cat != null && cat.item.accentColor.a > 0)
                    return cat.item.accentColor;
            }
            else if (obj.categories != null && obj.categories.Length > 0)
            {
                string firstCategory = obj.categories[0];
                if (firstCategory != null && firstCategory.Length > 0 && levelObjectCategoryCache.TryGetValue(firstCategory, out LevelObjectCategory cat2))
                {
                    if (cat2 != null && cat2.item.accentColor.a > 0)
                        return cat2.item.accentColor;
                }
            }
            else if (obj.item.accentColor.a > 0)
                return obj.item.accentColor;

            return Color.white;
        }

        #endregion
    }
}