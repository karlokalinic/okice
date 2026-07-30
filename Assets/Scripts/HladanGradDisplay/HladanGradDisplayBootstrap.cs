using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    private const int DefaultFrameRate = 60;
    private const int MaximumFrameRate = 120;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return;
#else
        var refreshRate = Screen.currentResolution.refreshRateRatio;
        var denominator = (int)refreshRate.denominator;
        var refreshRateHz = denominator > 0
            ? Mathf.RoundToInt((float)refreshRate.numerator / denominator)
            : DefaultFrameRate;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Clamp(refreshRateHz, DefaultFrameRate, MaximumFrameRate);
#endif
    }
}
