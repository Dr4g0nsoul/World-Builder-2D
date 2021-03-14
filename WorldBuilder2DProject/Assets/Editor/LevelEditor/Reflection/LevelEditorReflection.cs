using dr4g0nsoul.WorldBuilder2D.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace dr4g0nsoul.WorldBuilder2D.LevelEditor
{

    public class LevelEditorReflection
    {

        #region Level Objects

        public static Type[] GetLevelObjectTypes()
        {
            return GetDerivedTypes(typeof(LevelObject));
        }

        /// <summary> Get all classes deriving from baseType via reflection </summary>
        public static Type[] GetDerivedTypes(Type baseType)
        {
            List<System.Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t) && t != baseType).ToArray());
                }
                catch (ReflectionTypeLoadException) { }
            }
            if(baseType != null)
                types.Insert(0, baseType);
            return types.ToArray();
        }

        #endregion

        #region Utility

        public static string[] GetFormattedTypeNames(Type[] types)
        {
            List<string> typeNames = new List<string>();

            foreach(Type t in types)
            {
                string firstLetterUppercase = t.Name.First().ToString().ToUpper() + t.Name.Substring(1);
                typeNames.Add(SplitCamelCase(firstLetterUppercase));
            }

            return typeNames.ToArray();
        }

        public static string SplitCamelCase(string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        #endregion
    }

}
