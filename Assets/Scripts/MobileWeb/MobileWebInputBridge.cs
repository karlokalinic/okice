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

        private static readonly HashSet<string> PendingButtons = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FrameButtons = new(StringComparer.OrdinalIgnoreCase);

        private static Vector2 pendingLookDelta;
        private static Vector2 frameLookDelta;
        private static Vector2 move;

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

        private static void ApplyProfile(string profileName)
        {
            var normalized = (profileName ?? string.Empty).Trim().ToLowerInvariant();

            var targetFps = 30;
            var renderScale = 0.90f;
            var msaa = 2;
            var shadowDistance = 24f;
            var shadowCascades = 1;
            var maxAdditionalLights = 2;
            var lodBias = 1.6f;
            var hdr = true;
            var ssao = false;

            switch (normalized)
            {
                case "eco":
                    renderScale = 0.75f;
                    msaa = 1;
                    shadowDistance = 16f;
                    maxAdditionalLights = 1;
                    lodBias = 1.25f;
                    hdr = false;
                    break;

                case "quality":
                    targetFps = 60;
                    renderScale = 1.0f;
                    shadowDistance = 32f;
                    shadowCascades = 2;
                    lodBias = 2.0f;
                    ssao = true;
                    break;
            }

            QualitySettings.vSyncCount = 0;
            QualitySettings.lodBias = lodBias;
            QualitySettings.realtimeReflectionProbes = false;
            Application.targetFrameRate = targetFps;
            Application.backgroundLoadingPriority = ThreadPriority.Low;

            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = renderScale;
                urp.msaaSampleCount = msaa;
                urp.shadowDistance = shadowDistance;
                urp.shadowCascadeCount = shadowCascades;
                urp.maxAdditionalLightsCount = maxAdditionalLights;
                urp.supportsHDR = hdr;
            }

            // Full-resolution SSAO is one of the clearest fill-rate costs in the current PC renderer.
            // Keep it disabled in Eco/Balanced and allow it only in the explicit Quality profile.
            foreach (var feature in Resources.FindObjectsOfTypeAll<ScriptableRendererFeature>())
            {
                if (feature != null && feature.name.IndexOf("ScreenSpaceAmbientOcclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    feature.SetActive(ssao);
                }
            }
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
