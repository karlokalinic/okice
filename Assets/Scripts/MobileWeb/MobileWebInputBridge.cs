using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Karlolegend.Gradomraz.MobileWeb
{
    [DefaultExecutionOrder(-10000)]
    public sealed class MobileWebInputBridge : MonoBehaviour
    {
        public const string GameObjectName = "KARLOLEGEND_MobileWebInput";

        private const float GovernorWindowSeconds = 5f;
        private const float ScaleStep = 0.05f;

        private static readonly HashSet<string> PendingButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FrameButtons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static Vector2 pendingLookDelta;
        private static Vector2 frameLookDelta;
        private static Vector2 move;

        private UniversalRenderPipelineAsset activeUrp;
        private string activeProfile = "balanced";
        private int activeTargetFps = 30;
        private float minRenderScale = 0.72f;
        private float maxRenderScale = 0.90f;
        private float currentRenderScale = 0.90f;
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
            ApplyProfile("balanced");
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
            move = Vector2.zero;
            pendingLookDelta = Vector2.zero;
            frameLookDelta = Vector2.zero;
            PendingButtons.Clear();
            FrameButtons.Clear();
        }

        public static bool GetButtonDown(string buttonName)
        {
            return !string.IsNullOrWhiteSpace(buttonName) && FrameButtons.Contains(buttonName);
        }

        // Called from the custom WebGL template through unityInstance.SendMessage.
        public void SetMove(string value)
        {
            if (TryParsePair(value, out var parsed))
            {
                move = Vector2.ClampMagnitude(parsed, 1f);
            }
        }

        // Pointer deltas are accumulated between Unity frames, then exposed once per frame.
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
                move = Vector2.zero;
                pendingLookDelta = Vector2.zero;
                frameLookDelta = Vector2.zero;
                Application.targetFrameRate = 5;
                AudioListener.pause = true;
                return;
            }

            AudioListener.pause = false;
            Application.targetFrameRate = activeTargetFps;
            ResetGovernorWindow();
#endif
        }

        private void ApplyProfile(string profileName)
        {
            activeProfile = (profileName ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(activeProfile))
            {
                activeProfile = "balanced";
            }

            var renderScale = 0.90f;
            var msaa = 2;
            var shadowDistance = 24f;
            var shadowCascades = 1;
            var maxAdditionalLights = 2;
            var lodBias = 1.6f;
            var hdr = true;
            var ssao = false;

            activeTargetFps = 30;
            minRenderScale = 0.72f;
            maxRenderScale = 0.90f;

            switch (activeProfile)
            {
                case "eco":
                    renderScale = 0.75f;
                    msaa = 1;
                    shadowDistance = 16f;
                    maxAdditionalLights = 1;
                    lodBias = 1.25f;
                    hdr = false;
                    minRenderScale = 0.65f;
                    maxRenderScale = 0.75f;
                    break;

                case "quality":
                    activeTargetFps = 60;
                    renderScale = 1.0f;
                    shadowDistance = 32f;
                    shadowCascades = 2;
                    lodBias = 2.0f;
                    ssao = true;
                    minRenderScale = 0.78f;
                    maxRenderScale = 1.0f;
                    break;

                default:
                    activeProfile = "balanced";
                    break;
            }

            QualitySettings.vSyncCount = 0;
            QualitySettings.lodBias = lodBias;
            QualitySettings.realtimeReflectionProbes = false;
            Application.targetFrameRate = activeTargetFps;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

            activeUrp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (activeUrp != null)
            {
                activeUrp.renderScale = renderScale;
                activeUrp.msaaSampleCount = msaa;
                activeUrp.shadowDistance = shadowDistance;
                activeUrp.shadowCascadeCount = shadowCascades;
                activeUrp.maxAdditionalLightsCount = maxAdditionalLights;
                activeUrp.supportsHDR = hdr;
                currentRenderScale = renderScale;
            }

            SetSsao(ssao);
            ResetGovernorWindow();
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

            if (delta > targetFrameSeconds * 1.25f)
            {
                sampleSlowFrames++;
            }

            if (sampleElapsed < GovernorWindowSeconds)
            {
                return;
            }

            var averageFrameSeconds = sampleFrameSeconds / Mathf.Max(1, sampleFrames);
            var slowRatio = (float)sampleSlowFrames / Mathf.Max(1, sampleFrames);
            var underPressure = averageFrameSeconds > targetFrameSeconds * 1.08f || slowRatio > 0.18f;
            var veryStable = averageFrameSeconds < targetFrameSeconds * 0.78f && slowRatio < 0.02f;

            if (underPressure)
            {
                consecutiveStableWindows = 0;

                if (currentRenderScale > minRenderScale + 0.01f)
                {
                    SetRenderScale(currentRenderScale - ScaleStep);
                }
                else if (activeTargetFps > 30)
                {
                    // Thermal safety: if Quality cannot sustain 60 FPS even at its
                    // minimum resolution, preserve visual quality and settle at 30.
                    activeTargetFps = 30;
                    Application.targetFrameRate = 30;
                    SetSsao(false);
                }
            }
            else if (veryStable && activeProfile != "eco")
            {
                consecutiveStableWindows++;
                if (consecutiveStableWindows >= 3 && currentRenderScale < maxRenderScale - 0.01f)
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

            var parts = value.Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                return false;
            }

            pair = new Vector2(x, y);
            return true;
        }
    }
}
