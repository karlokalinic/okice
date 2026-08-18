using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.MobileWeb
{
    [DefaultExecutionOrder(-10000)]
    public sealed class MobileWebInputBridge : MonoBehaviour
    {
        public const string GameObjectName = "KARLOLEGEND_MobileWebInput";

        private const float GovernorWindowSeconds = 2f;
        private const float ScaleStep = 0.05f;
        private const int NormalTargetFps = 30;
        private const int EmergencyFallbackFps = 24;
        private const int MobilePhysicsHz = 30;
        private const int MobileRealVoices = 24;
        private const int MobileVirtualVoices = 96;

        private struct LightBaseline
        {
            public float range;
            public LightShadows shadows;
        }

        private static readonly HashSet<string> PendingButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FrameButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static Vector2 pendingLookDelta;
        private static Vector2 frameLookDelta;
        private static Vector2 move;
        private static bool audioConfigured;
        private static bool physicsConfigured;

        private readonly Dictionary<Light, LightBaseline> lightBaselines = new Dictionary<Light, LightBaseline>();

        private UniversalRenderPipelineAsset activeUrp;
        private string activeProfile = "balanced";
        private int activeTargetFps = NormalTargetFps;
        private float minRenderScale = 0.55f;
        private float maxRenderScale = 0.80f;
        private float currentRenderScale = 0.75f;
        private float baseShadowDistance = 8f;
        private int baseAdditionalLights = 1;
        private bool allowPostProcessing;
        private bool allowDepthTexture;
        private bool emergencyMode;
        private bool frameRateFallbackEngaged;
        private float sampleElapsed;
        private float sampleFrameSeconds;
        private int sampleFrames;
        private int sampleSlowFrames;
        private int consecutiveStableWindows;
        private int consecutiveCriticalWindows;
        private bool pageHidden;

        public static Vector2 Move => move;
        public static Vector2 LookDelta => frameLookDelta;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            var host = GameObject.Find(GameObjectName);
            if (host == null)
            {
                host = new GameObject(GameObjectName);
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<MobileWebInputBridge>() == null)
            {
                host.AddComponent<MobileWebInputBridge>();
            }
#endif
        }

        private void Awake()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            Application.runInBackground = false;
            ConfigurePhysicsForMobile();
            ConfigureAudioForMobile();
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyProfile("balanced");
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
            frameLookDelta = pendingLookDelta;
            pendingLookDelta = Vector2.zero;

            FrameButtons.Clear();
            foreach (var button in PendingButtons)
            {
                FrameButtons.Add(button);
            }
            PendingButtons.Clear();

#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            TickGovernor();
#endif
        }

        private void OnDisable()
        {
            ResetInputState();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetInputState();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ResetInputState();
            }
        }

        public static bool GetButtonDown(string buttonName)
        {
            return !string.IsNullOrWhiteSpace(buttonName) && FrameButtons.Contains(buttonName);
        }

        public void SetMove(string value)
        {
            if (TryParsePair(value, out var parsed))
            {
                move = Vector2.ClampMagnitude(parsed, 1f);
            }
        }

        public void AddLookDelta(string value)
        {
            if (!TryParsePair(value, out var parsed))
            {
                return;
            }

            pendingLookDelta += parsed;
            pendingLookDelta.x = Mathf.Clamp(pendingLookDelta.x, -120f, 120f);
            pendingLookDelta.y = Mathf.Clamp(pendingLookDelta.y, -120f, 120f);
        }

        public void PressButton(string buttonName)
        {
            if (!string.IsNullOrWhiteSpace(buttonName))
            {
                PendingButtons.Add(buttonName.Trim());
            }
        }

        public void ResetInput(string unused)
        {
            ResetInputState();
        }

        public void ConfigureProfile(string profileName)
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            ApplyProfile(profileName);
#endif
        }

        public void SetPageVisibility(string state)
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            pageHidden = string.Equals(state, "hidden", StringComparison.OrdinalIgnoreCase);

            if (pageHidden)
            {
                ResetInputState();
                Application.targetFrameRate = 5;
                AudioListener.pause = true;
                return;
            }

            AudioListener.pause = false;
            Application.targetFrameRate = activeTargetFps;
            consecutiveStableWindows = 0;
            consecutiveCriticalWindows = 0;
            ResetGovernorWindow();
#endif
        }

        private static void ConfigurePhysicsForMobile()
        {
            if (physicsConfigured)
            {
                return;
            }

            physicsConfigured = true;
            Time.fixedDeltaTime = 1f / MobilePhysicsHz;
            // At 30 Hz physics this permits at most roughly two catch-up steps after
            // a hitch instead of allowing a long FixedUpdate spiral to dominate WebGL.
            Time.maximumDeltaTime = 1f / 15f;
        }

        private static void ConfigureAudioForMobile()
        {
            if (audioConfigured)
            {
                return;
            }

            audioConfigured = true;
            try
            {
                var config = AudioSettings.GetConfiguration();
                var realVoices = Mathf.Min(config.numRealVoices, MobileRealVoices);
                var virtualVoices = Mathf.Min(config.numVirtualVoices, MobileVirtualVoices);
                virtualVoices = Mathf.Max(virtualVoices, realVoices);

                if (realVoices != config.numRealVoices || virtualVoices != config.numVirtualVoices)
                {
                    config.numRealVoices = realVoices;
                    config.numVirtualVoices = virtualVoices;
                    AudioSettings.Reset(config);
                }
            }
            catch (Exception exception)
            {
                // Audio optimization is optional; a device-specific audio backend
                // failure must never prevent the game from starting.
                Debug.LogWarning("Mobile WebGL audio budget could not be applied: " + exception.Message);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCameraPolicy();
            ApplyLightPolicy();
        }

        private void ApplyProfile(string profileName)
        {
            activeProfile = (profileName ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(activeProfile))
            {
                activeProfile = "balanced";
            }

            activeTargetFps = NormalTargetFps;
            emergencyMode = false;
            frameRateFallbackEngaged = false;
            consecutiveCriticalWindows = 0;

            var renderScale = 0.75f;
            var msaa = 1;
            var shadowCascades = 1;
            var lodBias = 1.05f;
            var mipLimit = 1;

            minRenderScale = 0.55f;
            maxRenderScale = 0.80f;
            baseShadowDistance = 8f;
            baseAdditionalLights = 1;
            allowPostProcessing = false;
            allowDepthTexture = false;

            switch (activeProfile)
            {
                case "eco":
                    renderScale = 0.60f;
                    minRenderScale = 0.50f;
                    maxRenderScale = 0.65f;
                    baseShadowDistance = 0f;
                    baseAdditionalLights = 0;
                    lodBias = 0.85f;
                    mipLimit = 2;
                    break;

                case "quality":
                    renderScale = 0.85f;
                    msaa = 2;
                    minRenderScale = 0.65f;
                    maxRenderScale = 0.90f;
                    baseShadowDistance = 16f;
                    baseAdditionalLights = 1;
                    lodBias = 1.30f;
                    mipLimit = 0;
                    allowPostProcessing = true;
                    allowDepthTexture = true;
                    break;

                default:
                    activeProfile = "balanced";
                    break;
            }

            QualitySettings.vSyncCount = 0;
            QualitySettings.lodBias = lodBias;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.realtimeReflectionProbes = false;
            if (QualitySettings.globalTextureMipmapLimit != mipLimit)
            {
                QualitySettings.globalTextureMipmapLimit = mipLimit;
            }
            Application.targetFrameRate = activeTargetFps;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

            // The PC renderer is authored as Forward+. In Forward+ Unity ignores
            // the classic per-object additional-light budget. Mobile forces classic
            // Forward so the 0/1 additional-light limit is real and predictable.
            ConfigureRendererDataForMobile();

            activeUrp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (activeUrp == null)
            {
                activeUrp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            }

            if (activeUrp != null)
            {
                activeUrp.renderScale = renderScale;
                activeUrp.msaaSampleCount = msaa;
                activeUrp.shadowDistance = baseShadowDistance;
                activeUrp.shadowCascadeCount = shadowCascades;
                activeUrp.maxAdditionalLightsCount = baseAdditionalLights;
                activeUrp.supportsHDR = false;
                activeUrp.supportsCameraOpaqueTexture = false;
                activeUrp.supportsCameraDepthTexture = allowDepthTexture;
                activeUrp.supportsDynamicBatching = true;
                currentRenderScale = renderScale;
            }

            SetSsao(false);
            ApplyCameraPolicy();
            ApplyLightPolicy();
            consecutiveStableWindows = 0;
            ResetGovernorWindow();
        }

        private static void ConfigureRendererDataForMobile()
        {
            foreach (var rendererData in Resources.FindObjectsOfTypeAll<UniversalRendererData>())
            {
                if (rendererData == null)
                {
                    continue;
                }

                var changed = false;
                if (rendererData.renderingMode != RenderingMode.Forward)
                {
                    rendererData.renderingMode = RenderingMode.Forward;
                    changed = true;
                }

                if (rendererData.shadowTransparentReceive)
                {
                    rendererData.shadowTransparentReceive = false;
                    changed = true;
                }

                if (rendererData.depthPrimingMode != DepthPrimingMode.Disabled)
                {
                    rendererData.depthPrimingMode = DepthPrimingMode.Disabled;
                    changed = true;
                }

                if (changed)
                {
                    rendererData.SetDirty();
                }
            }
        }

        private void ApplyLightPolicy()
        {
            var maxRange = activeProfile == "eco" ? 8f : activeProfile == "quality" ? 20f : 12f;

            foreach (var light in Resources.FindObjectsOfTypeAll<Light>())
            {
                if (light == null || !light.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (!lightBaselines.TryGetValue(light, out var baseline))
                {
                    baseline = new LightBaseline
                    {
                        range = light.range,
                        shadows = light.shadows
                    };
                    lightBaselines[light] = baseline;
                }

                if (light.type == LightType.Directional)
                {
                    light.shadows = (activeProfile == "eco" || emergencyMode)
                        ? LightShadows.None
                        : baseline.shadows;
                    continue;
                }

                // Punctual realtime shadow maps are too expensive for this scene's
                // very large light population. Keep authored contribution/range
                // semantics within a mobile cap, but never render their shadow maps.
                light.shadows = LightShadows.None;
                light.range = Mathf.Min(baseline.range, maxRange);
            }
        }

        private void ApplyCameraPolicy()
        {
            foreach (var camera in Camera.allCameras)
            {
                if (camera == null)
                {
                    continue;
                }

                var data = camera.GetUniversalAdditionalCameraData();
                data.requiresColorTexture = false;
                data.requiresDepthTexture = allowDepthTexture && !emergencyMode;
                data.renderPostProcessing = allowPostProcessing && !emergencyMode;
                data.renderShadows = !emergencyMode && baseShadowDistance > 0.01f;
                data.stopNaN = false;
                data.dithering = false;
            }
        }

        private void TickGovernor()
        {
            if (pageHidden || activeUrp == null || Time.unscaledDeltaTime <= 0f)
            {
                return;
            }

            var delta = Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            var targetFrameSeconds = 1f / Mathf.Max(1, activeTargetFps);

            sampleElapsed += delta;
            sampleFrameSeconds += delta;
            sampleFrames++;

            if (delta > targetFrameSeconds * 1.20f)
            {
                sampleSlowFrames++;
            }

            if (sampleElapsed < GovernorWindowSeconds)
            {
                return;
            }

            var averageFrameSeconds = sampleFrameSeconds / Mathf.Max(1, sampleFrames);
            var slowRatio = (float)sampleSlowFrames / Mathf.Max(1, sampleFrames);
            var underPressure = averageFrameSeconds > targetFrameSeconds * 1.05f || slowRatio > 0.10f;
            var stableAtCap = averageFrameSeconds <= targetFrameSeconds * 1.02f && slowRatio < 0.02f;

            if (underPressure)
            {
                consecutiveStableWindows = 0;

                if (currentRenderScale > minRenderScale + 0.01f)
                {
                    consecutiveCriticalWindows = 0;
                    SetRenderScale(currentRenderScale - ScaleStep);
                }
                else if (!emergencyMode)
                {
                    consecutiveCriticalWindows = 0;
                    EnterEmergencyMode();
                }
                else
                {
                    consecutiveCriticalWindows++;
                    if (!frameRateFallbackEngaged && consecutiveCriticalWindows >= 2)
                    {
                        frameRateFallbackEngaged = true;
                        activeTargetFps = EmergencyFallbackFps;
                        Application.targetFrameRate = activeTargetFps;
                        consecutiveCriticalWindows = 0;
                    }
                }
            }
            else if (stableAtCap && !emergencyMode && activeProfile != "eco")
            {
                consecutiveCriticalWindows = 0;
                consecutiveStableWindows++;
                if (consecutiveStableWindows >= 5 && currentRenderScale < maxRenderScale - 0.01f)
                {
                    SetRenderScale(currentRenderScale + ScaleStep);
                    consecutiveStableWindows = 0;
                }
            }
            else
            {
                consecutiveStableWindows = 0;
                consecutiveCriticalWindows = 0;
            }

            // Scene logic can enable cameras/lights after load. Re-assert the mobile
            // policy at the same low-frequency governor cadence instead of per frame.
            ApplyCameraPolicy();
            ApplyLightPolicy();
            ResetGovernorWindow();
        }

        private void EnterEmergencyMode()
        {
            emergencyMode = true;
            minRenderScale = Mathf.Min(minRenderScale, 0.50f);

            if (activeUrp != null)
            {
                activeUrp.shadowDistance = 0f;
                activeUrp.maxAdditionalLightsCount = 0;
                activeUrp.supportsCameraOpaqueTexture = false;
                activeUrp.supportsCameraDepthTexture = false;
            }

            SetSsao(false);
            ApplyCameraPolicy();
            ApplyLightPolicy();
            SetRenderScale(Mathf.Max(0.50f, currentRenderScale - ScaleStep));
        }

        private void SetRenderScale(float value)
        {
            currentRenderScale = Mathf.Clamp(value, minRenderScale, maxRenderScale);
            currentRenderScale = Mathf.Round(currentRenderScale * 20f) / 20f;

            if (activeUrp != null)
            {
                activeUrp.renderScale = currentRenderScale;
            }
        }

        private static void SetSsao(bool enabled)
        {
            foreach (var feature in Resources.FindObjectsOfTypeAll<ScriptableRendererFeature>())
            {
                if (feature != null && feature.name.IndexOf("ScreenSpaceAmbientOcclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    feature.SetActive(enabled);
                }
            }
        }

        private static void ResetInputState()
        {
            move = Vector2.zero;
            pendingLookDelta = Vector2.zero;
            frameLookDelta = Vector2.zero;
            PendingButtons.Clear();
            FrameButtons.Clear();
        }

        private void ResetGovernorWindow()
        {
            sampleElapsed = 0f;
            sampleFrameSeconds = 0f;
            sampleFrames = 0;
            sampleSlowFrames = 0;
        }

        private static bool TryParsePair(string value, out Vector2 pair)
        {
            pair = Vector2.zero;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var comma = value.IndexOf(',');
            if (comma <= 0 || comma >= value.Length - 1)
            {
                return false;
            }

            if (!float.TryParse(value.Substring(0, comma), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(value.Substring(comma + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                return false;
            }

            pair = new Vector2(x, y);
            return true;
        }
    }
}
