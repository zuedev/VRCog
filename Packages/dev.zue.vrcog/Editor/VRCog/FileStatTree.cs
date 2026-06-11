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
    private List<AssetSizeInfo> assetList = new List<AssetSizeInfo>();
    private Vector2 scrollPos;

    struct AssetSizeInfo
    {
        public Object asset;
        public long size;
        public string type;
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

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var info in assetList)
        {
            if (info.asset == null) continue;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.ObjectField(info.asset, info.asset.GetType(), false);
            GUILayout.Label(FormatSize(info.size), GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
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

        foreach (var asset in foundAssets)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    assetList.Add(new AssetSizeInfo { asset = asset, size = fi.Length, type = asset.GetType().Name });
                }
            }
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