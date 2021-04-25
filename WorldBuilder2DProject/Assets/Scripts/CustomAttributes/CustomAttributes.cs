using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.Util
{
    public class TagAttribute : PropertyAttribute { }

    public class ReadOnlyAttribute : PropertyAttribute { }

    public class SortingLayerAttribute : PropertyAttribute { }

    public class PhysicsLayerAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class CreateLevelObjectEditorExtensionAttribute : Attribute {

        public Type levelObjectType;

        public CreateLevelObjectEditorExtensionAttribute(Type levelObjectType)
        {
            this.levelObjectType = levelObjectType;
        }

    }

    public class LevelPickerAttribute : PropertyAttribute { }
}