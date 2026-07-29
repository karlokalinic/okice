using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Karlolegend.Gradomraz.Editor
{
    public sealed class WebGlSceneSanitizer : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var player in root.GetComponentsInChildren<VideoPlayer>(true))
                {
                    player.clip = null;
                    player.enabled = false;
                }
            }
        }
    }
}