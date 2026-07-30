using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(VideoPlayer))]
public sealed class StartupLogo : MonoBehaviour
{
    [Header("Scene loaded after the creator video")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField, Min(1f)] private float fallbackDelaySeconds = 15f;

    private Camera bootCamera;
    private VideoPlayer videoPlayer;
    private AsyncOperation nextSceneLoading;
    private bool transitionStarted;

    private void Awake()
    {
        bootCamera = GetComponent<Camera>();
        videoPlayer = GetComponent<VideoPlayer>();

        ConfigureBootCamera();

#if UNITY_WEBGL && !UNITY_EDITOR
        // Unity VideoPlayer support differs between browsers. WebGL proceeds directly rather
        // than exposing an unprepared or permanently black video surface.
        videoPlayer.enabled = false;
#else
        ConfigureVideoPlayer();
#endif
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        BeginPreloadingNextScene();
        StartCoroutine(ActivateWebGlSceneWhenReady());
#else
        // Give video decoding first access to disk/GPU resources. Loading the menu before
        // Prepare() caused the startup camera to remain visible as an empty black Unity scene.
        videoPlayer.Prepare();
        StartCoroutine(ContinueAfterFallbackDelay());
#endif
    }

    private void ConfigureBootCamera()
    {
        // The only valid image in this scene is the creator video. Nothing from the scene or
        // URP post-processing is allowed to flash before the first decoded video frame.
        bootCamera.clearFlags = CameraClearFlags.SolidColor;
        bootCamera.backgroundColor = Color.black;
        bootCamera.cullingMask = 0;
        bootCamera.allowHDR = false;
        bootCamera.allowMSAA = false;

        if (bootCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
        {
            cameraData.renderPostProcessing = false;
            cameraData.allowHDROutput = false;
        }
    }

    private void ConfigureVideoPlayer()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = bootCamera;
        videoPlayer.targetCameraAlpha = 1f;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        videoPlayer.prepareCompleted += HandlePrepared;
        videoPlayer.loopPointReached += HandleFinished;
        videoPlayer.errorReceived += HandleError;
    }

    private void HandlePrepared(VideoPlayer player)
    {
        if (transitionStarted)
        {
            return;
        }

        BeginPreloadingNextScene();
        player.Play();
    }

    private void HandleFinished(VideoPlayer player)
    {
        ContinueToNextScene();
    }

    private void HandleError(VideoPlayer player, string message)
    {
        Debug.LogError($"Creator video failed to play: {message}");
        ContinueToNextScene();
    }

    private void BeginPreloadingNextScene()
    {
        if (nextSceneLoading != null || transitionStarted)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"Startup scene cannot load '{nextSceneName}'. Check Build Settings.");
            return;
        }

        nextSceneLoading = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        if (nextSceneLoading != null)
        {
            nextSceneLoading.allowSceneActivation = false;
        }
    }

    private IEnumerator ContinueAfterFallbackDelay()
    {
        yield return new WaitForSecondsRealtime(fallbackDelaySeconds);

        if (!transitionStarted)
        {
            Debug.LogWarning("Creator video timed out; continuing to the main menu.");
            ContinueToNextScene();
        }
    }

    private IEnumerator ActivateWebGlSceneWhenReady()
    {
        while (nextSceneLoading != null && nextSceneLoading.progress < 0.9f)
        {
            yield return null;
        }

        ContinueToNextScene();
    }

    private void ContinueToNextScene()
    {
        if (transitionStarted)
        {
            return;
        }

        transitionStarted = true;
        videoPlayer?.Stop();

        if (nextSceneLoading != null)
        {
            nextSceneLoading.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= HandlePrepared;
        videoPlayer.loopPointReached -= HandleFinished;
        videoPlayer.errorReceived -= HandleError;
    }
}
