using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.MobileWeb
{
    /// <summary>
    /// Applies conservative, reversible scene-level budgets that are safe to enforce
    /// without knowing game-specific object semantics. It intentionally avoids
    /// disabling gameplay GameObjects, colliders, animators, or one-shot VFX.
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

        private readonly Dictionary<ParticleSystem, int> particleMaxBaselines = new Dictionary<ParticleSystem, int>();
        private readonly Dictionary<SkinnedMeshRenderer, bool> skinnedOffscreenBaselines = new Dictionary<SkinnedMeshRenderer, bool>();
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
            nextAuditTime = 0f;
            RemoveDestroyedReferences();
            ApplyBudgets();
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
            ApplySkinnedMeshBudget();
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
            foreach (var particleSystem in Resources.FindObjectsOfTypeAll<ParticleSystem>())
            {
                if (particleSystem == null || !particleSystem.gameObject.scene.IsValid())
                {
                    continue;
                }

                var main = particleSystem.main;

                // One-shot gameplay effects are left untouched. Only looping systems
                // receive a population ceiling, which primarily targets weather,
                // ambience, smoke, dust, etc. that can accumulate fill-rate cost.
                if (!main.loop)
                {
                    continue;
                }

                if (!particleMaxBaselines.TryGetValue(particleSystem, out var baseline))
                {
                    baseline = main.maxParticles;
                    particleMaxBaselines[particleSystem] = baseline;
                }

                var capped = Mathf.Min(baseline, maxParticles);
                if (main.maxParticles != capped)
                {
                    main.maxParticles = capped;
                }
            }
        }

        private void ApplySkinnedMeshBudget()
        {
            foreach (var renderer in Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>())
            {
                if (renderer == null || !renderer.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (!skinnedOffscreenBaselines.ContainsKey(renderer))
                {
                    skinnedOffscreenBaselines[renderer] = renderer.updateWhenOffscreen;
                }

                // Rendering an offscreen skinned mesh has no visual result. This does
                // not disable its GameObject, Animator, scripts, colliders, or events.
                renderer.updateWhenOffscreen = false;
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
            foreach (var light in Resources.FindObjectsOfTypeAll<Light>())
            {
                if (!CanDistanceCull(light))
                {
                    continue;
                }

                if (!lightActivity.TryGetValue(light, out var state))
                {
                    state = new LightActivityState
                    {
                        authoredEnabled = light.enabled,
                        managedDisabled = false
                    };
                }

                // When we are not currently overriding this light, any authored/script
                // change becomes the new baseline. This prevents the budgeter from
                // resurrecting a light that gameplay deliberately switched off.
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

            // If a script lives on the light GameObject, assume it may be gameplay,
            // flicker, alarm, puzzle, etc. and leave enable/disable semantics alone.
            return light.GetComponents<MonoBehaviour>().Length == 0;
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

        private void RemoveDestroyedReferences()
        {
            RemoveDestroyedKeys(particleMaxBaselines);
            RemoveDestroyedKeys(skinnedOffscreenBaselines);
            RemoveDestroyedKeys(lightActivity);
        }

        private static void RemoveDestroyedKeys<TComponent, TValue>(Dictionary<TComponent, TValue> dictionary)
            where TComponent : UnityEngine.Object
        {
            var remove = ListPool<TComponent>.Get();
            try
            {
                foreach (var pair in dictionary)
                {
                    if (pair.Key == null)
                    {
                        remove.Add(pair.Key);
                    }
                }

                foreach (var key in remove)
                {
                    dictionary.Remove(key);
                }
            }
            finally
            {
                ListPool<TComponent>.Release(remove);
            }
        }

        // Tiny local pool avoids allocating temporary key lists on scene transitions.
        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
