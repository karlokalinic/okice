using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
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
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string PipelineAssetPath = "Assets/MonoBehaviour/PC_RPAsset.asset";
        private const string DialogueDatabasePath = "Assets/MonoBehaviour/AFTERLIVES Dialogue Database.asset";
        private const int MinimumExpectedConversationCount = 70;

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build Windows 64")]
        public static void BuildWindows64()
        {
            PrepareBuild();
            var scenes = GetPlayableScenes();
            RecreateDirectory(Path.GetDirectoryName(OutputPath));

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            LogSummary("Windows", report, OutputPath);
            ThrowIfBuildFailed(report, "Windows");
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Build WebGL for itch.io")]
        public static void BuildWebGL()
        {
            PrepareBuild();

            PlayerSettings.WebGL.template = "PROJECT:Itch";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            var scenes = GetPlayableScenes();
            RecreateDirectory(WebGlOutputPath);

            BuildReport report;
            using (ApplyWebGlPipelineSettings())
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = WebGlOutputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });
            }

            if (report.summary.result == BuildResult.Succeeded)
            {
                CreateItchArchive();
            }

            LogSummary("WebGL", report, WebGlOutputPath);
            ThrowIfBuildFailed(report, "WebGL");
        }

        private static void PrepareBuild()
        {
            Selection.activeObject = null;

            if (!TmpFontAssetRepair.EnsureBuildReady(out var fontError))
            {
                throw new BuildFailedException(fontError);
            }

            ValidateDialogueDatabase();
        }

        private static void ValidateDialogueDatabase()
        {
            var database = AssetDatabase.LoadMainAssetAtPath(DialogueDatabasePath);
            if (database == null)
            {
                throw new BuildFailedException($"Missing dialogue database: {DialogueDatabasePath}");
            }

            var serializedDatabase = new SerializedObject(database);
            var conversations = serializedDatabase.FindProperty("conversations");
            if (conversations == null)
            {
                throw new BuildFailedException($"Dialogue database has no serialized conversations array: {DialogueDatabasePath}");
            }

            if (conversations.arraySize < MinimumExpectedConversationCount)
            {
                throw new BuildFailedException(
                    $"Dialogue database appears truncated: found {conversations.arraySize} conversations, expected at least {MinimumExpectedConversationCount}. " +
                    "Restore the database before building.");
            }
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

        private static string[] GetPlayableScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                .Select(scene => scene.path)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            InsertRequiredScene(scenes, MainMenuScenePath, insertAtStart: false);
            InsertRequiredScene(scenes, BootScenePath, insertAtStart: true);

            if (scenes.Count == 0)
            {
                throw new BuildFailedException("Build failed: no playable scenes are available.");
            }

            return scenes.ToArray();
        }

        private static void InsertRequiredScene(List<string> scenes, string scenePath, bool insertAtStart)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                throw new BuildFailedException($"Required scene is missing: {scenePath}");
            }

            scenes.RemoveAll(path => string.Equals(path, scenePath, StringComparison.OrdinalIgnoreCase));
            if (insertAtStart)
            {
                scenes.Insert(0, scenePath);
            }
            else
            {
                scenes.Add(scenePath);
            }
        }

        private static void RecreateDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);
        }

        private static void LogSummary(string platformName, BuildReport report, string output)
        {
            var summary = report.summary;
            Debug.Log(
                $"{platformName} build result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}, " +
                $"size={summary.totalSize}, output={output}");
        }

        private static void ThrowIfBuildFailed(BuildReport report, string platformName)
        {
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{platformName} build failed with {report.summary.totalErrors} error(s) and {report.summary.totalWarnings} warning(s).");
            }
        }

        private static PipelineSettingsSnapshot ApplyWebGlPipelineSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (asset == null)
            {
                throw new BuildFailedException($"WebGL build requires render pipeline asset at {PipelineAssetPath}.");
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
