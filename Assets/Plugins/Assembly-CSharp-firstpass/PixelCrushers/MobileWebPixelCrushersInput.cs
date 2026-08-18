using System;
using System.Reflection;
using UnityEngine;

namespace PixelCrushers
{
    /// <summary>
    /// Mobile-Web-only adapter for Dialogue System / PixelCrushers input.
    /// PixelCrushers lives in the first-pass assembly, while the gameplay mobile
    /// bridge lives in Assembly-CSharp. Reflection is resolved once and converted
    /// to a typed delegate so there is no per-frame reflection cost.
    /// </summary>
    [DefaultExecutionOrder(-9990)]
    public sealed class MobileWebPixelCrushersInput : MonoBehaviour
    {
        public const string GameObjectName = "KARLOLEGEND_PixelCrushersMobileInput";
        private const string BridgeTypeName =
            "Karlolegend.Gradomraz.MobileWeb.MobileWebInputBridge, Assembly-CSharp";

        private InputDeviceManager hookedManager;
        private InputDeviceManager.GetButtonDelegate originalGetButtonDown;
        private Func<string, bool> mobileGetButtonDown;
        private bool bridgeResolutionAttempted;

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
            ResolveMobileBridge();
            EnsureHook();
#endif
        }

        private void OnDisable()
        {
            RestoreHook();
        }

        private void OnDestroy()
        {
            RestoreHook();
        }

        private void ResolveMobileBridge()
        {
            if (mobileGetButtonDown != null || bridgeResolutionAttempted)
            {
                return;
            }

            bridgeResolutionAttempted = true;
            var bridgeType = Type.GetType(BridgeTypeName, throwOnError: false);
            var method = bridgeType?.GetMethod(
                "GetButtonDown",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            if (method == null)
            {
                Debug.LogWarning("Mobile WebGL: could not resolve MobileWebInputBridge.GetButtonDown for PixelCrushers.");
                return;
            }

            try
            {
                mobileGetButtonDown =
                    (Func<string, bool>)Delegate.CreateDelegate(typeof(Func<string, bool>), method);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Mobile WebGL: failed to bind PixelCrushers mobile button delegate: " + exception.Message);
            }
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
            if (mobileGetButtonDown != null && mobileGetButtonDown(buttonName))
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
