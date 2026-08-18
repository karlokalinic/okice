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

        private static readonly HashSet<string> PendingButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FrameButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static Vector2 pendingLookDelta;
        private static Vector2 frameLookDelta;
        private static Vector2 move;

        private UniversalRenderPipelineAsset activeUrp;
        private string activeProfile = "balanced";
        private int activeTargetFps = 30;
        private float minRenderScale = 0.55f;
        private float maxRenderScale = 0.78f;
        private float currentRenderScale = 0.72f;
        private float baseShadowDistance = 12f;
        private int baseAdditionalLights = 1;
        private bool allowPostProcessing = true;
        private bool allowDepthTexture = true;
        private bool emergencyMode;
        private float sampleElapsed;
        private float sampleFrameSeconds;
        private int sampleFrames;
        private int sampleSlowFrames;
        private int consecutiveStableWindows;
        private bool pageHidden;

        public static Vector2 Move => move;
        public static Vector2 LookDelta => frameLookDelta;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            if (GameObject.Find(GameObjectName) != null)
            {
                return;
            }

            var host = new GameObject(GameObjectName);
            DontDestroyOnLoad(host);
            host.AddComponent<MobileWebInputBridge>();
#endif
        }

        private void Awake()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            Application.runInBackground = false;
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
            ResetGovernorWindow();
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCameraPolicy();
        }

        private void ApplyProfile(string profileName)
        {
            activeProfile = (profileName ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(activeProfile))
            {
                activeProfile = "balanced";
            }

            activeTargetFps = 30;
            emergencyMode = false;

            var renderScale = 0.72f;
            var msaa = 1;
            var shadowCascades = 1;
            var lodBias = 1.10f;

            minRenderScale = 0.55f;
            maxRenderScale = 0.78f;
            baseShadowDistance = 12f;
            baseAdditionalLights = 1;
            allowPostProcessing = true;
            allowDepthTexture = true;

            switch (activeProfile)
            {
                case "eco":
                    renderScale = 0.60f;
                    minRenderScale = 0.50f;
                    maxRenderScale = 0.65f;
                    baseShadowDistance = 0f;
                    baseAdditionalLights = 0;
                    lodBias = 0.90f;
                    allowPostProcessing = false;
                    allowDepthTexture = false;
                    break;

                case "quality":
                    renderScale = 0.85f;
                    msaa = 2;
                    minRenderScale = 0.65f;
                    maxRenderScale = 0.90f;
                    baseShadowDistance = 18f;
                    baseAdditionalLights = 1;
                    lodBias = 1.35f;
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
            Application.targetFrameRate = activeTargetFps;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

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
            consecutiveStableWindows = 0;
            ResetGovernorWindow();
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
                    SetRenderScale(currentRenderScale - ScaleStep);
                }
                else if (!emergencyMode)
                {
                    EnterEmergencyMode();
                }
                else if (currentRenderScale > 0.50f + 0.01f)
                {
                    minRenderScale = 0.50f;
                    SetRenderScale(currentRenderScale - ScaleStep);
                }
            }
            else if (stableAtCap && !emergencyMode && activeProfile != "eco")
            {
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
            }

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
