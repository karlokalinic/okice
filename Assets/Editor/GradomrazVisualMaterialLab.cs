#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Explicit opt-in material lab. Never rewrites GRADOMRAZ scenes, shared
/// materials, renderer assignments, gameplay references or build scene lists.
/// </summary>
public static class GradomrazVisualMaterialLab
{
    private const string Root = "Assets/GRADOMRAZ";
    private const string Lab = Root + "/VisualLab";
    private const string PreviewScene = Lab + "/GRADOMRAZ-MaterialLab.unity";
    private static readonly (string shader, string filename)[] Candidates =
    {
        ("GRADOMRAZ/Atmosphere/Weathered Glass", "Weathered-Glass.mat"),
        ("GRADOMRAZ/Props/Lit Paper", "Lit-Paper.mat"),
        ("GRADOMRAZ/Environment/Wet Triplanar Concrete", "Wet-Concrete.mat")
    };

    [MenuItem("Tools/GRADOMRAZ/Visual Lab/1 - Validate Shader Candidates")]
    public static void Validate()
    {
        foreach (var candidate in Candidates)
        {
            Shader shader = Shader.Find(candidate.shader);
            if (shader == null)
                throw new InvalidOperationException(
                    $"GRADOMRAZ shader missing or not imported: {candidate.shader}");
            if (!shader.isSupported)
                Debug.LogWarning($"[GRADOMRAZ] Shader may not be supported on the current graphics API: {candidate.shader}");
            Debug.Log($"[GRADOMRAZ] Shader imported: {candidate.shader}");
        }
        Debug.Log("[GRADOMRAZ] Shader discovery OK. Compile WebGL variants and inspect Shader Inspector errors before production.");
    }

    [MenuItem("Tools/GRADOMRAZ/Visual Lab/2 - Create Candidate Materials")]
    public static void CreateCandidateMaterials()
    {
        Validate();
        EnsureFolder("Assets", "GRADOMRAZ");
        EnsureFolder(Root, "VisualLab");

        for (int i = 0; i < Candidates.Length; i++)
        {
            string target = Lab + "/" + Candidates[i].filename;
            if (AssetDatabase.LoadAssetAtPath<Material>(target) != null)
                continue; // Never overwrite tuning performed by an artist.
            Shader shader = Shader.Find(Candidates[i].shader);
            var material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(target) };
            material.enableInstancing = true;
            if (i == 0)
            {
                material.SetColor("_Tint", new Color(0.64f, 0.75f, 0.82f, 1));
                material.SetFloat("_Opacity", 0.18f);
                material.SetFloat("_DirtStrength", 0.25f);
            }
            else if (i == 1)
            {
                material.SetColor("_Tint", new Color(0.86f, 0.84f, 0.73f, 1));
            }
            else
            {
                material.SetColor("_Tint", new Color(0.53f, 0.58f, 0.60f, 1));
                material.SetFloat("_Dampness", 0.52f);
                material.SetFloat("_WorldTiling", 0.6f);
            }
            AssetDatabase.CreateAsset(material, target);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[GRADOMRAZ] Material candidates created in {Lab}; NO existing scene changed.");
    }

    [MenuItem("Tools/GRADOMRAZ/Visual Lab/3 - Build Isolated Preview Scene")]
    public static void BuildPreviewScene()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Leave Play Mode before building the preview scene.");
        CreateCandidateMaterials();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScene) != null)
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScene));
            Debug.Log("[GRADOMRAZ] Existing preview scene preserved: " + PreviewScene);
            return;
        }

        Scene previous = SceneManager.GetActiveScene();
        if (!previous.IsValid() || string.IsNullOrEmpty(previous.path))
            throw new InvalidOperationException(
                "Save the currently active scene before creating an additive preview. " +
                "Unity cannot open another scene additively over an unsaved Untitled scene.");
        Scene preview = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(preview);
            Material glass = AssetDatabase.LoadAssetAtPath<Material>(Lab + "/Weathered-Glass.mat");
            Material paper = AssetDatabase.LoadAssetAtPath<Material>(Lab + "/Lit-Paper.mat");
            Material concrete = AssetDatabase.LoadAssetAtPath<Material>(Lab + "/Wet-Concrete.mat");

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "WET CONCRETE / Reference floor";
            floor.transform.position = new Vector3(0, -0.2f, 0);
            floor.transform.localScale = new Vector3(13, 0.3f, 8);
            floor.GetComponent<Renderer>().sharedMaterial = concrete;

            Spawn(PrimitiveType.Cube, "01 / Damp industrial concrete", concrete,
                new Vector3(-3, 1.4f, 0), new Vector3(2.2f, 2.8f, 1.1f));
            Spawn(PrimitiveType.Cube, "02 / Thin paper surface", paper,
                new Vector3(0, 1.4f, 0), new Vector3(1.8f, 2.5f, 0.07f));
            Spawn(PrimitiveType.Cube, "03 / Weathered transparent pane", glass,
                new Vector3(3, 1.4f, 0), new Vector3(2, 2.7f, 0.06f));

            GameObject lightGo = new GameObject("Directional Light / preview only");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48, -30, 0);

            GameObject cameraGo = new GameObject("Preview Camera / not a runtime camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.tag = "Untagged";
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 42f;
            cameraGo.transform.position = new Vector3(8, 5, -11);
            cameraGo.transform.LookAt(new Vector3(0, 1.2f, 0));

            EditorSceneManager.MarkSceneDirty(preview);
            if (!EditorSceneManager.SaveScene(preview, PreviewScene))
                throw new InvalidOperationException("Failed to save isolated preview scene.");
            Debug.Log("[GRADOMRAZ] Saved opt-in material preview: " + PreviewScene);
        }
        finally
        {
            EditorSceneManager.CloseScene(preview, true);
            if (previous.IsValid() && previous.isLoaded)
                SceneManager.SetActiveScene(previous);
        }
    }

    private static void Spawn(PrimitiveType primitive, string name, Material material,
        Vector3 position, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string folder = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
