using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Loading
{
    public class GUIUtility
    {
        // internal static float RoundToPixelGrid(float v)
        private static MethodInfo s_Method_RoundToPixelGrid;
        public static float RoundToPixelGrid(float v)
        {
            if(s_Method_RoundToPixelGrid == null)
            {
                s_Method_RoundToPixelGrid = typeof(UnityEngine.GUIUtility).GetMethod("RoundToPixelGrid",
                    BindingFlags.NonPublic | BindingFlags.Static
                    );
            }
            return (float)s_Method_RoundToPixelGrid.Invoke(null, new object[] { v });
        }
    }
}