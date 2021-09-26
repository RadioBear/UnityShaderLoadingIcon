using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Loading
{
    /// <summary>
    /// Wrapper for unity internal SplitterGUILayout class
    /// </summary>
    public class SplitterGUILayout
    {
        private static readonly Type SplitterGuiLayoutType = typeof(Editor).Assembly.GetType("UnityEditor.SplitterGUILayout");


        public static void BeginHorizontalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            BeginSplit(state, GUIStyle.none, false, options);
        }

        public static void BeginHorizontalSplit(SplitterState state, GUIStyle style, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, false, options);
        }

        public static void BeginVerticalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            BeginSplit(state, GUIStyle.none, true, options);
        }

        public static void BeginVerticalSplit(SplitterState state, GUIStyle style, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, true, options);
        }

        // public static void EndVerticalSplit()
        private static MethodInfo s_Method_EndVerticalSplit;
        public static void EndVerticalSplit()
        {
            if (s_Method_EndVerticalSplit == null)
            {
                s_Method_EndVerticalSplit = SplitterGuiLayoutType.GetMethod("EndVerticalSplit",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.InvokeMethod
                    );
            }
            if (s_Method_EndVerticalSplit != null)
            {
                s_Method_EndVerticalSplit.Invoke(null, null);
            }
        }

        // public static void EndHorizontalSplit()
        private static MethodInfo s_Method_EndHorizontalSplit;
        public static void EndHorizontalSplit()
        {
            if (s_Method_EndHorizontalSplit == null)
            {
                s_Method_EndHorizontalSplit = SplitterGuiLayoutType.GetMethod("EndHorizontalSplit",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.InvokeMethod
                    );
            }
            if (s_Method_EndHorizontalSplit != null)
            {
                s_Method_EndHorizontalSplit.Invoke(null, null);
            }
        }

        public static Rect GetSplitterRect(SplitterState state, int i)
        {
            var g = Loading.SplitterGUILayout.GUISplitterGroup.GetTopLevel();
            float cursor = Loading.GUIUtility.RoundToPixelGrid(g.isVertical ? g.rect.y : g.rect.x);
            var splitterRect = g.isVertical ?
                        new Rect(state.xOffset + g.rect.x, cursor + state.realSizes[i] - state.splitSize / 2, g.rect.width, state.splitSize) :
                        new Rect(state.xOffset + cursor + state.realSizes[i] - state.splitSize / 2, g.rect.y, state.splitSize, g.rect.height);
            return splitterRect;
        }

        // public static void BeginSplit(SplitterState state, GUIStyle style, bool vertical, params GUILayoutOption[] options)
        private static MethodInfo s_Method_BeginSplit;
        public static void BeginSplit(SplitterState state, GUIStyle style, bool vertical, params GUILayoutOption[] options)
        {
            if(s_Method_BeginSplit == null)
            {
                s_Method_BeginSplit = SplitterGuiLayoutType.GetMethod("BeginSplit",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.InvokeMethod
                    );
            }
            if(s_Method_BeginSplit != null)
            {
                s_Method_BeginSplit.Invoke(null, new object[] { state.targetObject, style, vertical, options });
            }
        }

        /// <summary>
        /// Wrapper for unity internal SplitterGUILayout.GUISplitterGroup class
        /// </summary>
        public struct GUISplitterGroup
        {
            public static readonly Type GuiSplitterGroupType = SplitterGuiLayoutType.GetNestedType("GUISplitterGroup", BindingFlags.NonPublic);
            private readonly object guiSplitterGroup;
            //private SplitterState myState;

            // GUILayoutUtility.current.topLevel
            // internal static LayoutCache current = new LayoutCache();
            private static FieldInfo s_Field_current;
            private static FieldInfo s_Field_topLevel;
            public static GUISplitterGroup GetTopLevel()
            {
                if (s_Field_current == null)
                {
                    s_Field_current = typeof(GUILayoutUtility).GetField("current", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                }
                var layoutCache = s_Field_current.GetValue(null);
                if(layoutCache == null)
                {
                    return new GUISplitterGroup();
                }
                if (s_Field_topLevel == null)
                {
                    var layoutCacheType = layoutCache.GetType();
                    s_Field_topLevel = layoutCacheType.GetField("topLevel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
                var topLevelGroup = s_Field_topLevel.GetValue(layoutCache);
                UnityEngine.Assertions.Assert.IsTrue(topLevelGroup.GetType() == GuiSplitterGroupType);
                return new GUISplitterGroup(topLevelGroup);
            }

            public GUISplitterGroup(object guiSplitterGroup)
            {
                this.guiSplitterGroup = guiSplitterGroup;
            }

            public bool isVertical
            {
                get
                {
                    return (bool)GuiSplitterGroupType.InvokeMember("isVertical",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, guiSplitterGroup, null);
                }
                internal set
                {
                    GuiSplitterGroupType.InvokeMember("isVertical",
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.SetField, null, guiSplitterGroup, new object[] { value });
                }
            }
            public Rect rect
            {
                get
                {
                    return (Rect)GuiSplitterGroupType.InvokeMember("rect",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, guiSplitterGroup, null);
                }
            }
            public bool resetCoords
            {
                get
                {
                    return (bool)GuiSplitterGroupType.InvokeMember("resetCoords",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, guiSplitterGroup, null);
                }
                internal set
                {
                    GuiSplitterGroupType.InvokeMember("resetCoords",
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.SetField, null, guiSplitterGroup, new object[] { value });
                }
            }

            //public SplitterState state
            //{
            //    get
            //    {
            //        return myState;
            //    }
            //    internal set
            //    {
            //        myState = value;
            //        GuiSplitterGroupType.InvokeMember("state",
            //             BindingFlags.DeclaredOnly |
            //             BindingFlags.Public | BindingFlags.NonPublic |
            //             BindingFlags.Instance | BindingFlags.SetField, null, guiSplitterGroup, new object[] { value.targetObject });
            //    }
            //}

            public void ApplyOptions(GUILayoutOption[] options)
            {
                GuiSplitterGroupType.InvokeMember("ApplyOptions",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.InvokeMethod, null, guiSplitterGroup, new object[] { options });
            }
        }
    }
}