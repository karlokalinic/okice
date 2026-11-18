using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tiny Web-only bridge for the existing MainMenu UI.
/// Unity's normal Button.onClick / PlayMaker remains authoritative. If the
/// browser pointer reaches the canvas but the Input System misses a click,
/// the bridge forwards it through the same Button.onClick after a frame.
/// Lives across scene changes only to keep SendMessage available and silent
/// during gameplay. Only three button references exist while in MainMenu.
/// </summary>
public sealed class GradomrazWebMenuInputBridge : MonoBehaviour
{
    private static GradomrazWebMenuInputBridge instance;

    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private float lastPlayClick = -100f;
    private float lastOptionsClick = -100f;
    private float lastQuitClick = -100f;
    private readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    // Called by the Web-only scene staging step for reference integrity.
    public void Configure(Button play, Button options, Button quit)
    {
        playButton = play;
        optionsButton = options;
        quitButton = quit;
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (instance != this)
            return;
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    private void Start()
    {
        if (instance == this)
            BindScene(SceneManager.GetActiveScene());
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindScene(scene);
    }

    private void SceneUnloaded(Scene scene)
    {
        if (scene.name == "MainMenu")
            UnbindButtons();
    }

    private void BindScene(Scene scene)
    {
        UnbindButtons();
        if (!scene.IsValid() || !scene.isLoaded || scene.name != "MainMenu")
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                switch (button.name)
                {
                    case "PlayButton": playButton = button; break;
                    case "OptionsButton": optionsButton = button; break;
                    case "QuitButton": quitButton = button; break;
                }
            }
        }
        if (playButton == null || optionsButton == null || quitButton == null)
        {
            Debug.LogError("[GRADOMRAZ WEB MENU] Missing original menu buttons.");
            UnbindButtons();
            return;
        }

        playButton.onClick.AddListener(RecordPlay);
        optionsButton.onClick.AddListener(RecordOptions);
        quitButton.onClick.AddListener(RecordQuit);
        lastPlayClick = lastOptionsClick = lastQuitClick = -100f;
        Debug.Log("[GRADOMRAZ WEB MENU] Original PlayMaker buttons bound.");
    }

    private void UnbindButtons()
    {
        if (playButton != null) playButton.onClick.RemoveListener(RecordPlay);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(RecordOptions);
        if (quitButton != null) quitButton.onClick.RemoveListener(RecordQuit);
        playButton = optionsButton = quitButton = null;
    }

    private void RecordPlay()
    {
        lastPlayClick = Time.realtimeSinceStartup;
        Debug.Log("[GRADOMRAZ WEB MENU] PlayButton.onClick");
    }
    private void RecordOptions()
    {
        lastOptionsClick = Time.realtimeSinceStartup;
        Debug.Log("[GRADOMRAZ WEB MENU] OptionsButton.onClick");
    }
    private void RecordQuit()
    {
        lastQuitClick = Time.realtimeSinceStartup;
        Debug.Log("[GRADOMRAZ WEB MENU] QuitButton.onClick");
    }

    // Browser calls this via window.unityInstance.SendMessage after pointerup.
    public void WebPointerUp(string canvasPosition)
    {
        if (!isActiveAndEnabled || SceneManager.GetActiveScene().name != "MainMenu" ||
            string.IsNullOrEmpty(canvasPosition))
            return;

        int comma = canvasPosition.IndexOf(',');
        if (comma < 1 ||
            !float.TryParse(canvasPosition.Substring(0, comma), NumberStyles.Float,
                CultureInfo.InvariantCulture, out float x) ||
            !float.TryParse(canvasPosition.Substring(comma + 1), NumberStyles.Float,
                CultureInfo.InvariantCulture, out float y))
            return;

        Vector2 point = new Vector2(x, y);
        Button hit = HitTest(playButton, point) ? playButton :
                     HitTest(optionsButton, point) ? optionsButton :
                     HitTest(quitButton, point) ? quitButton : null;
        if (hit == null) hit = VisibleMainMenuLabelAt(point);
        if (hit == null)
        {
            Debug.Log("[GRADOMRAZ WEB MENU] Pointer missed buttons at " + x + "," + y);
            return;
        }
        Debug.Log("[GRADOMRAZ WEB MENU] Pointer hit " + hit.name + " at " + x + "," + y);
        StartCoroutine(ForwardIfNativeUiMissed(hit));
    }

    private Button VisibleMainMenuLabelAt(Vector2 point)
    {
        // Final Web-only fallback for the original TMP labels, whose visual
        // glyphs overflow 100px UGUI rects. Screen-height-derived coordinates
        // follow the menu CanvasScaler's MatchHeight layout as it resizes.
        // Only used after all three actual RectTransforms were checked.
        if (Screen.width <= 0 || Screen.height <= 0)
            return null;
        float xCenter = Screen.width * 0.5f + Screen.height / 6f;
        if (Mathf.Abs(point.x - xCenter) > Screen.height * 0.25f)
            return null;
        float vertical = point.y / Screen.height;
        if (vertical >= 0.390f && vertical <= 0.490f) return playButton;
        if (vertical >= 0.260f && vertical <= 0.365f) return optionsButton;
        if (vertical >= 0.170f && vertical <= 0.255f) return quitButton;
        return null;
    }
    private static bool HitTest(Button button, Vector2 point)
    {
        if (button == null || !button.isActiveAndEnabled || !button.IsInteractable() ||
            !button.gameObject.activeInHierarchy)
            return false;

        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        var rect = (RectTransform)button.transform;
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, point, camera))
            return true;
        // Existing TMP labels overflow their original 100x21.7 UI button
        // rectangles. Expand the raycast in local canvas units without
        // shifting, resizing or reflowing the visible typography.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, point, camera, out Vector2 local))
        {
            Rect bounds = rect.rect;
            if (local.x >= bounds.xMin - 135f && local.x <= bounds.xMax + 135f &&
                local.y >= bounds.yMin - 20f && local.y <= bounds.yMax + 20f)
                return true;
        }

        // The label is wider than the original 100px Button hit rectangle.
        // Honor the visible text's real hit area without changing its layout.
        var label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        return label != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                label.rectTransform, point, camera);
    }

    private IEnumerator ForwardIfNativeUiMissed(Button button)
    {
        yield return endOfFrame;
        if (button == null || !button.gameObject.activeInHierarchy || !button.IsInteractable())
            yield break;

        float lastClick = button == playButton ? lastPlayClick :
                          button == optionsButton ? lastOptionsClick : lastQuitClick;
        if (Time.realtimeSinceStartup - lastClick < 0.35f)
            yield break;

        Debug.Log("[GRADOMRAZ WEB MENU] Forwarding missed browser click to PlayMaker via " + button.name);
        button.onClick.Invoke();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneUnloaded -= SceneUnloaded;
    }

    private void OnDestroy()
    {
        UnbindButtons();
        if (instance == this)
            instance = null;
    }
}
