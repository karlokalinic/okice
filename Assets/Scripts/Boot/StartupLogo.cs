using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public sealed class StartupLogo : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField, Min(1f)] private float fallbackDelaySeconds = 15f;

    private VideoPlayer videoPlayer;
    private Coroutine preparationTimeout;
    private bool transitionStarted;

    private void Awake()
    {
        Time.timeScale = 1f;

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.targetCameraAlpha = 1f;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        videoPlayer.prepareCompleted += HandlePrepared;
        videoPlayer.loopPointReached += HandleFinished;
        videoPlayer.errorReceived += HandleError;
    }

    private void Start()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"Startup scene '{nextSceneName}' is not included in Build Settings.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        ContinueToNextScene();
#else
        preparationTimeout = StartCoroutine(ContinueIfPreparationFails());
        videoPlayer.Prepare();
#endif
    }

    private void HandlePrepared(VideoPlayer player)
    {
        if (transitionStarted)
        {
            return;
        }

        if (preparationTimeout != null)
        {
            StopCoroutine(preparationTimeout);
            preparationTimeout = null;
        }

        player.Play();
    }

    private void HandleFinished(VideoPlayer player)
    {
        ContinueToNextScene();
    }

    private void HandleError(VideoPlayer player, string message)
    {
        Debug.LogError($"Startup video error: {message}");
        ContinueToNextScene();
    }

    private IEnumerator ContinueIfPreparationFails()
    {
        yield return new WaitForSecondsRealtime(fallbackDelaySeconds);
        ContinueToNextScene();
    }

    private void ContinueToNextScene()
    {
        if (transitionStarted)
        {
            return;
        }

        transitionStarted = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
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
