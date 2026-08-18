using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers
{
    /// <summary>
    /// Mobile-Web-only adapter for Dialogue System / PixelCrushers input.
    /// HTML action buttons otherwise bypass PlayMaker and would never reach
    /// InputDeviceManager.IsButtonDown (used by Dialogue System QTEs/back input).
    /// </summary>
    [DefaultExecutionOrder(-9990)]
    public sealed class MobileWebPixelCrushersInput : MonoBehaviour
    {
        public const string GameObjectName = "KARLOLEGEND_PixelCrushersMobileInput";

        private static readonly HashSet<string> PendingButtons =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FrameButtons =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private InputDeviceManager hookedManager;
        private InputDeviceManager.GetButtonDelegate originalGetButtonDown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            var host = GameObject.Find(GameObjectName);
            if (host == null)
            {
                host = new GameObject(GameObjectName);
                DontDestroyOnLoad(host);
            }

            if (host.GetComponent<MobileWebPixelCrushersInput>() == null)
            {
                host.AddComponent<MobileWebPixelCrushersInput>();
            }
#endif
        }

        private void Update()
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            FrameButtons.Clear();
            foreach (var button in PendingButtons)
            {
                FrameButtons.Add(button);
            }
            PendingButtons.Clear();

            EnsureHook();
#endif
        }

        private void OnDisable()
        {
            PendingButtons.Clear();
            FrameButtons.Clear();
            RestoreHook();
        }

        private void OnDestroy()
        {
            RestoreHook();
        }

        public void PressButton(string buttonName)
        {
#if KARLOLEGEND_MOBILE_WEB && UNITY_WEBGL && !UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(buttonName))
            {
                PendingButtons.Add(buttonName.Trim());
            }
#endif
        }

        public void ResetInput(string unused)
        {
            PendingButtons.Clear();
            FrameButtons.Clear();
        }

        private void EnsureHook()
        {
            var manager = InputDeviceManager.instance;
            if (manager == null || ReferenceEquals(manager, hookedManager))
            {
                return;
            }

            RestoreHook();
            hookedManager = manager;
            originalGetButtonDown = manager.GetButtonDown;
            manager.GetButtonDown = GetButtonDownProxy;
        }

        private bool GetButtonDownProxy(string buttonName)
        {
            if (!string.IsNullOrWhiteSpace(buttonName) && FrameButtons.Contains(buttonName))
            {
                return true;
            }

            return originalGetButtonDown != null && originalGetButtonDown(buttonName);
        }

        private void RestoreHook()
        {
            if (hookedManager != null && hookedManager.GetButtonDown == GetButtonDownProxy)
            {
                hookedManager.GetButtonDown = originalGetButtonDown;
            }

            hookedManager = null;
            originalGetButtonDown = null;
        }
    }
}
