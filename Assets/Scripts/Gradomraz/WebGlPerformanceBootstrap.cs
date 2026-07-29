using UnityEngine;

namespace Karlolegend.Gradomraz
{
    internal static class WebGlPerformanceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 24f);
            QualitySettings.shadowCascades = 1;
            QualitySettings.lodBias = Mathf.Min(QualitySettings.lodBias, 1.25f);
            QualitySettings.enableLODCrossFade = false;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 128f;
#endif
        }
    }
}