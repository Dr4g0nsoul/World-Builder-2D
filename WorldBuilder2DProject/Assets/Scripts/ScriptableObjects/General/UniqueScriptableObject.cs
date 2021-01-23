using System;
using UnityEngine;
using XNode;

namespace dr4g0nsoul.WorldBuilder2D.Util
{
    public class UniqueScriptableObject : ScriptableObject
    {
        //Unique Identifier
        [ReadOnly] public string guid = Guid.Empty.ToString();

        //Genereate Unique Identifier
        public string GenerateGUID()
        {
            if (guid == Guid.Empty.ToString())
            {
                guid = Guid.NewGuid().ToString();
            }
            else
            {
                Debug.LogWarning($"{name} already has the unique identifier: {guid}");
            }
            return guid;
        }
    }

    [CreateNodeMenu(null)]
    public class UniqueNode : Node
    {
        //Unique Identifier
        [ReadOnly] public string guid = Guid.Empty.ToString();

        //Genereate Unique Identifier
        public string GenerateGUID()
        {
            if (guid == Guid.Empty.ToString())
            {
                guid = Guid.NewGuid().ToString();
            }
            else
            {
                Debug.LogWarning($"{name} already has the unique identifier: {guid}");
            }
            return guid;
        }
    }
}
