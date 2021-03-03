using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor
{
    public class WorldEditorMenu
    {
        [MenuItem("World Editor/Open World Editor _F12")]
        public static void OpenWorldEditor()
        {
            LevelController.Instance.OpenWorldEditor();
        }
    }

}
