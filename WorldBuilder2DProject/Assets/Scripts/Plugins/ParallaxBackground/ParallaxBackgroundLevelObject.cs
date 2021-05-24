using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.ParallaxBackgroundPlugin
{
    public class ParallaxBackgroundLevelObject : LevelObject
    {
        public ParallaxBackgroundLayer[] parallaxBackgroundLayers;
    }

    [System.Serializable]
    public struct ParallaxBackgroundLayer
    {
        public string name;
        public Sprite image;
        [Range(-2f, 2f)] public float parallaxSpeed;
        public float yOffset;
    }
}
