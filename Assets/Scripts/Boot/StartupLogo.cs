using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public sealed class StartupLogo : MonoBehaviour
{
    [Header("Scene loaded after the logo")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField, Min(1f)] private float fallbackDelaySeconds = 15f;

    private VideoPlayer videoPlayer;
    private AsyncOperation nextSceneLoading;
    private bool transitionStarted;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

#if UNITY_WEBGL && !UNITY_EDITOR
        videoPlayer.enabled = false;
        return;
#else
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.targetCameraAlpha = 1f;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        videoPlayer.prepareCompleted += HandlePrepared;
        videoPlayer.loopPointReached += HandleFinished;
        videoPlayer.errorReceived += HandleError;
#endif
    }

    private void Start()
    {
        // Load the main menu behind the logo, but do not show it yet.
        nextSceneLoading = SceneManager.LoadSceneAsync(
            nextSceneName,
            LoadSceneMode.Single
        );

        if (nextSceneLoading != null)
        {
            nextSceneLoading.allowSceneActivation = false;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(ActivateWebGlSceneWhenReady());
#else
        // Prepare first to avoid starting with missing or black frames.
        videoPlayer.Prepare();
        StartCoroutine(ContinueAfterFallbackDelay());
#endif
    }

    private void HandlePrepared(VideoPlayer player)
    {
        player.Play();
    }

    private void HandleFinished(VideoPlayer player)
    {
        ContinueToNextScene();
    }

    private void HandleError(VideoPlayer player, string message)
    {
        Debug.LogError($"Startup video error: {message}");

        // Never trap the player on the startup screen.
        ContinueToNextScene();
    }

    private IEnumerator ContinueAfterFallbackDelay()
    {
        yield return new WaitForSecondsRealtime(fallbackDelaySeconds);
        ContinueToNextScene();
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

        if (nextSceneLoading != null)
        {
            nextSceneLoading.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadSceneAsync(nextSceneName);
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