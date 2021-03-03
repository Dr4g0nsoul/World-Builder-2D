using System;
using UnityEngine;


namespace dr4g0nsoul.WorldBuilder2D.Util
{
    [System.Serializable]
    public class UniqueObject
    {
        //Unique Identifier
        [ReadOnly] public string guid = Guid.Empty.ToString();
    }
}