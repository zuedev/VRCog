// Editor window for auditing assets referenced by a hierarchy root.
//
// It collects unique materials, textures used by those materials, and meshes found under the selected GameObject, then displays them sorted by on-disk file size so large contributors are easy to spot.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileStatTree : EditorWindow
{
    private GameObject targetObject;
    [SerializeField] private List<AssetSizeInfo> assetList = new List<AssetSizeInfo>();
    private Vector2 scrollPos;

    [System.Serializable]
    struct AssetSizeInfo
    {
        public List<Object> assets;
        public long size;
        public string path;
    }

    [MenuItem("Tools/VRCog/File Stat Tree")]
    public static void ShowWindow() => GetWindow<FileStatTree>("File Stat Tree");

    private void OnGUI()
    {
        GUILayout.Label("Sort Hierarchy Assets by Size", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Root", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Analyze Hierarchy Assets"))
        {
            AnalyzeAssets();
        }

        EditorGUILayout.Space();

        if (assetList.Count > 0)
        {
            EditorGUILayout.HelpBox(
                "Sizes shown are on-disk file sizes and do not reflect runtime memory usage. " +
                "A compressed texture may be small on disk but large in VRAM once decompressed, and vice versa. " +
                "For accurate runtime figures, use the Memory Profiler package.",
                MessageType.Info);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var info in assetList)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (info.assets.Count == 1)
            {
                Object a = info.assets[0];
                if (a != null)
                {
                    EditorGUILayout.ObjectField(a, a.GetType(), false);
                    GUILayout.Label(a.GetType().Name, GUILayout.Width(90));
                }
                else
                    EditorGUILayout.LabelField(Path.GetFileName(info.path));
            }
            else
            {
                EditorGUILayout.LabelField($"{Path.GetFileName(info.path)}  ({info.assets.Count} assets)");
            }
            GUILayout.Label(FormatSize(info.size), GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            if (info.assets.Count > 1)
            {
                EditorGUI.indentLevel++;
                foreach (Object a in info.assets)
                {
                    if (a == null) continue;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(a, a.GetType(), false);
                    GUILayout.Label(a.GetType().Name, GUILayout.Width(90));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    private void AnalyzeAssets()
    {
        assetList.Clear();
        if (targetObject == null) return;

        HashSet<Object> foundAssets = new HashSet<Object>();
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);

        foreach (var ren in renderers)
        {
            foreach (var mat in ren.sharedMaterials)
            {
                if (mat == null) continue;
                
                // Track the Material
                foundAssets.Add(mat);

                // Track Textures within that material
                Shader shader = mat.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture tex = mat.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                        if (tex != null) foundAssets.Add(tex);
                    }
                }
            }
            
            // Track Mesh
            switch (ren)
            {
                case SkinnedMeshRenderer smr:
                    if (smr.sharedMesh != null) foundAssets.Add(smr.sharedMesh);
                    break;
                case ParticleSystemRenderer psr when psr.renderMode == ParticleSystemRenderMode.Mesh:
                    Mesh[] particleMeshes = new Mesh[psr.meshCount];
                    psr.GetMeshes(particleMeshes);
                    foreach (Mesh m in particleMeshes)
                    {
                        if (m != null) foundAssets.Add(m);
                    }
                    break;
                default:
                    MeshFilter mf = ren.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null) foundAssets.Add(mf.sharedMesh);
                    break;
            }
        }

        // Group assets by source file so each file's size is counted only once
        var byPath = new Dictionary<string, List<Object>>();
        foreach (var asset in foundAssets)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) continue;
            if (!byPath.TryGetValue(path, out List<Object> list))
            {
                list = new List<Object>();
                byPath[path] = list;
            }
            list.Add(asset);
        }

        foreach (var kvp in byPath)
        {
            FileInfo fi = new FileInfo(kvp.Key);
            if (fi.Exists)
                assetList.Add(new AssetSizeInfo { assets = kvp.Value, size = fi.Length, path = kvp.Key });
        }

        // Sort by size descending
        assetList = assetList.OrderByDescending(asf => asf.size).ToList();
    }

    private string FormatSize(long bytes)
    {
        if (bytes >= 1048576) return (bytes / 1048576f).ToString("F2") + " MB";
        return (bytes / 1024f).ToString("F2") + " KB";
    }
}