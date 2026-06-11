// Editor window for finding objects that use Poiyomi shaders in a hierarchy.
//
// It scans renderers under the selected root GameObject, lists each matching object once, and provides a quick Select action to jump directly to results for material cleanup.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zue.VRCog.Editor
{

    public class PoiyomiFinder : EditorWindow
    {
        private GameObject targetObject;
        private string shaderFilter = "poiyomi";
        private List<MatchInfo> results = new List<MatchInfo>();
        private Vector2 scrollPos;

        struct MatchInfo
        {
            public GameObject gameObject;
            public string matchedMaterial;
            public string matchedShader;
        }

        [MenuItem("Tools/VRCog/Poiyomi Finder")]
        public static void ShowWindow()
        {
            GetWindow<PoiyomiFinder>("Poiyomi Finder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Search Hierarchy for Shaders", EditorStyles.boldLabel);

            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Root", targetObject, typeof(GameObject), true);
            shaderFilter = EditorGUILayout.TextField("Shader Filter", shaderFilter);

            if (GUILayout.Button("Find Materials"))
            {
                FindMaterials();
            }

            EditorGUILayout.Space();

            if (results.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Found {results.Count} objects:", EditorStyles.helpBox);
                if (GUILayout.Button("Select All", GUILayout.Width(80)))
                {
                    Selection.objects = results.Select(r => (Object)r.gameObject).ToArray();
                }
                EditorGUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                foreach (MatchInfo info in results)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(info.gameObject, typeof(GameObject), true);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = info.gameObject;
                        EditorGUIUtility.PingObject(info.gameObject);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField($"mat: {info.matchedMaterial}  ·  shader: {info.matchedShader}", EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void FindMaterials()
        {
            results.Clear();
            if (targetObject == null) return;

            HashSet<GameObject> seen = new HashSet<GameObject>();
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer ren in renderers)
            {
                foreach (Material mat in ren.sharedMaterials)
                {
                    if (mat != null && mat.shader != null)
                    {
                        if (mat.shader.name.IndexOf(shaderFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (seen.Add(ren.gameObject))
                            {
                                results.Add(new MatchInfo
                                {
                                    gameObject = ren.gameObject,
                                    matchedMaterial = mat.name,
                                    matchedShader = mat.shader.name
                                });
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

} // namespace Zue.VRCog.Editor