#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class KLWindowsBuild
{
    [MenuItem("Tools/KARLOLEGEND/Build PC ASAP")]
    public static void BuildPcAsap()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new BuildFailedException("Windows x64 Build Support is not installed.");

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new BuildFailedException("No enabled scenes exist in Build Settings.");

        string missingScene = scenes.FirstOrDefault(path => !File.Exists(path));
        if (!string.IsNullOrEmpty(missingScene))
            throw new BuildFailedException($"An enabled scene is missing: {missingScene}");

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string buildDirectory = Path.Combine(projectRoot, "Builds", "PC-ASAP");
        string executablePath = Path.Combine(buildDirectory, "GRADOMRAZ.exe");

        if (Directory.Exists(buildDirectory))
            Directory.Delete(buildDirectory, true);
        Directory.CreateDirectory(buildDirectory);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[KARLOLEGEND] Building PC release -> {executablePath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Windows build failed: {report.summary.result}");

        if (!File.Exists(executablePath))
            throw new BuildFailedException("Unity reported success but GRADOMRAZ.exe is missing.");

        File.WriteAllText(
            Path.Combine(buildDirectory, "PLAY-GRADOMRAZ.bat"),
            "@echo off\r\ncd /d \"%~dp0\"\r\nstart \"\" \"GRADOMRAZ.exe\" -force-d3d11\r\n"
        );

        Debug.Log($"[KARLOLEGEND] PC BUILD OK // {report.summary.totalSize / (1024f * 1024f):F1} MiB // {executablePath}");
    }
}
#endif