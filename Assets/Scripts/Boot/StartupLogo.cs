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
        videoPlayer.enabled = false;
#else
        ConfigureVideoPlayer();
#endif
    }

    private void Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;

#if UNITY_WEBGL && !UNITY_EDITOR
        BeginTransition();
#else
        // Do not load the restored main menu while the 4K creator video is decoding.
        // Running both jobs together caused severe stalls on startup.
        videoPlayer.Prepare();
        StartCoroutine(ContinueIfPreparationStalls());
#endif
    }

    private void ConfigureBootCamera()
    {
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

        player.Play();
        StartCoroutine(ContinueIfPlaybackStalls(player));
    }

    private void HandleFinished(VideoPlayer player)
    {
        BeginTransition();
    }

    private void HandleError(VideoPlayer player, string message)
    {
        Debug.LogError($"Creator video failed to play: {message}");
        BeginTransition();
    }

    private IEnumerator ContinueIfPreparationStalls()
    {
        yield return new WaitForSecondsRealtime(fallbackDelaySeconds);

        if (!transitionStarted && !videoPlayer.isPrepared)
        {
            Debug.LogWarning("Creator video preparation timed out; continuing to the main menu.");
            BeginTransition();
        }
    }

    private IEnumerator ContinueIfPlaybackStalls(VideoPlayer player)
    {
        var expectedDuration = player.length > 0d ? (float)player.length : fallbackDelaySeconds;
        var maximumPlaybackTime = Mathf.Max(fallbackDelaySeconds, expectedDuration + 3f);
        yield return new WaitForSecondsRealtime(maximumPlaybackTime);

        if (!transitionStarted)
        {
            Debug.LogWarning("Creator video playback timed out; continuing to the main menu.");
            BeginTransition();
        }
    }

    private void BeginTransition()
    {
        if (transitionStarted)
        {
            return;
        }

        transitionStarted = true;
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"Startup scene cannot load '{nextSceneName}'. Check Build Settings.");
            yield break;
        }

        // Leave the final decoded logo frame on the camera while the menu loads. Do not expose
        // scene geometry, a Unity skybox or an intermediate post-processing frame.
        yield return null;

        nextSceneLoading = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        if (nextSceneLoading == null)
        {
            Debug.LogError($"Unity did not create a load operation for '{nextSceneName}'.");
            yield break;
        }

        nextSceneLoading.allowSceneActivation = true;

        while (!nextSceneLoading.isDone)
        {
            yield return null;
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
