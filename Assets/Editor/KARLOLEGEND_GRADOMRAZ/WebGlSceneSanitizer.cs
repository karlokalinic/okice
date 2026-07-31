using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Removes unsupported VideoPlayer clips only from scenes processed for a real WebGL build.
    /// Unity may invoke scene processors with a null BuildReport during import/editor workflows.
    /// </summary>
    public sealed class WebGlSceneSanitizer : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null)
                {
                    continue;
                }

                foreach (var player in root.GetComponentsInChildren<VideoPlayer>(true))
                {
                    if (player == null)
                    {
                        continue;
                    }

                    player.clip = null;
                    player.enabled = false;
                }
            }
        }
    }
}
