﻿using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "Level Editor/Generate Settings Object")]
    public class LevelEditorSettings : ScriptableObject
    {

        public const int SORTING_ORDER_MIN_LIMIT = -10000;
        public const int SORTING_ORDER_MAX_LIMIT = 10000;

        [Header("General")]

        //Tag which specifies where the object root for all the level objects is located
        [Tag] public string levelEditorRootTag;

        //List of all the level object tags
        [HideInInspector] public LevelObjectCategory[] levelObjectCategories;

        //List of all the parallax layers
        [HideInInspector] public LevelLayer[] levelLayers;

        [Header("Other Settings")]
        //Size of the quick select bar
        public int quickSelectBarSize = 20;

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
        //Accent Color that is displayed on hover
        [ColorUsage(false)] public Color accentColor = Color.white;
    }

    [System.Serializable]
    public class LevelObjectCategory
    {
        //The current category id
        [ReadOnly] public string guid = Guid.Empty.ToString();

        //The representation of an element in the level editor
        public LevelEditorItem item;
    }

    [System.Serializable]
    public class LevelLayer
    {
        //The current layer id
        [ReadOnly] public string guid = Guid.Empty.ToString();

        //The representation of an element in the level editor
        public LevelEditorItem item;

        //The sorting layer for every game object placed on this layer
        public bool overrideSortingLayer;
        [SortingLayer] public string sortingLayer;

        /*
        //Base sorting order is increased by this offset for each level object spawned on this layer
        public int sortingOrderOffset;
        //Current base sorting order offset
        public int currBaseSortingOrder;
        */

        //The physics layer for every game object placed on this layer
        public bool overridePhysicsLayer;
        [PhysicsLayer] public int physicsLayer;

        //If only the root object is affected
        public bool onlyRootObject;

        //Which layers should not be overridden
        public LayerMask layersToNotOverride;

        //The horizontal parallax scrolling speed of the layer
        [Range(0f, 2f)] public float parallaxSpeed = 1f;
    }
}