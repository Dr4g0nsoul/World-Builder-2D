using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dr4g0nsoul.WorldBuilder2D.Game
{

    public class LevelManager : MonoBehaviour
    {

        [Header("Important References")]
        public Camera mainCamera;
        public Collider2D playerCollider;

        [Header("Level Management")]
        [LevelPicker] public string startLevel;
        public bool skipStartLevelLoading;
        [Tooltip("This event is called whenever the player enters an active and connected level exit. Dynamic Parameters: Level Transition")]
        public UnityEvent<LevelTransitionInfo> onLevelExitTriggered;
        [Tooltip("This event is called when the next level after a level transition has finished loading. Dynamic Parameters: Level Transition and Entry Point")]
        public UnityEvent<LevelTransitionInfo, Vector2> onLevelEntered;
        public UnityEvent onPreviousLevelUnloadStart;
        public UnityEvent<float> onPreviousLevelUnloading;
        public UnityEvent onPreviousLevelUnloadComplete;
        public UnityEvent onNewLevelLoadStart;
        public UnityEvent<float> onNewLevelLoading;
        public UnityEvent onNewLevelLoadComplete;

        //Parallax Scrolling
        public ParallaxScrolling ParallaxScrolling { get => parallaxScrolling; }
        private ParallaxScrolling parallaxScrolling;

        public LevelLoader LevelLoader { get => levelLoader; }
        private LevelLoader levelLoader;

        private LevelEditorSettings levelEditorSettings;
        
        // Start is called before the first frame update
        void Start()
        {
            //Check if all references are set
            if (mainCamera == null) { Debug.LogError("No main camera set!"); Destroy(gameObject); }
            if (playerCollider == null) { Debug.LogError("No player collider set!"); Destroy(gameObject); }

            //Initialize variables
            levelEditorSettings = LevelEditorSettingsController.Instance.GetLevelEditorSettings();
            if (levelEditorSettings == null)
            {
                Debug.LogError("No level editor settings found!");
                Destroy(gameObject);
            }

            //Initialize modules
            levelLoader = new LevelLoader(this);
            parallaxScrolling = new ParallaxScrolling(this);
            
        }

        #region Physics related functionality

        // Update is called once per frame
        void FixedUpdate()
        {
            levelLoader.CheckLevelExits();
        }

        #region Level Transitions

        

        #endregion

        #endregion

    }
}
