using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.Performance
{
    /// <summary>
    /// Low-overhead telemetry for ordinary WebGL and low-spec native Windows testing.
    /// Browser builds write to DevTools; native players write to Unity Player.log.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class CompatibilityTelemetry : MonoBehaviour
    {
        private const float ReportIntervalSeconds = 10f;
        private const int SampleCapacity = 300;
        private readonly float[] frameMs = new float[SampleCapacity];
        private readonly float[] sortBuffer = new float[SampleCapacity];

        private int sampleCount;
        private int writeIndex;
        private float nextReportTime;
        private int sceneTransitions;
        private string lastScene = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if !UNITY_EDITOR
            if (!ShouldInstall())
            {
                return;
            }

            var host = GameObject.Find("KARLOLEGEND_CompatibilityTelemetry");
            if (host == null)
            {
                host = new GameObject("KARLOLEGEND_CompatibilityTelemetry");
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<CompatibilityTelemetry>() == null)
            {
                host.AddComponent<CompatibilityTelemetry>();
            }
#endif
        }

        private static bool ShouldInstall()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return true;
            }

            var memoryMb = SystemInfo.systemMemorySize;
            var threads = SystemInfo.processorCount;
            var gpu = (SystemInfo.graphicsDeviceName ?? string.Empty).ToLowerInvariant();
            var intelIntegrated = gpu.Contains("intel") &&
                                  (gpu.Contains("uhd") || gpu.Contains("hd graphics") || gpu.Contains("iris"));

            return (memoryMb > 0 && memoryMb <= 6144) ||
                   (intelIntegrated && threads <= 4 && (memoryMb <= 0 || memoryMb <= 8192));
        }

        private void Awake()
        {
#if !UNITY_EDITOR
            SceneManager.sceneLoaded += OnSceneLoaded;
            lastScene = SceneManager.GetActiveScene().name;
            nextReportTime = Time.unscaledTime + ReportIntervalSeconds;
#endif
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        private void Update()
        {
#if !UNITY_EDITOR
            var delta = Time.unscaledDeltaTime;
            if (delta > 0f && delta < 1f)
            {
                frameMs[writeIndex] = delta * 1000f;
                writeIndex = (writeIndex + 1) % SampleCapacity;
                sampleCount = Mathf.Min(sampleCount + 1, SampleCapacity);
            }

            if (Time.unscaledTime >= nextReportTime)
            {
                Report();
                nextReportTime = Time.unscaledTime + ReportIntervalSeconds;
            }
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneTransitions++;
            lastScene = scene.name;
            sampleCount = 0;
            writeIndex = 0;
        }

        private void Report()
        {
            if (sampleCount <= 0)
            {
                return;
            }

            var sum = 0f;
            var worst = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var value = frameMs[i];
                sortBuffer[i] = value;
                sum += value;
                if (value > worst) worst = value;
            }

            Array.Sort(sortBuffer, 0, sampleCount);
            var p95Index = Mathf.Clamp(Mathf.CeilToInt(sampleCount * 0.95f) - 1, 0, sampleCount - 1);
            var p95 = sortBuffer[p95Index];
            var average = sum / sampleCount;
            var fps = average > 0.001f ? 1000f / average : 0f;
            var allocatedMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            var reservedMb = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);

            var renderScale = 1f;
            var shadowDistance = 0f;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                renderScale = urp.renderScale;
                shadowDistance = urp.shadowDistance;
            }

            Debug.Log(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "[CompatibilityTelemetry] scene={0} avgFps={1:F1} avgMs={2:F1} p95Ms={3:F1} worstMs={4:F1} target={5} renderScale={6:F2} shadowM={7:F1} allocMB={8:F0} reservedMB={9:F0} RAMSignalMB={10} CPUThreads={11} GPU='{12}' transitions={13}",
                string.IsNullOrEmpty(lastScene) ? "<none>" : lastScene,
                fps,
                average,
                p95,
                worst,
                Application.targetFrameRate,
                renderScale,
                shadowDistance,
                allocatedMb,
                reservedMb,
                SystemInfo.systemMemorySize,
                SystemInfo.processorCount,
                SystemInfo.graphicsDeviceName,
                sceneTransitions));
        }
    }
}
