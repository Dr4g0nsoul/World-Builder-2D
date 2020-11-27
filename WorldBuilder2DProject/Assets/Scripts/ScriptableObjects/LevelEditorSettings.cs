using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "Level Editor/Generate Settings Object")]
    public class LevelEditorSettings : ScriptableObject
    {

        [HideInInspector] public int nextCategoryId = 0;
        [HideInInspector] public int nextLayerId = 0;
        [HideInInspector] public int[] deletedLayerIds;

        [Header("General")]
        //Level prefab that gets instantiated
        public GameObject levelPrefab;

        //Tag which specifies where the object root for all the level objects is located
        [Tag] public string levelObjectsRootTag;

        //List of all the level item categories
        [HideInInspector] public LevelEditorCategory[] levelEditorCategories;

        //List of all the parallax layers
        [HideInInspector] public ParallaxLayer[] levelEditorLayers;

    }

    [System.Serializable]
    public class LevelEditorItem
    {
        //Name of the item
        public string name;
        //Thumbnail which is displayed in the level editor
        public Texture2D thumbnail;
        //Description that is displayed when hovering over the item
        [TextArea] public string description;
        //Optional prefab which is instantiated instead of an empty one
        public GameObject optionalPrefab;
    }

    [System.Serializable]
    public class LevelEditorCategory
    {
        //The current category id
        [ReadOnly] public int id = 0;

        //The last sub-category id assigned
        [ReadOnly] public int nextSubId = 0;

        //The representation of an element in the level editor
        public LevelEditorItem item;

        //List of all the subcategories
        public LevelEditorSubCategory[] subCategories;
    }

    [System.Serializable]
    public class LevelEditorSubCategory
    {
        //The current sub-category id
        [ReadOnly] public int id = 0;

        //The representation of an element in the level editor
        public LevelEditorItem item;
    }

    [System.Serializable]
    public class ParallaxLayer
    {
        //The current layer id
        [ReadOnly] public int id = 0;

        //The representation of an element in the level editor
        public LevelEditorItem item;

        //The horizontal parallax scrolling speed of the layer
        public float parallaxSpeed;
    }
}