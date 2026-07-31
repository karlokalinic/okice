using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;

        var hdr = HDROutputSettings.main;
        if (hdr.available && hdr.active)
        {
            hdr.RequestHDRModeChange(false);
        }
    }
}
