using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.ParallaxBackgroundPlugin
{

    public class ParallaxBackground : MonoBehaviour
    {
        public ParallaxBackgroundLevelObject parallaxBackgroundSettings;
        public Transform backgroundContainer;
        private ParallaxBackgroundLayerContainer[] backgrounds;

        private Transform cameraTransform;
        private Vector2 previousCameraPosition;


        // Start is called before the first frame update
        void Start()
        {
            
            SetupCamera();
        }



        private void SetupCamera()
        {
            if (backgroundContainer != null)
            {
                int count = backgroundContainer.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    DestroyImmediate(backgroundContainer.GetChild(i).gameObject);
#else
                    Destroy(backgroundContainer.GetChild(i).gameObject);
#endif
                    backgrounds = null;
                }
            }

            Camera cam = Camera.main ?? Camera.current;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView sceneView = UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>();
                cam = sceneView.camera;
            }
#endif


            if (cam != null && parallaxBackgroundSettings.parallaxBackgroundLayers != null)
            {
                cameraTransform = cam.transform;
                previousCameraPosition = Vector2.zero;
                //previousCameraPosition = cameraTransform.position;
                
                int backgroundsCount = parallaxBackgroundSettings.parallaxBackgroundLayers.Length;
                if (backgroundsCount < 1)
                {
                    Debug.LogWarning($"No Backgrounds set for {name}");
                    Destroy(gameObject);
                }
                backgrounds = new ParallaxBackgroundLayerContainer[backgroundsCount];

                for (int i = 0; i < backgroundsCount; i++)
                {
                    GameObject background = new GameObject();
                    background.transform.rotation = Quaternion.identity;
                    background.transform.parent = backgroundContainer;
                    background.transform.localScale = Vector3.one;
                    background.transform.localPosition = new Vector3(0f, parallaxBackgroundSettings.parallaxBackgroundLayers[i].yOffset, 0f);
                    background.name = $"Background {i + 1}";
                    SpriteRenderer sr = background.AddComponent<SpriteRenderer>();
                    sr.sprite = parallaxBackgroundSettings.parallaxBackgroundLayers[i].image;
                    sr.sortingOrder = i;
                    backgrounds[i] = new ParallaxBackgroundLayerContainer(background.transform);
                }
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            
            Vector2 distance = (Vector2)cameraTransform.position - previousCameraPosition;

            for (int i = 0; i < backgrounds.Length; i++)
            {
                if(backgrounds[i] != null)
                backgrounds[i].Move(Vector2.Scale(distance, new Vector2(parallaxBackgroundSettings.parallaxBackgroundLayers[i].parallaxSpeed, parallaxBackgroundSettings.parallaxBackgroundLayers[i].yOffset)).x);
            }

            previousCameraPosition = cameraTransform.position;

            transform.position = (Vector2)cameraTransform.position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (cameraTransform == null || backgrounds == null)
                {
                    SetupCamera();
                }
                if (cameraTransform == null || backgrounds == null) return;

                Update();
            }
        }
#endif

        public class ParallaxBackgroundLayerContainer
        {
            private Transform backgroundTransform;
            private float sizeX;
            private float lastPosition;
            

            public ParallaxBackgroundLayerContainer(Transform backgroundTransform)
            {
                this.backgroundTransform = backgroundTransform;
                SpriteRenderer sr = backgroundTransform.GetComponent<SpriteRenderer>();
                sizeX = sr.sprite.bounds.size.x;

                //Create left side
                GameObject leftSide = new GameObject("Left");
                leftSide.transform.parent = backgroundTransform;
                leftSide.transform.localPosition = Vector3.left * sizeX;
                leftSide.transform.localScale = Vector3.one;
                SpriteRenderer leftSr = leftSide.AddComponent<SpriteRenderer>();
                leftSr.sprite = sr.sprite;
                leftSr.sortingLayerName = sr.sortingLayerName;
                leftSr.sortingOrder = sr.sortingOrder;

                //Create right side
                GameObject rightSide = new GameObject("Right");
                rightSide.transform.parent = backgroundTransform;
                rightSide.transform.localPosition = Vector3.right * sizeX;
                rightSide.transform.localScale = Vector3.one;
                SpriteRenderer rightSr = rightSide.AddComponent<SpriteRenderer>();
                rightSr.sprite = sr.sprite;
                rightSr.sortingLayerName = sr.sortingLayerName;
                rightSr.sortingOrder = sr.sortingOrder;

                lastPosition = 0f;
            }

            public void Move(float distance)
            {
                backgroundTransform.localPosition = new Vector3(Modulo(lastPosition - distance, sizeX), backgroundTransform.localPosition.y, 0f);
                lastPosition = backgroundTransform.localPosition.x;
            }

            private float Modulo(float a, float b)
            {
                return a - b * Mathf.Floor(a / b);
            }
        }
    }
}
