#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes *experimental* copies of SampleScene and Bridge's renderer-only
/// buildings subtree. The canonical SampleScene is never edited.
/// This is an authoring step, not an automatic deployment or scene switch.
/// </summary>
public static class GradomrazBridgeStreamingStage
{
    private const string Canonical = "Assets/Scenes/SampleScene.unity";
    private const string GameCopyFolder = "Assets/GRADOMRAZ/ExperimentalBuilds";
    private const string GameCopy = GameCopyFolder + "/SampleScene.unity";
    private const string VisualFolder = "Assets/GRADOMRAZ/StreamingScenes";
    private const string VisualScene = VisualFolder + "/BridgeBackground.unity";

    [MenuItem("Tools/GRADOMRAZ/Streaming/Stage Bridge Buildings (Non-Destructive)")]
    public static void StageBridgeBuildings()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Leave Play Mode before scene migration.");
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Canonical))
            throw new InvalidOperationException("Canonical scene missing: " + Canonical);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameCopy) ||
            AssetDatabase.LoadAssetAtPath<SceneAsset>(VisualScene))
            throw new InvalidOperationException(
                "Existing staged scenes are never overwritten. Move them manually to restage.");

        Scene canonicalInEditor = SceneManager.GetSceneByPath(Canonical);
        if (canonicalInEditor.IsValid() && canonicalInEditor.isLoaded && canonicalInEditor.isDirty)
            throw new InvalidOperationException(
                "Save or revert the canonical SampleScene in the Editor first. " +
                "Unsaved edits cannot be safely migrated.");

        EnsureFolder("Assets", "GRADOMRAZ");
        EnsureFolder("Assets/GRADOMRAZ", "ExperimentalBuilds");
        EnsureFolder("Assets/GRADOMRAZ", "StreamingScenes");

        // Copy the source on disk first so no modification to the canonical
        // scene or its persistent fileIDs can be performed below.
        if (!AssetDatabase.CopyAsset(Canonical, GameCopy))
            throw new InvalidOperationException("Unable to copy canonical scene.");
        AssetDatabase.ImportAsset(GameCopy, ImportAssetOptions.ForceSynchronousImport);

        Scene initialActive = SceneManager.GetActiveScene();
        if (!initialActive.IsValid() || string.IsNullOrEmpty(initialActive.path))
            throw new InvalidOperationException(
                "Save the currently active scene before staging an experimental split.");

        Scene gameplay = default;
        Scene visuals = default;
        bool savedVisuals = false;
        bool savedGameplay = false;
        try
        {
            gameplay = EditorSceneManager.OpenScene(GameCopy, OpenSceneMode.Additive);
            GameObject bridge = LocateBridge(gameplay);
            Transform buildings = bridge.transform.Find("buildings");
            if (buildings == null)
                throw new InvalidOperationException(
                    "Bridge/buildings subtree not found. Canonical source layout changed.");

            int meshRendererCount = 0, objectCount = 0;
            foreach (Transform tr in buildings.GetComponentsInChildren<Transform>(true))
            {
                objectCount++;
                foreach (Component c in tr.GetComponents<Component>())
                {
                    if (c == null)
                        throw new InvalidOperationException("Missing component at " + tr.name);
                    if (c is MeshRenderer)
                        meshRendererCount++;
                    else if (!(c is Transform) && !(c is MeshFilter))
                        throw new InvalidOperationException(
                            "Bridge/buildings no longer visual-only: " + c.GetType().Name +
                            " on " + tr.name + ". Refusing destructive stage.");
                }
            }

            // Source scene reference preflight: 6,337 objects / 5,832 renderers.
            // A changed layout needs a fresh scan, not silent extraction.
            if (objectCount != 6337 || meshRendererCount != 5832)
                throw new InvalidOperationException(
                    "Bridge/buildings layout changed (objects=" + objectCount +
                    ", renderers=" + meshRendererCount + "). Re-run the reference audit.");

            // Transform reference parent is only in the canonical scene; the
            // clone is explicitly detached while preserving world transforms.
            Transform parent = buildings.parent;
            if ((parent.lossyScale - Vector3.one).sqrMagnitude > 0.0001f)
                throw new InvalidOperationException(
                    "Bridge parent transform scale changed. Extraction could distort geometry.");

            visuals = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(visuals);
            GameObject clone = UnityEngine.Object.Instantiate(buildings.gameObject);
            clone.name = buildings.name;
            if (clone.transform.parent != null)
                clone.transform.SetParent(null, true);
            clone.transform.SetPositionAndRotation(buildings.position, buildings.rotation);
            clone.transform.localScale = buildings.lossyScale;
            SceneManager.MoveGameObjectToScene(clone, visuals);
            EditorSceneManager.MarkSceneDirty(visuals);

            if (!EditorSceneManager.SaveScene(visuals, VisualScene))
                throw new InvalidOperationException("Could not save streaming visual scene.");
            savedVisuals = true;

            // Only the explicitly named, separately copied experimental scene
            // loses these objects. The canonical original is never opened for edits.
            SceneManager.SetActiveScene(gameplay);
            UnityEngine.Object.DestroyImmediate(buildings.gameObject);

            // Opt-in anchor is inert until URL is configured after hashed bundle
            // publication. Its lifecycle follows Bridge and never replaces FSMs.
            GameObject anchor = new GameObject("GRADOMRAZ Bridge Visual Stream (set hashed URL)");
            SceneManager.MoveGameObjectToScene(anchor, gameplay);
            anchor.transform.SetParent(bridge.transform, false);
            var streamer = anchor.AddComponent<GradomrazRemoteVisualRegion>();
            var serialized = new SerializedObject(streamer);
            serialized.FindProperty("loadWhenActive").boolValue = true;
            serialized.FindProperty("releaseWhenInactive").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(gameplay);
            if (!EditorSceneManager.SaveScene(gameplay, GameCopy))
                throw new InvalidOperationException("Could not save experimental gameplay scene.");
            savedGameplay = true;
            Debug.Log("[GRADOMRAZ] Experimental split staged: " + GameCopy + " plus " +
                VisualScene + " (6,337 objects / 5,832 mesh renderers). " +
                "Canonical " + Canonical + " is unchanged. " +
                "Create the hashed bundle, configure stream URL on Bridge anchor, " +
                "and test this alternate gameplay scene in a separate WebGL build.");
        }
        finally
        {
            if (visuals.IsValid() && visuals.isLoaded)
                EditorSceneManager.CloseScene(visuals, true);
            if (gameplay.IsValid() && gameplay.isLoaded)
                EditorSceneManager.CloseScene(gameplay, true);
            if (initialActive.IsValid() && initialActive.isLoaded)
                SceneManager.SetActiveScene(initialActive);
            if (!savedVisuals || !savedGameplay)
            {
                // If any validation or scene-save step failed, do not leave
                // incomplete alternate scenes that could be built accidentally.
                if (!savedGameplay && AssetDatabase.LoadAssetAtPath<SceneAsset>(GameCopy))
                    AssetDatabase.DeleteAsset(GameCopy);
                if (savedVisuals && !savedGameplay &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(VisualScene))
                    AssetDatabase.DeleteAsset(VisualScene);
            }
        }
    }

    private static GameObject LocateBridge(Scene scene)
    {
        GameObject result = null;
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            Transform environment = go.name == "Environment" ? go.transform : null;
            if (environment == null)
                continue;
            foreach (Transform child in environment)
            {
                if (child.name != "Bridge")
                    continue;
                if (result != null)
                    throw new InvalidOperationException("More than one Bridge root found.");
                result = child.gameObject;
            }
        }
        if (result == null)
            throw new InvalidOperationException("Environment/Bridge root not found.");
        return result;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string full = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
