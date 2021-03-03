using dr4g0nsoul.WorldBuilder2D.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{

    [System.Serializable]
    public class LevelExit : UniqueObject
    {
        //General
        public string name;
        public bool active;

        //Connection Information
        public string otherLevelGuid;

        //Level Entry / Exit
        public Rect levelExitTrigger;
        public Vector2 entryPoint;

        public virtual bool CanEnter() => true;
    }

    [System.Serializable]
    public class LevelEntryPoint
    {
        public string levelGuid;
        public Vector2 entryPointLocation;
    }
}