using System;
using System.Collections.Generic;
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
        private const string ItchWebGlArchivePath = "Builds/GRADOMRAZ-by-KARLOLEGEND-WebGL-itch.zip";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string PipelineAssetPath = "Assets/MonoBehaviour/PC_RPAsset.asset";

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build Windows 64")]
        public static void BuildWindows64()
        {
            Selection.activeObject = null;
            TmpFontAssetRepair.Run();

            var scenes = GetPlayableScenes();

            if (scenes.Length == 0)
            {
                UnityEngine.Debug.LogError("Build failed: no playable scenes in EditorBuildSettings.");
                EditorApplication.Exit(1);
                return;
            }

            var outputDirectory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                if (Directory.Exists(outputDirectory))
                {
                    Directory.Delete(outputDirectory, true);
                }

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

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build WebGL for itch.io")]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.template = "PROJECT:Itch";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            var scenes = GetPlayableScenes();

            if (scenes.Length == 0)
            {
                UnityEngine.Debug.LogError("Build failed: no playable scenes in EditorBuildSettings.");
                EditorApplication.Exit(1);
                return;
            }

            if (Directory.Exists(WebGlOutputPath))
            {
                Directory.Delete(WebGlOutputPath, true);
            }

            BuildReport report;
            using (var pipelineSettings = ApplyWebGlPipelineSettings())
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = WebGlOutputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });
            }

            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                CreateItchArchive();
            }

            UnityEngine.Debug.Log(
                $"WebGL build result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, size={summary.totalSize}, output={WebGlOutputPath}, itchArchive={ItchWebGlArchivePath}");

            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void CreateItchArchive()
        {
            if (File.Exists(ItchWebGlArchivePath))
            {
                File.Delete(ItchWebGlArchivePath);
            }

            ZipFile.CreateFromDirectory(WebGlOutputPath, ItchWebGlArchivePath, System.IO.Compression.CompressionLevel.Optimal, false);
        }

        private static string[] GetPlayableScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.Equals(scene.path, BootScenePath, StringComparison.OrdinalIgnoreCase))
                .Select(scene => scene.path)
                .ToArray();
        }

        private static PipelineSettingsSnapshot ApplyWebGlPipelineSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"WebGL build requires render pipeline asset at {PipelineAssetPath}.");
            }

            var snapshot = new PipelineSettingsSnapshot(asset);
            var serializedAsset = new SerializedObject(asset);
            snapshot.SetInt(serializedAsset, "m_MSAA", 1);
            snapshot.SetFloat(serializedAsset, "m_RenderScale", 0.85f);
            snapshot.SetInt(serializedAsset, "m_MainLightShadowmapResolution", 1024);
            snapshot.SetInt(serializedAsset, "m_AdditionalLightsShadowmapResolution", 1024);
            snapshot.SetInt(serializedAsset, "m_AdditionalLightsPerObjectLimit", 2);
            snapshot.SetFloat(serializedAsset, "m_ShadowDistance", 24f);
            snapshot.SetInt(serializedAsset, "m_ShadowCascadeCount", 1);
            snapshot.SetInt(serializedAsset, "m_SoftShadowQuality", 0);
            snapshot.SetInt(serializedAsset, "m_ColorGradingLutSize", 32);
            serializedAsset.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return snapshot;
        }

        private sealed class PipelineSettingsSnapshot : IDisposable
        {
            private readonly RenderPipelineAsset asset;
            private readonly Dictionary<string, int> integerValues = new();
            private readonly Dictionary<string, float> floatValues = new();

            public PipelineSettingsSnapshot(RenderPipelineAsset asset)
            {
                this.asset = asset;
            }

            public void SetInt(SerializedObject serializedAsset, string propertyName, int value)
            {
                var property = serializedAsset.FindProperty(propertyName);
                if (property == null || property.propertyType != SerializedPropertyType.Integer)
                {
                    return;
                }

                integerValues[propertyName] = property.intValue;
                property.intValue = value;
            }

            public void SetFloat(SerializedObject serializedAsset, string propertyName, float value)
            {
                var property = serializedAsset.FindProperty(propertyName);
                if (property == null || property.propertyType != SerializedPropertyType.Float)
                {
                    return;
                }

                floatValues[propertyName] = property.floatValue;
                property.floatValue = value;
            }

            public void Dispose()
            {
                var serializedAsset = new SerializedObject(asset);
                foreach (var pair in integerValues)
                {
                    var property = serializedAsset.FindProperty(pair.Key);
                    if (property != null)
                    {
                        property.intValue = pair.Value;
                    }
                }

                foreach (var pair in floatValues)
                {
                    var property = serializedAsset.FindProperty(pair.Key);
                    if (property != null)
                    {
                        property.floatValue = pair.Value;
                    }
                }

                serializedAsset.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
