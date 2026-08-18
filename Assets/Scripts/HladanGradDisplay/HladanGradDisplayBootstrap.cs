using System;
using UnityEngine;

public static class HladanGradDisplayBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;

        var compatibilityTarget = IsCompatibilityTarget();
        Application.targetFrameRate = compatibilityTarget ? 30 : 60;
        Application.backgroundLoadingPriority = compatibilityTarget
            ? ThreadPriority.Low
            : ThreadPriority.Normal;

        var hdr = HDROutputSettings.main;
        if (hdr.available && hdr.active)
        {
            hdr.RequestHDRModeChange(false);
        }
    }

    private static bool IsCompatibilityTarget()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Browser builds should start conservative before any runtime hardware hint.
        return true;
#elif !UNITY_EDITOR
        var memoryMb = SystemInfo.systemMemorySize;
        var threads = SystemInfo.processorCount;
        var gpu = (SystemInfo.graphicsDeviceName ?? string.Empty).ToLowerInvariant();
        var intelIntegrated = gpu.Contains("intel") &&
                              (gpu.Contains("uhd") || gpu.Contains("hd graphics") || gpu.Contains("iris"));

        if (memoryMb > 0 && memoryMb <= 6144)
        {
            return true;
        }

        return intelIntegrated && threads <= 4 && (memoryMb <= 0 || memoryMb <= 8192);
#else
        return false;
#endif
    }
}
