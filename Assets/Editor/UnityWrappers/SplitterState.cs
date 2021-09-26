using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Loading
{
    /// <summary>
    /// Wrapper for unity internal SplitterState class
    /// </summary>
    public struct SplitterState
    {
        private static readonly Type SplitterStateType = typeof(Editor).Assembly.GetType("UnityEditor.SplitterState");
        public object targetObject;
        //public float[] realSizes;

        //private static readonly FieldInfo RealSizesInfo = SplitterStateType.GetField(
        //    "realSizes",
        //    BindingFlags.DeclaredOnly |
        //    BindingFlags.Public |
        //    BindingFlags.NonPublic |
        //    BindingFlags.Instance |
        //    BindingFlags.GetField);

        public SplitterState(float[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            targetObject = SplitterStateType.InvokeMember(null,
            BindingFlags.DeclaredOnly |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.CreateInstance, null, null, new object[] { relativeSizes, minSizes, maxSizes });
            //realSizes = (float[])RealSizesInfo.GetValue(targetObject);
        }

        public SplitterState(object splitter)
        {
            this.targetObject = splitter;
            //realSizes = (float[])RealSizesInfo.GetValue(splitter);
        }

        public int ID
        {
            get
            {
                return (int)SplitterStateType.InvokeMember("ID",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
            }
            internal set
            {
                SplitterStateType.InvokeMember("ID",
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.SetField, null, targetObject, new object[] { value });
            }
        }


        // public float xOffset;
        private static FieldInfo s_Field_xOffset;
        private static void CheckField_xOffset()
        {
            if (s_Field_xOffset == null)
            {
                s_Field_xOffset = SplitterStateType.GetField("xOffset",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField);
            }
        }
        public float xOffset
        {
            get
            {
                //return (float)SplitterStateType.InvokeMember("xOffset",
                //    BindingFlags.DeclaredOnly |
                //    BindingFlags.Public | BindingFlags.NonPublic |
                //    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
                CheckField_xOffset();
                return (float)s_Field_xOffset.GetValue(targetObject);
            }
        }

        // public float splitSize;
        private static FieldInfo s_Field_splitSize;
        private static void CheckField_splitSize()
        {
            if (s_Field_splitSize == null)
            {
                s_Field_splitSize = SplitterStateType.GetField("splitSize",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField);
            }
        }
        public float splitSize
        {
            get
            {
                //return (int)SplitterStateType.InvokeMember("splitSize",
                //    BindingFlags.DeclaredOnly |
                //    BindingFlags.Public | BindingFlags.NonPublic |
                //    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
                CheckField_splitSize();
                return (float)s_Field_splitSize.GetValue(targetObject);
            }
        }

        // public float[] realSizes;
        private static FieldInfo s_Field_realSizes;
        private static void CheckField_realSizes()
        {
            if (s_Field_realSizes == null)
            {
                s_Field_realSizes = SplitterStateType.GetField("realSizes",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField);
            }
        }
        public float[] realSizes
        {
            get
            {
                CheckField_realSizes();
                return (float[])s_Field_realSizes.GetValue(targetObject);
            }
        }

        // public float[] relativeSizes; // these should always add up to 1
        private static FieldInfo s_Field_relativeSizes;
        private static void CheckField_relativeSizes()
        {
            if (s_Field_relativeSizes == null)
            {
                s_Field_relativeSizes = SplitterStateType.GetField("relativeSizes",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField);
            }
        }
        public float[] relativeSizes
        {
            get
            {
                //return (float[])SplitterStateType.InvokeMember("relativeSizes",
                //    BindingFlags.DeclaredOnly |
                //    BindingFlags.Public | BindingFlags.NonPublic |
                //    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
                CheckField_relativeSizes();
                return (float[])s_Field_relativeSizes.GetValue(targetObject);
            }
        }
        public int splitterInitialOffset
        {
            get
            {
                return (int)SplitterStateType.InvokeMember("splitterInitialOffset",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
            }
            internal set
            {
                SplitterStateType.InvokeMember("splitterInitialOffset",
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.SetField, null, targetObject, new object[] { value });
            }
        }
        public int currentActiveSplitter
        {
            get
            {
                return (int)SplitterStateType.InvokeMember("currentActiveSplitter",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetField, null, targetObject, null);
            }
            internal set
            {
                SplitterStateType.InvokeMember("currentActiveSplitter",
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.SetField, null, targetObject, new object[] { value });
            }
        }

        public static SplitterState FromRelative(float[] relativeSizes, float[] minSizes, float[] maxSizes)
        {
            var splitterState = SplitterStateType.InvokeMember("FromRelative", 
                 BindingFlags.Public | BindingFlags.NonPublic | 
                 BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { relativeSizes, minSizes, maxSizes });
            return new SplitterState(splitterState);
        }

        public static SplitterState FromRelative(float[] relativeSizes, float[] minSizes, float[] maxSizes, int splitSize)
        {
            var splitterState = SplitterStateType.InvokeMember("FromRelative",
                 BindingFlags.Public | BindingFlags.NonPublic |
                 BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { relativeSizes, minSizes, maxSizes, splitSize });
            return new SplitterState(splitterState);
        }

        public void RealToRelativeSizes()
        {
            SplitterStateType.InvokeMember("RealToRelativeSizes",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.InvokeMethod, null, targetObject, null);
        }

        public void DoSplitter(int currentActiveSplitter, int v, int num3)
        {
            SplitterStateType.InvokeMember("DoSplitter",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.InvokeMethod, null, targetObject, new object[] { currentActiveSplitter, v, num3 });
        }
    }

}