using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class ProjectRepair
{
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";
    private const string GameplayVolumeProfilePath = "Assets/MonoBehaviour/New Volume Profile.asset";
    private const string MotionBlurPath = "Assets/MonoBehaviour/MotionBlur.asset";
    private const string DialogueFontPath = "Assets/Font/VT323-Regular SDF.asset";

    [MenuItem("Tools/Project Repair/Bake Performance Fonts")]
    public static void BakePerformanceFonts()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DialogueFontPath);
        if (font == null)
        {
            throw new InvalidOperationException($"Could not load '{DialogueFontPath}'.");
        }

        if (font.atlasWidth != 1024 || font.atlasHeight != 1024)
        {
            throw new InvalidOperationException(
                $"VT323 atlas must be 1024x1024, not {font.atlasWidth}x{font.atlasHeight}."
            );
        }
        font.isMultiAtlasTexturesEnabled = false;

        string requiredCharacters = BuildPerformanceFontCharacters();
        string charactersToAdd = new string(
            requiredCharacters
                .Where(character =>
                    font.characterTable.All(entry => entry.unicode != character)
                )
                .ToArray()
        );
        if (!string.IsNullOrEmpty(charactersToAdd))
        {
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            if (!font.TryAddCharacters(charactersToAdd, out string missingCharacters))
            {
                throw new InvalidOperationException(
                    $"VT323 is missing required glyphs: {missingCharacters}"
                );
            }
        }

        font.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(font);
        EditorUtility.SetDirty(font.material);
        foreach (Texture2D atlasTexture in font.atlasTextures)
        {
            EditorUtility.SetDirty(atlasTexture);
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Baked {font.characterTable.Count} VT323 characters into a static "
            + $"{font.atlasWidth}x{font.atlasHeight} atlas."
        );
    }

    private static string BuildPerformanceFontCharacters()
    {
        StringBuilder characters = new StringBuilder(120);
        for (char character = ' '; character <= '~'; character++)
        {
            characters.Append(character);
        }

        characters.Append(
            "\u00a0\u0106\u0107\u010c\u010d\u0110\u0111\u0160\u0161\u017d\u017e"
            + "\u2013\u2014\u2018\u2019\u201c\u201d\u2026"
        );
        return characters.ToString();
    }

    [MenuItem("Tools/Project Repair/Audit Gameplay Freeze Paths")]
    public static void AuditGameplayFreezePaths()
    {
        Scene scene = EditorSceneManager.OpenScene(
            GameplayScenePath,
            OpenSceneMode.Single
        );

        string[] names = { "PauseMenu", "PausedPanel", "Dialogue Manager", "Player" };
        foreach (string name in names)
        {
            GameObject gameObject = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(transform => transform.gameObject)
                .FirstOrDefault(candidate => candidate.name == name);

            if (gameObject == null)
            {
                Debug.Log($"Freeze audit: '{name}' was not found.");
                continue;
            }

            string components = string.Join(
                ", ",
                gameObject
                    .GetComponents<Component>()
                    .Where(component => component != null)
                    .Select(component => component.GetType().FullName)
            );
            Debug.Log(
                $"Freeze audit: '{name}' active={gameObject.activeInHierarchy}; components=[{components}]"
            );
        }

        foreach (
            MonoBehaviour component in scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(component => component != null)
        )
        {
            string typeName = component.GetType().FullName;
            if (
                typeName.IndexOf("DialogueSystemTrigger", StringComparison.Ordinal) < 0
                && typeName.IndexOf("PauseGameOnConversation", StringComparison.Ordinal) < 0
                && typeName.IndexOf("UIShowHideController", StringComparison.Ordinal) < 0
                && typeName.IndexOf("InputDeviceManager", StringComparison.Ordinal) < 0
                && typeName.IndexOf("PlayMakerFSM", StringComparison.Ordinal) < 0
            )
            {
                continue;
            }

            SerializedObject serialized = new SerializedObject(component);
            string[] properties =
            {
                "pauseGameDuringConversation",
                "pauseOnOpen",
                "pauseOnClose",
                "pauseWhileOpen",
                "useUnscaledTime",
                "m_Enabled",
            };
            string settings = string.Join(
                ", ",
                properties
                    .Select(propertyName => serialized.FindProperty(propertyName))
                    .Where(property => property != null)
                    .Select(property => $"{property.name}={SerializedPropertyValue(property)}")
            );
            Debug.Log(
                $"Freeze audit: '{component.gameObject.name}' {typeName}; enabled={component.enabled}; {settings}"
            );

            string fsmSummary = DescribePlayMakerFsm(component);
            if (
                !string.IsNullOrEmpty(fsmSummary)
                && (
                    component.gameObject.name == "PauseMenu"
                    || fsmSummary.Contains("StartConversation")
                    || fsmSummary.Contains("ScaleTime")
                )
            )
            {
                Debug.Log(
                    $"Freeze audit FSM: '{GetHierarchyPath(component.transform)}' {fsmSummary}"
                );
            }
        }
    }

    [MenuItem("Tools/Project Repair/Repair Enabled Build Scenes")]
    public static void RepairEnabledBuildScenes()
    {
        int removedCount = RepairMissingScriptsInEnabledScenes();
        Debug.Log($"Removed {removedCount} missing script components from enabled build scenes.");
    }

    [MenuItem("Tools/Project Repair/Validate Enabled Build Scenes")]
    public static void ValidateEnabledBuildScenes()
    {
        int missingCount = CountMissingScriptsInEnabledScenes();
        if (missingCount != 0)
        {
            throw new InvalidOperationException(
                $"Enabled build scenes still contain {missingCount} missing script components."
            );
        }

        Debug.Log("Enabled build scenes contain no missing script components.");
    }

    public static void RepairAndBuildWindows()
    {
        RepairGameplayPostProcessing();
        RepairEnabledBuildScenes();
        ValidateEnabledBuildScenes();

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string buildDirectory = Path.Combine(projectRoot, "Builds", "CopilotValidation");
        string buildPath = Path.Combine(buildDirectory, "GRADOMRAZ.exe");

        if (Directory.Exists(buildDirectory))
        {
            Directory.Delete(buildDirectory, true);
        }

        Directory.CreateDirectory(buildDirectory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledBuildScenePaths(),
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Windows build failed: {report.summary.result}."
            );
        }

        Debug.Log(
            $"Windows build succeeded: {buildPath} ({report.summary.totalSize} bytes)."
        );
    }

    [MenuItem("Tools/Project Repair/Repair Gameplay Post Processing")]
    public static void RepairGameplayPostProcessing()
    {
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
            GameplayVolumeProfilePath
        );
        MotionBlur motionBlur = AssetDatabase.LoadAssetAtPath<MotionBlur>(
            MotionBlurPath
        );

        if (profile == null || motionBlur == null)
        {
            throw new InvalidOperationException(
                "The gameplay Volume profile or Motion Blur override could not be loaded."
            );
        }

        motionBlur.active = true;
        if (!profile.components.Contains(motionBlur))
        {
            profile.components.Add(motionBlur);
        }

        Scene scene = EditorSceneManager.OpenScene(
            GameplayScenePath,
            OpenSceneMode.Single
        );
        Volume globalVolume = scene
            .GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Volume>(true))
            .FirstOrDefault(volume => volume.isGlobal);

        if (globalVolume == null)
        {
            throw new InvalidOperationException(
                "The gameplay scene does not contain a global Volume."
            );
        }

        globalVolume.sharedProfile = profile;
        EditorUtility.SetDirty(motionBlur);
        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(globalVolume);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static int RepairMissingScriptsInEnabledScenes()
    {
        int removedCount = 0;

        foreach (string scenePath in GetEnabledBuildScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int removedInScene = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Sum(transform => GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject));

            if (removedInScene > 0)
            {
                EditorSceneManager.SaveScene(scene);
                removedCount += removedInScene;
            }
        }

        AssetDatabase.SaveAssets();
        return removedCount;
    }

    private static int CountMissingScriptsInEnabledScenes()
    {
        int missingCount = 0;

        foreach (string scenePath in GetEnabledBuildScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            missingCount += scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Sum(transform => transform.GetComponents<Component>().Count(component => component == null));
        }

        return missingCount;
    }

    private static string[] GetEnabledBuildScenePaths()
    {
        string[] scenePaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenePaths.Length == 0)
        {
            throw new InvalidOperationException("No scenes are enabled in Build Settings.");
        }

        return scenePaths;
    }

    private static string SerializedPropertyValue(SerializedProperty property)
    {
        return property.propertyType switch
        {
            SerializedPropertyType.Boolean => property.boolValue.ToString(),
            SerializedPropertyType.Integer => property.intValue.ToString(),
            SerializedPropertyType.Float => property.floatValue.ToString(),
            _ => property.propertyType.ToString(),
        };
    }

    private static string DescribePlayMakerFsm(MonoBehaviour component)
    {
        if (component.GetType().Name != "PlayMakerFSM")
        {
            return string.Empty;
        }

        object fsm = component.GetType().GetProperty("Fsm")?.GetValue(component);
        if (fsm == null)
        {
            return string.Empty;
        }

        object startState = fsm.GetType().GetProperty("StartState")?.GetValue(fsm);
        object states = fsm.GetType().GetProperty("States")?.GetValue(fsm);
        if (states is not System.Collections.IEnumerable stateList)
        {
            return $"start={startState}";
        }

        string[] stateSummaries = stateList
            .Cast<object>()
            .Where(state => state != null)
            .Select(state =>
            {
                string stateName = state.GetType().GetProperty("Name")?.GetValue(state)?.ToString();
                object actions = state.GetType().GetProperty("Actions")?.GetValue(state);
                string actionNames = actions is System.Collections.IEnumerable actionList
                    ? string.Join(
                        ",",
                        actionList
                            .Cast<object>()
                            .Where(action => action != null)
                            .Select(action => action.GetType().Name)
                    )
                    : string.Empty;
                return $"{stateName}[{actionNames}]";
            })
            .ToArray();

        return $"start={startState}; states={string.Join(" | ", stateSummaries)}";
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string[] names = transform
            .GetComponentsInParent<Transform>(true)
            .Reverse()
            .Select(parent => parent.name)
            .ToArray();
        return string.Join("/", names);
    }
}