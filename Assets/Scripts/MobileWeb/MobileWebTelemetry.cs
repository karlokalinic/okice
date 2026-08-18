using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.MobileWeb
{
    /// <summary>
    /// Low-overhead production telemetry for physical-device validation.
    /// Writes one compact line to the browser console every ten seconds.
    /// No network transport, no analytics SDK, and no per-frame allocations.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class MobileWebTelemetry : MonoBehaviour
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
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            var host = GameObject.Find("KARLOLEGEND_MobileTelemetry");
            if (host == null)
            {
                host = new GameObject("KARLOLEGEND_MobileTelemetry");
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<MobileWebTelemetry>() == null)
            {
                host.AddComponent<MobileWebTelemetry>();
            }
#endif
        }

        private void Awake()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded += OnSceneLoaded;
            lastScene = SceneManager.GetActiveScene().name;
            nextReportTime = Time.unscaledTime + ReportIntervalSeconds;
#endif
        }

        private void OnDestroy()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        private void Update()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
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

            float sum = 0f;
            float worst = 0f;
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
                "[MobileWebTelemetry] scene={0} avgFps={1:F1} avgMs={2:F1} p95Ms={3:F1} worstMs={4:F1} target={5} renderScale={6:F2} shadowM={7:F1} allocMB={8:F0} reservedMB={9:F0} transitions={10}",
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
                sceneTransitions));
        }
    }
}
