using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tiny set of opt-in, WebGL-only material upgrades for known scene assets.
/// One clone per source material; no texture duplication or source edits.
/// The scan is split over frames to avoid a large scene-start hitch.
/// </summary>
public sealed class GradomrazWebMaterials : MonoBehaviour
{
    private static GradomrazWebMaterials instance;
    private readonly Dictionary<Material, Material> replacements =
        new Dictionary<Material, Material>(8);
    private Coroutine migration;
    private readonly List<Material> sharedMaterialBuffer = new List<Material>(4);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer || instance != null)
            return;
        var go = new GameObject("GRADOMRAZ Web Materials (4 shared clones)");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GradomrazWebMaterials>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    private void Start()
    {
        SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (migration != null)
        {
            StopCoroutine(migration);
            migration = null;
        }
        if (scene.IsValid() && scene.name == "SampleScene")
            migration = StartCoroutine(ApplyOnce(scene));
    }

    private IEnumerator ApplyOnce(Scene scene)
    {
        yield return null;
        if (!scene.isLoaded)
            yield break;

        Shader glass = Shader.Find("GRADOMRAZ/Atmosphere/Weathered Glass");
        Shader wet = Shader.Find("GRADOMRAZ/Environment/Wet Triplanar Concrete");
        Shader paper = Shader.Find("GRADOMRAZ/Props/Lit Paper");
        if (glass == null || wet == null || paper == null)
        {
            Debug.LogWarning("[GRADOMRAZ] Optional Web shader missing; leaving original materials intact.");
            yield break;
        }

        int scanned = 0, changed = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // One bounded renderer traversal array per root, spread over
            // several frames. Four shared material clones at most.
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer == null)
                    continue;
                // Non-allocating Unity list API: no 19k temporary Material[]
                // objects when inspecting the serialized scene.
                renderer.GetSharedMaterials(sharedMaterialBuffer);
                bool replaced = false;
                for (int i = 0; i < sharedMaterialBuffer.Count; i++)
                {
                    Material source = sharedMaterialBuffer[i];
                    if (source == null || !IsCandidate(source.name))
                        continue;
                    if (!replacements.TryGetValue(source, out Material improved))
                    {
                        improved = BuildReplacement(source, glass, wet, paper);
                        replacements.Add(source, improved);
                    }
                    if (improved != null && improved != source)
                    {
                        sharedMaterialBuffer[i] = improved;
                        replaced = true;
                    }
                }
                if (replaced)
                {
                    renderer.SetSharedMaterials(sharedMaterialBuffer);
                    changed++;
                }
                if (++scanned % 400 == 0)
                    yield return null;
            }
        }
        Debug.Log("[GRADOMRAZ] Web visual pass: scanned " + scanned +
                  " renderers, upgraded " + changed +
                  " material usages, made " + replacements.Count +
                  " shared material clones.");
        migration = null;
    }

    private static bool IsCandidate(string name)
    {
        return name == "window" ||
               name == "Road" ||
               name == "Alphalt_Floor" ||
               name == "Newspaper_Diffuse_Normal_Combine_256x256_P";
    }

    private static Material BuildReplacement(
        Material source, Shader glass, Shader wet, Shader paper)
    {
        bool windows = source.name == "window";
        bool signage = source.name == "Newspaper_Diffuse_Normal_Combine_256x256_P";
        var result = new Material(windows ? glass : signage ? paper : wet);
        result.name = "GRADOMRAZ WEB / " + source.name;
        result.enableInstancing = source.enableInstancing;

        Texture tex = null;
        if (source.HasProperty("_BaseMap"))
            tex = source.GetTexture("_BaseMap");
        if (tex == null && source.HasProperty("_MainTex"))
            tex = source.GetTexture("_MainTex");
        if (tex != null)
        {
            result.SetTexture("_BaseMap", tex);
            if (source.HasProperty("_BaseMap"))
            {
                result.SetTextureScale("_BaseMap", source.GetTextureScale("_BaseMap"));
                result.SetTextureOffset("_BaseMap", source.GetTextureOffset("_BaseMap"));
            }
        }

        if (windows)
        {
            result.SetColor("_Tint", new Color(0.68f, 0.78f, 0.84f, 1f));
            result.SetFloat("_Opacity", 0.30f);
            result.SetFloat("_DirtStrength", 0.23f);
            result.SetFloat("_ReflectionStrength", 0.30f);
        }
        else if (signage)
        {
            result.SetColor("_Tint", new Color(0.96f, 0.94f, 0.87f, 1f));
            result.SetFloat("_Cutoff", 0.04f);
            result.SetFloat("_AmbientFloor", 0.33f);
        }
        else
        {
            // Limit the more expensive three-axis sampling to two known
            // asphalt/road material assets, never thousands of buildings.
            result.SetColor("_Tint", new Color(0.83f, 0.87f, 0.90f, 1f));
            result.SetFloat("_WorldTiling", 0.22f);
            result.SetFloat("_Dampness", 0.46f);
            result.SetFloat("_WetSpecular", 0.50f);
        }
        return result;
    }

    private void SceneUnloaded(Scene scene)
    {
        // The persistent host outlives the gameplay scene. Release all four
        // cloned material instances every time the chapter reloads; otherwise
        // each new scene reference leaks another set of material clones.
        if (scene.name != "SampleScene")
            return;
        if (migration != null)
        {
            StopCoroutine(migration);
            migration = null;
        }
        ReleaseMaterialClones();
    }

    private void ReleaseMaterialClones()
    {
        foreach (Material material in replacements.Values)
        {
            if (material != null)
                Destroy(material);
        }
        replacements.Clear();
        sharedMaterialBuffer.Clear();
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneUnloaded -= SceneUnloaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
        ReleaseMaterialClones();

    }
}
