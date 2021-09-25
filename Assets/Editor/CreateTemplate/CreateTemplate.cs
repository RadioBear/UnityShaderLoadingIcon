using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CreateTemplate
{
    private const string k_RootAssetPath = "Assets/Loading";
    private const string k_FolderName = "Loading_";
    private const string k_ShaderTemplateAssetPath = "Assets/Editor/CreateTemplate/ShaderTemplate.shader.txt";

    private const string k_MaterialFolderName = "Material";
    private const string k_ShaderFolderName = "Shader";
    private const string k_PrefabFolderName = "Prefab";

    private readonly static Vector2 s_ImageElementSize = new Vector2(100f, 100f);

    [MenuItem("Tools/Create Loading")]
    private static void DoCreateLoading()
    {
        var newFullPath = GenerateUniqueSysFullPath();
        if(System.IO.Directory.Exists(newFullPath))
        {
            Debug.LogError("GenerateUniqueSysFullPath Fail");
            return;
        }

        System.IO.Directory.CreateDirectory(newFullPath);
        AssetDatabase.ImportAsset(GetAssetPathFromSysFullPath(newFullPath));

        var materialFolderPath = System.IO.Path.Combine(newFullPath, k_MaterialFolderName);
        var shaderFolderPath = System.IO.Path.Combine(newFullPath, k_ShaderFolderName);
        var prefabFolderPath = System.IO.Path.Combine(newFullPath, k_PrefabFolderName);
        System.IO.Directory.CreateDirectory(materialFolderPath);
        System.IO.Directory.CreateDirectory(shaderFolderPath);
        System.IO.Directory.CreateDirectory(prefabFolderPath);
        AssetDatabase.ImportAsset(GetAssetPathFromSysFullPath(materialFolderPath));
        AssetDatabase.ImportAsset(GetAssetPathFromSysFullPath(shaderFolderPath));
        AssetDatabase.ImportAsset(GetAssetPathFromSysFullPath(prefabFolderPath));

        var name = System.IO.Path.GetFileName(newFullPath);
        var shaderSysFullPath = System.IO.Path.Combine(shaderFolderPath, name + ".shader");
        var newShader = CreateShader(shaderSysFullPath, name);
        if (newShader == null)
        {
            Debug.LogError("Create Shader Fail:" + shaderSysFullPath);
            return;
        }
        var materialSysFullPath = System.IO.Path.Combine(materialFolderPath, name + ".mat");
        var newMaterial = CreateMaterial(materialSysFullPath, name, newShader);
        if (newMaterial == null)
        {
            Debug.LogError("Create Material Fail:" + materialSysFullPath);
            return;
        }
        var prefabSysFullPath = System.IO.Path.Combine(prefabFolderPath, name + ".prefab");
        var newPrefab = CreatePrefab(prefabSysFullPath, name, newMaterial);
        if (newPrefab == null)
        {
            Debug.LogError("Create Prefab Fail:" + prefabSysFullPath);
            return;
        }

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<DefaultAsset>(GetAssetPathFromSysFullPath(newFullPath)));
        Debug.Log("Create Succeed:" + newFullPath);
    }

    public static string GetMaterialAssetPath(string fullPath, string name)
    {
        var materialFolderPath = System.IO.Path.Combine(fullPath, k_MaterialFolderName);
        var materialSysFullPath = System.IO.Path.Combine(materialFolderPath, name + ".mat");
        var assetPath = GetAssetPathFromSysFullPath(materialSysFullPath);
        return assetPath;
    }

    private static string GetRootSysFullPath()
    {
        return GetSysFullPathFromAssetPath(k_RootAssetPath);
    }

    private static string GetSysFullPathFromAssetPath(string assetPath)
    {
        return NiceSysPath(System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, assetPath));
    }

    private static string GetAssetPathFromSysFullPath(string fullPath)
    {
        var start = System.IO.Directory.GetParent(Application.dataPath).FullName;
        if (fullPath.StartsWith(start, System.StringComparison.Ordinal))
        {
            fullPath = fullPath.Substring(start.Length);
            if(fullPath[0] == System.IO.Path.DirectorySeparatorChar)
            {
                fullPath = fullPath.Substring(1);
            }
        }
        return NiceAssetPath(fullPath);
    }


    private static string NiceSysPath(string unityPath)
    {
        return System.IO.Path.DirectorySeparatorChar == '\\' ? unityPath.Replace('/', '\\') : unityPath;
    }

    private static string NiceAssetPath(string unityPath)
    {
        return System.IO.Path.DirectorySeparatorChar == '\\' ? unityPath.Replace('\\', '/') : unityPath;
    }

    public static void GetAllLoadingSysFullPath(List<string> _out_list)
    {
        var fileNamePattern = new System.Text.RegularExpressions.Regex(@$"^{k_FolderName}(\d+)$");
        var rootSysFullPath = GetRootSysFullPath();
        var all_directories = System.IO.Directory.GetDirectories(rootSysFullPath, k_FolderName + "*", System.IO.SearchOption.TopDirectoryOnly);
        for (int i = 0; i < all_directories.Length; ++i)
        {
            var fullName = all_directories[i];
            var name = System.IO.Path.GetFileName(fullName);
            if(fileNamePattern.IsMatch(name))
            {
                _out_list.Add(fullName);
            }
        }
    }

    private static string GenerateUniqueSysFullPath()
    {
        var fileNamePattern = new System.Text.RegularExpressions.Regex(@$"^{k_FolderName}(\d+)$");
        var rootSysFullPath = GetRootSysFullPath();
        var all_directories = System.IO.Directory.GetDirectories(rootSysFullPath, k_FolderName + "*", System.IO.SearchOption.TopDirectoryOnly);
        int index = 0;
        for(int i = 0; i < all_directories.Length; ++i)
        {
            var fullName = all_directories[i];
            var name = System.IO.Path.GetFileName(fullName);
            var match = fileNamePattern.Match(name);
            if(match != null && match.Success)
            {
                var intStr = match.Groups[1].Value;
                int val;
                if(int.TryParse(intStr, out val))
                {
                    index = Mathf.Max(index, val);
                }
            }
        }

        return NiceSysPath(System.IO.Path.Combine(rootSysFullPath, k_FolderName + (index + 1)));
    }

    private static Shader CreateShader(string fullPath, string shaderName)
    {
        var path = GetSysFullPathFromAssetPath(k_ShaderTemplateAssetPath);
        if(!System.IO.File.Exists(path))
        {
            return null;
        }
        var text = System.IO.File.ReadAllText(path);
        text = text.Replace("<%ShaderName%>", shaderName);
        System.IO.File.WriteAllText(fullPath, text);
        var assetPath = GetAssetPathFromSysFullPath(fullPath);
        AssetDatabase.ImportAsset(assetPath);
        return AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
    }

    private static Material CreateMaterial(string fullPath, string matName, Shader shader)
    {
        var mat = new Material(shader);
        mat.name = matName;
        var assetPath = GetAssetPathFromSysFullPath(fullPath);
        AssetDatabase.CreateAsset(mat, assetPath);
        return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
    }

    private static GameObject CreatePrefab(string fullPath, string prefabName, Material mat)
    {
        var newPrefab = new GameObject(prefabName);
        RectTransform rectTransform = newPrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = s_ImageElementSize;
        var newImage = newPrefab.AddComponent<Image>();
        newImage.material = mat;
        newImage.raycastTarget = false;
        newImage.maskable = false;
        newPrefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var assetPath = GetAssetPathFromSysFullPath(fullPath);
        var prefab = PrefabUtility.SaveAsPrefabAsset(newPrefab, assetPath);
        UnityEngine.Object.DestroyImmediate(newPrefab);
        return prefab;
    }
}
