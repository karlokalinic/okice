// Quality Upgrade Tools for HLADAN GRAD
// Adds a Tools menu to restore the original rendering/quality settings if the
// quality upgrade ever looks wrong, plus timestamped snapshots.
//
// The canonical "undo" source is the pristine backup created *before* any
// upgrade was applied, stored OUTSIDE the Assets folder (so Unity never imports
// duplicate .asset GUIDs): <ProjectRoot>/_QualityUpgradeBackup/
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HladanGrad.QualityUpgrade
{
    public static class QualityUpgradeTools
    {
        private const string PristineBackupFolder = "_QualityUpgradeBackup";
        private const string SnapshotRootFolder = "_QualitySnapshots";

        // Files that the quality upgrade touched. Forward slashes; backup file
        // names replace each slash with a double underscore (matches the backup
        // that was created before the upgrade).
        private static readonly string[] TrackedFiles =
        {
            "ProjectSettings/ProjectSettings.asset",
            "ProjectSettings/QualitySettings.asset",
            "ProjectSettings/GraphicsSettings.asset",
            "Assets/MonoBehaviour/PC_RPAsset.asset",
            "Assets/MonoBehaviour/PC_Renderer.asset",
        };

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string PristineBackupRoot => Path.Combine(ProjectRoot, PristineBackupFolder);

        [MenuItem("Tools/HLADAN GRAD Quality/Restore Original Settings (UNDO Upgrade)", false, 10)]
        public static void RestoreOriginalSettings()
        {
            if (!Directory.Exists(PristineBackupRoot))
            {
                EditorUtility.DisplayDialog(
                    "No backup found",
                    "The pristine backup folder was not found at:\n\n" + PristineBackupRoot +
                    "\n\nCannot undo automatically.",
                    "OK");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "Undo Quality Upgrade?",
                "This restores the ORIGINAL rendering & quality settings (color space, " +
                "render scale, MSAA, HDR precision, shadow resolution, LUT size, LOD bias) " +
                "from the pristine backup.\n\nUnity must be RESTARTED afterwards for the " +
                "color-space change to fully apply.\n\nProceed?",
                "Restore Originals", "Cancel");
            if (!ok) return;

            int restored = 0;
            foreach (var rel in TrackedFiles)
            {
                string backupFile = Path.Combine(PristineBackupRoot, rel.Replace("/", "__"));
                string target = Path.Combine(ProjectRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(backupFile))
                {
                    Debug.LogWarning("[QualityUpgrade] Backup missing for " + rel + " (" + backupFile + ")");
                    continue;
                }
                try
                {
                    File.Copy(backupFile, target, true);
                    restored++;
                    if (rel.StartsWith("Assets/"))
                        AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogError("[QualityUpgrade] Failed to restore " + rel + ": " + e.Message);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Restore complete",
                restored + " file(s) restored to their original values.\n\n" +
                "Please RESTART the Unity Editor now so the color space (Linear/Gamma) " +
                "change takes effect.",
                "OK");
        }

        [MenuItem("Tools/HLADAN GRAD Quality/Create Timestamped Snapshot", false, 11)]
        public static void CreateSnapshot()
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string snapDir = Path.Combine(Path.Combine(ProjectRoot, SnapshotRootFolder), stamp);
            Directory.CreateDirectory(snapDir);

            int n = 0;
            foreach (var rel in TrackedFiles)
            {
                string src = Path.Combine(ProjectRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(src)) continue;
                string dst = Path.Combine(snapDir, rel.Replace("/", "__"));
                File.Copy(src, dst, true);
                n++;
            }
            Debug.Log("[QualityUpgrade] Snapshot of " + n + " file(s) saved to " + snapDir);
            EditorUtility.DisplayDialog("Snapshot created",
                n + " settings file(s) copied to:\n\n" + snapDir, "OK");
        }

        [MenuItem("Tools/HLADAN GRAD Quality/Restore English Dialogue (UNDO Translation)", false, 20)]
        public static void RestoreEnglishDialogue()
        {
            string trBak = Path.Combine(ProjectRoot, "_TranslationBackup");
            // rel path in project -> backup file name in _TranslationBackup
            var items = new (string rel, string backupName)[]
            {
                ("Assets/MonoBehaviour/AFTERLIVES Dialogue Database.asset", "AFTERLIVES Dialogue Database.asset"),
                ("Assets/Scenes/MainMenu.unity", "MainMenu.unity"),
                ("Assets/Scenes/SampleScene.unity", "SampleScene.unity"),
            };
            string dbBackup = Path.Combine(trBak, items[0].backupName);
            if (!File.Exists(dbBackup))
            {
                EditorUtility.DisplayDialog("No translation backup",
                    "Expected the original English dialogue database at:\n\n" + dbBackup, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Undo Croatian translation?",
                "This restores the ORIGINAL English dialogue database and menu scenes " +
                "from backup, replacing the Croatian translation.\n\nProceed?",
                "Restore English", "Cancel"))
                return;

            int restored = 0;
            foreach (var it in items)
            {
                string backup = Path.Combine(trBak, it.backupName);
                if (!File.Exists(backup)) continue;
                string target = Path.Combine(ProjectRoot, it.rel.Replace('/', Path.DirectorySeparatorChar));
                try
                {
                    File.Copy(backup, target, true);
                    AssetDatabase.ImportAsset(it.rel, ImportAssetOptions.ForceUpdate);
                    restored++;
                }
                catch (Exception e)
                {
                    Debug.LogError("[Translation] Failed to restore " + it.rel + ": " + e.Message);
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Restore complete",
                restored + " file(s) restored to English.", "OK");
        }

        [MenuItem("Tools/HLADAN GRAD Quality/Toggle Color Space (Linear/Gamma)", false, 1)]
        public static void ToggleColorSpace()
        {
            ColorSpace now = PlayerSettings.colorSpace;
            ColorSpace next = now == ColorSpace.Linear ? ColorSpace.Gamma : ColorSpace.Linear;
            if (!EditorUtility.DisplayDialog("Toggle color space",
                "Current: " + now + "\nSwitch to: " + next +
                "\n\nUnity will reimport shaders/textures and this needs a short moment. " +
                "Compare the two to pick what looks best. Proceed?",
                "Switch to " + next, "Cancel"))
                return;
            PlayerSettings.colorSpace = next;
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Color space = " + next,
                "Color space is now " + next + ". If it looks washed out in Linear, make " +
                "sure the CRT shader loaded (Tools ▸ ... ▸ Reimport Shaders).", "OK");
        }

        [MenuItem("Tools/HLADAN GRAD Quality/Reimport Shaders (fix CRT load error)", false, 25)]
        public static void ReimportShaders()
        {
            string[] shaderPaths =
            {
                "Assets/Resources/shaders/crt.shader",
            };
            int n = 0;
            foreach (string p in shaderPaths)
            {
                if (File.Exists(Path.Combine(ProjectRoot, p.Replace('/', Path.DirectorySeparatorChar))))
                {
                    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    n++;
                }
            }
            // Also reimport any other .shader under Assets/Resources to clear stale artifacts.
            foreach (string guid in AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Resources" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Shaders reimported",
                "Force-reimported the CRT shader (and Resources shaders) to clear the " +
                "stale 'Failed to load / corrupted' import artifact.\n\nIf the CRT error " +
                "persists, close Unity, delete Library/ShaderCache, and reopen.", "OK");
        }

        [MenuItem("Tools/HLADAN GRAD Quality/Open Change Log", false, 30)]
        public static void OpenChangeLog()
        {
            string log = Path.Combine(ProjectRoot, "QUALITY_UPGRADE_LOG.md");
            if (File.Exists(log))
                EditorUtility.RevealInFinder(log);
            else
                EditorUtility.DisplayDialog("Change log not found",
                    "Expected at:\n" + log, "OK");
        }
    }
}
