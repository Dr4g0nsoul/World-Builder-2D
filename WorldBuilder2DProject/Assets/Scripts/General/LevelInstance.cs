﻿using dr4g0nsoul.WorldBuilder2D.Util;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelInstance : MonoBehaviour
    {
        public const string NO_LAYER = "Unknown Layer";
        public const string UNCATEGORIZED = "Uncategorized";


        [ReadOnly] public string level;

        private LevelNode levelNode;
        public LevelNode Level
        {
            get
            {
                if(levelNode == null)
                {
                    LevelController controller = LevelController.Instance;
                    if(controller != null)
                    {
                        levelNode = controller.GetLevel(level);
                    }
                }
                return levelNode;
            }
        }

        //Level Transforms
        public LevelTransformLayer[] levelTransforms;
        public LevelTransformLayer unknownLayer;

        #region Hierarchy Management

        #region Public Functions

        public LevelTransformLayer[] GetLevelLayers()
        {
            return levelTransforms;
        }

        #endregion

        private void CheckTransforms()
        {
            if(levelTransforms == null)
            {
                levelTransforms = new LevelTransformLayer[0];
            }
            for(int i = levelTransforms.Length - 1; i >= 0; i--)
            {
                if(levelTransforms[i].layerTransform == null)
                {
                    List<LevelTransformLayer> levelTransformsList = levelTransforms.ToList();
                    levelTransformsList.RemoveAt(i);
                    levelTransforms = levelTransformsList.ToArray();
                }
                else
                {

                    for(int j = levelTransforms[i].levelTransformCategories.Length - 1; j >= 0; j--)
                    {
                        if (levelTransforms[i].levelTransformCategories[j].categoryTransform == null)
                        {
                            List<LevelTransformCategory> levelTransformCategoriesList = levelTransforms[i].levelTransformCategories.ToList();
                            levelTransformCategoriesList.RemoveAt(j);
                            levelTransforms[i].levelTransformCategories = levelTransformCategoriesList.ToArray();
                        }
                    }

                    if(levelTransforms[i].uncategorized.categoryTransform == null)
                    {
                        levelTransforms[i].uncategorized = CreateLevelTransformUncategorized(levelTransforms[i]);
                    }
                }
            }
            if(unknownLayer.layerTransform == null)
            {
                unknownLayer = CreateUnknownLevelTransformLayer();
            }
        }

        public Transform FindParentTransform(LevelObject obj, string levelLayer, LevelEditorSettings settings)
        {
            if(obj != null && !string.IsNullOrEmpty(levelLayer) && settings != null)
            {
                CheckTransforms();

                LevelLayer levelLayerObject = null;
                //Check if layer exists
                if (!Array.Exists(settings.levelLayers, (val) => 
                { 
                    if(val.guid == levelLayer)
                    {
                        levelLayerObject = val;
                        return true;
                    }
                    return false;
                }))
                {
                    //Assign it to unkown layer
                    return FindParentTransformCategory(obj, unknownLayer, settings);
                }

                //Get layer
                foreach(LevelTransformLayer transformLayer in levelTransforms)
                {
                    if(transformLayer.layerGuid == levelLayer)
                    {
                        return FindParentTransformCategory(obj, transformLayer, settings);
                    }
                }

                //Create layer because not found and use that instead
                return FindParentTransformCategory(obj, CreateLevelTransformLayer(levelLayerObject), settings);
                
            }

            return null;
        }

        private Transform FindParentTransformCategory(LevelObject obj, LevelTransformLayer transformLayer, LevelEditorSettings settings)
        {
            string targetCategory = obj.mainCategory;

            if(obj != null && !string.IsNullOrEmpty(targetCategory) && settings != null)
            {
                LevelObjectCategory categoryObject = null;
                
                //Check if category exists
                if (!Array.Exists(settings.levelObjectCategories, (val) => 
                { 
                    if (val.guid == targetCategory)
                    {
                        categoryObject = val;
                        return true;
                    }
                    return false;
                }))
                {
                    //Assign it to uncategorized
                    return FindLevelObjectTransform(obj, unknownLayer.uncategorized);
                }

                //Get category
                foreach (LevelTransformCategory transformCategory in transformLayer.levelTransformCategories)
                {
                    if (transformCategory.categoryGuid == targetCategory)
                    {
                        return FindLevelObjectTransform(obj, transformCategory);
                    }
                }

                //Create category transform because not found
                return FindLevelObjectTransform(obj, CreateLevelTransformCategory(categoryObject, transformLayer));

            }

            return null;
        }

        private Transform FindLevelObjectTransform(LevelObject obj, LevelTransformCategory levelTransformCategory)
        {
            if(obj != null && !string.IsNullOrEmpty(obj.item.name) && levelTransformCategory.categoryTransform != null)
            {
                //Search for first transfrom with the level object name which is not a prefab
                foreach(Transform potentialParent in levelTransformCategory.categoryTransform)
                {
                    if(potentialParent.parent == levelTransformCategory.categoryTransform //Is direct child
                        && PrefabUtility.GetCorrespondingObjectFromSource(potentialParent) == null //Is not a prefab
                        //&& potentialParent.GetComponents(typeof(Component)).Length < 2 //Has only transform component on it
                        && potentialParent.name == obj.item.name) //Has the same name
                    {
                        return potentialParent;
                    }
                }
            }

            //If none is found create it
            GameObject levelObjectContainer = new GameObject();
            levelObjectContainer.name = obj.name;
            levelObjectContainer.transform.position = Vector2.zero;
            levelObjectContainer.transform.rotation = Quaternion.identity;
            levelObjectContainer.transform.parent = levelTransformCategory.categoryTransform;

            return levelObjectContainer.transform;
        }

        private LevelTransformLayer CreateLevelTransformLayer(LevelLayer layer)
        {
            LevelTransformLayer newLevelTransformLayer = new LevelTransformLayer();
            if (layer != null)
            {
                //Create Layer container
                GameObject containerObject = new GameObject();
                containerObject.transform.position = Vector2.zero;
                containerObject.transform.rotation = Quaternion.identity;
                containerObject.name = layer.item.name;
                containerObject.transform.parent = transform;

                //Create Layer transform container
                newLevelTransformLayer.layerGuid = layer.guid;
                newLevelTransformLayer.layerTransform = containerObject.transform;
                newLevelTransformLayer.levelTransformCategories = new LevelTransformCategory[0];

                //Create unknown category
                GameObject unknownCategory = new GameObject();
                unknownCategory.transform.position = Vector2.zero;
                unknownCategory.transform.rotation = Quaternion.identity;
                unknownCategory.name = UNCATEGORIZED;
                unknownCategory.transform.parent = newLevelTransformLayer.layerTransform;

                //Create unknown category transform container
                LevelTransformCategory unknownCategoryTransformContainer = new LevelTransformCategory()
                {
                    categoryGuid = Guid.Empty.ToString(),
                    categoryTransform = unknownCategory.transform
                };
                newLevelTransformLayer.uncategorized = unknownCategoryTransformContainer;

                //Resize array and add layer
                Array.Resize(ref levelTransforms, levelTransforms.Length + 1);
                levelTransforms[levelTransforms.Length - 1] = newLevelTransformLayer;

            }

            return newLevelTransformLayer;
        }

        private LevelTransformLayer CreateUnknownLevelTransformLayer()
        {
            LevelTransformLayer newLevelTransformLayer = new LevelTransformLayer();
            //Create Layer container
            GameObject containerObject = new GameObject();
            containerObject.transform.position = Vector2.zero;
            containerObject.transform.rotation = Quaternion.identity;
            containerObject.name = NO_LAYER;
            containerObject.transform.parent = transform;

            //Create Layer transform container
            newLevelTransformLayer.layerGuid = Guid.Empty.ToString();
            newLevelTransformLayer.layerTransform = containerObject.transform;
            newLevelTransformLayer.levelTransformCategories = new LevelTransformCategory[0];

            //Create unknown category
            GameObject unknownCategory = new GameObject();
            unknownCategory.transform.position = Vector2.zero;
            unknownCategory.transform.rotation = Quaternion.identity;
            unknownCategory.name = UNCATEGORIZED;
            unknownCategory.transform.parent = newLevelTransformLayer.layerTransform;

            //Create unknown category transform container
            LevelTransformCategory unknownCategoryTransformContainer = new LevelTransformCategory()
            {
                categoryGuid = Guid.Empty.ToString(),
                categoryTransform = unknownCategory.transform
            };
            newLevelTransformLayer.uncategorized = unknownCategoryTransformContainer;

            //Resize array and add layer
            Array.Resize(ref levelTransforms, levelTransforms.Length + 1);
            levelTransforms[levelTransforms.Length - 1] = newLevelTransformLayer;

            return newLevelTransformLayer;
        }

        private LevelTransformCategory CreateLevelTransformUncategorized(LevelTransformLayer parentTransformLayer)
        {
            LevelTransformCategory newLevelTransformCategory = new LevelTransformCategory();
            if (parentTransformLayer.layerTransform != null)
            {
                GameObject containerObject = new GameObject();
                containerObject.transform.position = Vector2.zero;
                containerObject.transform.rotation = Quaternion.identity;
                containerObject.name = UNCATEGORIZED;
                containerObject.transform.parent = parentTransformLayer.layerTransform;

                newLevelTransformCategory.categoryGuid = Guid.Empty.ToString();
                newLevelTransformCategory.categoryTransform = containerObject.transform;
            }

            return newLevelTransformCategory;
        }

        private LevelTransformCategory CreateLevelTransformCategory(LevelObjectCategory category, LevelTransformLayer parentTransformLayer)
        {
            LevelTransformCategory newLevelTransformCategory = new LevelTransformCategory();
            if (category != null && parentTransformLayer.layerTransform != null)
            {

                GameObject containerObject = new GameObject();
                containerObject.transform.position = Vector2.zero;
                containerObject.transform.rotation = Quaternion.identity;
                containerObject.name = category.item.name;
                containerObject.transform.parent = parentTransformLayer.layerTransform;

                newLevelTransformCategory.categoryGuid = category.guid;
                newLevelTransformCategory.categoryTransform = containerObject.transform;

                //Get LevelInstance transform layer
                int index = -1;
                for(int i = 0; i < levelTransforms.Length; i++)
                {
                    if (levelTransforms[i].layerGuid == parentTransformLayer.layerGuid)
                    {
                        index = i;
                        break;
                    }
                }

                Array.Resize(ref levelTransforms[index].levelTransformCategories, levelTransforms[index].levelTransformCategories.Length + 1);
                levelTransforms[index].levelTransformCategories[levelTransforms[index].levelTransformCategories.Length - 1] = newLevelTransformCategory;

            }

            return newLevelTransformCategory;
        }

        #endregion

    }

    [Serializable]
    public struct LevelTransformLayer
    {
        public string layerGuid;
        public Transform layerTransform;
        public LevelTransformCategory[] levelTransformCategories;
        public LevelTransformCategory uncategorized;
    }

    [Serializable]
    public struct LevelTransformCategory
    {
        public string categoryGuid;
        public Transform categoryTransform;
    }
}
