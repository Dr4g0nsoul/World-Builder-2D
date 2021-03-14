using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Reflection;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{
    [CreateLevelObjectEditorExtension(typeof(LevelObject))]
    public class LevelObjectEditorExtension
    {

        public LevelObject Target
        {
            get
            {
                if(_target == null)
                {
                    Debug.LogError($"Target is empty for {GetType().Name}");
                }
                return _target;
            }
            set
            {
                if (value != null && value is LevelObject)
                {
                    _target = value;
                }
                else
                {
                    Debug.LogError($"Illegal target {(value == null ? "null" : value.GetType().Name)} for {GetType().Name}");
                }
            }
        }
        private LevelObject _target;

        ///////////////////////////////////////
        // Extendable Properties and Methods //
        ///////////////////////////////////////

        // Level Object creation Methods //

        /// <summary>
        /// Called when the Level Object scriptable object is initially created to initialize the Prefab
        /// </summary>
        /// <param name="currentPrefab">The current prefab</param>
        /// <param name="prefabPath">The current prefab folder location</param>
        /// <returns>The altered Prefab or null if no alteration to the Prefab needs to be made</returns>
        public virtual GameObject OnLevelObjectCreate(GameObject currentPrefab, string prefabPath) => null;

        // Object Placement Methods //

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
            Debug.Log($"Hovering {Target.item.name}");
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
            Debug.Log($"Spawn {Target.item.name} at {worldPos} with parent {parentTransform}");
            GameObject newObject = PrefabUtility.InstantiatePrefab(Target.objectPrefab) as GameObject;
            if (newObject != null)
            {
                newObject.transform.position = worldPos;
                newObject.transform.rotation = Quaternion.identity;
                newObject.transform.parent = parentTransform;

                Undo.RegisterCreatedObjectUndo(newObject, $"LevelEditor: Placed {Target.item.name}");//Undo function
                Selection.activeObject = newObject;
            }
        }

        // Level Inspector Methods //

        /// <summary>
        /// Override to change wheter or not to display a level editor tool inpector window on Level Object selection
        /// </summary>
        /// <returns>Wheter or not a level editor tool inpector window is shown on Level Object selection</returns>
        public virtual bool UseLevelEditorToolInspector() => false;

        /// <summary>
        /// Override this method to draw an Inspector Window when this level object is selected
        /// </summary>
        public virtual void OnLevelEditorToolInspectorGUI() { }

        // Custom Inspector Tab //

        /// <summary>
        /// Set name of the additional custom inspector tab of the Level Object
        /// </summary>
        /// <returns>The name of the tab or null/empty string if no additional tab is implemented or should be shown</returns>
        public virtual string CustomInspectorTabName() => null;

        /// <summary>
        /// Custom Inspector On Enable GUI
        /// </summary>
        public virtual void OnCustomInspectorEnable() { }

        /// <summary>
        /// Custom Inspector Tab GUI
        /// </summary>
        /// <param name="levelObjectEditor">The level object editor that calls this function</param>
        /// <param name="serializedObject">Serialized object representing the level object</param>
        public virtual void OnCustomInspectorTabGUI(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject) { }


        ///////////////////////////////////////
        //           Static Methods          //
        ///////////////////////////////////////

        /// <summary>
        /// Return an instance of the custom level object editor extension
        /// </summary>
        /// <typeparam name="E">A valid LevelObjectEditorExtension</typeparam>
        /// <param name="levelObject">The corresponding level object</param>
        /// <returns>A new instance of the custom level object editor extension</returns>
        public static E GetLevelObjectEditorExtension<E>(LevelObject levelObject) where E : LevelObjectEditorExtension, new()
        {
            if (levelObject != null && levelObject is LevelObject)
            {
                Type[] extensions = LevelEditorReflection.GetDerivedTypes(typeof(LevelObjectEditorExtension));
                foreach (Type extension in extensions)
                {
                    CreateLevelObjectEditorExtensionAttribute createExtensionAttribute = extension.GetCustomAttribute<CreateLevelObjectEditorExtensionAttribute>();
                    if (createExtensionAttribute != null && createExtensionAttribute.levelObjectType == levelObject.GetType())
                    {
                        E extensionInstance = new E();
                        extensionInstance.Target = levelObject;
                        return extensionInstance;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Return an instance of the custom level object editor extension
        /// </summary>
        /// <typeparam name="E">A valid LevelObjectEditorExtension</typeparam>
        /// <param name="levelObject">The corresponding level object</param>
        /// <returns>A new instance of the custom level object editor extension</returns>
        public static LevelObjectEditorExtension GetBaseLevelObjectEditorExtension(LevelObject levelObject)
        {
            if (levelObject != null && levelObject is LevelObject)
            {
                Type[] extensions = LevelEditorReflection.GetDerivedTypes(typeof(LevelObjectEditorExtension));
                foreach (Type extension in extensions)
                {
                    CreateLevelObjectEditorExtensionAttribute createExtensionAttribute = extension.GetCustomAttribute<CreateLevelObjectEditorExtensionAttribute>();
                    if (createExtensionAttribute != null && createExtensionAttribute.levelObjectType == levelObject.GetType())
                    {
                        if (Activator.CreateInstance(extension) is LevelObjectEditorExtension extensionInstance)
                        {
                            extensionInstance.Target = levelObject;
                            return extensionInstance;
                        }
                    }
                }
            }
            return null;
        }
    }
}
