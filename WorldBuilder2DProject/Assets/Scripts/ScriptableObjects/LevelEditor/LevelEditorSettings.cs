using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [CreateAssetMenu(fileName = "LevelEditorSettings", menuName = "Level Editor/Generate Settings Object")]
    public class LevelEditorSettings : ScriptableObject
    {

        [Header("General")]

        //Tag which specifies where the object root for all the level objects is located
        [Tag] public string levelEditorRootTag;

        //List of all the level object tags
        [HideInInspector] public LevelObjectCategory[] levelObjectCategories;

        //List of all the parallax layers
        [HideInInspector] public LevelLayer[] levelLayers;

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

        //The horizontal parallax scrolling speed of the layer
        public float parallaxSpeed;
    }
}