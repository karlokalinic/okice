using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Web-only, allocation-free runtime quality governor. Does not mutate any
/// scene, PlayMaker FSM, material, camera rig or authored weather controller.
/// </summary>
public sealed class GradomrazWebRuntimeQuality : MonoBehaviour
{
    private const float PreferredScale = 0.94f;
    private const float MinimumScale = 0.72f;
    private const float EvaluationSeconds = 4f;
    private static GradomrazWebRuntimeQuality instance;
    private Volume volume;
    private VolumeProfile profile;
    private Bloom bloom;
    private FilmGrain grain;
    private UniversalRenderPipelineAsset pipeline;
    private float filteredFrameSeconds = 1f / 60f;
    private float evaluationTimer;
    private float scale = PreferredScale;
    private int warmedFrames;
    private Coroutine cameraActivation;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        // In WebGL this saves GPU memory and keeps a paused browser tab idle.
        // Desktop builds and PlayMaker editor testing retain existing settings.
        if (Application.platform != RuntimePlatform.WebGLPlayer || instance != null)
            return;

        var host = new GameObject("GRADOMRAZ Web Runtime Quality");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<GradomrazWebRuntimeQuality>();
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = false;
        QualitySettings.vSyncCount = 0;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 256f;
        pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline != null)
        {
            scale = Mathf.Clamp(pipeline.renderScale, MinimumScale, PreferredScale);
        }

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "GRADOMRAZ — restrained night-grade (runtime)";
        var grade = profile.Add<ColorAdjustments>(true);
        grade.contrast.Override(9f);
        grade.saturation.Override(-10f);
        grade.postExposure.Override(-0.05f);

        var whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.temperature.Override(-7f);
        whiteBalance.tint.Override(-3f);

        var tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.12f);
        bloom.intensity.Override(0.14f);
        bloom.scatter.Override(0.65f);
        bloom.highQualityFiltering.Override(false);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.17f);
        vignette.smoothness.Override(0.37f);
        vignette.rounded.Override(false);

        grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Medium1);
        grain.intensity.Override(0.10f);
        grain.response.Override(0.82f);

        volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = -10f; // Never override higher-priority authored volumes.
        volume.weight = 1f;
        volume.sharedProfile = profile;
        volume.enabled = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool inGameplay = scene.IsValid() && scene.name == "SampleScene";
        if (volume != null)
            volume.enabled = inGameplay;

        if (cameraActivation != null)
        {
            StopCoroutine(cameraActivation);
            cameraActivation = null;
        }

        if (inGameplay)
        {
            // Preserve chapter and UI cameras. Attach post to only the enabled
            // gameplay camera, after scene-initialization components have run.
            cameraActivation = StartCoroutine(ActivateCameraAfterScene(scene));
        }
    }

    private IEnumerator ActivateCameraAfterScene(Scene scene)
    {
        yield return null;
        if (!scene.IsValid() || !scene.isLoaded)
            yield break;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
            {
                if (cam == null || !cam.enabled || !cam.gameObject.activeInHierarchy)
                    continue;

                var data = cam.GetComponent<UniversalAdditionalCameraData>();
                if (data == null)
                    data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                data.renderPostProcessing = true;
                yield break;
            }
        }
        Debug.LogWarning("[GRADOMRAZ] No enabled gameplay camera for optional Web post-processing.");
        cameraActivation = null;
    }

    private void Update()
    {
        if (volume == null || !volume.enabled || pipeline == null)
            return;

        float delta = Time.unscaledDeltaTime;
        if (delta <= 0f || delta > 0.4f)
            return; // background-tab spikes are not evidence of a slow GPU.

        filteredFrameSeconds = Mathf.Lerp(filteredFrameSeconds, delta, 0.045f);
        warmedFrames++;
        if (warmedFrames < 180)
            return;

        evaluationTimer += delta;
        if (evaluationTimer < EvaluationSeconds)
            return;
        evaluationTimer = 0f;

        float nextScale = scale;
        if (filteredFrameSeconds > 0.022f)
            nextScale -= 0.045f;
        else if (filteredFrameSeconds < 0.0145f)
            nextScale += 0.025f;

        nextScale = Mathf.Clamp(nextScale, MinimumScale, PreferredScale);
        if (Mathf.Abs(scale - nextScale) < 0.001f)
            return;

        scale = nextScale;
        pipeline.renderScale = scale;
        // The expensive noise pass disappears only under sustained pressure.
        if (grain != null)
            grain.active = scale >= 0.82f;
        if (bloom != null)
            bloom.active = scale >= 0.77f;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
        if (profile != null)
            Destroy(profile);
    }
}
