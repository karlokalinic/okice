#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Strict release preflight. This verifies content, references, importer
/// configuration and scene-level PlayMaker presence; it never claims
/// browser FPS or the behavior of every PlayMaker transition.
/// </summary>
public static class GradomrazWebReleaseValidator
{
    public static void Validate()
    {
        const string release = "Assets/GRADOMRAZ/WebRelease/";
        const string originalBoot = "Assets/Scenes/Boot.unity";
        const string originalGameplay = "Assets/Scenes/SampleScene.unity";
        const string webBoot = release + "Boot.unity";
        const string webMenu = release + "MainMenu.unity";
        const string webClip = release + "GRADOMRAZ_Intro_Web_1080p30.mp4";
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        string Hash(string relative)
        {
            using (var sha = SHA256.Create())
            using (var file = File.OpenRead(Path.Combine(root, relative)))
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }

        if (Application.unityVersion != "6000.6.9f1")
            throw new InvalidOperationException("Unexpected Unity version " + Application.unityVersion);
        if (Hash("Assets/Scenes/MainMenu.unity") != "175734700CDF4FA7DFA674080E40E7D540652659E012E13AEC5F65E498E58FAD" ||
            Hash(originalBoot) != "7BBB4528B2612485CB2FA212DFEC18369765BF0F2513CB8141DDF1CDB09C5C58" ||
            Hash(originalGameplay) != "5C28C64A01E17DCCC1EC8C41E5243AA01D724403EBB57E72C6112A6D9E25A211")
            throw new InvalidOperationException("Canonical scenes changed since baseline.");
        string[] expectedScenePaths = {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/SampleScene.unity"
        };
        var originalBuild = EditorBuildSettings.scenes;
        if (originalBuild.Length != 3 ||
            !originalBuild.Select(s => s.path).SequenceEqual(expectedScenePaths) ||
            originalBuild.Any(s => !s.enabled))
            throw new InvalidOperationException("Original build scene list changed.");

        if (!PlayerSettings.WebGL.progressiveAssetLoading ||
            PlayerSettings.WebGL.nameFilesAsHashes ||
            PlayerSettings.WebGL.compressionFormat != WebGLCompressionFormat.Brotli ||
            PlayerSettings.WebGL.decompressionFallback ||
            !PlayerSettings.WebGL.dataCaching)
            throw new InvalidOperationException("WebGL compression/progressive cache settings invalid.");

        var optimized = AssetDatabase.LoadAssetAtPath<VideoClip>(webClip);
        var original = AssetDatabase.LoadAssetAtPath<VideoClip>(
            "Assets/Sprite/logo_4K_60FPS_MAX.mp4");
        if (optimized == null || original == null)
            throw new InvalidOperationException("Expected intro video assets not imported.");
        var originalVideoBytes = new FileInfo(Path.Combine(root,
            "Assets/Sprite/logo_4K_60FPS_MAX.mp4")).Length;
        var optimizedVideoBytes = new FileInfo(Path.Combine(root, webClip)).Length;
        if (optimizedVideoBytes >= originalVideoBytes * .5)
            throw new InvalidOperationException("Web intro video is not smaller by at least half.");

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            release + "GRADOMRAZ_Web_URP.asset");
        if (pipeline == null || pipeline.msaaSampleCount > 2 ||
            pipeline.renderScale > 0.95f || pipeline.shadowDistance > 35f ||
            !pipeline.supportsHDR)
            throw new InvalidOperationException("Web-specific URP profile invalid.");

        var pipelineSettings = new SerializedObject(pipeline);
        if (pipelineSettings.FindProperty("m_RequireOpaqueTexture").boolValue ||
            !pipelineSettings.FindProperty("m_RequireDepthTexture").boolValue)
            throw new InvalidOperationException(
                "Web profile must skip unused opaque copy and preserve SSAO depth.");

        int streamingTextures = 0, compressedLongAudio = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
        {
            var texImporter = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
            if (texImporter != null && texImporter.streamingMipmaps)
                streamingTextures++;
        }
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioImporter != null && clip != null && clip.length >= 10.0f &&
                audioImporter.defaultSampleSettings.loadType == AudioClipLoadType.CompressedInMemory)
                compressedLongAudio++;
        }
        if (streamingTextures < 126 || compressedLongAudio < 10)
            throw new InvalidOperationException("Memory import results missing: " +
                streamingTextures + " streamed textures, " + compressedLongAudio + " long compressed audio.");
        // Unity 6000.6 may report stale managed runtime settings before
        // the Web pipeline is active. Read and assert the serialized
        // URPShaderStrippingSetting that the player build uses.
        var globalSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
            "Assets/MonoBehaviour/UniversalRenderPipelineGlobalSettings1.asset");
        if (globalSettings == null)
            throw new InvalidOperationException("Web URP global settings missing.");
        var stripping = new SerializedObject(globalSettings);
        var cursor = stripping.GetIterator();
        int postFxFlags = 0;
        while (cursor.NextVisible(true))
        {
            if (cursor.name != "m_StripUnusedPostProcessingVariantsAndResources") continue;
            postFxFlags++;
            if (cursor.boolValue)
                throw new InvalidOperationException("URP serialized settings still strip postprocessing shaders.");
        }
        if (postFxFlags < 2)
            throw new InvalidOperationException("Expected both legacy and Unity 6.6 URP postprocessing flags.");
        string[] shaders = {
            "GRADOMRAZ/Atmosphere/Weathered Glass",
            "GRADOMRAZ/Environment/Wet Triplanar Concrete",
            "GRADOMRAZ/Props/Lit Paper"
        };
        var graphics = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        var list = graphics.FindProperty("m_AlwaysIncludedShaders");
        if (list == null) throw new InvalidOperationException("Graphic settings shader list missing");
        foreach (string name in shaders)
        {
            Shader shader = Shader.Find(name);
            if (shader == null)
                throw new InvalidOperationException("Shader missing: " + name);
            bool present = false;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    present = true;
            if (!present)
                throw new InvalidOperationException("Shader not retained in player: " + name);
        }

        Scene boot = EditorSceneManager.OpenScene(webBoot, OpenSceneMode.Single);
        int bootVideoPlayers = 0;
        foreach (var go in boot.GetRootGameObjects())
        foreach (var player in go.GetComponentsInChildren<VideoPlayer>(true))
        {
            bootVideoPlayers++;
            if (player.clip != optimized)
                throw new InvalidOperationException("Web Boot does not use optimized intro.");
        }
        if (bootVideoPlayers != 1)
            throw new InvalidOperationException("Expected one VideoPlayer on Web Boot.");

        Scene menuScene = EditorSceneManager.OpenScene(webMenu, OpenSceneMode.Single);
        int inputTargets = 0;
        foreach (GameObject menuRoot in menuScene.GetRootGameObjects())
        foreach (Button button in menuRoot.GetComponentsInChildren<Button>(true))
        {
            if (button.name != "PlayButton" && button.name != "OptionsButton" &&
                button.name != "QuitButton")
                continue;
            Image hitArea = button.GetComponent<Image>();
            if (hitArea == null || !hitArea.enabled || !hitArea.raycastTarget ||
                Mathf.Abs(hitArea.color.a - 0.001f) > 0.0001f ||
                hitArea.raycastPadding.x > -130f || hitArea.raycastPadding.y > -15f)
                throw new InvalidOperationException("Web UI button hit area not fixed: " + button.name);
            bool hasClickEvent = false;
            foreach (Component component in button.GetComponents<Component>())
                if (component != null && component.GetType().Name == "PlayMakerUiClickEvent")
                    hasClickEvent = true;
            if (!hasClickEvent)
                throw new InvalidOperationException("PlayMaker UI click bridge missing: " + button.name);
            inputTargets++;
        }
        if (inputTargets != 3)
            throw new InvalidOperationException("Expected exactly three raycastable PlayMaker menu buttons.");        var inputBridges = menuScene.GetRootGameObjects()
            .Select(root => root.GetComponent<GradomrazWebMenuInputBridge>())
            .Where(bridge => bridge != null).ToArray();
        if (inputBridges.Length != 1 || inputBridges[0].gameObject.name != "GRADOMRAZ_WEB_INPUT")
            throw new InvalidOperationException("Missing unique WebGL menu input bridge.");
        var bridgeSettings = new SerializedObject(inputBridges[0]);
        foreach (string property in new[] { "playButton", "optionsButton", "quitButton" })
        {
            var target = bridgeSettings.FindProperty(property);
            if (target == null || target.objectReferenceValue == null)
                throw new InvalidOperationException("Unbound Web menu input bridge field: " + property);
        }
        Scene scene = EditorSceneManager.OpenScene(originalGameplay, OpenSceneMode.Single);
        int objects = 0, renderers = 0, playmaker = 0, missing = 0;
        var stack = new System.Collections.Generic.Stack<Transform>();
        foreach (var go in scene.GetRootGameObjects()) stack.Push(go.transform);
        while (stack.Count > 0)
        {
            Transform tr = stack.Pop();
            objects++;
            foreach (var c in tr.GetComponents<Component>())
            {
                if (c == null) { missing++; continue; }
                if (c is MeshRenderer) renderers++;
                if (c.GetType().Name == "PlayMakerFSM") playmaker++;
            }
            for (int i = 0; i < tr.childCount; i++) stack.Push(tr.GetChild(i));
        }
        if (objects != 22333 || renderers != 19082 || playmaker != 392 || missing != 0)
            throw new InvalidOperationException("PlayMaker/scene baseline changed: " +
                objects + "/" + renderers + "/" + playmaker + "/" + missing);

        var report = new StringBuilder();
        report.AppendLine("GRADOMRAZ RELEASE VALIDATOR PASS");
        report.AppendLine("Unity: " + Application.unityVersion);
        report.AppendLine("Progressive: TRUE; Brotli: TRUE; unversioned assets: revalidate via Worker");
        report.AppendLine("Original source scenes: SHA256 unchanged");
        report.AppendLine("Mipmap-streaming textures: " + streamingTextures +
                          "; long compressed audio clips: " + compressedLongAudio);
        report.AppendLine("Web URP: HDR and depth retained, unused opaque copy disabled");
        report.AppendLine("PlayMaker FSMs: " + playmaker + "; missing components: " + missing);
        report.AppendLine("Web Menu click targets: " + inputTargets);
        report.AppendLine("Original-PlayMaker Web pointer fallback bridge: 1");
        report.AppendLine("Video source bytes: " + originalVideoBytes +
                          "; Web bytes: " + optimizedVideoBytes);
        report.AppendLine("Renderer count: " + renderers + "; GameObjects: " + objects);
        report.AppendLine("URP HDR enabled, reduced shadows, MSAA 2, render scale <= .95");
        report.AppendLine("3 Web-only shaders present; 2 serialized URP post-process stripping flags disabled");
        report.AppendLine("WebGL player build: NOT performed (requires Web Build Support)");
        var dir = Path.Combine(root, "Builds", "GRADOMRAZ-Web6");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "validation-pass.txt"), report.ToString());
        Debug.Log("[GRADOMRAZ-WEB] " + report);
    }
}
#endif
