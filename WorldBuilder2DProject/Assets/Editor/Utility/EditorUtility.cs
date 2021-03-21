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
        /// <param name="folder">Folder where the asset is located ending with '/', has to start with "Assets/"</param>
        /// <param name="assetName">Name of the asset</param>
        /// <param name="asset">The asset that has to be created</param>
        /// <param name="fileType">The filetype with initial dot</param>
        /// <returns></returns>
        public static bool CreateAssetAndFolders(string folder, string assetName, UnityEngine.Object asset, string fileType = null)
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
                string actualFileType = ".asset";
                if(fileType != null && fileType.Length > 2 && fileType.StartsWith(".")) {
                    actualFileType = fileType;
                }

                try
                {
                    AssetDatabase.CreateAsset(asset, currParent + assetName + actualFileType);
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


        /// <summary>
        /// Creates a folder and creating also the parent folders specified in its path
        /// </summary>
        /// <param name="path">Folder where the asset is located ending with '/', has to start with "Assets/"</param>
        /// <returns></returns>
        public static bool CreateFolders(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("EditorUtility::CreateFolders: Folder path is null or empty");
                return false;
            }
            if (path.StartsWith("Assets/"))
            {
                if (!path.EndsWith("/"))
                    path += "/";

                //Create folders
                int currIndex = 0;
                string currParent = "";
                string currFolderToCreate;
                bool isFolder = true;
                while (isFolder)
                {
                    currIndex = path.IndexOf('/', currIndex) + 1;
                    currParent = path.Substring(0, currIndex);
                    if (path.IndexOf('/', currIndex) > 0)
                    {
                        currFolderToCreate = path.Substring(currIndex, path.IndexOf('/', currIndex) - currIndex);
                        if (!AssetDatabase.IsValidFolder(currParent + currFolderToCreate))
                        {
                            if (AssetDatabase.CreateFolder(currParent.TrimEnd('/'), currFolderToCreate) == null)
                            {
                                Debug.LogError($"EditorUtility::CreateFolders: Folder \"{currParent + currFolderToCreate}\" can not be created");
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

                return true;
            }
            else
            {
                Debug.LogError($"EditorUtility::CreateFolders: Folderpath of {path} has to start with \"Assets/\"");
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
                mousepos.y = Screen.height - mousepos.y - 40.0f;
                return cam.ScreenToWorldPoint(mousepos);
            }
            return Vector2.zero;
        }

        public static Vector2 WorldToSceneViewPos(Vector2 worldPos)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Camera cam = view.camera;
                Vector2 scenePos = cam.WorldToScreenPoint(worldPos);
                scenePos.y = Screen.height - scenePos.y - 40.0f;
                return scenePos;
            }
            return Vector2.zero;
        }

        public static bool IsMouseInsideSceneView(SceneView view)
        {
            return view != null && EditorWindow.focusedWindow == view
                && Event.current.mousePosition.x >= 0 && Event.current.mousePosition.y >= 0
                && Event.current.mousePosition.x <= view.camera.pixelWidth && Event.current.mousePosition.y <= view.camera.pixelHeight;
        }

        #endregion

        #region Custom Inspector

        public static Rect ResizeRect(Rect rect, Handles.CapFunction resizeCap, Handles.CapFunction moveCap, Color capCol, Color fillCol, float capSize, float snap, bool editable = true)
        {
            Vector2 halfRectSize = new Vector2(rect.size.x * 0.5f, rect.size.y * 0.5f);

            Vector3[] rectangleCorners =
                {
                new Vector3(rect.position.x - halfRectSize.x, rect.position.y - halfRectSize.y, 0),   // Bottom Left
                new Vector3(rect.position.x + halfRectSize.x, rect.position.y - halfRectSize.y, 0),   // Bottom Right
                new Vector3(rect.position.x + halfRectSize.x, rect.position.y + halfRectSize.y, 0),   // Top Right
                new Vector3(rect.position.x - halfRectSize.x, rect.position.y + halfRectSize.y, 0)    // Top Left
            };

            Handles.color = fillCol;
            Handles.DrawSolidRectangleWithOutline(rectangleCorners, new Color(fillCol.r, fillCol.g, fillCol.b, 0.25f), capCol);

            var newPosition = rect.position;
            var newSize = rect.size;



            if (editable)
            {

                Handles.color = capCol;
                newPosition = Handles.FreeMoveHandle(newPosition, Quaternion.identity, capSize, new Vector2(snap, snap), moveCap);

                Vector3[] handlePoints =
                {
                    new Vector3(rect.position.x - halfRectSize.x, rect.position.y, 0),   // Left
                    new Vector3(rect.position.x + halfRectSize.x, rect.position.y, 0),   // Right
                    new Vector3(rect.position.x, rect.position.y + halfRectSize.y, 0),   // Top
                    new Vector3(rect.position.x, rect.position.y - halfRectSize.y, 0)    // Bottom 
                };

                var leftHandle = Handles.Slider(handlePoints[0], -Vector3.right, capSize, resizeCap, snap).x - handlePoints[0].x;
                var rightHandle = Handles.Slider(handlePoints[1], Vector3.right, capSize, resizeCap, snap).x - handlePoints[1].x;
                var topHandle = Handles.Slider(handlePoints[2], Vector3.up, capSize, resizeCap, snap).y - handlePoints[2].y;
                var bottomHandle = Handles.Slider(handlePoints[3], -Vector3.up, capSize, resizeCap, snap).y - handlePoints[3].y;

                newSize = new Vector2(
                    Mathf.Max(.1f, newSize.x - leftHandle + rightHandle),
                    Mathf.Max(.1f, newSize.y + topHandle - bottomHandle));

                newPosition = new Vector2(
                    newPosition.x + leftHandle * .5f + rightHandle * .5f,
                    newPosition.y + topHandle * .5f + bottomHandle * .5f);
            }

            return new Rect(newPosition.x, newPosition.y, newSize.x, newSize.y);
        }

        public static void DrawRect(Rect rect, Color borderCol, Color fillCol)
        {
            Vector2 halfRectSize = new Vector2(rect.size.x * 0.5f, rect.size.y * 0.5f);

            Vector3[] rectangleCorners =
                {
                new Vector3(rect.position.x - halfRectSize.x, rect.position.y - halfRectSize.y, 0),   // Bottom Left
                new Vector3(rect.position.x + halfRectSize.x, rect.position.y - halfRectSize.y, 0),   // Bottom Right
                new Vector3(rect.position.x + halfRectSize.x, rect.position.y + halfRectSize.y, 0),   // Top Right
                new Vector3(rect.position.x - halfRectSize.x, rect.position.y + halfRectSize.y, 0)    // Top Left
            };

            Handles.color = fillCol;
            Handles.DrawSolidRectangleWithOutline(rectangleCorners, new Color(fillCol.r, fillCol.g, fillCol.b, 0.25f), borderCol);
        }

        #endregion
    }

}