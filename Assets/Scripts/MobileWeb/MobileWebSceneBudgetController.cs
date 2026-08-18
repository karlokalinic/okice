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

        private readonly Dictionary<ParticleSystem, int> particleMaxBaselines = new Dictionary<ParticleSystem, int>();
        private readonly Dictionary<SkinnedMeshRenderer, bool> skinnedOffscreenBaselines = new Dictionary<SkinnedMeshRenderer, bool>();

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
    }
}
