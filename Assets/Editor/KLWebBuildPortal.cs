#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class KLWebBuildPortal : EditorWindow
{
    private const string RootKey = "KARLOLEGEND.WebPortal.Root";
    private const string SlotKey = "KARLOLEGEND.WebPortal.Slot";
    private const int InitialMemoryMiB = 256;
    private const int MaximumMemoryMiB = 2048;

    private enum GameSlot { Game01, Game02 }

    private string portalRoot = "";
    private GameSlot slot = GameSlot.Game01;

    [MenuItem("Tools/KARLOLEGEND/Web Build Portal")]
    private static void Open() => GetWindow<KLWebBuildPortal>("Web Build Portal");

    private void OnEnable()
    {
        portalRoot = EditorPrefs.GetString(RootKey, FindDefaultPortalRoot());
        slot = (GameSlot)EditorPrefs.GetInt(SlotKey, 0);
        minSize = new Vector2(620, 360);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("KARLOLEGEND // UNITY -> WEB PORTAL", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Builds this Unity project directly into the local portal staging slot. " +
            "The portal itself stays small; the upload script sends this folder to Cloudflare R2.",
            MessageType.Info);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Portal project root");
        EditorGUILayout.BeginHorizontal();
        portalRoot = EditorGUILayout.TextField(portalRoot);
        if (GUILayout.Button("Choose...", GUILayout.Width(90)))
        {
            var picked = EditorUtility.OpenFolderPanel("Choose KARLO_COLD_CITY_WEBGL_PORTAL", portalRoot, "");
            if (!string.IsNullOrWhiteSpace(picked)) portalRoot = picked;
        }
        EditorGUILayout.EndHorizontal();

        slot = (GameSlot)EditorGUILayout.EnumPopup("Target slot", slot);
        string gameId = slot == GameSlot.Game01 ? "game-01" : "game-02";
        string output = string.IsNullOrWhiteSpace(portalRoot)
            ? "<choose portal root>"
            : Path.Combine(portalRoot, "_unity-builds", gameId);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Exact output", output, EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("APPLY WEB RELEASE SETTINGS", GUILayout.Height(38)))
            ApplyReleaseSettings();
        GUI.enabled = IsPortalRootValid();
        if (GUILayout.Button("BUILD TO PORTAL", GUILayout.Height(38)))
            Build(gameId, output);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            $"Release preset: Brotli, native decompression, hashed build files, Data Caching, {InitialMemoryMiB} MiB initial memory " +
            $"with geometric growth to {MaximumMemoryMiB} MiB, size-optimized IL2CPP, low managed stripping, no debug symbols, and no Web threads. " +
            "The authored quality, render pipeline, scenes, and input settings are preserved.",
            MessageType.None);
    }

    private static string FindDefaultPortalRoot()
    {
        string configured = Environment.GetEnvironmentVariable("KARLO_WEB_PORTAL_ROOT");
        if (IsPortalRootValid(configured)) return configured;

        string downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "KARLO_COLD_CITY_WEBGL_PORTAL",
            "KARLO_COLD_CITY_WEBGL_PORTAL");
        return IsPortalRootValid(downloads) ? downloads : "";
    }

    private static bool IsPortalRootValid(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && Directory.Exists(path)
            && File.Exists(Path.Combine(path, "wrangler.jsonc"));
    }

    private bool IsPortalRootValid() => IsPortalRootValid(portalRoot);

    private void Persist()
    {
        EditorPrefs.SetString(RootKey, portalRoot ?? "");
        EditorPrefs.SetInt(SlotKey, (int)slot);
    }

    [MenuItem("Tools/KARLOLEGEND/Apply Optimized Web Release Settings")]
    public static void ApplyOptimizedWebReleaseSettings()
    {
        EnsureWebBuildProfile();

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.analyzeBuildSize = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.initialMemorySize = InitialMemoryMiB;
        PlayerSettings.WebGL.maximumMemorySize = MaximumMemoryMiB;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.geometricMemoryGrowthStep = 0.2f;
        PlayerSettings.WebGL.memoryGeometricGrowthCap = 96;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.HighPerformance;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.showDiagnostics = false;
        PlayerSettings.WebGL.useEmbeddedResources = false;

        PlayerSettings.mipStripping = true;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;

        string templateDir = Path.Combine(Application.dataPath, "WebGLTemplates", "ColdCity");
        if (Directory.Exists(templateDir))
            PlayerSettings.WebGL.template = "PROJECT:ColdCity";

        AssetDatabase.SaveAssets();
        Debug.Log(
            $"[KARLOLEGEND] Optimized Web release settings applied: Brotli, {InitialMemoryMiB}-{MaximumMemoryMiB} MiB geometric memory, " +
            "DXT, release IL2CPP optimized for size, low managed stripping, no symbols, no threads.");
    }

    private void ApplyReleaseSettings()
    {
        Persist();
        ApplyOptimizedWebReleaseSettings();
    }

    [MenuItem("Tools/KARLOLEGEND/Build Game 01 To Portal %#b")]
    public static void BuildGame01ToDefaultPortal()
    {
        string root = FindDefaultPortalRoot();
        if (!IsPortalRootValid(root))
        {
            FailBuild("The KARLO Web portal folder could not be found.");
            return;
        }

        EditorPrefs.SetString(RootKey, root);
        EditorPrefs.SetInt(SlotKey, (int)GameSlot.Game01);
        BuildToPortal("game-01", Path.Combine(root, "_unity-builds", "game-01"));
    }

    private static void EnsureWebBuildProfile()
    {
        Type contextType = typeof(BuildProfile).Assembly.GetType("UnityEditor.Build.Profile.BuildProfileContext");
        Type moduleType = typeof(BuildProfile).Assembly.GetType("UnityEditor.Build.Profile.BuildProfileModuleUtil");
        if (contextType == null || moduleType == null)
        {
            Debug.LogWarning("[KARLOLEGEND] Unity Build Profile API was unavailable; continuing with global Web settings.");
            return;
        }

        const BindingFlags allStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        const BindingFlags allInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        PropertyInfo instanceProperty = contextType.GetProperty("instance", allStatic);
        MethodInfo getPlatformId = moduleType.GetMethod("GetPlatformId", allStatic);
        MethodInfo getOrCreate = contextType.GetMethod("GetOrCreateClassicPlatformBuildProfile", allInstance);
        if (instanceProperty == null || getPlatformId == null || getOrCreate == null)
        {
            Debug.LogWarning("[KARLOLEGEND] Unity Build Profile factory was unavailable; continuing with global Web settings.");
            return;
        }

        object context = instanceProperty.GetValue(null);
        object platformId = getPlatformId.Invoke(null, new object[] { BuildTarget.WebGL, StandaloneBuildSubtarget.Default });
        BuildProfile profile = getOrCreate.Invoke(context, new[] { platformId }) as BuildProfile;
        if (profile != null && BuildProfile.GetActiveBuildProfile() != profile)
            BuildProfile.SetActiveBuildProfile(profile);
    }

    private void Build(string gameId, string output)
    {
        Persist();
        BuildToPortal(gameId, output);
    }

    private static void BuildToPortal(string gameId, string output)
    {
        ApplyOptimizedWebReleaseSettings();

        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            FailBuild("Unity Web Build Support is not installed for this editor.");
            return;
        }

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            FailBuild("No enabled scenes exist in Build Profiles / build scene list.");
            return;
        }

        string missingScene = scenes.FirstOrDefault(path => !File.Exists(path));
        if (!string.IsNullOrEmpty(missingScene))
        {
            FailBuild($"An enabled build scene is missing:\n{missingScene}");
            return;
        }

        Directory.CreateDirectory(output);
        foreach (string item in Directory.EnumerateFileSystemEntries(output))
        {
            if (Directory.Exists(item)) Directory.Delete(item, true);
            else File.Delete(item);
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        Debug.Log($"[KARLOLEGEND] Building {PlayerSettings.productName} -> {output}");
        bool previousStripEngineCode = PlayerSettings.stripEngineCode;
        BuildReport report;
        try
        {
            PlayerSettings.stripEngineCode = true;
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            PlayerSettings.stripEngineCode = previousStripEngineCode;
            AssetDatabase.SaveAssets();
        }

        if (report.summary.result == BuildResult.Succeeded)
        {
            ulong bytes = report.summary.totalSize;
            Debug.Log($"[KARLOLEGEND] WEB BUILD OK // {gameId} // {bytes / (1024f * 1024f):F1} MiB // {output}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Web Build complete",
                    $"Built {gameId} successfully.\n\n{output}\n\nNext: run the portal upload script for {gameId}.",
                    "Reveal folder");
                EditorUtility.RevealInFinder(output);
            }
        }
        else
        {
            Debug.LogError($"[KARLOLEGEND] WEB BUILD FAILED // {report.summary.result}");
            FailBuild(report.summary.result.ToString());
        }
    }

    private static void FailBuild(string message)
    {
        if (Application.isBatchMode)
            throw new BuildFailedException(message);

        EditorUtility.DisplayDialog("Web Build failed", message, "OK");
    }
}
#endif