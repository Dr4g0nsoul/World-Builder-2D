﻿using System;
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

        private SortedDictionary<string, LevelObject> levelObjects = new SortedDictionary<string, LevelObject>();

        //Filtering
        private SortedDictionary<string, LevelObject> filteredLevelObjects = new SortedDictionary<string, LevelObject>();
        private string lastSearch = "";
        private bool lastSearchAll = false;
        private SortedDictionary<string, LevelObject> searchFilteredLevelObjects = new SortedDictionary<string, LevelObject>();

        //Level Object data
        private List<string> quickSelectBar = new List<string>();
        private List<string> currentWorldFavorites = new List<string>();
        private List<string> currentLevelFavorites = new List<string>();

        //Cached Level Editor Settings Data
        private SortedDictionary<string, LevelObjectCategory> levelObjectCategoryCache;
        private SortedDictionary<string, LevelLayer> levelLayerCache;

        #region Initialization

        public void LoadLevelObjects()
        {

            //Reset lists
            levelObjects.Clear();
            currentWorldFavorites.Clear();
            currentLevelFavorites.Clear();


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
                    levelObjectCategoryCache = new SortedDictionary<string, LevelObjectCategory>();
                
                levelObjectCategoryCache.Clear();

                foreach(LevelObjectCategory cat in settings.levelObjectCategories)
                {
                    levelObjectCategoryCache.Add(cat.guid, cat);
                }

                //Build level layer cache
                if (levelLayerCache == null)
                    levelLayerCache = new SortedDictionary<string, LevelLayer>();

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

        #region Filtering level objects

        public SortedDictionary<string, LevelObject> GetFilteredLevelObjects(string search = null, bool searchAll = false)
        {
            if (string.IsNullOrEmpty(search))
            {
                return filteredLevelObjects;
            }
            else
            {
                if (search != lastSearch || lastSearchAll != searchAll)
                {
                    Debug.Log("hi");
                    ApplySearchFilter(search, searchAll);
                }
                lastSearch = search;
                lastSearchAll = searchAll;

                return searchFilteredLevelObjects;
            }
        }

        public void ApplyFilters(List<string> categories)
        {
            filteredLevelObjects.Clear();
            foreach (LevelObject obj in levelObjects.Values)
            {
                //Check if level object contains all categories
                if (!categories.Except(obj.categories.ToList()).Any())
                    filteredLevelObjects.Add(obj.guid, obj);
            }
            lastSearch = "";
        }

        public void ApplyFilters(string layer)
        {
            filteredLevelObjects.Clear();
            foreach (LevelObject obj in levelObjects.Values)
            {
                //Check if level object contains layer
                if (Array.Exists(obj.levelLayers, (val) => val == layer))
                {
                    filteredLevelObjects.Add(obj.guid, obj);
                }
            }
            lastSearch = "";
        }

        public void ApplyFilters(List<string> categories, string layer)
        {
            filteredLevelObjects.Clear();
            foreach(LevelObject obj in levelObjects.Values)
            {
                //Check if level object contains layer
                if(Array.Exists(obj.levelLayers, (val) => val == layer))
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
                if(!Array.Exists(obj.categories, (val) => val == category))
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
                if(obj.item.name.ToLower().Contains(search.ToLower()))
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

        #region Level Object Layers

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
            else if (obj.categories.Length > 0)
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