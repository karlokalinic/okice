#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds isolated visual-only additive Unity scenes as remotely hosted WebGL
/// AssetBundles. The original gameplay scene is NOT modified by this tool.
/// Staging output is deliberately outside Assets and outside Git.
/// </summary>
public static class GradomrazVisualBundleBuilder
{
    private const string SourceFolder = "Assets/GRADOMRAZ/StreamingScenes";
    private const string StageFolder = "_GRADOMRAZ_Streaming_Staging";

    [MenuItem("Tools/GRADOMRAZ/Streaming/Build Visual-Only WebGL Bundles")]
    public static void BuildVisualBundles()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Leave Play Mode before building visual bundles.");
        if (!AssetDatabase.IsValidFolder(SourceFolder))
        {
            Debug.LogWarning("[GRADOMRAZ] No visual-only streaming scenes. Create scenes under " +
                SourceFolder + " after checking Tools/gradomraz-scene-reference-audit.mjs.");
            return;
        }
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            throw new InvalidOperationException("Install Unity WebGL Build Support first.");

        var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { SourceFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.Ordinal).ToArray();
        if (scenePaths.Length == 0)
        {
            Debug.LogWarning("[GRADOMRAZ] No .unity scenes found under " + SourceFolder);
            return;
        }

        var inPlayer = new HashSet<string>(
            EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path),
            StringComparer.OrdinalIgnoreCase);
        foreach (string scenePath in scenePaths)
        {
            if (inPlayer.Contains(scenePath))
                throw new InvalidOperationException("Streaming scene is still in main WebGL build: " +
                    scenePath + ". This would duplicate payload in initial .data.");
            VerifyOnlyStandaloneVisuals(scenePath);
        }

        string stage = Path.GetFullPath(Path.Combine(Application.dataPath, "..", StageFolder));
        Directory.CreateDirectory(stage);
        var builds = scenePaths.Select(p => new AssetBundleBuild
        {
            assetBundleName = "gradomraz-" +
                Path.GetFileNameWithoutExtension(p).ToLowerInvariant().Replace(' ', '-') + ".bundle",
            assetNames = new[] { p }
        }).ToArray();

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            stage, builds, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
        if (manifest == null)
            throw new InvalidOperationException("WebGL AssetBundle build failed; see Unity Console.");

        var output = new List<string>();
        foreach (var build in builds)
        {
            string filePath = Path.Combine(stage, build.assetBundleName);
            byte[] bytes = File.ReadAllBytes(filePath);
            string hash;
            using (var sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", "").ToLowerInvariant().Substring(0, 16);
            string publishedName = Path.GetFileNameWithoutExtension(build.assetBundleName) +
                "." + hash + ".bundle";
            string stagedPath = Path.Combine(stage, publishedName);
            File.Copy(filePath, stagedPath, true);
            output.Add(publishedName + "\t" + bytes.Length + "\t" + build.assetNames[0] +
                "\t/unity/game-01/Streaming/" + publishedName);
        }

        string index = Path.Combine(stage, "GRADOMRAZ-Streaming-Upload-Manifest.tsv");
        File.WriteAllLines(index, output);
        Debug.Log("[GRADOMRAZ] Created " + output.Count + " LZ4/chunked visual-only scene bundles in " +
            stage + ". Original SampleScene / PlayMaker / build scene list unchanged. " +
            "Upload only the hashed *.bundle files after full WebGL QA. Manifest: " + index);
    }

    private static void VerifyOnlyStandaloneVisuals(string path)
    {
        Scene existing = SceneManager.GetSceneByPath(path);
        bool openedHere = !existing.IsValid() || !existing.isLoaded;
        Scene scene = openedHere
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)
            : existing;
        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform tr in root.GetComponentsInChildren<Transform>(true))
                {
                    foreach (Component component in tr.GetComponents<Component>())
                    {
                        if (component == null)
                            throw new InvalidOperationException("Missing script/component in streaming scene " + path);
                        if (component is Transform || component is MeshFilter ||
                            component is MeshRenderer)
                            continue;
                        throw new InvalidOperationException(
                            "Streaming scenes may contain only Transform/MeshFilter/MeshRenderer. " +
                            "Move gameplay, PlayMaker, colliders, animation, cameras and lighting " +
                            "back to the main scene. Invalid component: " +
                            component.GetType().Name + " at " + path + "/" + tr.name);
                    }
                }
            }
        }
        finally
        {
            if (openedHere && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }
}
#endif
