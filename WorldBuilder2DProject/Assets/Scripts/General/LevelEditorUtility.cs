using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Util
{

    public class LevelEditorUtility
    {

        public static string GetObjectGuid(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            int guidBegin = objectName.LastIndexOf('_');
            guidBegin = Mathf.Min(guidBegin + 1, objectName.Length);

            if (Guid.TryParse(objectName.Substring(guidBegin, objectName.Length - guidBegin), out Guid result))
                return result.ToString();

            return null;
        }

        public static string GetObjectGuid(GameObject gameObject)
        {
            if (gameObject != null)
            {
                return GetObjectGuid(gameObject.name);
            }
            return null;
        }

        public static string GetObjectGuid(Transform transform)
        {
            if (transform != null)
            {
                return GetObjectGuid(transform.gameObject.name);
            }
            return null;
        }
    }
}
