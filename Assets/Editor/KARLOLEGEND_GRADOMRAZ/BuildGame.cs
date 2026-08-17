using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Central editor-only build entry point for GRADOMRAZ.
    ///
    /// WHY THIS EXISTS:
    /// Instead of manually opening Unity Build Settings, choosing a platform, selecting folders,
    /// remembering WebGL compression settings, and creating an itch.io ZIP by hand, this class turns
    /// those steps into deterministic commands under the Unity menu:
    ///
    ///     KARLOLEGEND / GRADOMRAZ / Build Windows 64
    ///     KARLOLEGEND / GRADOMRAZ / Build WebGL for itch.io
    ///
    /// WINDOWS FLOW:
    ///     validate enabled scenes + Boot scene order
    ///                 ↓
    ///     delete/recreate previous Windows output folder
    ///                 ↓
    ///     Unity BuildPipeline builds StandaloneWindows64
    ///                 ↓
    ///     log result; batch process exits with code 1 on failure
    ///
    /// WEBGL FLOW:
    ///     configure itch template + Brotli + memory
    ///                 ↓
    ///     validate scenes
    ///                 ↓
    ///     delete/recreate Builds/WebGL
    ///                 ↓
    ///     Unity builds WebGL
    ///                 ↓
    ///     only on SUCCESS: zip WebGL folder for itch.io
    ///                 ↓
    ///     log result; batch process exits non-zero on failure
    ///
    /// This class lives under Assets/Editor, so it is tooling executed by the Unity Editor/build machine;
    /// it is not gameplay code shipped as a normal runtime MonoBehaviour.
    /// </summary>
    public static class GradomrazBuild
    {
        /// <summary>
        /// Final Windows executable location relative to project root.
        /// Path.GetDirectoryName(OutputPath) therefore resolves to Builds/GRADOMRAZ-Windows.
        /// </summary>
        private const string OutputPath = "Builds/GRADOMRAZ-Windows/GRADOMRAZ.exe";

        /// <summary>
        /// Directory Unity writes the WebGL player into BEFORE it is packaged for itch.io.
        /// </summary>
        private const string WebGlOutputPath = "Builds/WebGL";

        /// <summary>
        /// Final ZIP produced from Builds/WebGL after a successful WebGL build.
        /// This ZIP is the distribution artifact intended for itch.io upload.
        /// </summary>
        private const string ItchWebGlArchivePath = "Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-itch.zip";

        /// <summary>
        /// Contract for scene ordering: Boot.unity must be scene index 0 among enabled Build Settings scenes.
        /// The build deliberately FAILS if this is not true rather than producing a subtly wrong executable.
        /// </summary>
        private const string BootScenePath = "Assets/Scenes/Boot.unity";

        /// <summary>
        /// Builds the 64-bit Windows version.
        /// MenuItem exposes this static method as a clickable command in the Unity Editor.
        /// The method is synchronous from this tool's point of view: BuildPipeline.BuildPlayer does the build
        /// and returns a BuildReport describing success/failure, warnings, errors, timing, etc.
        /// </summary>
        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build Windows 64")]
        public static void BuildWindows64()
        {
            // Explicit project policy for this output: do not advertise HDR-display support in the Windows player.
            PlayerSettings.allowHDRDisplaySupport = false;

            // Always start from a clean output directory. This prevents stale files from an older build surviving
            // and making a broken/incomplete current build look valid.
            RecreateDirectory(Path.GetDirectoryName(OutputPath));

            // BuildPlayerOptions is the complete recipe handed to Unity's build system.
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                // GetPlayableScenes() is intentionally a validation step, not just a list getter.
                scenes = GetPlayableScenes(),
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            // One reporting path is shared by local Editor builds and headless/CI builds.
            ReportResult("Windows", report, OutputPath);
        }

        /// <summary>
        /// Builds the browser version and, if Unity reports SUCCESS, packages it into an itch.io-ready ZIP.
        /// Nothing is archived after a failed build, which prevents an old/partial WebGL folder from being
        /// mistaken for a valid release artifact.
        /// </summary>
        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build WebGL for itch.io")]
        public static void BuildWebGL()
        {
            // Use the project-specific itch WebGL template rather than Unity's generic default page.
            PlayerSettings.WebGL.template = "PROJECT:Itch";

            // Brotli produces compressed WebGL deployment files. The web host must serve them correctly.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;

            // Allow Unity WebGL to cache downloaded data in the browser between compatible runs.
            PlayerSettings.WebGL.dataCaching = true;

            // WebAssembly heap starts at 512 MB and may grow up to 2048 MB.
            // "Geometric" growth means memory expands in larger proportional steps rather than one fixed increment.
            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            // Clean build: remove the entire previous WebGL directory and create an empty replacement.
            RecreateDirectory(WebGlOutputPath);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = GetPlayableScenes(),
                locationPathName = WebGlOutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            // Packaging is gated by Unity's authoritative BuildResult rather than by "does a folder exist?".
            if (report.summary.result == BuildResult.Succeeded)
            {
                CreateItchArchive();
            }

            ReportResult("WebGL", report, WebGlOutputPath);
        }

        /// <summary>
        /// Returns all ENABLED scenes from Unity Editor Build Settings in their configured order.
        ///
        /// It also enforces two release invariants:
        /// 1. at least one scene must be enabled;
        /// 2. Assets/Scenes/Boot.unity must be first.
        ///
        /// Throwing here stops the build immediately. That is deliberate "fail fast" behaviour: scene-order
        /// configuration is treated as part of program correctness, not as a warning somebody might miss.
        /// </summary>
        private static string[] GetPlayableScenes()
        {
            var scenes = EditorBuildSettings.scenes
                // Disabled Build Settings entries remain visible in Unity but must not enter the player.
                .Where(scene => scene.enabled)
                // BuildPipeline needs file paths, not EditorBuildSettingsScene objects.
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("Build failed: no enabled scenes in EditorBuildSettings.");
            }

            // Scene index 0 controls the initial scene launched by a normal player build.
            if (!string.Equals(scenes[0], BootScenePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Build failed: {BootScenePath} must be the first enabled scene.");
            }

            return scenes;
        }

        /// <summary>
        /// Implements a CLEAN output folder in three steps:
        ///
        ///     validate path
        ///         ↓
        ///     if old directory exists → recursively delete it
        ///         ↓
        ///     create fresh empty directory
        ///
        /// `true` in Directory.Delete(path, true) means recursive deletion, including files/subdirectories.
        /// </summary>
        private static void RecreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // Refuse to pass an empty/meaningless path into destructive filesystem operations.
                throw new ArgumentException("Build output directory is empty.", nameof(path));
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Replaces the previous itch ZIP with a fresh archive containing the current WebGL directory contents.
        /// includeBaseDirectory=false means the ZIP contains the WebGL files themselves at its root instead of
        /// wrapping everything inside an additional `WebGL/` folder.
        /// </summary>
        private static void CreateItchArchive()
        {
            if (File.Exists(ItchWebGlArchivePath))
            {
                File.Delete(ItchWebGlArchivePath);
            }

            ZipFile.CreateFromDirectory(
                WebGlOutputPath,
                ItchWebGlArchivePath,
                System.IO.Compression.CompressionLevel.Optimal,
                false);
        }

        /// <summary>
        /// Final common result handler for both platforms.
        ///
        /// INTERACTIVE EDITOR:
        ///     always logs summary; Unity stays open even if build failed.
        ///
        /// BATCH/CI MODE:
        ///     logs summary; if build failed, EditorApplication.Exit(1) returns a conventional non-zero
        ///     process exit code so GitHub Actions/another supervisor can mark the job as FAILED.
        ///
        /// This distinction is what makes the same build code usable by a human clicking a menu and by automation.
        /// </summary>
        private static void ReportResult(string platformName, BuildReport report, string output)
        {
            var summary = report.summary;

            // Compact machine/human-readable single-line result for the Unity Console or CI log.
            Debug.Log($"{platformName} build: {summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, output={output}");

            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
            {
                // Non-zero process exit status is the key signal external automation understands as failure.
                EditorApplication.Exit(1);
            }
        }
    }
}
