using dr4g0nsoul.WorldBuilder2D.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{

    [CreateNodeMenu("Levels/Create Level")]
    public class LevelNode : UniqueNode
    {

        [HideInInspector] public Scene assignedScene;
    }

}
