﻿using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;
using XNodeEditor;

public class LevelController
{
    #region Singleton

    private static LevelController s_levelController;

    public static LevelController Instance
    {
        get
        {
            if(s_levelController == null)
            {
                s_levelController = new LevelController();
            }
            return s_levelController;
        }
    }

    #endregion



    //World Graph constants
    public const string WORLD_EDITOR_GRAPH_LOCATION = "WorldEditor";
    public const string WORLD_EDITOR_GRAPH_FILE_NAME = "WorldEditorGraph";

    //Level constants
    public const string LEVEL_SCENE_LOCATION = "Assets/Scenes/Levels";
    public const string LEVEL_THUMBNAIL_LOCATION = "Assets/Editor/Resources/Levels/Thumbnails";
    public const string LEVEL_THUMBNAIL_RESOURCE_LOCATION = "Levels/Thumbnails";
    public const string LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX = "Thumbnail";

    //World Graph variables
    private WorldEditorGraph worldEditorGraph;

    //Level variables
    private SortedDictionary<string, LevelNode> levels;
    private SortedDictionary<string, Texture2D> thumbnailCache;


    #region Initialization

    public LevelController()
    {
        LoadLevels();
    }

    public void LoadLevels()
    {
        GetWorldEditorGraph();

        //Clear data
        if (levels == null)
            levels = new SortedDictionary<string, LevelNode>();
        levels.Clear();

        if (worldEditorGraph != null)
        {

            //Get all levels
            foreach (Node node in worldEditorGraph.nodes)
            {
                if (node is LevelNode)
                {
                    LevelNode lNode = node as LevelNode;
                    levels.Add(lNode.guid, lNode);
                }
            }
        }
    }

    #endregion

    #region Levels

    public LevelNode GetLevel(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return null;

        levels.TryGetValue(guid, out LevelNode lNode);
        return lNode;
    }

    public bool IsValidLevel(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return false;

        return levels.ContainsKey(guid);
    }

    public bool IsValidLevel(GameObject levelRoot)
    {
        if(levelRoot != null)
        {
            LevelInstance levelInstance = levelRoot.GetComponent<LevelInstance>();
            if(levelInstance != null && IsValidLevel(levelInstance.level))
            {
                //TODO: further checks for level structure
                return true;
            }
        }
        return false;
    }

    #region Thumbnail

    public void EmptyLevelThumbnailsCache()
    {
        if(thumbnailCache == null)
        {
            thumbnailCache = new SortedDictionary<string, Texture2D>();
        }
        thumbnailCache.Clear();
    }

    public Texture2D GetLevelThumbnail(string guid)
    {
        if (thumbnailCache == null)
        {
            thumbnailCache = new SortedDictionary<string, Texture2D>();
        }

        if (IsValidLevel(guid))
        {
            if(thumbnailCache.ContainsKey(guid))
            {
                return thumbnailCache[guid];
            }
            else
            {
                Texture2D levelThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>($"{LEVEL_THUMBNAIL_LOCATION}/{LEVEL_THUMBNAIL_RESOURCE_NAME_PREFIX}_{guid}.png");
                if(levelThumbnail != null)
                {
                    thumbnailCache.Add(guid, levelThumbnail);
                    return levelThumbnail;
                }
            }
        }
        return null;
    }

    #endregion

    #endregion

    #region World Editor

    public WorldEditorGraph GetWorldEditorGraph()
    {
        if(worldEditorGraph == null)
            worldEditorGraph = Resources.Load<WorldEditorGraph>(WORLD_EDITOR_GRAPH_LOCATION + "/" + WORLD_EDITOR_GRAPH_FILE_NAME);
        return worldEditorGraph;
    }

    public void OpenWorldEditor()
    {
        WorldEditorGraph worldEditor = LevelController.Instance.GetWorldEditorGraph();

        if (EditorWindow.HasOpenInstances<NodeEditorWindow>())
        {
            EditorWindow.FocusWindowIfItsOpen<NodeEditorWindow>();
        }
        else
        {
            NodeEditorWindow.Open(worldEditor);
        }

        if (worldEditor.nodes != null && worldEditor.nodes.Count > 0)
        {
            ProjectWindowUtil.ShowCreatedAsset(worldEditor.nodes[0]);
            NodeEditorWindow.current.Home();
        }
    }

    #endregion
}