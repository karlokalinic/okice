#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Author an isolated, reproducible Unity 6.6 Web build. The canonical Boot
/// and SampleScene stay byte-for-byte untouched, as do PlayMaker FSMs.
/// </summary>
public static class GradomrazWebReleaseBuilder
{
    private const string OriginalBoot = "Assets/Scenes/Boot.unity";
    private const string Gameplay = "Assets/Scenes/SampleScene.unity";
    private const string Menu = "Assets/Scenes/MainMenu.unity";
    private const string WebAssets = "Assets/GRADOMRAZ/WebRelease";
    private const string WebBoot = WebAssets + "/Boot.unity";
    private const string WebMenu = WebAssets + "/MainMenu.unity";
    private const string WebIntro = WebAssets + "/GRADOMRAZ_Intro_Web_1080p30.mp4";
    private const string OriginalPipeline = "Assets/MonoBehaviour/PC_RPAsset.asset";
    private const string WebPipeline = WebAssets + "/GRADOMRAZ_Web_URP.asset";
    private const string OutputFolder = "Builds/GRADOMRAZ-Web6";
    private static readonly string[] ReleaseScenes = { WebBoot, WebMenu, Gameplay };

    [MenuItem("Tools/GRADOMRAZ/Web Release/1 - Stage progressive Web release")]
    public static void Stage()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Leave Play Mode before preparing the Web build.");

        var sourceBoot = AssetDatabase.LoadAssetAtPath<SceneAsset>(OriginalBoot);
        var gameplay = AssetDatabase.LoadAssetAtPath<SceneAsset>(Gameplay);
        var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(Menu);
        var webClip = AssetDatabase.LoadAssetAtPath<VideoClip>(WebIntro);
        var pipelineSource = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(OriginalPipeline);
        if (sourceBoot == null || gameplay == null || menu == null ||
            webClip == null || pipelineSource == null)
            throw new InvalidOperationException(
                "Release source missing. Need all three original scenes, the optimized " +
                "intro video and the original URP pipeline.");

        string basePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string menuHashBefore = Checksum(Path.Combine(basePath, Menu));
        string bootHashBefore = Checksum(Path.Combine(basePath, OriginalBoot));
        string gameplayHashBefore = Checksum(Path.Combine(basePath, Gameplay));

        if (!AssetDatabase.IsValidFolder(WebAssets))
        {
            if (!AssetDatabase.IsValidFolder("Assets/GRADOMRAZ"))
                throw new InvalidOperationException("GRADOMRAZ asset root is missing.");
            AssetDatabase.CreateFolder("Assets/GRADOMRAZ", "WebRelease");
        }

        var setup = EditorSceneManager.GetSceneManagerSetup();
        foreach (Scene openScene in Enumerable.Range(0, SceneManager.sceneCount)
                     .Select(SceneManager.GetSceneAt))
        {
            if (openScene.isDirty)
                throw new InvalidOperationException(
                    "Save or revert unsaved open scenes before staging the release.");
        }

        try
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(WebBoot))
            {
                if (!AssetDatabase.CopyAsset(OriginalBoot, WebBoot))
                    throw new IOException("Could not clone Boot scene.");
            }

            // Keep the actual scene name "Boot"; scene-index and name-based
            // transition scripts should behave as in the original build.
            var boot = EditorSceneManager.OpenScene(WebBoot, OpenSceneMode.Single);
            VideoPlayer video = null;
            int count = 0;
            foreach (GameObject root in boot.GetRootGameObjects())
            {
                foreach (VideoPlayer candidate in root.GetComponentsInChildren<VideoPlayer>(true))
                {
                    video = candidate;
                    count++;
                }
            }
            if (count != 1 || video == null)
                throw new InvalidOperationException(
                    "Unexpected Boot hierarchy: exactly one VideoPlayer required.");
            if (video.clip == null ||
                (video.clip.name != "logo_4K_60FPS_MAX" &&
                 video.clip.name != "GRADOMRAZ_Intro_Web_1080p30"))
                throw new InvalidOperationException(
                    "Boot VideoPlayer changed; refusing to substitute an unrelated clip.");

            video.clip = webClip; // only on the cloned Boot scene.
            EditorSceneManager.MarkSceneDirty(boot);
            if (!EditorSceneManager.SaveScene(boot, WebBoot))
                throw new IOException("Could not save optimized Boot clone.");

            // The source MainMenu disables the Image components on three
            // Button objects. A disabled Graphic cannot receive raycasts.
            // Preserve the source scene and all PlayMaker FSMs; make a Web
            // clone with fully transparent yet raycastable button targets.
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(WebMenu))
            {
                if (!AssetDatabase.CopyAsset(Menu, WebMenu))
                    throw new IOException("Could not clone MainMenu scene.");
            }
            var webMenu = EditorSceneManager.OpenScene(WebMenu, OpenSceneMode.Single);
            int repairedButtons = 0;
            Button play = null, options = null, quit = null;
            foreach (GameObject root in webMenu.GetRootGameObjects())
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    if (button.name != "PlayButton" && button.name != "OptionsButton" &&
                        button.name != "QuitButton")
                        continue;
                    Image image = button.GetComponent<Image>();
                    if (image == null)
                        throw new InvalidOperationException("Missing image for " + button.name);
                    image.enabled = true;
                    image.raycastTarget = true;
                    image.raycastPadding = new Vector4(-135f, -20f, -135f, -20f);
                    Color transparent = image.color;
                    transparent.a = 0.001f; // visually invisible, nonzero to avoid CanvasRenderer culling
                    image.color = transparent;
                    repairedButtons++;
                    if (button.name == "PlayButton") play = button;
                    else if (button.name == "OptionsButton") options = button;
                    else if (button.name == "QuitButton") quit = button;
                }
            }
            if (repairedButtons != 3)
                throw new InvalidOperationException("Expected exactly 3 menu buttons, got " + repairedButtons);
            // A browser pointer listener forwards missed Unity UI clicks to
            // these SAME Button.onClick events; PlayMaker transitions remain
            // unchanged and no scene is loaded directly by the bridge.
            GameObject bridgeRoot = webMenu.GetRootGameObjects()
                .FirstOrDefault(root => root.name == "GRADOMRAZ_WEB_INPUT");
            if (bridgeRoot == null)
            {
                SceneManager.SetActiveScene(webMenu);
                bridgeRoot = new GameObject("GRADOMRAZ_WEB_INPUT");
            }
            var bridge = bridgeRoot.GetComponent<GradomrazWebMenuInputBridge>();
            if (bridge == null)
                bridge = bridgeRoot.AddComponent<GradomrazWebMenuInputBridge>();
            bridge.Configure(play, options, quit);
            EditorSceneManager.MarkSceneDirty(webMenu);
            if (!EditorSceneManager.SaveScene(webMenu, WebMenu))
                throw new IOException("Could not save Web MainMenu clone.");
        }
        finally
        {
            if (!Application.isBatchMode && setup != null && setup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        UniversalRenderPipelineAsset webPipeline =
            AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(WebPipeline);
        if (webPipeline == null)
        {
            webPipeline = UnityEngine.Object.Instantiate(pipelineSource);
            webPipeline.name = "GRADOMRAZ Web — cinematic adaptive quality";
            AssetDatabase.CreateAsset(webPipeline, WebPipeline);
        }

        var so = new SerializedObject(webPipeline);
        SetInt(so, "m_MSAA", 2); // halve MSAA samples; retain spatial edge quality.
        SetFloat(so, "m_RenderScale", 0.94f); // dynamic governor 0.72–0.94.
        SetInt(so, "m_MainLightShadowmapResolution", 2048);
        SetInt(so, "m_AdditionalLightsShadowmapResolution", 1024);
        SetInt(so, "m_AdditionalLightsPerObjectLimit", 3);
        SetFloat(so, "m_ShadowDistance", 32f);
        SetInt(so, "m_ShadowCascadeCount", 2);
        SetBool(so, "m_SupportsHDR", true);
        // 24 inspected authored shaders do not sample _CameraOpaqueTexture.
        // Keeping the separate opaque-copy buffer would waste browser GPU RAM.
        SetBool(so, "m_RequireOpaqueTexture", false);
        SetBool(so, "m_RequireDepthTexture", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(webPipeline);

        PlayerSettings.WebGL.progressiveAssetLoading = true;
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.threadsSupport = false; // no COOP/COEP dependency.
        PlayerSettings.WebGL.initialMemorySize = 256;
        PlayerSettings.WebGL.maximumMemorySize = 2048;

        IncludeWebShaders();
        PreserveWebPostProcessingVariants();
        AssetDatabase.SaveAssets();
        string menuHashAfter = Checksum(Path.Combine(basePath, Menu));
        string bootHashAfter = Checksum(Path.Combine(basePath, OriginalBoot));
        string gameplayHashAfter = Checksum(Path.Combine(basePath, Gameplay));
        if (bootHashBefore != bootHashAfter || menuHashBefore != menuHashAfter || gameplayHashBefore != gameplayHashAfter)
            throw new InvalidOperationException(
                "Canonical scene unexpectedly modified. Do not deploy this release.");

        string staged = Path.Combine(basePath, WebBoot);
        string videoPath = Path.Combine(basePath, WebIntro);
        var report = new StringBuilder();
        report.AppendLine("Unity editor: " + Application.unityVersion);
        report.AppendLine("Canonical Menu SHA256 unchanged: " + menuHashAfter);
        report.AppendLine("Transparent Web menu raycast hit areas repaired: 3");
        report.AppendLine("Canonical Boot SHA256 unchanged: " + bootHashAfter);
        report.AppendLine("Canonical gameplay SHA256 unchanged: " + gameplayHashAfter);
        report.AppendLine("Web Boot scene bytes: " + new FileInfo(staged).Length);
        report.AppendLine("Web video bytes: " + new FileInfo(videoPath).Length);
        report.AppendLine("Progressive asset loading: " + PlayerSettings.WebGL.progressiveAssetLoading);
        report.AppendLine("Name files as hashes: " + PlayerSettings.WebGL.nameFilesAsHashes);
        report.AppendLine("Compression: " + PlayerSettings.WebGL.compressionFormat);
        report.AppendLine("Web URP asset: " + WebPipeline);
        report.AppendLine("Build scene paths: " + string.Join(", ", ReleaseScenes));
        string reportFolder = Path.Combine(basePath, OutputFolder);
        Directory.CreateDirectory(reportFolder);
        File.WriteAllText(Path.Combine(reportFolder, "staging-report.txt"), report.ToString());
        Debug.Log("[GRADOMRAZ-WEB] STAGE COMPLETE\n" + report);
    }

    [MenuItem("Tools/GRADOMRAZ/Web Release/2 - Build production WebGL")]
    public static void BuildRelease()
    {
        GradomrazWebImporterTuner.TuneMemoryImporters();
        Stage();
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            throw new InvalidOperationException(
                "WebGL Build Support is not fully installed. Do not publish an " +
                "incomplete player. Install Unity 6000.6.9f1 Web Build Support first.");

        var webPipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(WebPipeline);
        if (webPipeline == null)
            throw new InvalidOperationException("Staged Web pipeline missing.");

        var currentDefaultPipeline = GraphicsSettings.defaultRenderPipeline;
        var currentQualityPipeline = QualitySettings.renderPipeline;
        float currentMipmapBudget = QualitySettings.streamingMipmapsMemoryBudget;
        float currentLodBias = QualitySettings.lodBias;
        bool currentMipmapStreaming = QualitySettings.streamingMipmapsActive;

        try
        {
            GraphicsSettings.defaultRenderPipeline = webPipeline;
            QualitySettings.renderPipeline = webPipeline;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 256f;
            QualitySettings.lodBias = 2f;

            string destination = Path.Combine(
                Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                OutputFolder);
            Directory.CreateDirectory(destination);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = ReleaseScenes,
                locationPathName = destination,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            if (report == null || report.summary.result != BuildResult.Succeeded ||
                !File.Exists(Path.Combine(destination, "index.html")))
                throw new InvalidOperationException(
                    "Web release failed; check Unity Editor log and BuildReport.");
            File.WriteAllText(
                Path.Combine(destination, "build-success.txt"),
                "GRADOMRAZ Unity 6000.6.9f1 Web release\n" +
                "Result: " + report.summary.result + "\n" +
                "Output size (bytes): " + report.summary.totalSize + "\n" +
                "Scenes: " + string.Join(", ", ReleaseScenes) + "\n" +
                "Progressive: " + PlayerSettings.WebGL.progressiveAssetLoading + "\n");
            Debug.Log("[GRADOMRAZ-WEB] BUILD PASS: " + destination);
        }
        finally
        {
            GraphicsSettings.defaultRenderPipeline = currentDefaultPipeline;
            QualitySettings.renderPipeline = currentQualityPipeline;
            QualitySettings.streamingMipmapsActive = currentMipmapStreaming;
            QualitySettings.streamingMipmapsMemoryBudget = currentMipmapBudget;
            QualitySettings.lodBias = currentLodBias;
        }
    }

    private static void PreserveWebPostProcessingVariants()
    {
        // Unity 6.6 migrated URP stripping flags into a SerializeReference
        // URPShaderStrippingSetting. Its default '1' strips EVERY GLES3 Bloom
        // variant when the Volume is created at runtime, despite a shader
        // being on GraphicsSettings.alwaysIncludedShaders.
        var path = "Assets/MonoBehaviour/UniversalRenderPipelineGlobalSettings1.asset";
        var global = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (global == null)
            throw new InvalidOperationException("Missing URP global settings: " + path);

        var settings = new SerializedObject(global);
        var property = settings.GetIterator();
        int found = 0;
        while (property.NextVisible(true))
        {
            if (property.name == "m_StripUnusedPostProcessingVariantsAndResources")
            {
                property.boolValue = false;
                found++;
            }
        }
        if (found == 0)
            throw new InvalidOperationException("Cannot find Unity 6.6 URP postprocessing stripper flag.");
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(global);

        // The GraphicsSettings service caches the managed reference separately
        // from SerializedObject; update both before ShaderBuildPreprocessor runs.
        if (GraphicsSettings.TryGetRenderPipelineSettings<URPShaderStrippingSetting>(out var liveSettings))
            liveSettings.stripUnusedPostProcessingVariants = false;

        // The editor may still report the previous live settings instance
        // until the Web renderer becomes active. The serialized URP settings
        // and the compiled GLES3 shader variant counts are the source of
        // truth for this release, not the inactive pipeline's in-memory cache.
        Debug.Log("[GRADOMRAZ-WEB] Serialized URP postprocessing stripping disabled; verify GLES3 Bloom variants in BuildReport.");

    }
    private static void IncludeWebShaders()
    {
        // This is the 6000.6 public editor API. GraphicsSettings has no
        // assignable alwaysIncludedShaders property in this Unity version.
        var settings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        var shaders = settings.FindProperty("m_AlwaysIncludedShaders");
        if (shaders == null || !shaders.isArray)
            throw new InvalidOperationException("Cannot find GraphicsSettings always-included shader array.");
        string[] names = {
            "GRADOMRAZ/Atmosphere/Weathered Glass",
            "GRADOMRAZ/Environment/Wet Triplanar Concrete",
            "GRADOMRAZ/Props/Lit Paper",
            // Runtime-created Volume overrides are invisible to the WebGL
            // shader stripper; retain only the post effects that URP
            // creates for cinematic grade and its built-in fallback passes.
            "Hidden/Universal Render Pipeline/Bloom",
            "Hidden/Universal Render Pipeline/UberPost",
            "Hidden/Universal Render Pipeline/FinalPost",
            "Hidden/Universal Render Pipeline/LutBuilderHdr",
            "Hidden/Universal Render Pipeline/Edge Adaptive Spatial Upsampling",
            "Hidden/Universal Render Pipeline/GaussianDepthOfField",
            "Hidden/Universal Render Pipeline/BokehDepthOfField",
            "Hidden/Universal Render Pipeline/PaniniProjection"
        };
        foreach (string name in names)
        {
            Shader shader = Shader.Find(name);
            if (shader == null)
                throw new InvalidOperationException("Required GRADOMRAZ shader not imported: " + name);
            bool present = false;
            for (int i = 0; i < shaders.arraySize; i++)
            {
                if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    present = true;
                    break;
                }
            }
            if (!present)
            {
                var index = shaders.arraySize;
                shaders.InsertArrayElementAtIndex(index);
                shaders.GetArrayElementAtIndex(index).objectReferenceValue = shader;
            }
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
    private static string Checksum(string file)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(file))
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
    }

    private static void SetInt(SerializedObject so, string field, int value)
    {
        var property = so.FindProperty(field);
        if (property == null)
            throw new InvalidOperationException("URP property unavailable in 17.6: " + field);
        property.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string field, float value)
    {
        var property = so.FindProperty(field);
        if (property == null)
            throw new InvalidOperationException("URP property unavailable in 17.6: " + field);
        property.floatValue = value;
    }

    private static void SetBool(SerializedObject so, string field, bool value)
    {
        var property = so.FindProperty(field);
        if (property == null)
            throw new InvalidOperationException("URP property unavailable in 17.6: " + field);
        property.boolValue = value;
    }
}
#endif
