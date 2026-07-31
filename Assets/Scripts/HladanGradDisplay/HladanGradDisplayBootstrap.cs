using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    private const int ProductionFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Do not bind simulation/render cost to the physical monitor refresh rate. On 144/165/200 Hz
        // displays the previous code forced the restored project and all fullscreen effects to run
        // several times more often than the original production target.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = ProductionFrameRate;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }
}
