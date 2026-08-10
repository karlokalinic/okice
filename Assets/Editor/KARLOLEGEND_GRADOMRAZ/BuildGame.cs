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
        private const string ItchWebGlArchivePath = "Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-itch.zip";
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
                CreateItchArchive();
            }

            ReportResult("WebGL", report, WebGlOutputPath);
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
