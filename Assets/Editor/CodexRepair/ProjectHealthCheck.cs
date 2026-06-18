using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CodexRepair
{
    public static class ProjectHealthCheck
    {
        private const string ReportPath = "_CodexRepair/Reports/ProjectHealthReport.txt";

        public static void Run()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "_CodexRepair/Reports");

            var report = new StringBuilder();
            var severeIssues = 0;

            report.AppendLine("Codex Unity Project Health Report");
            report.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine();

            severeIssues += CheckMaterials(report);
            severeIssues += CheckScenes(report);

            report.AppendLine();
            report.AppendLine($"SevereIssues={severeIssues}");

            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
            Debug.Log(report.ToString());

            if (severeIssues > 0)
            {
                EditorApplication.Exit(1);
            }
        }

        private static int CheckMaterials(StringBuilder report)
        {
            var severe = 0;
            var materialGuids = AssetDatabase.FindAssets("t:Material");
            var missingShader = 0;
            var unsupportedShader = 0;
            var internalErrorShader = 0;

            foreach (var guid in materialGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                if (material.shader == null)
                {
                    missingShader++;
                    report.AppendLine($"MATERIAL_MISSING_SHADER {path}");
                    continue;
                }

                if (material.shader.name.IndexOf("InternalErrorShader", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    internalErrorShader++;
                    report.AppendLine($"MATERIAL_INTERNAL_ERROR_SHADER {path}");
                }

                if (!material.shader.isSupported)
                {
                    unsupportedShader++;
                    report.AppendLine($"MATERIAL_UNSUPPORTED_SHADER {path} :: {material.shader.name}");
                }
            }

            if (missingShader > 0 || internalErrorShader > 0)
            {
                severe += missingShader + internalErrorShader;
            }

            report.AppendLine("Material Summary");
            report.AppendLine($"  Total={materialGuids.Length}");
            report.AppendLine($"  MissingShader={missingShader}");
            report.AppendLine($"  InternalErrorShader={internalErrorShader}");
            report.AppendLine($"  UnsupportedShader={unsupportedShader}");
            report.AppendLine();
            return severe;
        }

        private static int CheckScenes(StringBuilder report)
        {
            var severe = 0;
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            report.AppendLine("Scene Summary");
            report.AppendLine($"  EnabledBuildScenes={scenes.Length}");

            foreach (var sceneEntry in scenes)
            {
                var scene = EditorSceneManager.OpenScene(sceneEntry.path, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                var allObjects = roots.SelectMany(GetSelfAndChildren).ToArray();

                var missingScripts = allObjects.Sum(CountMissingComponents);
                var cameras = allObjects.Select(go => go.GetComponent<Camera>()).Count(camera => camera != null && camera.enabled);
                var audioListeners = allObjects.Select(go => go.GetComponent<AudioListener>()).Count(listener => listener != null && listener.enabled);
                var eventSystems = allObjects.Select(go => go.GetComponent<EventSystem>()).Count(system => system != null && system.enabled);
                var renderers = allObjects.Select(go => go.GetComponent<Renderer>()).Where(renderer => renderer != null).ToArray();
                var renderersWithMissingMaterials = renderers.Count(RendererHasMissingMaterial);
                var tmpTextsWithMissingFont = allObjects
                    .Select(go => go.GetComponent<TMP_Text>())
                    .Count(text => text != null && (text.font == null || text.font.atlasTextures == null || text.font.atlasTextures.Length == 0 || text.font.atlasTextures[0] == null));

                report.AppendLine($"  Scene={scene.path}");
                report.AppendLine($"    GameObjects={allObjects.Length}");
                report.AppendLine($"    Cameras={cameras}");
                report.AppendLine($"    AudioListeners={audioListeners}");
                report.AppendLine($"    EventSystems={eventSystems}");
                report.AppendLine($"    Renderers={renderers.Length}");
                report.AppendLine($"    MissingScripts={missingScripts}");
                report.AppendLine($"    RenderersWithMissingMaterials={renderersWithMissingMaterials}");
                report.AppendLine($"    TMPTextsWithMissingFontAtlas={tmpTextsWithMissingFont}");

                if (missingScripts > 0 || cameras == 0 || renderersWithMissingMaterials > 0 || tmpTextsWithMissingFont > 0)
                {
                    severe += missingScripts + renderersWithMissingMaterials + tmpTextsWithMissingFont + (cameras == 0 ? 1 : 0);
                }
            }

            report.AppendLine();
            return severe;
        }

        private static bool RendererHasMissingMaterial(Renderer renderer)
        {
            return renderer.sharedMaterials.Any(material => material == null || material.shader == null);
        }

        private static int CountMissingComponents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>().Count(component => component == null);
        }

        private static GameObject[] GetSelfAndChildren(GameObject root)
        {
            return root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject).ToArray();
        }
    }
}
