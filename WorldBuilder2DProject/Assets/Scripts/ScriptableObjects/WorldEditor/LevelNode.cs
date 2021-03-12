using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using dr4g0nsoul.WorldBuilder2D.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CreateNodeMenu("Levels/Create Level")]
    public class LevelNode : UniqueNode
    {
        //General
        public string levelName;
        [TextArea(4, 10)] public string levelDescription;

        public Rect levelBoundaries = new Rect(0, 0, 20, 12);

        //Level Entries and Exits
        public LevelExit[] levelExits = new LevelExit[0];
        public bool levelExitsUpdated;

        //Preferred Items
        public PreferredItems preferredItems;

        //Debug
        [ReadOnly] public string assignedSceneName;
        [ReadOnly] public string assignedScenePath;

        public override object GetValue(NodePort port)
        {
            foreach(LevelExit exit in levelExits)
            {
                //Debug.Log($"{exit.guid} == {Util.LevelEditorUtility.GetObjectGuid(port.fieldName)}");
                if (exit.guid == Util.LevelEditorUtility.GetObjectGuid(port.fieldName))
                    return exit;
            }
            Debug.LogError($"NodePort {port.fieldName} has no valid level exit!");
            return null;
        }
    }

    [System.Serializable]
    public struct PreferredItems
    {
        public string[] categories;
        public string[] levelObjects;
    }

}
