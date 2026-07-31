using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.Editor
{
    /// <summary>
    /// Removes missing MonoBehaviour components from Unity's temporary build-scene copy.
    /// Source scene assets are never modified. A missing script cannot execute in the player,
    /// but leaving the dead component in the serialized build can destabilize UI and scene load.
    /// </summary>
    public sealed class BuildSceneMissingScriptRepair : IProcessSceneWithReport
    {
        public int callbackOrder => -900;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var removedCount = 0;

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null)
                {
                    continue;
                }

                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform == null || transform.gameObject == null)
                    {
                        continue;
                    }

                    var gameObject = transform.gameObject;
                    var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                    if (missingCount <= 0)
                    {
                        continue;
                    }

                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    removedCount += missingCount;
                }
            }

            if (removedCount > 0)
            {
                Debug.LogWarning(
                    $"GRADOMRAZ build repair removed {removedCount} missing-script component(s) " +
                    $"from the temporary build copy of '{scene.path}'. Source scene was not modified.");
            }
        }
    }
}
