using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Opt-in remote visual-only scene bundle loader. This component does not
/// change PlayMaker entities, object references, collision or scene order.
/// A region must be authored as its OWN visual-only AssetBundle scene and
/// hosted outside the initial WebGL .data file.
/// </summary>
[DisallowMultipleComponent]
public sealed class GradomrazRemoteVisualRegion : MonoBehaviour
{
    [Tooltip("Example: /unity/game-01/Streaming/outside-visuals.bundle. Empty = disabled.")]
    [SerializeField] private string assetBundleUrl = "";
    [SerializeField] private bool loadWhenActive = false;
    [SerializeField] private bool releaseWhenInactive = true;

    private string requestedUrl;

    private void OnEnable()
    {
        if (loadWhenActive)
            Acquire();
    }

    private void OnDisable()
    {
        if (releaseWhenInactive)
            Release();
    }

    private void OnDestroy()
    {
        Release();
    }

    /// <summary>Explicit preload call; can be invoked before a chapter transition.</summary>
    public void Acquire()
    {
        if (!Application.isPlaying || string.IsNullOrWhiteSpace(assetBundleUrl))
            return;

        string url = Resolve(assetBundleUrl);
        if (requestedUrl == url)
            return;
        Release();
        requestedUrl = url;
        GradomrazVisualBundleHost.Request(url);
    }

    /// <summary>Release after a chapter transition. Shared requests are reference-counted.</summary>
    public void Release()
    {
        if (string.IsNullOrEmpty(requestedUrl))
            return;
        GradomrazVisualBundleHost.Release(requestedUrl);
        requestedUrl = null;
    }

    private static string Resolve(string configured)
    {
        if (Uri.TryCreate(configured, UriKind.Absolute, out Uri absolute))
        {
            if (absolute.Scheme != Uri.UriSchemeHttps && absolute.Scheme != Uri.UriSchemeHttp)
                throw new InvalidOperationException("Visual AssetBundle URL must use HTTP(S).");
            return absolute.AbsoluteUri;
        }

        // WebGL same-origin: relative URLs are resolved against the deployed host,
        // not a separate editor/preview site. In Editor, use full URLs to test.
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Uri origin = new Uri(Application.absoluteURL);
            return new Uri(origin, configured).AbsoluteUri;
        }
        throw new InvalidOperationException(
            "Use an absolute HTTP(S) visual AssetBundle URL when testing outside WebGL.");
    }
}

internal sealed class GradomrazVisualBundleHost : MonoBehaviour
{
    private sealed class Region
    {
        public string url;
        public int requests;
        public Coroutine operation;
        public AssetBundle bundle;
        public string scenePath;
        public bool loaded;
        public bool unavailable;
    }

    private static GradomrazVisualBundleHost instance;
    private readonly Dictionary<string, Region> regions = new Dictionary<string, Region>();

    private static GradomrazVisualBundleHost EnsureInstance()
    {
        if (instance != null)
            return instance;
        GameObject go = new GameObject("GRADOMRAZ Visual Bundle Streamer");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GradomrazVisualBundleHost>();
        return instance;
    }

    internal static void Request(string url)
    {
        var host = EnsureInstance();
        if (!host.regions.TryGetValue(url, out Region region))
        {
            region = new Region { url = url };
            host.regions.Add(url, region);
        }
        region.requests++;
        if (region.operation == null && !region.loaded && !region.unavailable)
            region.operation = host.StartCoroutine(host.Process(region));
    }

    internal static void Release(string url)
    {
        if (instance == null || !instance.regions.TryGetValue(url, out Region region))
            return;
        region.requests = Mathf.Max(0, region.requests - 1);
        if (region.requests == 0 && region.operation == null &&
            (region.loaded || region.bundle != null))
            region.operation = instance.StartCoroutine(instance.Process(region));
    }

    private IEnumerator Process(Region region)
    {
        float started = Time.realtimeSinceStartup;
        while (true)
        {
            if (region.requests > 0 && region.bundle == null && !region.unavailable)
            {
                using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(region.url))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("[GRADOMRAZ] Visual bundle request failed: " +
                            region.url + " / " + request.error);
                        region.unavailable = true;
                        break;
                    }

                    region.bundle = DownloadHandlerAssetBundle.GetContent(request);
                    if (region.bundle == null)
                    {
                        Debug.LogError("[GRADOMRAZ] No AssetBundle returned from " + region.url);
                        region.unavailable = true;
                        break;
                    }
                }
                string[] scenes = region.bundle.GetAllScenePaths();
                if (scenes.Length != 1)
                {
                    Debug.LogError("[GRADOMRAZ] Expected exactly one visual-only scene in " +
                        region.url + "; received " + scenes.Length);
                    region.bundle.Unload(true);
                    region.bundle = null;
                    region.unavailable = true;
                    break;
                }
                region.scenePath = scenes[0];
            }

            if (region.requests > 0 && region.bundle != null && !region.loaded)
            {
                // This does not rely on PlayerSettings/BuildSettings scenes:
                // scene data is supplied by the downloaded remote AssetBundle.
                AsyncOperation operation = SceneManager.LoadSceneAsync(
                    region.scenePath, LoadSceneMode.Additive);
                if (operation == null)
                {
                    Debug.LogError("[GRADOMRAZ] Failed to start visual scene load: " + region.scenePath);
                    region.unavailable = true;
                    break;
                }
                yield return operation;
                region.loaded = true;
                Debug.Log("[GRADOMRAZ] Visual scene ready " + region.scenePath + " in " +
                    (Time.realtimeSinceStartup - started).ToString("F2") + "s");
                continue;
            }

            if (region.requests == 0 && region.loaded)
            {
                Scene scene = SceneManager.GetSceneByPath(region.scenePath);
                if (scene.IsValid() && scene.isLoaded)
                {
                    AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                    if (unload != null)
                        yield return unload;
                }
                region.loaded = false;
                continue;
            }

            if (region.requests == 0 && region.bundle != null)
            {
                region.bundle.Unload(true);
                region.bundle = null;
                region.scenePath = null;
            }
            break;
        }
        region.operation = null;

        // While a request was in flight, the chapter may have changed again.
        // Reconcile the final state without mutating gameplay GameObjects.
        if (!region.unavailable && ((region.requests > 0 && !region.loaded) ||
            (region.requests == 0 && region.loaded)))
            region.operation = StartCoroutine(Process(region));
        if (region.requests == 0 && region.bundle == null)
            regions.Remove(region.url);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
