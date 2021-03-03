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

        public Rect levelBoundaries;

        //Level Entries and Exits
        public LevelExit[] levelExits;
        public bool levelExitsUpdated;

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

}
