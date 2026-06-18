using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CodexRepair
{
    public static class BuildGame
    {
        private const string OutputPath = "_CodexRepair/Builds/Windows/ExportedProject.exe";

        public static void BuildWindows64()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                UnityEngine.Debug.LogError("Build failed: no enabled scenes in EditorBuildSettings.");
                EditorApplication.Exit(1);
                return;
            }

            var outputDirectory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            UnityEngine.Debug.Log(
                $"Windows build result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, size={summary.totalSize}, output={OutputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
