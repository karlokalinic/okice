using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

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
            var previousTextureSubtarget = EditorUserBuildSettings.webGLBuildSubtarget;
            var previousCodeOptimization = UnityEditor.WebGL.UserBuildSettings.codeOptimization;

            try
            {
                // Mobile-web release policy: small download, conservative heap, no development payload.
                PlayerSettings.WebGL.template = "PROJECT:Mobile";
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.dataCaching = true;
                PlayerSettings.WebGL.initialMemorySize = 384;
                PlayerSettings.WebGL.maximumMemorySize = 1024;
                PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

                // ASTC is the preferred mobile texture target for modern iOS/Android GPUs.
                EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

                // Unity recommends Disk Size with LTO for shipping mobile-web builds.
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
                    CreateArchive(MobileWebGlOutputPath, MobileWebGlArchivePath);
                }

                ReportResult("Mobile WebGL", report, MobileWebGlOutputPath);
            }
            finally
            {
                // A mobile build must never silently mutate the desktop WebGL configuration.
                PlayerSettings.WebGL.template = previousTemplate;
                PlayerSettings.WebGL.compressionFormat = previousCompression;
                PlayerSettings.WebGL.dataCaching = previousDataCaching;
                PlayerSettings.WebGL.initialMemorySize = previousInitialMemory;
                PlayerSettings.WebGL.maximumMemorySize = previousMaximumMemory;
                PlayerSettings.WebGL.memoryGrowthMode = previousGrowthMode;
                EditorUserBuildSettings.webGLBuildSubtarget = previousTextureSubtarget;
                UnityEditor.WebGL.UserBuildSettings.codeOptimization = previousCodeOptimization;
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
