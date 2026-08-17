using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Karlolegend.Gradomraz.Editor
{
    public static class GradomrazBuild
    {
        private const string OutputPath = "Builds/GRADOMRAZ-Windows/GRADOMRAZ.exe";
        private const string WindowsArchivePath = "Builds/GRADOMRAZ-Windows.zip";
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

            if (report.summary.result == BuildResult.Succeeded)
            {
                WriteManifest(Path.GetDirectoryName(OutputPath), "windows", report);
                if (File.Exists(WindowsArchivePath)) File.Delete(WindowsArchivePath);
                ZipFile.CreateFromDirectory(Path.GetDirectoryName(OutputPath), WindowsArchivePath, CompressionLevel.Optimal, false);
            }

            ReportResult("Windows", report, OutputPath);
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build WebGL for itch.io")]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.template = "PROJECT:Itch";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.threadsSupport = false;

            // Canonical build also serves phones. Reserve substantially less memory at startup than
            // the old 512 MB build, then grow in bounded geometric steps instead of betting that
            // WebKit can provide a huge contiguous block up front.
            PlayerSettings.WebGL.initialMemorySize = 256;
            PlayerSettings.WebGL.maximumMemorySize = 1024;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.geometricMemoryGrowthStep = 0.15f;
            PlayerSettings.WebGL.memoryGeometricGrowthCap = 32;
            PlayerSettings.WebGL.showDiagnostics = false;

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
                WriteManifest(WebGlOutputPath, "webgl", report);
                CreateItchArchive();
            }

            ReportResult("WebGL", report, WebGlOutputPath);
        }

        public static void ValidateProductionSource()
        {
            var scenes = GetPlayableScenes();
            if (scenes.Length < 1) throw new InvalidOperationException("GRADOMRAZ: no playable scenes.");
            Debug.Log("GRADOMRAZ PRODUCTION SOURCE VALIDATION: PASS");
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

            ZipFile.CreateFromDirectory(WebGlOutputPath, ItchWebGlArchivePath, CompressionLevel.Optimal, false);
        }

        private static void WriteManifest(string directory, string profile, BuildReport report)
        {
            var sha = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local";
            var run = Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "local";
            var text = new StringBuilder()
                .AppendLine("GRADOMRAZ / KARLOLEGEND")
                .AppendLine("profile=" + profile)
                .AppendLine("unity=" + Application.unityVersion)
                .AppendLine("sourceCommit=" + sha)
                .AppendLine("ciRun=" + run)
                .AppendLine("buildUtc=" + DateTime.UtcNow.ToString("O"))
                .AppendLine("result=" + report.summary.result)
                .AppendLine("bytes=" + report.summary.totalSize);
            if (profile == "webgl")
            {
                text.AppendLine("compression=Brotli")
                    .AppendLine("threads=false")
                    .AppendLine("wasmInitialMemoryMb=256")
                    .AppendLine("wasmMaximumMemoryMb=1024")
                    .AppendLine("wasmGeometricGrowth=0.15")
                    .AppendLine("wasmGrowthCapMb=32");
            }
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "GRADOMRAZ-BUILD-MANIFEST.txt"), text.ToString(), new UTF8Encoding(false));
        }

        private static void ReportResult(string platformName, BuildReport report, string output)
        {
            var summary = report.summary;
            Debug.Log($"{platformName} build: {summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, output={output}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"GRADOMRAZ {platformName} build failed: {summary.result}; errors={summary.totalErrors}");
            }
        }
    }
}
