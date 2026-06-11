// Editor window for finding objects that use Poiyomi shaders in a hierarchy.
//
// It scans renderers under the selected root GameObject, lists each matching object once, and provides a quick Select action to jump directly to results for material cleanup.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PoiyomiFinder : EditorWindow
{
    private GameObject targetObject;
    private List<GameObject> results = new List<GameObject>();
    private Vector2 scrollPos;

    [MenuItem("Tools/VRCog/Poiyomi Finder")]
    public static void ShowWindow()
    {
        GetWindow<PoiyomiFinder>("Poiyomi Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search Hierarchy for Poiyomi Shaders", EditorStyles.boldLabel);
        
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Root", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Find Poiyomi Materials"))
        {
            FindMaterials();
        }

        EditorGUILayout.Space();

        if (results.Count > 0)
        {
            GUILayout.Label($"Found {results.Count} objects:", EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            foreach (GameObject obj in results)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = obj;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void FindMaterials()
    {
        results.Clear();
        if (targetObject == null) return;

        // Get all renderers in children
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer ren in renderers)
        {
            foreach (Material mat in ren.sharedMaterials)
            {
                if (mat != null && mat.shader != null)
                {
                    // Check if the shader name contains "poiyomi" (case-insensitive)
                    if (mat.shader.name.ToLower().Contains("poiyomi"))
                    {
                        if (!results.Contains(ren.gameObject))
                        {
                            results.Add(ren.gameObject);
                        }
                        break; // Move to next renderer once found
                    }
                }
            }
        }
    }
}