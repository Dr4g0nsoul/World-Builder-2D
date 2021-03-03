using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelInstance : MonoBehaviour
    {
        public const string NO_LAYER = "Unknown Layer";
        public const string UNCATEGORIZED = "Uncategorized";


        [ReadOnly] public string level;

        //Level Transforms
        public LevelTransformLayer[] levelTransforms;
        public LevelTransformLayer unknownLayer;

        #region Hierarchy Management

        public Transform FindParentTransform(LevelObject obj, string levelLayer, LevelEditorSettings settings)
        {
            if(obj != null && !string.IsNullOrEmpty(levelLayer) && settings != null)
            {
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
                    return FindParentTransformCategory(obj.mainCategory, unknownLayer, settings);
                }

                //Get layer
                foreach(LevelTransformLayer transformLayer in levelTransforms)
                {
                    if(transformLayer.layerGuid == levelLayer)
                    {
                        Transform a = FindParentTransformCategory(obj.mainCategory, transformLayer, settings);
                        Debug.Log($"return2 {a}");
                        return a;
                    }
                }

                //Create layer because not found and use that instead
                return FindParentTransformCategory(obj.mainCategory, CreateLevelTransformLayer(levelLayerObject), settings);
                
            }

            return null;
        }

        private Transform FindParentTransformCategory(string targetCategory, LevelTransformLayer transformLayer, LevelEditorSettings settings)
        {
            if(!string.IsNullOrEmpty(targetCategory) && settings != null)
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
                    return unknownLayer.uncategorized.categoryTransform;
                }

                //Get category
                foreach (LevelTransformCategory transformCategory in transformLayer.levelTransformCategories)
                {
                    Debug.Log($"{transformCategory.categoryGuid} == {targetCategory}");
                    if (transformCategory.categoryGuid == targetCategory)
                    {
                        Debug.Log($"return {transformCategory.categoryTransform}");
                        return transformCategory.categoryTransform;
                    }
                }

                //Create category transform because not found
                Transform a = CreateLevelTransformCategory(categoryObject, transformLayer).categoryTransform;
                Debug.Log(a);
                return a;

            }

            return null;
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
