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
    /// Production safety layer for restored scenes whose serialized menu callbacks or legacy
    /// post-processing data are incomplete. It does not edit source scenes or run in Edit Mode.
    /// </summary>
    public sealed class ProductionRuntimeRecovery : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";
        private const string GameplaySceneName = "SampleScene";
        private const string ExposurePreference = "GRADOMRAZ_DisplayExposure";
        private const float DefaultExposure = 0.8f;

        private static ProductionRuntimeRecovery instance;

        private bool transitionRunning;
        private GameObject loadingOverlay;
        private GameObject fallbackOptionsOverlay;
        private UnityEngine.UI.Text fallbackOptionsStatus;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var host = new GameObject("GRADOMRAZ Production Runtime Recovery");
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

            ApplyStableFramePacing();
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
            StartCoroutine(StabilizeLoadedScene(scene));
        }

        private IEnumerator StabilizeLoadedScene(Scene scene)
        {
            // Wait until Awake/OnEnable from the restored scene has completed before repairing UI.
            yield return null;

            Time.timeScale = 1f;
            AudioListener.pause = false;
            ApplyStableFramePacing();
            StabilizeGlobalVolumes(scene);

            if (scene.name == MainMenuSceneName)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                RecoverMainMenuButtons(scene);
            }
        }

        private static void ApplyStableFramePacing()
        {
            // The previous bootstrap attempted to render at the monitor refresh rate. On a 200 Hz
            // display that made the restored scene and every fullscreen effect run up to 200 fps.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }

        private void RecoverMainMenuButtons(Scene scene)
        {
            foreach (var button in GetSceneComponents<Button>(scene))
            {
                if (button == null)
                {
                    continue;
                }

                var descriptor = GetButtonDescriptor(button);

                if (IsNewGameButton(descriptor))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => BeginSceneTransition(GameplaySceneName, button));
                    button.interactable = true;
                }
                else if (IsOptionsButton(descriptor))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ToggleOptions(scene, button));
                    button.interactable = true;
                }
                else if (IsQuitButton(descriptor))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(Application.Quit);
                    button.interactable = true;
                }
            }
        }

        private void BeginSceneTransition(string sceneName, Button sourceButton)
        {
            if (transitionRunning)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Cannot start game because scene '{sceneName}' is not present in the build.");
                return;
            }

            transitionRunning = true;
            Time.timeScale = 1f;
            AudioListener.pause = false;

            if (sourceButton != null)
            {
                sourceButton.interactable = false;
            }

            StartCoroutine(LoadSceneProduction(sceneName));
        }

        private IEnumerator LoadSceneProduction(string sceneName)
        {
            ShowLoadingOverlay();
            yield return null;

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"Unity returned no loading operation for scene '{sceneName}'.");
                transitionRunning = false;
                HideLoadingOverlay();
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                UpdateLoadingOverlay(operation.progress);
                yield return null;
            }

            UpdateLoadingOverlay(1f);
            yield return null;
            HideLoadingOverlay();
            transitionRunning = false;
        }

        private void ToggleOptions(Scene scene, Button sourceButton)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;

            var existingPanel = FindExistingOptionsPanel(scene, sourceButton);
            if (existingPanel != null)
            {
                existingPanel.SetActive(!existingPanel.activeSelf);
                return;
            }

            EnsureFallbackOptionsOverlay();
            fallbackOptionsOverlay.SetActive(!fallbackOptionsOverlay.activeSelf);
            UpdateFallbackOptionsStatus();
        }

        private static GameObject FindExistingOptionsPanel(Scene scene, Button sourceButton)
        {
            GameObject bestCandidate = null;
            var bestScore = int.MinValue;

            foreach (var rectTransform in GetSceneComponents<RectTransform>(scene))
            {
                if (rectTransform == null || rectTransform.gameObject == sourceButton.gameObject)
                {
                    continue;
                }

                if (rectTransform.IsChildOf(sourceButton.transform) || sourceButton.transform.IsChildOf(rectTransform))
                {
                    continue;
                }

                var descriptor = Normalize(rectTransform.gameObject.name);
                if (!ContainsAny(descriptor, "OPTIONS", "SETTINGS", "OPCIJE", "POSTAVKE"))
                {
                    continue;
                }

                if (rectTransform.GetComponent<Button>() != null)
                {
                    continue;
                }

                var score = 0;
                if (!rectTransform.gameObject.activeSelf) score += 4;
                if (ContainsAny(descriptor, "PANEL", "MENU", "WINDOW", "ROOT")) score += 3;
                if (rectTransform.GetComponent<CanvasGroup>() != null) score += 2;
                if (rectTransform.GetComponentsInChildren<Button>(true).Length > 0) score += 1;

                if (score > bestScore)
                {
                    bestCandidate = rectTransform.gameObject;
                    bestScore = score;
                }
            }

            return bestCandidate;
        }

        private void StabilizeGlobalVolumes(Scene scene)
        {
            var exposure = PlayerPrefs.GetFloat(ExposurePreference, DefaultExposure);

            foreach (var volume in GetSceneComponents<Volume>(scene))
            {
                if (volume == null || !volume.isGlobal || volume.sharedProfile == null)
                {
                    continue;
                }

                // Accessing profile creates a runtime instance. The source asset stays untouched.
                var profile = volume.profile;
                if (profile == null)
                {
                    continue;
                }

                DisableComponent<MotionBlur>(profile);
                DisableComponent<DepthOfField>(profile);
                DisableComponent<FilmGrain>(profile);
                DisableComponent<Vignette>(profile);
                DisableComponent<ShadowsMidtonesHighlights>(profile);
                DisableComponent<ChannelMixer>(profile);
                DisableComponent<SplitToning>(profile);
                DisableComponent<ColorCurves>(profile);
                DisableComponent<ScreenSpaceLensFlare>(profile);
                DisableComponent<LensDistortion>(profile);

                if (profile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.active = true;
                    colorAdjustments.postExposure.Override(exposure);
                    colorAdjustments.contrast.Override(8f);
                    colorAdjustments.saturation.Override(-4f);
                    colorAdjustments.colorFilter.Override(Color.white);
                }

                if (profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.active = true;
                    bloom.intensity.Override(Mathf.Min(bloom.intensity.value, 0.25f));
                    bloom.highQualityFiltering.Override(false);
                }
            }
        }

        private static void DisableComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var component))
            {
                component.active = false;
            }
        }

        private void EnsureFallbackOptionsOverlay()
        {
            if (fallbackOptionsOverlay != null)
            {
                return;
            }

            fallbackOptionsOverlay = CreateOverlayCanvas("GRADOMRAZ Recovery Options", 32760);
            fallbackOptionsOverlay.transform.SetParent(transform, false);

            var panel = CreateImage("Panel", fallbackOptionsOverlay.transform, new Color(0.025f, 0.025f, 0.03f, 0.96f));
            Stretch(panel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CreateText("Title", panel.transform, "OPCIJE", 34, new Vector2(0.2f, 0.78f), new Vector2(0.8f, 0.9f));
            fallbackOptionsStatus = CreateText("Status", panel.transform, string.Empty, 20, new Vector2(0.2f, 0.64f), new Vector2(0.8f, 0.75f));

            CreateButton(panel.transform, "PUNI ZASLON", new Vector2(0.3f, 0.51f), new Vector2(0.7f, 0.59f), () =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                UpdateFallbackOptionsStatus();
            });

            CreateButton(panel.transform, "V-SYNC", new Vector2(0.3f, 0.41f), new Vector2(0.7f, 0.49f), () =>
            {
                var enableVSync = QualitySettings.vSyncCount == 0;
                QualitySettings.vSyncCount = enableVSync ? 1 : 0;
                Application.targetFrameRate = enableVSync ? -1 : 60;
                UpdateFallbackOptionsStatus();
            });

            CreateButton(panel.transform, "SVJETLIJE", new Vector2(0.3f, 0.31f), new Vector2(0.495f, 0.39f), () => AdjustExposure(0.2f));
            CreateButton(panel.transform, "TAMNIJE", new Vector2(0.505f, 0.31f), new Vector2(0.7f, 0.39f), () => AdjustExposure(-0.2f));
            CreateButton(panel.transform, "NATRAG", new Vector2(0.3f, 0.18f), new Vector2(0.7f, 0.26f), () => fallbackOptionsOverlay.SetActive(false));

            fallbackOptionsOverlay.SetActive(false);
        }

        private void AdjustExposure(float delta)
        {
            var exposure = Mathf.Clamp(
                PlayerPrefs.GetFloat(ExposurePreference, DefaultExposure) + delta,
                -0.5f,
                2.5f);

            PlayerPrefs.SetFloat(ExposurePreference, exposure);
            PlayerPrefs.Save();
            StabilizeGlobalVolumes(SceneManager.GetActiveScene());
            UpdateFallbackOptionsStatus();
        }

        private void UpdateFallbackOptionsStatus()
        {
            if (fallbackOptionsStatus == null)
            {
                return;
            }

            var exposure = PlayerPrefs.GetFloat(ExposurePreference, DefaultExposure);
            var sync = QualitySettings.vSyncCount > 0 ? "UKLJUČEN" : "ISKLJUČEN / 60 FPS";
            fallbackOptionsStatus.text = $"SVJETLINA {exposure:+0.0;-0.0;0.0}    V-SYNC {sync}";
        }

        private void ShowLoadingOverlay()
        {
            if (loadingOverlay == null)
            {
                loadingOverlay = CreateOverlayCanvas("GRADOMRAZ Loading", 32767);
                loadingOverlay.transform.SetParent(transform, false);

                var background = CreateImage("Background", loadingOverlay.transform, Color.black);
                Stretch(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                CreateText("LoadingText", background.transform, "UČITAVANJE… 0%", 26, new Vector2(0.25f, 0.44f), new Vector2(0.75f, 0.56f));
            }

            loadingOverlay.SetActive(true);
        }

        private void UpdateLoadingOverlay(float progress)
        {
            if (loadingOverlay == null)
            {
                return;
            }

            var text = loadingOverlay.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (text != null)
            {
                var normalizedProgress = Mathf.Clamp01(progress / 0.9f);
                text.text = $"UČITAVANJE… {Mathf.RoundToInt(normalizedProgress * 100f)}%";
            }
        }

        private void HideLoadingOverlay()
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }
        }

        private static GameObject CreateOverlayCanvas(string name, int sortingOrder)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return gameObject;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static UnityEngine.UI.Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Text));
            gameObject.transform.SetParent(parent, false);

            var rectTransform = gameObject.GetComponent<RectTransform>();
            Stretch(rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            var text = gameObject.GetComponent<UnityEngine.UI.Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction action)
        {
            var image = CreateImage(label, parent, new Color(0.15f, 0.15f, 0.18f, 1f));
            Stretch(image.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            CreateText("Label", image.transform, label, 20, Vector2.zero, Vector2.one);
            return button;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static T[] GetSceneComponents<T>(Scene scene) where T : Component
        {
            var count = 0;
            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                if (root != null)
                {
                    count += root.GetComponentsInChildren<T>(true).Length;
                }
            }

            var results = new T[count];
            var offset = 0;

            foreach (var root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                var components = root.GetComponentsInChildren<T>(true);
                components.CopyTo(results, offset);
                offset += components.Length;
            }

            return results;
        }

        private static string GetButtonDescriptor(Button button)
        {
            var builder = new StringBuilder(button.gameObject.name);

            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                builder.Append(' ').Append(tmp.text);
            }

            var legacyText = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (legacyText != null)
            {
                builder.Append(' ').Append(legacyText.text);
            }

            return Normalize(builder.ToString());
        }

        private static bool IsNewGameButton(string descriptor)
        {
            return (descriptor.Contains("NEW") && descriptor.Contains("GAME")) ||
                   (descriptor.Contains("NOVA") && descriptor.Contains("IGRA")) ||
                   ContainsAny(descriptor, "START GAME", "POKRENI IGRU");
        }

        private static bool IsOptionsButton(string descriptor)
        {
            return ContainsAny(descriptor, "OPTIONS", "SETTINGS", "OPCIJE", "POSTAVKE");
        }

        private static bool IsQuitButton(string descriptor)
        {
            return ContainsAny(descriptor, "QUIT", "EXIT", "IZLAZ", "IZADI");
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (value.Contains(candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
