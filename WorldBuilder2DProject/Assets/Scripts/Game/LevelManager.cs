using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Game
{

    public class LevelManager : MonoBehaviour
    {

        [Header("Important References")]
        public Camera mainCamera;
        public Collider2D playerCollider;

        [Header("Level Management")]
        public string startLevel;
        public Event onPreviousLevelUnloadStart;
        public Event onPreviousLevelUnloading;
        public Event onPreviousLevelUnloadComplete;
        public Event onNewLevelLoadStart;
        public Event onNewLevelLoading;
        public Event onNewLevelLoadComplete;

        private LevelController levelController;
        private LevelInstance currentLevel;

        // Start is called before the first frame update
        void Start()
        {
            levelController = LevelController.Instance;
            LoadLevel(startLevel);
        }

        // Update is called once per frame
        void FixedUpdate()
        {

        }

        #region Level Management

        public void LoadLevel(string levelGuid)
        {

        }

        #endregion

    }
}
