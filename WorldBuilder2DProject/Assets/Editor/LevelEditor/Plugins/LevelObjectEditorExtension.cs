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
        /// Called when level object is selected in the level editor tool
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        public virtual void OnLevelObjectSelected(LevelObject obj) { }

        /// <summary>
        /// Called when level object is de-selected or another Level Object is selected in the level editor tool
        /// </summary>
        /// <param name="obj">The previously selected level object</param>
        public virtual void OnLevelObjectDeSelected(LevelObject obj) { }

        /// <summary>
        /// Set to true if you want to turn the mouse into the currently selected GameObject in SceneView,
        /// false otherwise
        /// </summary>
        public virtual bool UseTemporaryIndicator => true;

        /// <summary>
        /// Called on each SceneView GUI call if the mouse is in a valid position inside the SceneView Window
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        /// <param name="temporaryObject">The gameobject used for the temporary indicator, null if UseTemporaryIndicator is false</param>
        /// <param name="worldPos">Position of the mouse in world coordinates</param>
        /// <param name="mousePos">Pixel position of the mouse in the current SceneView window</param>
        public virtual void HoverObject(LevelObject obj, GameObject temporaryObject, Vector2 worldPos, Vector2 mousePos) { }


        /// <summary>
        /// Set to true if you want to turn enable OnMouseDrag and disable deafault mouseDrag Behavior,
        /// false otherwise
        /// </summary>
        public virtual bool EnableDragging => false;

        /// <summary>
        /// Called on each SceneView GUI call if the mouse button 0 is pressed and the mouse moving
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        /// <param name="temporaryObject">The gameobject used for the temporary indicator, null if UseTemporaryIndicator is false</param>
        /// <param name="worldPos">Position of the mouse in world coordinates</param>
        /// <param name="mousePos">Pixel position of the mouse in the current SceneView window</param>
        public virtual void OnMouseDrag(LevelObject obj, GameObject temporaryObject, Vector2 worldPos, Vector2 mousePos) { }

        /// <summary>
        /// Called whenever the user wants to spawn this object
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        /// <param name="temporaryObject">The gameobject used for the temporary indicator, null if UseTemporaryIndicator is false</param>
        /// <param name="parentTransform">
        ///     The transform of the Level Object container.
        ///     Set the parent transform of the Instantiated object to this transform.
        /// </param>
        /// <param name="worldPos">Position of the mouse in world coordinates</param>
        /// <param name="mousePos">Pixel position of the mouse in the current SceneView window</param>
        /// <returns>Spawned GameObject</returns>
        public virtual GameObject SpawnObject(LevelObject obj, GameObject temporaryObject, Transform parentTransform, Vector2 worldPos, Vector2 mousePos)
        {
            GameObject newObject = PrefabUtility.InstantiatePrefab(obj.objectPrefab) as GameObject;
            if (newObject != null)
            {
                newObject.transform.position = worldPos;
                newObject.transform.rotation = Quaternion.identity;
                newObject.transform.parent = parentTransform;

                Undo.RegisterCreatedObjectUndo(newObject, $"LevelEditor: Placed {obj.item.name}");//Undo function
                Selection.activeObject = newObject;
                return newObject;
            }
            return null;
        }

        /// <summary>
        /// Called when sorting layer options are being applied, which is whenever a temporaryObject or the level object itself is spawned
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        /// <param name="spawningObject">The game object being spawned</param>
        /// <param name="sortingLayerName">The target sorting layer</param>
        public virtual void OnApplySortingLayers(LevelObject obj, GameObject spawningObject, string sortingLayerName)
        {
            ApplyToChildrenRecursively(spawningObject.transform, (child) =>
            {
                SpriteRenderer[] spriteRenderers = child.GetComponents<SpriteRenderer>();
                foreach(SpriteRenderer sr in spriteRenderers)
                {
                    sr.sortingLayerName = sortingLayerName;
                }
            });
        }

        /// <summary>
        /// Called when physics layer options are applied, which is whenever a temporaryObject or the level object itself is spawned
        /// </summary>
        /// <param name="obj">The currently selected level object</param>
        /// <param name="spawningObject">The game object being spawned</param>
        /// <param name="targetLayer">The target physics layer</param>
        /// <param name="onlyRootObject">If only the root object should be affected</param>
        /// <param name="layersNotToOverride">The layers which should not be overridden</param>
        /// <param name="removePhysicsComponents">Whether or not to remove physics related components</param>
        public virtual void OnApplyPhysicsLayers(LevelObject obj, GameObject spawningObject, int targetLayer, bool onlyRootObject, LayerMask layersNotToOverride, bool removePhysicsComponents)
        {
            if (onlyRootObject)
            {
                //Can gameobject layer be overwritten?
                if((layersNotToOverride & 1 << spawningObject.layer) == 0)
                {
                    spawningObject.layer = targetLayer;
                    if(removePhysicsComponents)
                    {
                        Collider2D[] colliders = spawningObject.GetComponents<Collider2D>();
                        for(int i = colliders.Length - 1; i >= 0; i--)
                        {
                            GameObject.DestroyImmediate(colliders[i]);
                        }
                        Rigidbody2D[] rigidbodies = spawningObject.GetComponents<Rigidbody2D>();
                        for (int i = rigidbodies.Length - 1; i >= 0; i--)
                        {
                            GameObject.DestroyImmediate(rigidbodies[i]);
                        }
                    }
                }
            }
            else
            {
                ApplyToChildrenRecursively(spawningObject.transform, (child) =>
                {
                    //Can gameobject layer be overwritten?
                    if ((layersNotToOverride & 1 << child.gameObject.layer) == 0)
                    {
                        child.gameObject.layer = targetLayer;
                        if (removePhysicsComponents)
                        {
                            Collider2D[] colliders = child.gameObject.GetComponents<Collider2D>();
                            for (int i = colliders.Length - 1; i >= 0; i--)
                            {
                                GameObject.DestroyImmediate(colliders[i]);
                            }
                            Rigidbody2D[] rigidbodies = child.gameObject.GetComponents<Rigidbody2D>();
                            for (int i = rigidbodies.Length - 1; i >= 0; i--)
                            {
                                GameObject.DestroyImmediate(rigidbodies[i]);
                            }
                        }
                    }
                });
            }
        }

        // Level Inspector Methods //

        /// <summary>
        /// Override to change wheter or not to display a level editor tool inpector window on Level Object selection
        /// </summary>
        /// <returns>Wheter or not a level editor tool inpector window is shown on Level Object selection</returns>
        public virtual bool UseLevelEditorToolInspector() => true;

        /// <summary>
        /// Override this method to draw an Inspector Window when this level object is selected
        /// </summary>
        public virtual void OnLevelEditorToolInspectorGUI(LevelObject obj) 
        {
            GUILayout.Space(15f);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Open Prefab"))
            {
                LevelEditorTool.SetBeforeGUIAction(() =>
                {
                    UnityEditor.EditorUtility.FocusProjectWindow();
                    ProjectWindowUtil.ShowCreatedAsset(obj.objectPrefab);
                    AssetDatabase.OpenAsset(obj.objectPrefab);
                }, EventType.Repaint);
            }
            if(GUILayout.Button("Open Level Object"))
            {
                LevelEditorTool.SetBeforeGUIAction(() =>
                {
                    UnityEditor.EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(obj);
                }, EventType.Repaint);
            }
            GUILayout.EndHorizontal();
        }

        // Custom Inspector Tab //

        /// <summary>
        /// Set name of the additional custom inspector tab of the Level Object
        /// </summary>
        /// <returns>The name of the tab or null/empty string if no additional tab is implemented or should be shown</returns>
        public virtual string CustomInspectorTabName() => null;

        /// <summary>
        /// Custom Inspector On Enable GUI
        /// </summary>
        /// <param name="levelObjectEditor">The level object editor that calls this function</param>
        /// <param name="serializedObject">Serialized object representing the level object</param>
        public virtual void OnCustomInspectorEnable(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject) { }

        /// <summary>
        /// Custom Inspector On Disable GUI
        /// </summary>
        /// <param name="levelObjectEditor">The level object editor that calls this function</param>
        /// <param name="serializedObject">Serialized object representing the level object</param>
        public virtual void OnCustomInspectorDisable(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject) { }

        /// <summary>
        /// Custom Inspector Tab GUI
        /// </summary>
        /// <param name="levelObjectEditor">The level object editor that calls this function</param>
        /// <param name="serializedObject">Serialized object representing the level object</param>
        public virtual void OnCustomInspectorTabGUI(LevelObjectEditor levelObjectEditor, SerializedObject serializedObject) { }

        /// <summary>
        /// Called when the scene view loses focus
        /// while the level editor tool is active and this Level Object is currently selected
        /// </summary>
        public virtual void OnSceneWindowLostFocus() { }


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


        #region Helper functions

        public void ApplyToChildrenRecursively(Transform parent, Action<Transform> action)
        {
            if(action != null)
            {
                if(parent.childCount > 0)
                {
                    foreach(Transform child in parent)
                    {
                        ApplyToChildrenRecursively(child, action);
                    }
                }
                action.Invoke(parent);
            }
        }

        #endregion
    }
}
