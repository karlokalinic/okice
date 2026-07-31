using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(Camera), typeof(VideoPlayer))]
public sealed class StartupLogo : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField, Min(1f)] private float fallbackDelaySeconds = 15f;

    private VideoPlayer videoPlayer;
    private AsyncOperation sceneLoad;
    private bool transitionRequested;

    private void Awake()
    {
        Time.timeScale = 1f;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;

        var cameraComponent = GetComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;
        cameraComponent.cullingMask = 0;
        cameraComponent.allowHDR = false;
        cameraComponent.allowMSAA = false;

        if (cameraComponent.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
        {
            cameraData.renderPostProcessing = false;
            cameraData.allowHDROutput = false;
        }

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = cameraComponent;
        videoPlayer.targetCameraAlpha = 1f;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void Start()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"Startup scene '{nextSceneName}' is not included in Build Settings.");
            return;
        }

        sceneLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        sceneLoad.allowSceneActivation = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        videoPlayer.enabled = false;
        RequestTransition();
#else
        videoPlayer.Prepare();
        StartCoroutine(FallbackAfterDelay());
#endif
    }

    private void OnPrepared(VideoPlayer player)
    {
        if (!transitionRequested)
        {
            player.Play();
        }
    }

    private void OnFinished(VideoPlayer player)
    {
        RequestTransition();
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogError($"Startup video error: {message}");
        RequestTransition();
    }

    private IEnumerator FallbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(fallbackDelaySeconds);
        RequestTransition();
    }

    private void RequestTransition()
    {
        if (transitionRequested)
        {
            return;
        }

        transitionRequested = true;
        StartCoroutine(ActivateWhenReady());
    }

    private IEnumerator ActivateWhenReady()
    {
        while (sceneLoad != null && sceneLoad.progress < 0.9f)
        {
            yield return null;
        }

        if (sceneLoad != null)
        {
            sceneLoad.allowSceneActivation = true;
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}
