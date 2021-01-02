using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Util
{

    public class EditorUtility
    {

        #region Files

        /// <summary>
        /// Creates an asset in a folder and creating also the folder specified in its path
        /// </summary>
        /// <param name="absolutePath">Folder where the asset is located</param>
        /// <param name="assetName">Name of the asset</param>
        /// <param name="asset">The asset that has to be created</param>
        /// <returns></returns>
        public static bool CreateAssetAndFolders(string folder, string assetName, UnityEngine.Object asset)
        {
            assetName = assetName.TrimStart('/');
            if(string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(assetName) || asset == null)
            {
                Debug.LogError("EditorUtility::CreateAssetAndFolders: Folder, assetName or asset are null or empty");
                return false;
            }
            if (folder.StartsWith("Assets/"))
            {
                if (!folder.EndsWith("/"))
                    folder += "/";

                //Create folders
                int currIndex = 0;
                string currParent = "";
                string currFolderToCreate;
                bool isFolder = true;
                while(isFolder)
                {
                    currIndex = folder.IndexOf('/', currIndex)+1;
                    currParent = folder.Substring(0, currIndex);
                    if (folder.IndexOf('/', currIndex) > 0)
                    {
                        currFolderToCreate = folder.Substring(currIndex, folder.IndexOf('/', currIndex) - currIndex);
                        if (!AssetDatabase.IsValidFolder(currParent + currFolderToCreate))
                        {
                            if (AssetDatabase.CreateFolder(currParent.TrimEnd('/'), currFolderToCreate) == null)
                            {
                                Debug.LogError($"EditorUtility::CreateAssetAndFolders: Folder \"{currParent + currFolderToCreate}\" can not be created");
                                return false;
                            }
                            Debug.Log($"Created Folder: {currFolderToCreate} with parent {currParent}");
                        }
                    }
                    else
                    {
                        isFolder = false;
                    }
                }

                //Create asset
                try
                {
                    AssetDatabase.CreateAsset(asset, currParent + assetName + ".asset");
                }
                catch(UnityException ex)
                {
                    Debug.LogError($"EditorUtility::CreateAssetAndFolders: {ex.Message}");
                    return false;
                }
                return true;
            }
            else
            {
                Debug.LogError($"EditorUtility::CreateAssetAndFolders: Foldername of {assetName}.asset has to start with \"Assets/\"");
            }
            return false;
        }

        #endregion

        #region Level Editor

        public static Vector2 SceneViewToWorldPos(SceneView view)
        {
            if (view != null)
            {
                Camera cam = view.camera;
                Vector2 mousepos = Event.current.mousePosition;
                mousepos.y = Screen.height - mousepos.y - 36.0f;
                return cam.ScreenToWorldPoint(mousepos);
            }
            return Vector2.zero;
        }

        #endregion
    }

}