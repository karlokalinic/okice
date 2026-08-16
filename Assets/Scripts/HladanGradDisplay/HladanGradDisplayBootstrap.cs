using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;

#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
        // Start conservatively before the HTML shell applies Eco/Balanced/Quality.
        // This prevents the loading/menu phase from needlessly running at 60+ FPS.
        Application.targetFrameRate = 30;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
#else
        Application.targetFrameRate = 60;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
#endif

        var hdr = HDROutputSettings.main;
        if (hdr.available && hdr.active)
        {
            hdr.RequestHDRModeChange(false);
        }
    }
}
