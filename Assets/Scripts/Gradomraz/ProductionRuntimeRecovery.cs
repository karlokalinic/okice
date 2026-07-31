using System.Collections;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Karlolegend.Gradomraz
{
    /// <summary>
    /// Minimal production recovery for the restored main menu and the excessively dark runtime
    /// volume. It deliberately avoids dependencies on the scene's missing legacy scripts.
    /// </summary>
    public sealed class ProductionRuntimeRecovery : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";
        private const string GameplayScene = "SampleScene";
        private const string ExposureKey = "GRADOMRAZ_DisplayExposure";
        private const float DefaultExposure = 1f;

        private static ProductionRuntimeRecovery instance;

        private bool loading;
        private bool showOptions;
        private float loadingProgress;
        private string statusMessage = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var host = new GameObject("GRADOMRAZ Production Runtime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<ProductionRuntimeRecovery>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyProductionTiming();

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            loading = false;
            loadingProgress = 1f;
            statusMessage = string.Empty;
            showOptions = false;

            Time.timeScale = 1f;
            AudioListener.pause = false;
            ApplyProductionTiming();
            StabilizeSceneVolumes(scene);

            if (scene.name == MainMenuScene)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                StartCoroutine(RecoverMenuAfterInitialization(scene));
            }
        }

        private IEnumerator RecoverMenuAfterInitialization(Scene scene)
        {
            // Let the original scene finish Awake/OnEnable, then replace its broken callbacks.
            yield return null;

            foreach (var button in Object.FindObjectsByType<Button>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (button == null || button.gameObject.scene != scene)
                {
                    continue;
                }

                var descriptor = Describe(button);

                if (IsNewGame(descriptor))
                {
                    ReplaceClick(button, () => StartGame(button));
                }
                else if (IsOptions(descriptor))
                {
                    ReplaceClick(button, ToggleOptions);
                }
                else if (IsQuit(descriptor))
                {
                    ReplaceClick(button, () => Application.Quit());
                }
            }
        }

        private static void ReplaceClick(Button button, UnityEngine.Events.UnityAction action)
        {
            // Replacing the complete event removes persistent Inspector callbacks as well.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
            button.interactable = true;
        }

        private void StartGame(Button sourceButton)
        {
            if (loading)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(GameplayScene))
            {
                statusMessage = $"SCENA '{GameplayScene}' NIJE UKLJUČENA U BUILD.";
                Debug.LogError(statusMessage);
                return;
            }

            if (sourceButton != null)
            {
                sourceButton.interactable = false;
            }

            StartCoroutine(LoadGameplay());
        }

        private IEnumerator LoadGameplay()
        {
            loading = true;
            showOptions = false;
            loadingProgress = 0f;
            statusMessage = "UČITAVANJE";

            Time.timeScale = 1f;
            AudioListener.pause = false;
            yield return null;

            var operation = SceneManager.LoadSceneAsync(GameplayScene, LoadSceneMode.Single);
            if (operation == null)
            {
                loading = false;
                statusMessage = "UNITY NIJE POKRENUO UČITAVANJE SCENE.";
                Debug.LogError(statusMessage);
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
                yield return null;
            }
        }

        private void ToggleOptions()
        {
            if (loading)
            {
                return;
            }

            showOptions = !showOptions;
            statusMessage = string.Empty;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void AdjustExposure(float delta)
        {
            var exposure = Mathf.Clamp(
                PlayerPrefs.GetFloat(ExposureKey, DefaultExposure) + delta,
                0f,
                2.5f);

            PlayerPrefs.SetFloat(ExposureKey, exposure);
            PlayerPrefs.Save();
            StabilizeSceneVolumes(SceneManager.GetActiveScene());
        }

        private static void ApplyProductionTiming()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        }

        private static void StabilizeSceneVolumes(Scene scene)
        {
            var exposure = PlayerPrefs.GetFloat(ExposureKey, DefaultExposure);

            foreach (var volume in Object.FindObjectsByType<Volume>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (volume == null || volume.gameObject.scene != scene ||
                    !volume.isGlobal || volume.sharedProfile == null)
                {
                    continue;
                }

                // Volume.profile creates a runtime instance; the source asset is not overwritten.
                var profile = volume.profile;
                if (profile == null)
                {
                    continue;
                }

                Disable<MotionBlur>(profile);
                Disable<DepthOfField>(profile);
                Disable<FilmGrain>(profile);
                Disable<Vignette>(profile);
                Disable<ShadowsMidtonesHighlights>(profile);
                Disable<ScreenSpaceLensFlare>(profile);

                if (profile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.active = true;
                    colorAdjustments.postExposure.Override(exposure);
                    colorAdjustments.contrast.Override(10f);
                    colorAdjustments.saturation.Override(-5f);
                    colorAdjustments.colorFilter.Override(Color.white);
                }

                if (profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.active = true;
                    bloom.intensity.Override(Mathf.Min(bloom.intensity.value, 0.2f));
                    bloom.highQualityFiltering.Override(false);
                }
            }
        }

        private static void Disable<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var component))
            {
                component.active = false;
            }
        }

        private void OnGUI()
        {
            if (!loading && !showOptions && string.IsNullOrEmpty(statusMessage))
            {
                return;
            }

            var previousDepth = GUI.depth;
            GUI.depth = -10000;

            if (loading)
            {
                DrawLoadingOverlay();
            }
            else if (showOptions)
            {
                DrawOptionsOverlay();
            }
            else
            {
                DrawStatusOverlay();
            }

            GUI.depth = previousDepth;
        }

        private void DrawLoadingOverlay()
        {
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);

            var percent = Mathf.RoundToInt(loadingProgress * 100f);
            var labelRect = new Rect(0f, Screen.height * 0.45f, Screen.width, 60f);
            var previousSize = GUI.skin.label.fontSize;
            var previousAlignment = GUI.skin.label.alignment;
            GUI.skin.label.fontSize = 26;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(labelRect, $"UČITAVANJE… {percent}%");
            GUI.skin.label.fontSize = previousSize;
            GUI.skin.label.alignment = previousAlignment;
        }

        private void DrawOptionsOverlay()
        {
            var width = Mathf.Min(560f, Screen.width - 40f);
            var height = 430f;
            var area = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Space(12f);
            GUILayout.Label("OPCIJE", CenteredLabel(28));
            GUILayout.Space(20f);

            var exposure = PlayerPrefs.GetFloat(ExposureKey, DefaultExposure);
            GUILayout.Label($"SVJETLINA: {exposure:0.0}", CenteredLabel(20));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TAMNIJE", GUILayout.Height(45f))) AdjustExposure(-0.2f);
            if (GUILayout.Button("SVJETLIJE", GUILayout.Height(45f))) AdjustExposure(0.2f);
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
            if (GUILayout.Button(
                    Screen.fullScreen ? "PROZORSKI NAČIN" : "PUNI ZASLON",
                    GUILayout.Height(45f)))
            {
                Screen.fullScreen = !Screen.fullScreen;
            }

            if (GUILayout.Button(
                    QualitySettings.vSyncCount > 0 ? "V-SYNC: UKLJUČEN" : "V-SYNC: ISKLJUČEN / 60 FPS",
                    GUILayout.Height(45f)))
            {
                var enable = QualitySettings.vSyncCount == 0;
                QualitySettings.vSyncCount = enable ? 1 : 0;
                Application.targetFrameRate = enable ? -1 : 60;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("NATRAG", GUILayout.Height(50f)))
            {
                showOptions = false;
            }

            GUILayout.Space(12f);
            GUILayout.EndArea();
        }

        private void DrawStatusOverlay()
        {
            var rect = new Rect(20f, Screen.height - 90f, Screen.width - 40f, 60f);
            GUI.Box(rect, statusMessage);
        }

        private static GUIStyle CenteredLabel(int fontSize)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                wordWrap = true
            };
        }

        private static string Describe(Button button)
        {
            var builder = new StringBuilder(button.gameObject.name);

            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                builder.Append(' ').Append(tmp.text);
            }

            var legacy = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (legacy != null)
            {
                builder.Append(' ').Append(legacy.text);
            }

            var decomposed = builder.ToString().Normalize(NormalizationForm.FormD);
            builder.Clear();

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool IsNewGame(string value)
        {
            return (value.Contains("NEW") && value.Contains("GAME")) ||
                   (value.Contains("NOVA") && value.Contains("IGRA")) ||
                   value.Contains("START GAME") ||
                   value.Contains("POKRENI IGRU");
        }

        private static bool IsOptions(string value)
        {
            return value.Contains("OPTIONS") || value.Contains("SETTINGS") ||
                   value.Contains("OPCIJE") || value.Contains("POSTAVKE");
        }

        private static bool IsQuit(string value)
        {
            return value.Contains("QUIT") || value.Contains("EXIT") ||
                   value.Contains("IZLAZ") || value.Contains("IZADI");
        }
    }
}
