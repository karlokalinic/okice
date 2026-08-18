using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.MobileWeb
{
    /// <summary>
    /// Applies conservative, reversible scene-level budgets without changing
    /// gameplay objects, colliders, animator update semantics, or one-shot VFX.
    /// Scene component discovery happens on scene load; the recurring budget pass
    /// iterates cached components and performs no scene-wide object search.
    /// </summary>
    [DefaultExecutionOrder(-9980)]
    public sealed class MobileWebSceneBudgetController : MonoBehaviour
    {
        private const float ReauditIntervalSeconds = 2f;
        private const float StaticLightCullMargin = 8f;

        private struct LightActivityState
        {
            public bool authoredEnabled;
            public bool managedDisabled;
        }

        private readonly List<ParticleSystem> loopingParticles = new List<ParticleSystem>();
        private readonly List<Light> distanceCullLights = new List<Light>();
        private readonly Dictionary<ParticleSystem, int> particleMaxBaselines = new Dictionary<ParticleSystem, int>();
        private readonly Dictionary<Light, LightActivityState> lightActivity = new Dictionary<Light, LightActivityState>();

        private float nextAuditTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            var host = GameObject.Find("KARLOLEGEND_MobileSceneBudget");
            if (host == null)
            {
                host = new GameObject("KARLOLEGEND_MobileSceneBudget");
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<MobileWebSceneBudgetController>() == null)
            {
                host.AddComponent<MobileWebSceneBudgetController>();
            }
#endif
        }

        private void Awake()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        private void OnDestroy()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RestoreManagedLights();
#endif
        }

        private void Start()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            CacheSceneComponents();
            ApplyBudgets();
#endif
        }

        private void Update()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            if (Time.unscaledTime < nextAuditTime)
            {
                return;
            }

            ApplyBudgets();
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreManagedLights();
            CacheSceneComponents();
            nextAuditTime = 0f;
            ApplyBudgets();
        }

        private void CacheSceneComponents()
        {
            loopingParticles.Clear();
            distanceCullLights.Clear();
            particleMaxBaselines.Clear();
            lightActivity.Clear();

            foreach (var particleSystem in Resources.FindObjectsOfTypeAll<ParticleSystem>())
            {
                if (particleSystem == null || !particleSystem.gameObject.scene.IsValid())
                {
                    continue;
                }

                var main = particleSystem.main;
                if (!main.loop)
                {
                    continue;
                }

                loopingParticles.Add(particleSystem);
                particleMaxBaselines[particleSystem] = main.maxParticles;
            }

            foreach (var light in Resources.FindObjectsOfTypeAll<Light>())
            {
                if (!CanDistanceCull(light))
                {
                    continue;
                }

                distanceCullLights.Add(light);
                lightActivity[light] = new LightActivityState
                {
                    authoredEnabled = light.enabled,
                    managedDisabled = false
                };
            }
        }

        private void ApplyBudgets()
        {
            nextAuditTime = Time.unscaledTime + ReauditIntervalSeconds;

            var renderScale = GetRenderScale();
            var particleCap = Application.targetFrameRate <= 24
                ? 96
                : renderScale >= 0.82f ? 384
                : renderScale >= 0.68f ? 256
                : 128;

            ApplyParticleBudget(particleCap);
            ApplySafeStaticLightCulling();
        }

        private static float GetRenderScale()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
            {
                urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            }

            return urp != null ? urp.renderScale : 1f;
        }

        private void ApplyParticleBudget(int maxParticles)
        {
            for (var index = 0; index < loopingParticles.Count; index++)
            {
                var particleSystem = loopingParticles[index];
                if (particleSystem == null || !particleMaxBaselines.TryGetValue(particleSystem, out var baseline))
                {
                    continue;
                }

                var main = particleSystem.main;
                var capped = Mathf.Min(baseline, maxParticles);
                if (main.maxParticles != capped)
                {
                    main.maxParticles = capped;
                }
            }
        }

        private void ApplySafeStaticLightCulling()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                RestoreManagedLights();
                return;
            }

            var cameraPosition = camera.transform.position;
            for (var index = 0; index < distanceCullLights.Count; index++)
            {
                var light = distanceCullLights[index];
                if (light == null || !lightActivity.TryGetValue(light, out var state))
                {
                    continue;
                }

                // When we are not overriding the light, preserve an authored change.
                // Script-bearing light objects are excluded from this list entirely.
                if (!state.managedDisabled)
                {
                    state.authoredEnabled = light.enabled;
                }

                if (!state.authoredEnabled)
                {
                    state.managedDisabled = false;
                    lightActivity[light] = state;
                    continue;
                }

                var effectiveDistance = Mathf.Max(1f, light.range + StaticLightCullMargin);
                var shouldBeVisible = (light.transform.position - cameraPosition).sqrMagnitude <= effectiveDistance * effectiveDistance;

                if (!shouldBeVisible && !state.managedDisabled)
                {
                    light.enabled = false;
                    state.managedDisabled = true;
                }
                else if (shouldBeVisible && state.managedDisabled)
                {
                    light.enabled = state.authoredEnabled;
                    state.managedDisabled = false;
                }

                lightActivity[light] = state;
            }
        }

        private static bool CanDistanceCull(Light light)
        {
            if (light == null || !light.gameObject.scene.IsValid() || !light.gameObject.isStatic)
            {
                return false;
            }

            if (light.type != LightType.Point && light.type != LightType.Spot)
            {
                return false;
            }

            // Gameplay/flicker/puzzle light objects usually carry a script. Leave all
            // such enable/disable semantics entirely under the game's control.
            return !light.TryGetComponent<MonoBehaviour>(out _);
        }

        private void RestoreManagedLights()
        {
            foreach (var pair in lightActivity)
            {
                var light = pair.Key;
                var state = pair.Value;
                if (light != null && state.managedDisabled)
                {
                    light.enabled = state.authoredEnabled;
                }
            }
        }
    }
}
