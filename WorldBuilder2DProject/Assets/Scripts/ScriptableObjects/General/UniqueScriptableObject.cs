﻿using System;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Util
{
    public class UniqueScriptableObject : ScriptableObject
    {
        //Unique Identifier
        [ReadOnly] public string guid = Guid.Empty.ToString();

        //Genereate Unique Identifier
        public void GenerateGUID()
        {
            if (guid == Guid.Empty.ToString())
            {
                guid = Guid.NewGuid().ToString();
            }
            else
            {
                Debug.LogWarning($"{name} already has the unique identifier: {guid}");
            }
        }
    }
}