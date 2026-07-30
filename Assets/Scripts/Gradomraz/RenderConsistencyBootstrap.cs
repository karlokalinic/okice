using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz
{
    /// <summary>
    /// Runtime backstop for machines whose Windows HDR state differs from the Editor.
    /// URP may still render into HDR buffers internally; only the physical display output is SDR.
    /// </summary>
    public static class RenderConsistencyBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            QualitySettings.SetQualityLevel(0, true);
            DisableDisplayHdr();

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DisableDisplayHdr();

            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var camera in cameras)
            {
                if (camera.cameraType != CameraType.Game)
                {
                    continue;
                }

                if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
                {
                    cameraData.allowHDROutput = false;
                }
            }
        }

        private static void DisableDisplayHdr()
        {
#if !UNITY_WEBGL
            try
            {
                var hdrOutput = HDROutputSettings.main;
                if (hdrOutput.available && hdrOutput.active)
                {
                    hdrOutput.RequestHDRModeChange(false);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not force SDR display output: {exception.Message}");
            }
#endif
        }
    }
}
