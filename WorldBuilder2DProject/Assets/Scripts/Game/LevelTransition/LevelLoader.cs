using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dr4g0nsoul.WorldBuilder2D.Game
{
    public class LevelLoader
    {

        private LevelManager manager;

        private bool loadingLevel;
        private LevelTransitionInfo lastLevelTransition;
        private bool inLevelTransition;
        private LevelController levelController;
        private LevelInstance currentLevel;
        public LevelInstance CurrentLevel 
        {
            get {
                if(currentLevel == null)
                {
                    currentLevel = FindCurrentLevelInstance();
                }
                return currentLevel;
            }
        }


        private LevelLoader() { }

        public LevelLoader(LevelManager manager)
        {
            this.manager = manager;
            levelController = LevelController.Instance;
            loadingLevel = false;
            inLevelTransition = false;

            currentLevel = FindCurrentLevelInstance();

            //Load start level
            if (manager.skipStartLevelLoading)
            {
                BeforeLevelLoadingDoneAction();
            }
            else
            {
                LoadLevel(manager.startLevel);
            }
        }

        public void LoadLevel(LevelTransitionInfo levelTransition)
        {
            LoadLevel(levelTransition.targetExit.targetLevel, true);
        }

        public void LoadLevelByName(string levelName)
        {
            if (loadingLevel) return;

            LevelNode[] levels = levelController.GetLevels();
            foreach (LevelNode level in levels)
            {
                if (level.levelName == levelName)
                {
                    LoadLevel(level.guid);
                    return;
                }
            }
        }

        public void LoadLevel(LevelInfo level, bool isLevelTransition = false)
        {
            LoadLevel(level.guid, isLevelTransition);
        }

        public void LoadLevel(LevelNode level)
        {
            if (level != null) LoadLevel(level.guid);
        }

        public void LoadLevel(string levelGuid, bool isLevelTransition = false)
        {
            if (!loadingLevel)
            {
                var levelNode = levelController.GetLevel(levelGuid);
                if (levelNode != null && !string.IsNullOrEmpty(levelNode.assignedScenePath))
                {
                    inLevelTransition = isLevelTransition;
                    loadingLevel = true;
                    manager.StartCoroutine(LevelLoadingRoutine(levelNode.assignedScenePath));
                }
            }

        }

        public void CheckLevelExits()
        {
            if (!loadingLevel && currentLevel != null && manager.playerCollider != null)
            {
                LevelExit[] exits = currentLevel.Level.levelExits;
                foreach (LevelExit exit in exits)
                {
                    if (exit.active && manager.playerCollider.bounds.Intersects(new Bounds(exit.levelExitTrigger.position, exit.levelExitTrigger.size)))
                    {
                        var exitPort = currentLevel.Level.GetPort(exit.guid);
                        if (exitPort != null && exitPort.ConnectionCount > 0) {
                            var targetPort = exitPort?.GetConnection(0);
                            if (targetPort != null && targetPort.node is LevelNode targetLevelNode)
                            {
                                foreach (LevelExit otherExit in targetLevelNode.levelExits)
                                {
                                    if (otherExit.guid == targetPort.fieldName)
                                    {
                                        lastLevelTransition = CreateTransitionInfo(exit, currentLevel.Level, otherExit, targetLevelNode);
                                        manager.onLevelExitTriggered.Invoke(lastLevelTransition);
                                        return;
                                    }
                                }
                                Debug.LogError($"Target level exit {targetPort.fieldName} not found in Level {exit.otherLevelGuid}");
                            }
                        }
                    }
                }
            }
        }

        IEnumerator LevelLoadingRoutine(string scenePath)
        {
            //Unload current level
            if (currentLevel != null)
            {
                var levelNode = currentLevel.Level;
                if (levelNode != null)
                {
                    Scene currScene = SceneManager.GetSceneByPath(levelNode.assignedScenePath);
                    if (currScene.IsValid())
                    {
                        //Start level unloading
                        yield return manager.StartCoroutine(AsyncLevelLoading(currScene.path, true));
                    }
                }
            }

            //Load next level
            yield return manager.StartCoroutine(AsyncLevelLoading(scenePath));

            if(inLevelTransition)
            {
                inLevelTransition = false;
                manager.onLevelEntered.Invoke(lastLevelTransition, lastLevelTransition.targetExit.entryPoint);
            }

            loadingLevel = false;
        }

        IEnumerator AsyncLevelLoading(string scenePath, bool unload = false)
        {
            AsyncOperation asyncOperation;
            asyncOperation = unload ? SceneManager.UnloadSceneAsync(scenePath) : SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            if (unload)
            {
                manager.onPreviousLevelUnloadStart.Invoke();
            }
            else
            {
                manager.onNewLevelLoadStart.Invoke();
                asyncOperation.allowSceneActivation = true;
            }

            while (!asyncOperation.isDone)
            {
                if (unload)
                {
                    manager.onPreviousLevelUnloading.Invoke(asyncOperation.progress);
                }
                else
                {
                    manager.onNewLevelLoading.Invoke(asyncOperation.progress);
                }
                yield return null;
            }


            if (unload)
            {
                manager.onPreviousLevelUnloadComplete.Invoke();
            }
            else
            {
                BeforeLevelLoadingDoneAction();
                manager.onNewLevelLoadComplete.Invoke();
            }
        }

        private void BeforeLevelLoadingDoneAction()
        {
            currentLevel = FindCurrentLevelInstance();
            if (currentLevel == null)
            {
                Debug.LogError("Current level has no level instance");
                return;
            }

            manager.ParallaxScrolling.SetupParallaxScrolling();
        }

        private LevelInstance FindCurrentLevelInstance()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene currScene = SceneManager.GetSceneAt(i);
                GameObject[] rootObjects = currScene.GetRootGameObjects();
                if (rootObjects.Length > 0)
                {
                    LevelInstance levelInstance = rootObjects[0].GetComponent<LevelInstance>();
                    if (levelInstance != null)
                    {
                        return levelInstance;
                    }
                }
            }
            return null;
        }

        private LevelTransitionInfo CreateTransitionInfo(LevelExit currentExit, LevelNode currentLevel, LevelExit targetExit, LevelNode targetLevel)
        {
            return new LevelTransitionInfo()
            {
                currentExit = CreateLevelExitInfo(currentExit, currentLevel),
                targetExit = CreateLevelExitInfo(targetExit, targetLevel)
            };
            
        }

        private LevelExitInfo CreateLevelExitInfo(LevelExit levelExit, LevelNode level)
        {
            return new LevelExitInfo()
            {
                guid = levelExit.guid,
                name = levelExit.name,
                active = levelExit.active,
                targetLevel = CreateLevelInfo(level),
                levelExitTrigger = levelExit.levelExitTrigger,
                entryPoint = levelExit.entryPoint
            };
        }

        private LevelInfo CreateLevelInfo(LevelNode levelNode)
        {
            return new LevelInfo()
            {
                guid = levelNode.guid,
                levelName = levelNode.levelName,
                levelDescription = levelNode.levelDescription,
                levelBoundary = levelNode.levelBoundaries
            };
        }

        

    }

    public struct LevelTransitionInfo
    {
        public LevelExitInfo currentExit;
        public LevelExitInfo targetExit;
    }

    public struct LevelExitInfo
    {
        //General
        public string guid;
        public string name;
        public bool active;

        //Connection Information
        public LevelInfo targetLevel;

        //Level Entry / Exit
        public Rect levelExitTrigger;
        public Vector2 entryPoint;
    }

    public struct LevelInfo
    {
        public string guid;
        public string levelName;
        public string levelDescription;

        public Rect levelBoundary;
    }

    
}
