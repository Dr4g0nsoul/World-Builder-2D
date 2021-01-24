using dr4g0nsoul.WorldBuilder2D.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CreateNodeMenu("Levels/Create Level")]
    public class LevelNode : UniqueNode
    {

        public string assignedSceneName;
        public string assignedScenePath;
        public int numberOfInputs;
        public int numberOfOutputs;

        public Rect levelBoundaries;
    }

}
