using System;
using System.Runtime.InteropServices;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Karlolegend.Gradomraz.MobileWeb
{
    [DefaultExecutionOrder(-9950)]
    public sealed class MobileWebBrowserModeBridge : MonoBehaviour
    {
        private const string HostName = "KARLOLEGEND_MobileWebBrowserMode";
        private const float DialoguePollInterval = 0.15f;
        private const int MenuMode = 0;
        private const int GameplayMode = 1;
        private const int DialogueMode = 2;

        private int publishedMode = -1;
        private float nextDialoguePoll;

#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Karlolegend_SetInputMode(int mode);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            var host = GameObject.Find(HostName);
            if (host == null)
            {
                host = new GameObject(HostName);
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<MobileWebBrowserModeBridge>() == null)
            {
                host.AddComponent<MobileWebBrowserModeBridge>();
            }
#endif
        }

        private void Awake()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded += OnSceneLoaded;
            PublishCurrentMode(true);
#endif
        }

        private void OnDestroy()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        private void Update()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            if (Time.unscaledTime < nextDialoguePoll)
            {
                return;
            }

            nextDialoguePoll = Time.unscaledTime + DialoguePollInterval;
            PublishCurrentMode(false);
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            publishedMode = -1;
            PublishCurrentMode(true);
#endif
        }

#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
        private void PublishCurrentMode(bool force)
        {
            var scene = SceneManager.GetActiveScene();
            var nextMode = MenuMode;

            if (scene.IsValid() && string.Equals(scene.name, "SampleScene", StringComparison.OrdinalIgnoreCase))
            {
                if (DialogueManager.isConversationActive)
                {
                    nextMode = DialogueMode;
                }
                else if (Time.timeScale <= 0.001f)
                {
                    // Pause screens, notes and other modal UI commonly stop game time.
                    // Treat them as UI mode so canvas taps cannot simultaneously move
                    // the player or rotate the camera behind the modal.
                    nextMode = MenuMode;
                }
                else
                {
                    nextMode = GameplayMode;
                }
            }

            if (!force && nextMode == publishedMode)
            {
                return;
            }

            publishedMode = nextMode;
            Karlolegend_SetInputMode(nextMode);
        }
#endif
    }
}
