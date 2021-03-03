using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    public class LevelObject : UniqueScriptableObject
    {

        //Prefab that will be spawned
        [ReadOnly] public GameObject objectPrefab;

        //Item display properties
        public LevelEditorItem item;

        //Main category (Color displayed in the level editor)
        public string mainCategory = null;
        //Item category (e.g. Hazards, Semisolids, Enemies, Bosses, ...)
        public string[] categories = null;
        //In which layers this item is available
        public string[] levelLayers = null;



        ///////////////////////////////////////
        // Extendable Properties and Methods //
        ///////////////////////////////////////
        
        /// <summary>
        /// Set to true if you want to turn the mouse into the currently selected GameObject in SceneView,
        /// false otherwise
        /// </summary>
        public virtual bool UseTemporaryIndicator => true;

        /// <summary>
        /// Called on each SceneView GUI call if the mouse is in a valid position inside the SceneView Window
        /// </summary>
        /// <param name="temporaryObject">The gameobject used for the temporary indicator, null if UseTemporaryIndicator is false</param>
        /// <param name="worldPos">Position of the mouse in world coordinates</param>
        /// <param name="mousePos">Pixel position of the mouse in the current SceneView window</param>
        public virtual void HoverObject(GameObject temporaryObject, Vector2 worldPos, Vector2 mousePos)
        {
            Debug.Log($"Hovering {item.name}");
        }

        /// <summary>
        /// Called whenever the user wants to spawn this object
        /// </summary>
        /// <param name="temporaryObject">The gameobject used for the temporary indicator, null if UseTemporaryIndicator is false</param>
        /// <param name="parentTransform">
        ///     The transform of the Level Object container.
        ///     Set the parent transform of the Instantiated object to this transform.
        /// </param>
        /// <param name="worldPos">Position of the mouse in world coordinates</param>
        /// <param name="mousePos">Pixel position of the mouse in the current SceneView window</param>
        public virtual void SpawnObject(GameObject temporaryObject, Transform parentTransform, Vector2 worldPos, Vector2 mousePos)
        {
            Debug.Log($"Spawn {item.name} at {worldPos} with parent {parentTransform}");
            GameObject newObject = PrefabUtility.InstantiatePrefab(objectPrefab) as GameObject;
            if (newObject != null)
            {
                newObject.transform.position = worldPos;
                newObject.transform.rotation = Quaternion.identity;
                newObject.transform.parent = parentTransform;

                Undo.RegisterCreatedObjectUndo(newObject, $"LevelEditor: Placed {item.name}");//Undo function
                Selection.activeObject = newObject;
            }
        }
    }

}
