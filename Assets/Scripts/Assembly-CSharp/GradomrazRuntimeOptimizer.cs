using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GradomrazRuntimeOptimizer : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

    private Coroutine openingGate;
    private float timeScaleBeforeGate;
    private bool audioPausedBeforeGate;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        GameObject optimizer = new GameObject("GRADOMRAZ Runtime Optimizer");
        DontDestroyOnLoad(optimizer);
        optimizer.AddComponent<GradomrazRuntimeOptimizer>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.name == GameplaySceneName;
        if (isGameplayScene)
        {
            timeScaleBeforeGate = Time.timeScale > 0f ? Time.timeScale : 1f;
            audioPausedBeforeGate = AudioListener.pause;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        if (isGameplayScene)
        {
            if (openingGate != null)
            {
                StopCoroutine(openingGate);
            }

            openingGate = StartCoroutine(ReleaseOpeningAfterFirstFrames());
        }
    }

    private IEnumerator ReleaseOpeningAfterFirstFrames()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Time.timeScale = timeScaleBeforeGate;
        AudioListener.pause = audioPausedBeforeGate;
        openingGate = null;
    }
}