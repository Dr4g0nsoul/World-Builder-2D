using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace dr4g0nsoul.WorldBuilder2D.Game
{

    public class LevelManager : MonoBehaviour
    {

        [Header("Important References")]
        public Camera mainCamera;
        public Collider2D playerCollider;

        [Header("Level Management")]
        public string startLevel;
        public bool skipLevelLoading;
        public UnityEvent onPreviousLevelUnloadStart;
        public UnityEvent<float> onPreviousLevelUnloading;
        public UnityEvent onPreviousLevelUnloadComplete;
        public UnityEvent onNewLevelLoadStart;
        public UnityEvent<float> onNewLevelLoading;
        public UnityEvent<LevelExit> onNewLevelLoadComplete;
        private bool loadingLevel;
        private Coroutine loadingLevelRoutine;
        private LevelExit lastLevelEntry;

        private LevelEditorSettings levelEditorSettings;
        private LevelController levelController;
        private LevelInstance currentLevel;

        // Start is called before the first frame update
        void Start()
        {
            //Check if all references are set
            if (mainCamera == null) { Debug.LogError("No main camera set!"); Destroy(gameObject); }
            if (playerCollider == null) { Debug.LogError("No player collider set!"); Destroy(gameObject); }

            //Initialize variables
            loadingLevel = false;

            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            if(levelEditorSettings == null)
            {
                Debug.LogError("No level editor settings found!");
                Destroy(gameObject);
            }
            levelController = LevelController.Instance;
            currentLevel = FindCurrentLevelInstance();

            //Load start level
            if (skipLevelLoading)
            {
                BeforeLevelLoadingDoneAction();
            }
            else
            {
                LoadLevel(startLevel);
            }
        }

        

        #region Level Management

        public void LoadLevel(string levelGuid)
        {
            if (!loadingLevel)
            {
                var levelNode = levelController.GetLevel(levelGuid);
                if (levelNode != null)
                {
                    Scene level = SceneManager.GetSceneByPath(levelNode.assignedScenePath);
                    loadingLevel = true;
                    loadingLevelRoutine = StartCoroutine(LevelLoadingRoutine(level));
                }
            }

        }

        IEnumerator LevelLoadingRoutine(Scene newScene)
        {
            //Unload current level
            if(currentLevel != null)
            {
                var levelNode = levelController.GetLevel(currentLevel.level);
                if(levelNode != null)
                {
                    Scene currScene = SceneManager.GetSceneByPath(levelNode.assignedScenePath);
                    //Start level unloading
                    yield return StartCoroutine(AsyncLevelLoading(currScene, true));
                }
            }
            
            //Load next level
            yield return StartCoroutine(AsyncLevelLoading(newScene));

            loadingLevel = false;
        }

        IEnumerator AsyncLevelLoading(Scene scene, bool unload = false)
        {
            AsyncOperation asyncOperation;
            asyncOperation = unload ? SceneManager.UnloadSceneAsync(scene.path) : SceneManager.LoadSceneAsync(scene.path);

            if (unload)
            {
                onPreviousLevelUnloadStart.Invoke();
            }
            else
            {
                onNewLevelLoadStart.Invoke();
                asyncOperation.allowSceneActivation = true;
            }

            while (!asyncOperation.isDone)
            {
                if(unload)
                {
                    onPreviousLevelUnloading.Invoke(asyncOperation.progress);
                }
                else
                {
                    onNewLevelLoading.Invoke(asyncOperation.progress);
                }
                yield return null;
            }

            BeforeLevelLoadingDoneAction();

            if (unload)
            {
                onPreviousLevelUnloadComplete.Invoke();
            }
            else
            {
                onNewLevelLoadComplete.Invoke(lastLevelEntry);
            }
        }

        private void BeforeLevelLoadingDoneAction()
        {
            currentLevel = FindCurrentLevelInstance();
            if(currentLevel == null)
            {
                Debug.LogError("Current level has no level instance");
                return;
            }

            SetupParallaxScrolling();
        }

        private LevelInstance FindCurrentLevelInstance()
        {
            for(int i = 0; i<SceneManager.sceneCount; i++)
            {
                Scene currScene = SceneManager.GetSceneAt(i);
                GameObject[] rootObjects = currScene.GetRootGameObjects();
                if(rootObjects.Length > 0)
                {
                    LevelInstance levelInstance = rootObjects[0].GetComponent<LevelInstance>();
                    if(levelInstance != null)
                    {
                        return levelInstance;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Parallax Scrolling

        private void SetupParallaxScrolling()
        {
            if(currentLevel != null)
            {
                LevelTransformLayer[] layerTransforms = currentLevel.GetLevelLayers();
                foreach(LevelTransformLayer layerTransform in layerTransforms)
                {
                    //Get parallax layer speed
                    foreach(LevelLayer layer in levelEditorSettings.levelLayers)
                    {
                        if(layer.parallaxSpeed != 1f && layer.guid == layerTransform.layerGuid)
                        {
                            ParallaxLayer parallaxLayer = layerTransform.layerTransform.gameObject.AddComponent<ParallaxLayer>();
                            parallaxLayer.Setup(mainCamera, layer.parallaxSpeed);
                        }
                    }
                }
            }
        }

        #endregion


        #region Physics related functionality

        // Update is called once per frame
        void FixedUpdate()
        {
            CheckLevelExits();
        }

        #region Level Transitions

        private void CheckLevelExits()
        {
            if(!loadingLevel && currentLevel != null)
            {
                LevelExit[] exits = currentLevel.Level.levelExits;
                foreach (LevelExit exit in exits)
                {
                    if (exit.active && playerCollider.bounds.Intersects(new Bounds(exit.levelExitTrigger.position, exit.levelExitTrigger.size)))
                    {
                        var exitPort = currentLevel.Level.GetPort(exit.guid);
                        var targetPort = exitPort?.GetConnection(0);
                        if(targetPort != null && targetPort.node is LevelNode targetLevelNode) 
                        {
                            foreach(LevelExit otherExit in targetLevelNode.levelExits)
                            {
                                if(otherExit.guid == targetPort.fieldName)
                                {
                                    lastLevelEntry = otherExit;
                                    LoadLevel(exit.otherLevelGuid);
                                    return;
                                }
                            }
                            Debug.LogError($"Target level exit {targetPort.fieldName} not found in Level {exit.otherLevelGuid}");
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}
