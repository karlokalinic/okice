using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.Performance
{
    /// <summary>
    /// Compatibility path for low-end family/parent hardware and ordinary desktop WebGL.
    /// A machine such as a 4 GB, 2C/4T Intel UHD laptop should never receive the same
    /// render workload as a modern gaming desktop just because both run Windows.
    /// </summary>
    [DefaultExecutionOrder(-12000)]
    public sealed class LowSpecCompatibilityGovernor : MonoBehaviour
    {
        public const string GameObjectName = "KARLOLEGEND_Compatibility";

        private const float SampleWindowSeconds = 2f;
        private const float ScaleStep = 0.05f;
        private const float NormalScale = 0.72f;
        private const float SevereScale = 0.62f;
        private const float NormalFloor = 0.58f;
        private const float SevereFloor = 0.50f;
        private const int TargetFps = 30;

        private UniversalRenderPipelineAsset lowSpecPipeline;
        private bool severe;
        private float currentScale;
        private float minScale;
        private float sampleElapsed;
        private float sampleFrameSeconds;
        private int sampleFrames;
        private int stableWindows;
        private bool shadowsDisabledByGovernor;

        public static bool Active { get; private set; }
        public static bool Severe => Active && instance != null && instance.severe;
        private static LowSpecCompatibilityGovernor instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if !UNITY_EDITOR
            if (!ShouldUseCompatibilityMode(out var initialSevere))
            {
                return;
            }

            Active = true;
            var host = GameObject.Find(GameObjectName);
            if (host == null)
            {
                host = new GameObject(GameObjectName);
                DontDestroyOnLoad(host);
            }

            var governor = host.GetComponent<LowSpecCompatibilityGovernor>();
            if (governor == null)
            {
                governor = host.AddComponent<LowSpecCompatibilityGovernor>();
            }
            governor.severe = initialSevere;
#endif
        }

        private static bool ShouldUseCompatibilityMode(out bool initialSevere)
        {
            initialSevere = false;

            // Ordinary browser WebGL is a compatibility target by default. JavaScript
            // sends a stronger Eco hint after startup when deviceMemory/CPU indicates it.
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return true;
            }

            var memoryMb = SystemInfo.systemMemorySize;
            var logicalProcessors = SystemInfo.processorCount;
            var gpuName = (SystemInfo.graphicsDeviceName ?? string.Empty).ToLowerInvariant();
            var intelIntegrated = gpuName.Contains("intel") &&
                                  (gpuName.Contains("uhd") || gpuName.Contains("hd graphics") || gpuName.Contains("iris"));

            var fourToSixGbClass = memoryMb > 0 && memoryMb <= 6144;
            var constrainedIntelLaptop = intelIntegrated && logicalProcessors <= 4 &&
                                         (memoryMb <= 0 || memoryMb <= 8192);

            initialSevere = fourToSixGbClass || constrainedIntelLaptop;
            return initialSevere;
        }

        private void Awake()
        {
#if !UNITY_EDITOR
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            Active = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyCompatibilityProfile();
#endif
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
                Active = false;
            }
#endif
        }

        private void Update()
        {
#if !UNITY_EDITOR
            if (lowSpecPipeline == null || Time.unscaledDeltaTime <= 0f)
            {
                return;
            }

            var delta = Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            sampleElapsed += delta;
            sampleFrameSeconds += delta;
            sampleFrames++;

            if (sampleElapsed < SampleWindowSeconds)
            {
                return;
            }

            var averageSeconds = sampleFrameSeconds / Mathf.Max(1, sampleFrames);
            var targetSeconds = 1f / TargetFps;
            var underPressure = averageSeconds > targetSeconds * 1.08f;
            var comfortablyStable = averageSeconds < targetSeconds * 1.015f;

            if (underPressure)
            {
                stableWindows = 0;
                if (currentScale > minScale + 0.01f)
                {
                    SetRenderScale(currentScale - ScaleStep);
                }
                else if (!shadowsDisabledByGovernor)
                {
                    shadowsDisabledByGovernor = true;
                    lowSpecPipeline.shadowDistance = 0f;
                    ApplyScenePolicy();
                }
            }
            else if (comfortablyStable && !severe && !shadowsDisabledByGovernor)
            {
                stableWindows++;
                if (stableWindows >= 5 && currentScale < NormalScale - 0.01f)
                {
                    SetRenderScale(currentScale + ScaleStep);
                    stableWindows = 0;
                }
            }
            else
            {
                stableWindows = 0;
            }

            ResetSamples();
#endif
        }

        /// <summary>
        /// Browser shell hardware hint. "eco" is used for <=4 GB browser memory signal,
        /// <=4 logical processors, Data Saver, or similarly constrained devices.
        /// </summary>
        public void ConfigureBrowserHint(string hint)
        {
#if !UNITY_EDITOR
            if (string.Equals((hint ?? string.Empty).Trim(), "eco", StringComparison.OrdinalIgnoreCase))
            {
                severe = true;
                ApplyCompatibilityProfile();
            }
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyScenePolicy();
        }

        private void ApplyCompatibilityProfile()
        {
            lowSpecPipeline = Resources.Load<UniversalRenderPipelineAsset>("LowSpec_RPAsset");
            if (lowSpecPipeline == null)
            {
                Debug.LogWarning("[Compatibility] LowSpec_RPAsset is missing; applying only global quality limits.");
            }
            else
            {
                // Unity supports switching the active SRP asset at runtime. This avoids
                // carrying the PC Forward+ workload onto low-end integrated graphics.
                QualitySettings.renderPipeline = lowSpecPipeline;

                lowSpecPipeline.renderScale = severe ? SevereScale : NormalScale;
                lowSpecPipeline.msaaSampleCount = 1;
                lowSpecPipeline.shadowDistance = severe ? 0f : 8f;
                lowSpecPipeline.shadowCascadeCount = 1;
                lowSpecPipeline.maxAdditionalLightsCount = 1;
                lowSpecPipeline.supportsHDR = false;
                lowSpecPipeline.supportsCameraDepthTexture = false;
                lowSpecPipeline.supportsCameraOpaqueTexture = false;
                lowSpecPipeline.supportsDynamicBatching = true;

                currentScale = lowSpecPipeline.renderScale;
                minScale = severe ? SevereFloor : NormalFloor;
            }

            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.shadowDistance = severe ? 0f : Mathf.Min(QualitySettings.shadowDistance, 8f);
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.lodBias = Mathf.Min(QualitySettings.lodBias, severe ? 0.85f : 1.0f);
            QualitySettings.globalTextureMipmapLimit = Mathf.Max(QualitySettings.globalTextureMipmapLimit, severe ? 2 : 1);

            Application.targetFrameRate = TargetFps;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

            shadowsDisabledByGovernor = severe;
            ApplyScenePolicy();
            ResetSamples();

            var memoryLabel = Application.platform == RuntimePlatform.WebGLPlayer ? "wasmHeapSignal" : "RAM";
            Debug.Log(
                $"[Compatibility] active severe={severe} {memoryLabel}={SystemInfo.systemMemorySize}MB " +
                $"CPUThreads={SystemInfo.processorCount} GPU='{SystemInfo.graphicsDeviceName}' " +
                $"renderScale={(lowSpecPipeline != null ? lowSpecPipeline.renderScale : -1f):0.00}");
        }

        private void ApplyScenePolicy()
        {
            foreach (var camera in Camera.allCameras)
            {
                if (camera == null)
                {
                    continue;
                }

                var data = camera.GetUniversalAdditionalCameraData();
                data.requiresColorTexture = false;
                data.requiresDepthTexture = false;
                data.renderPostProcessing = false;
                data.renderShadows = !severe && !shadowsDisabledByGovernor;
                data.stopNaN = false;
                data.dithering = false;
            }

            foreach (var light in Resources.FindObjectsOfTypeAll<Light>())
            {
                if (light == null || !light.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (light.type == LightType.Directional)
                {
                    if (severe || shadowsDisabledByGovernor)
                    {
                        light.shadows = LightShadows.None;
                    }
                    continue;
                }

                // Keep authored illumination but remove the expensive part that hurts
                // integrated GPUs: punctual shadow maps and very long influence ranges.
                light.shadows = LightShadows.None;
                light.range = Mathf.Min(light.range, severe ? 8f : 12f);
            }
        }

        private void SetRenderScale(float value)
        {
            if (lowSpecPipeline == null)
            {
                return;
            }

            currentScale = Mathf.Clamp(value, minScale, NormalScale);
            currentScale = Mathf.Round(currentScale * 20f) / 20f;
            lowSpecPipeline.renderScale = currentScale;
        }

        private void ResetSamples()
        {
            sampleElapsed = 0f;
            sampleFrameSeconds = 0f;
            sampleFrames = 0;
        }
    }
}
