using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Keeps Editor, standard Build Profiles and custom builds on the same render pipeline.
    /// The project renders internally in HDR, but final monitor output is deliberately SDR so
    /// Windows HDR state cannot change exposure, black levels or tonemapping between machines.
    /// </summary>
    public sealed class RenderConsistencyBuildGuard : IPreprocessBuildWithReport
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string PipelineAssetPath = "Assets/MonoBehaviour/PC_RPAsset.asset";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ApplyProjectSettings();
        }

        [InitializeOnLoadMethod]
        private static void ScheduleEditorAlignment()
        {
            EditorApplication.delayCall -= ApplyProjectSettings;
            EditorApplication.delayCall += ApplyProjectSettings;
        }

        public static void ApplyProjectSettings()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                throw new BuildFailedException(
                    $"Render consistency check failed: missing render pipeline asset at {PipelineAssetPath}.");
            }

            if (QualitySettings.names.Length == 0)
            {
                throw new BuildFailedException("Render consistency check failed: the project has no quality levels.");
            }

            // Graphics and the active quality level must resolve to the exact same URP asset.
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.SetQualityLevel(0, true);
            QualitySettings.renderPipeline = pipeline;

            // Keep HDR render buffers, lighting and bloom inside URP, but do not let Windows
            // switch the physical display into HDR. That switch was producing the very dark build.
            PlayerSettings.useHDRDisplay = false;
            PlayerSettings.allowHDRDisplaySupport = false;

            EnsureBootSceneIsFirst();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureBootSceneIsFirst()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath) == null)
            {
                throw new BuildFailedException(
                    $"Render consistency check failed: startup scene is missing at {BootScenePath}.");
            }

            var remainingScenes = EditorBuildSettings.scenes
                .Where(scene => !string.Equals(scene.path, BootScenePath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            remainingScenes.Insert(0, new EditorBuildSettingsScene(BootScenePath, true));
            EditorBuildSettings.scenes = remainingScenes.ToArray();
        }
    }
}
