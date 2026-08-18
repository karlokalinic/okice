using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Removes desktop-only scene asset references from the in-memory scene copy
    /// that Unity serializes into the production mobile WebGL player.
    /// The source scene is never modified, so Windows keeps the boot video.
    /// </summary>
    public sealed class MobileWebBuildStripper : IProcessSceneWithReport
    {
        public int callbackOrder => -1000;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            if (!string.Equals(PlayerSettings.WebGL.template, "PROJECT:Mobile", StringComparison.Ordinal))
            {
                return;
            }

            var normalizedPath = (scene.path ?? string.Empty).Replace('\\', '/');
            if (!normalizedPath.EndsWith("/Boot.unity", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var strippedClips = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var player in root.GetComponentsInChildren<VideoPlayer>(true))
                {
                    if (player.clip == null)
                    {
                        continue;
                    }

                    player.clip = null;
                    strippedClips++;
                }
            }

            if (strippedClips > 0)
            {
                UnityEngine.Debug.Log($"Mobile WebGL stripper removed {strippedClips} desktop boot VideoClip reference(s) from {scene.path}.");
            }
        }
    }
}
