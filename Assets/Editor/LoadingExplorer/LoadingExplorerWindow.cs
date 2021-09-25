using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LoadingExplorerWindow : EditorWindow
{
    class PerFrameBuffer
    {
        public readonly static int _Time = Shader.PropertyToID("_Time");
        public readonly static int _SinTime = Shader.PropertyToID("_SinTime");
        public readonly static int _CosTime = Shader.PropertyToID("_CosTime");
        public readonly static int unity_DeltaTime = Shader.PropertyToID("unity_DeltaTime");
        public readonly static int _TimeParameters = Shader.PropertyToID("_TimeParameters");
    }

    private const int k_PreviewRTSize = 128;
    private const int k_FullPreviewRTSize = 256;
    private const int k_ItemListWidth = 120;
    private const int k_ItemHeight = 50;
    private const int k_ItemSpace = 16;
    private const int k_Space = 5;

    protected const float k_BottomToolbarHeight = 21f;


    class Styles
    {
        public static readonly GUIContent EmptyViewLabel = EditorGUIUtility.TrTextContent("Select an Item on the left to see details");


        public static readonly GUIStyle CenteredText = GetStyle("CN CenteredText");

        public static readonly GUIStyle ChannelStripAreaBackground = GetStyle("CurveEditorBackground"); //"flow background";
        public static readonly GUIStyle PreToolbar = GetStyle("preToolbar");
        public static readonly GUIStyle PreToolbar2 = GetStyle("preToolbar2");

        public static readonly GUIStyle DragHandle = GetStyle("RL DragHandle");

        static GUIStyle GetStyle(string styleName)
        {
            return styleName; // Implicit construction of GUIStyle
        }

        private static GUIStyle s_Selection;
        public static GUIStyle SelectionStyle
        {
            get
            {
                if (s_Selection == null)
                {
                    s_Selection = new GUIStyle("TV Selection");
                }
                return s_Selection;
            }
        }
    }

    class Reflection
    {
        public static class GUI
        {
            // internal static UnityEngineInternal.GenericStack scrollViewStates { get; set; } = new UnityEngineInternal.GenericStack();
            private static System.Collections.Stack s_scrollViewStates;
            public static System.Collections.Stack scrollViewStates
            {
                get
                {
                    if (s_scrollViewStates == null)
                    {
                        var property = typeof(UnityEngine.GUI).GetProperty("scrollViewStates", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                        s_scrollViewStates = property.GetValue(null) as System.Collections.Stack;
                    }
                    return s_scrollViewStates;
                }
            }
        }
        public static class ScrollViewState
        {
            private static System.Type s_Type;
            public static System.Type Type
            {
                get
                {
                    if (s_Type == null)
                    {
                        s_Type = typeof(UnityEngine.GUI).Assembly.GetType("UnityEngine.ScrollViewState", false);
                    }
                    return s_Type;
                }
            }

            // public Rect viewRect;
            private static System.Reflection.FieldInfo s_viewRect;
            public static Rect get_viewRect(object o)
            {
                if (s_viewRect == null)
                {
                    s_viewRect = Type.GetField("viewRect", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                }
                if (s_viewRect != null)
                {
                    return (Rect)s_viewRect.GetValue(o);
                }
                return Rect.zero;
            }
        }
    }

    private class LoadingData
    {
        public string name;
        public Material mat;
    }

    [SerializeField]
    private List<LoadingData> m_LoadingList = new List<LoadingData>();
    [SerializeField]
    private int m_CurViewIndex = -1;

    [SerializeField]
    private Vector2 m_ScrollPos;


    private Editor m_MaterialEditor;

    [SerializeField]
    private Vector2 m_MaterialEditorScrollPos;


    [MenuItem("Tools/Loading View")]
    private static void DoMenu()
    {
        GetWindow<LoadingExplorerWindow>();
    }

    private void OnEnable()
    {
        if (m_LoadingList == null)
        {
            m_LoadingList = new List<LoadingData>();
        }
        InitList();
        this.wantsMouseMove = true;
    }

    private void OnDisable()
    {
        ClearList();

        if (m_MaterialEditor != null)
        {
            UnityEngine.Object.DestroyImmediate(m_MaterialEditor);
            m_MaterialEditor = null;
        }
    }

    private void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.Width(k_ItemListWidth));
            {
                DrawList();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                DrawEditor();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                RefreshList();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawList()
    {
        var itemHeight = (k_ItemHeight + k_Space + k_ItemSpace);
        var totalCount = m_LoadingList.Count;
        var totalViewHeight = totalCount * itemHeight;
        var scrollRect = GUILayoutUtility.GetRect(k_ItemListWidth, -1, GUILayout.Width(k_ItemListWidth), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
        var viewRect = new Rect(0, 0, k_ItemListWidth, totalViewHeight);
        if (totalViewHeight > scrollRect.height)
        {
            var horizontalScrollbarWidth = GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x;
            viewRect.width -= horizontalScrollbarWidth;
        }
        if (Event.current.type == EventType.Repaint)
        {
            ExcuteRender();
        }

        m_ScrollPos = GUI.BeginScrollView(scrollRect, m_ScrollPos, viewRect);
        {
            int firstIndex = Mathf.FloorToInt(m_ScrollPos.y / itemHeight);
            int endIndex = Mathf.Min(Mathf.FloorToInt((m_ScrollPos.y + scrollRect.height) / itemHeight), totalCount - 1);

            for (int i = firstIndex; i <= endIndex; ++i)
            {
                var item = m_LoadingList[i];
                var screenRect = new Rect(0, i * itemHeight, viewRect.width, itemHeight);
                DrawItem(i, screenRect, item);
            }
        }
        GUI.EndScrollView();
    }

    private void DrawItem(int index, Rect screenRect, LoadingData data)
    {
        var textSize = EditorStyles.label.CalcSize(EditorGUIUtility.TrTempContent(data.name));
        var textRect = new Rect(screenRect.x + (screenRect.width - textSize.x) * 0.5f, screenRect.yMin, textSize.x, textSize.y);
        var spaceRect = new Rect(screenRect.x, screenRect.yMax - k_ItemSpace, screenRect.width, k_ItemSpace);
        var imageRect = new Rect(screenRect.x, textRect.yMax + k_Space, screenRect.width, spaceRect.yMin - (textRect.yMax + k_Space));
        var buttonRect = new Rect(screenRect.x, textRect.yMin, screenRect.width, imageRect.yMax - textRect.yMin);

        bool highLight = false;
        var pos = Event.current.mousePosition;
        if (buttonRect.Contains(pos))
        {
            highLight = true;
        }
        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
        {
            DoSelectItem(index);
        }

        if (Event.current.type == EventType.Repaint)
        {
            if (m_CurViewIndex == index)
            {
                Styles.SelectionStyle.Draw(buttonRect, false, false, true, true);
            }
            else if (highLight)
            {
                Styles.SelectionStyle.Draw(buttonRect, true, false, false, false);
            }

            EditorGUI.DropShadowLabel(textRect, data.name, EditorStyles.label);

            var minValue = Mathf.Min(imageRect.width, imageRect.height);
            var drawRect = imageRect;
            drawRect.x = imageRect.x + (imageRect.width - minValue) * 0.5f;
            drawRect.y = imageRect.y + (imageRect.height - minValue) * 0.5f;
            drawRect.width = minValue;
            drawRect.height = minValue;
            if (data.mat != null)
            {
                var old_rt = RenderTexture.active;
                var desc = new RenderTextureDescriptor(k_PreviewRTSize, k_PreviewRTSize);
                var new_rt = RenderTexture.GetTemporary(desc);
                RenderTexture.active = new_rt;
                GL.Clear(true, true, Color.clear);
                Graphics.Blit(EditorGUIUtility.whiteTexture, new_rt, data.mat);
                RenderTexture.active = old_rt;
                GUI.DrawTexture(drawRect, new_rt);
                RenderTexture.ReleaseTemporary(new_rt);
            }

            {
                const int k_Thick = 1;
                const int k_Inter = 10;
                var rect = spaceRect;
                rect.xMin += k_Inter;
                rect.xMax -= k_Inter;
                rect.yMin = rect.y + ((rect.height - k_Thick) * 0.5f);
                rect.height = k_Thick;
                var color = EditorStyles.label.normal.textColor;
                color.a = 0.5f;
                EditorGUI.DrawRect(rect, color);
            }
        }
    }

    private void DrawEditor()
    {
        EditorGUILayout.BeginVertical(Styles.ChannelStripAreaBackground);
        if (m_CurViewIndex >= 0 && m_CurViewIndex < m_LoadingList.Count)
        {
            var data = m_LoadingList[m_CurViewIndex];
            DrawPreview(data);
            DrawPreviewBar();
            DrawSelectedMaterialEditor(data);
        }
        else
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Styles.EmptyViewLabel, Styles.CenteredText);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawPreview(LoadingData data)
    {
        var layoutRect = GUILayoutUtility.GetRect(50f, -1f, 50f, -1f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        var minValue = Mathf.Min(layoutRect.width, layoutRect.height);
        var drawRect = layoutRect;
        drawRect.x = layoutRect.x + (layoutRect.width - minValue) * 0.5f;
        drawRect.y = layoutRect.y + (layoutRect.height - minValue) * 0.5f;
        drawRect.width = minValue;
        drawRect.height = minValue;

        var old_rt = RenderTexture.active;
        var desc = new RenderTextureDescriptor(k_FullPreviewRTSize, k_FullPreviewRTSize);
        var new_rt = RenderTexture.GetTemporary(desc);
        RenderTexture.active = new_rt;
        GL.Clear(true, true, Color.clear);
        Graphics.Blit(EditorGUIUtility.whiteTexture, new_rt, data.mat);
        RenderTexture.active = old_rt;
        GUI.DrawTexture(drawRect, new_rt);
        RenderTexture.ReleaseTemporary(new_rt);
    }

    private void DrawPreviewBar()
    {
        Rect dragRect;
        Rect dragIconRect = new Rect();
        const float dragPadding = 3f;
        EditorGUILayout.BeginHorizontal(Styles.PreToolbar, GUILayout.Height(k_BottomToolbarHeight));
        {
            GUILayout.FlexibleSpace();
            dragRect = GUILayoutUtility.GetLastRect();

            dragIconRect.x = dragRect.x + dragPadding;
            dragIconRect.y = dragRect.y + (k_BottomToolbarHeight - Styles.DragHandle.fixedHeight) / 2;
            dragIconRect.width = dragRect.width - dragPadding * 2;
            dragIconRect.height = Styles.DragHandle.fixedHeight;


            if (Event.current.type == EventType.Repaint)
            {
                // workaround: To properly center the image because it already has a 1px bottom padding
                dragIconRect.y += 1;
                Styles.DragHandle.Draw(dragIconRect, GUIContent.none, false, false, false, false);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSelectedMaterialEditor(LoadingData data)
    {
        var mat = data.mat;
        if (mat != null)
        {
            m_MaterialEditorScrollPos = EditorGUILayout.BeginScrollView(m_MaterialEditorScrollPos);
            EditorGUILayout.BeginVertical();
            DrawMaterialInspector(mat, ref m_MaterialEditor);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawMaterialInspector(UnityEngine.Object target, ref Editor editor)
    {
        if (editor != null && (editor.target != target || target == null))
        {
            UnityEngine.Object.DestroyImmediate(editor);
            editor = null;
        }

        if (editor == null && target != null)
        {
            editor = Editor.CreateEditor(target);
        }

        if (editor == null)
        {
            return;
        }

        //const float kSpaceForFoldoutArrow = 10f;
        //Rect titleRect = Editor.DrawHeaderGUI(editor, editor.targetTitle, kSpaceForFoldoutArrow);
        //int id = GUIUtility.GetControlID(45678, FocusType.Passive);

        //Rect renderRect = EditorGUI.GetInspectorTitleBarObjectFoldoutRenderRect(titleRect);
        //renderRect.y = titleRect.yMax - 17f; // align with bottom
        //bool oldVisible = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(target);
        //bool newVisible = EditorGUI.DoObjectFoldout(oldVisible, titleRect, renderRect, editor.targets, id);

        //if (newVisible != oldVisible)
        //    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(target, newVisible);

        bool oldVisible = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(target);
        if (!oldVisible)
        {
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(target, true);
        }
        editor.OnInspectorGUI();

    }

    private void ExcuteRender()
    {
#if UNITY_EDITOR
        float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
#else
        float time = Time.time;
#endif
        float deltaTime = Time.deltaTime;
        float smoothDeltaTime = Time.smoothDeltaTime;

        SetShaderTimeValues(time, deltaTime, smoothDeltaTime);
    }

    /// <summary>
    /// Set shader time variables as described in https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
    /// </summary>
    /// <param name="cmd">CommandBuffer to submit data to GPU.</param>
    /// <param name="time">Time.</param>
    /// <param name="deltaTime">Delta time.</param>
    /// <param name="smoothDeltaTime">Smooth delta time.</param>
    void SetShaderTimeValues(float time, float deltaTime, float smoothDeltaTime)
    {
        float timeEights = time / 8f;
        float timeFourth = time / 4f;
        float timeHalf = time / 2f;

        // Time values
        Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
        Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
        Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
        Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
        Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);

        Shader.SetGlobalVector(PerFrameBuffer._Time, timeVector);
        Shader.SetGlobalVector(PerFrameBuffer._SinTime, sinTimeVector);
        Shader.SetGlobalVector(PerFrameBuffer._CosTime, cosTimeVector);
        Shader.SetGlobalVector(PerFrameBuffer.unity_DeltaTime, deltaTimeVector);
        Shader.SetGlobalVector(PerFrameBuffer._TimeParameters, timeParametersVector);
    }

    private void InitList()
    {
        m_LoadingList.Clear();
        List<string> loadingList = new List<string>();
        CreateTemplate.GetAllLoadingSysFullPath(loadingList);
        for (int i = 0; i < loadingList.Count; ++i)
        {
            var fullName = loadingList[i];
            var name = System.IO.Path.GetFileName(fullName);

            var matAssetPath = CreateTemplate.GetMaterialAssetPath(fullName, name);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matAssetPath);
            if (mat == null)
            {
                continue;
            }

            var data = new LoadingData();
            data.name = name;
            data.mat = mat;

            m_LoadingList.Add(data);
        }
    }

    private void ClearList()
    {
        m_LoadingList.Clear();
        m_CurViewIndex = -1;
    }

    private void RefreshList()
    {
        ClearList();
        InitList();
    }

    private void DoSelectItem(int index)
    {
        if (index >= 0 && index < m_LoadingList.Count)
        {
            m_CurViewIndex = index;
        }
        else
        {
            m_CurViewIndex = -1;
        }
    }
}
