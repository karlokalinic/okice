using UnityEngine;

// HLADAN GRAD — display bootstrap.
// Forces the game to target 2560x1440 @ 200 Hz and caps the framerate at 200 fps.
// Runs automatically at startup (no scene wiring needed). Safe to delete to revert.
public static class HladanGradDisplayBootstrap
{
    private const int TargetWidth = 2560;
    private const int TargetHeight = 1440;
    private const int TargetRefreshHz = 200;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Uncap vSync and cap the framerate at the monitor's refresh (200 Hz).
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetRefreshHz;

#if !UNITY_EDITOR
        // In a build, request 2560x1440 @ 200 Hz using the current fullscreen mode.
        // (The Editor uses the Game view resolution, so this is build-only.)
        FullScreenMode mode = Screen.fullScreenMode;
        RefreshRate rr = new RefreshRate { numerator = TargetRefreshHz, denominator = 1 };
        Screen.SetResolution(TargetWidth, TargetHeight, mode, rr);
#endif
    }
}
