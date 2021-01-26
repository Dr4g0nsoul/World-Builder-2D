using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

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

    //World Graph variables
    private WorldEditorGraph worldEditorGraph;

    //Level variables
    private SortedDictionary<string, LevelNode> levels;


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

    #endregion

    #region World Editor

    public WorldEditorGraph GetWorldEditorGraph()
    {
        if(worldEditorGraph == null)
            worldEditorGraph = Resources.Load<WorldEditorGraph>(WORLD_EDITOR_GRAPH_LOCATION + "/" + WORLD_EDITOR_GRAPH_FILE_NAME);
        return worldEditorGraph;
    }

    #endregion
}
