using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
        int numerator = (int)refreshRate.numerator;
        int denominator = (int)refreshRate.denominator;
        int refreshRateHz = denominator > 0
            ? numerator / denominator
            : -1;

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = refreshRateHz > 0 ? refreshRateHz : -1;
    }
}
