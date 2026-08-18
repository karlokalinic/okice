using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Karlolegend.Gradomraz.Editor
{
    public static class GradomrazBuild
    {
        private const string OutputPath = "Builds/GRADOMRAZ-Windows/GRADOMRAZ.exe";
        private const string WebGlOutputPath = "Builds/WebGL";
        private const string MobileWebGlOutputPath = "Builds/WebGL-Mobile";
        private const string ItchWebGlArchivePath = "Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-itch.zip";
        private const string MobileWebGlArchivePath = "Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-mobile.zip";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string MobilePipelineAssetPath = "Assets/MonoBehaviour/Mobile_RPAsset.asset";

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build Windows 64")]
        public static void BuildWindows64()
        {
            PlayerSettings.allowHDRDisplaySupport = false;
            RecreateDirectory(Path.GetDirectoryName(OutputPath));

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = GetPlayableScenes(),
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            ReportResult("Windows", report, OutputPath);
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build WebGL for itch.io")]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.template = "PROJECT:Itch";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            RecreateDirectory(WebGlOutputPath);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = GetPlayableScenes(),
                locationPathName = WebGlOutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result == BuildResult.Succeeded)
            {
                CreateArchive(WebGlOutputPath, ItchWebGlArchivePath);
            }

            ReportResult("WebGL", report, WebGlOutputPath);
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build Mobile WebGL (iOS Safari + Android Chrome)")]
        public static void BuildMobileWebGL()
        {
            var previousTemplate = PlayerSettings.WebGL.template;
            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            var previousDataCaching = PlayerSettings.WebGL.dataCaching;
            var previousInitialMemory = PlayerSettings.WebGL.initialMemorySize;
            var previousMaximumMemory = PlayerSettings.WebGL.maximumMemorySize;
            var previousGrowthMode = PlayerSettings.WebGL.memoryGrowthMode;
            var previousGeometricGrowthStep = PlayerSettings.WebGL.geometricMemoryGrowthStep;
            var previousGeometricGrowthCap = PlayerSettings.WebGL.memoryGeometricGrowthCap;
            var previousPowerPreference = PlayerSettings.WebGL.powerPreference;
            var previousNameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes;
            var previousDecompressionFallback = PlayerSettings.WebGL.decompressionFallback;
            var previousThreadsSupport = PlayerSettings.WebGL.threadsSupport;
            var previousExceptionSupport = PlayerSettings.WebGL.exceptionSupport;
            var previousDebugSymbols = PlayerSettings.WebGL.debugSymbolMode;
            var previousShowDiagnostics = PlayerSettings.WebGL.showDiagnostics;
            var previousTextureSubtarget = EditorUserBuildSettings.webGLBuildSubtarget;
            var previousCodeOptimization = UnityEditor.WebGL.UserBuildSettings.codeOptimization;
            var previousQualityPipeline = QualitySettings.renderPipeline;
            var previousDefaultPipeline = GraphicsSettings.defaultRenderPipeline;

            try
            {
                var mobilePipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobilePipelineAssetPath);
                if (mobilePipeline == null)
                {
                    throw new InvalidOperationException($"Mobile WebGL build requires {MobilePipelineAssetPath}.");
                }

                // Use an actual lightweight mobile pipeline during the build instead of
                // booting the PC Forward+ renderer and trying to dismantle it after launch.
                // Restored in finally so Windows/desktop assets remain untouched.
                QualitySettings.renderPipeline = mobilePipeline;
                GraphicsSettings.defaultRenderPipeline = mobilePipeline;

                PlayerSettings.WebGL.template = "PROJECT:Mobile";
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.dataCaching = true;

                // Content-heavy scene: avoid repeated visible WASM heap growth while still
                // keeping a strict 1 GB ceiling and modest geometric increments.
                PlayerSettings.WebGL.initialMemorySize = 512;
                PlayerSettings.WebGL.maximumMemorySize = 1024;
                PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
                PlayerSettings.WebGL.geometricMemoryGrowthStep = 0.10f;
                PlayerSettings.WebGL.memoryGeometricGrowthCap = 64;

                PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.LowPower;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.threadsSupport = false;
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
                PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
                PlayerSettings.WebGL.showDiagnostics = false;

                EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
                UnityEditor.WebGL.UserBuildSettings.codeOptimization =
                    UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;

                RecreateDirectory(MobileWebGlOutputPath);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = GetPlayableScenes(),
                    locationPathName = MobileWebGlOutputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                    extraScriptingDefines = new[] { "KARLOLEGEND_MOBILE_WEB" }
                });

                if (report.summary.result == BuildResult.Succeeded)
                {
                    WriteMobileHostingFiles();
                    CreateArchive(MobileWebGlOutputPath, MobileWebGlArchivePath);
                }

                ReportResult("Mobile WebGL", report, MobileWebGlOutputPath);
            }
            finally
            {
                PlayerSettings.WebGL.template = previousTemplate;
                PlayerSettings.WebGL.compressionFormat = previousCompression;
                PlayerSettings.WebGL.dataCaching = previousDataCaching;
                PlayerSettings.WebGL.initialMemorySize = previousInitialMemory;
                PlayerSettings.WebGL.maximumMemorySize = previousMaximumMemory;
                PlayerSettings.WebGL.memoryGrowthMode = previousGrowthMode;
                PlayerSettings.WebGL.geometricMemoryGrowthStep = previousGeometricGrowthStep;
                PlayerSettings.WebGL.memoryGeometricGrowthCap = previousGeometricGrowthCap;
                PlayerSettings.WebGL.powerPreference = previousPowerPreference;
                PlayerSettings.WebGL.nameFilesAsHashes = previousNameFilesAsHashes;
                PlayerSettings.WebGL.decompressionFallback = previousDecompressionFallback;
                PlayerSettings.WebGL.threadsSupport = previousThreadsSupport;
                PlayerSettings.WebGL.exceptionSupport = previousExceptionSupport;
                PlayerSettings.WebGL.debugSymbolMode = previousDebugSymbols;
                PlayerSettings.WebGL.showDiagnostics = previousShowDiagnostics;
                EditorUserBuildSettings.webGLBuildSubtarget = previousTextureSubtarget;
                UnityEditor.WebGL.UserBuildSettings.codeOptimization = previousCodeOptimization;
                QualitySettings.renderPipeline = previousQualityPipeline;
                GraphicsSettings.defaultRenderPipeline = previousDefaultPipeline;
            }
        }

        private static string[] GetPlayableScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("Build failed: no enabled scenes in EditorBuildSettings.");
            }

            if (!string.Equals(scenes[0], BootScenePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Build failed: {BootScenePath} must be the first enabled scene.");
            }

            return scenes;
        }

        private static void RecreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Build output directory is empty.", nameof(path));
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        private static void CreateArchive(string sourceDirectory, string archivePath)
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(sourceDirectory, archivePath, CompressionLevel.Optimal, false);
        }

        private static void WriteMobileHostingFiles()
        {
            var cloudflareHeaders = @"/index.html
  Cache-Control: no-cache, no-store, must-revalidate

/Build/*.js.br
  Content-Encoding: br
  Content-Type: application/javascript
  Cache-Control: public, max-age=31536000, immutable

/Build/*.wasm.br
  Content-Encoding: br
  Content-Type: application/wasm
  Cache-Control: public, max-age=31536000, immutable

/Build/*.data.br
  Content-Encoding: br
  Content-Type: application/octet-stream
  Cache-Control: public, max-age=31536000, immutable

/Build/*
  Cache-Control: public, max-age=31536000, immutable
";

            File.WriteAllText(Path.Combine(MobileWebGlOutputPath, "_headers"), cloudflareHeaders);

            var deploymentNote =
                "GRADOMRAZ mobile WebGL\n" +
                "- Serve over HTTPS.\n" +
                "- Preserve Content-Encoding: br for .br build assets.\n" +
                "- Serve .wasm.br as application/wasm.\n" +
                "- Prefer a top-level page on iOS Safari so IndexedDB caching is available.\n" +
                "- Build files use content hashes, so immutable cache headers are safe.\n";

            File.WriteAllText(Path.Combine(MobileWebGlOutputPath, "DEPLOYMENT.txt"), deploymentNote);
        }

        private static void ReportResult(string platformName, BuildReport report, string output)
        {
            var summary = report.summary;
            Debug.Log($"{platformName} build: {summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, output={output}");

            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
