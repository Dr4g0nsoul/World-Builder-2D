using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Game
{
    public class ParallaxScrolling
    {

        private LevelManager manager;
        private LevelEditorSettings levelEditorSettings;

        private ParallaxScrolling() { }

        public ParallaxScrolling(LevelManager manager)
        {
            this.manager = manager;
            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            if (levelEditorSettings == null)
            {
                GameObject.Destroy(manager);
                Debug.LogError("Level Editor Settings not found!");
            }
        }


        public void SetupParallaxScrolling()
        {
            LevelInstance currentLevel = manager.LevelLoader.CurrentLevel;
            if (currentLevel != null)
            {
                LevelTransformLayer[] layerTransforms = currentLevel.GetLevelLayers();
                foreach (LevelTransformLayer layerTransform in layerTransforms)
                {
                    //Get parallax layer speed
                    foreach (LevelLayer layer in levelEditorSettings.levelLayers)
                    {
                        if (layer.parallaxSpeed != 1f && layer.guid == layerTransform.layerGuid)
                        {
                            ParallaxLayer parallaxLayer = layerTransform.layerTransform.gameObject.AddComponent<ParallaxLayer>();
                            parallaxLayer.Setup(manager.mainCamera, layer.parallaxSpeed);
                        }
                    }
                }
            }
        }

    }
}
