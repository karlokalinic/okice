using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Karlolegend.Gradomraz.Editor
{
    [InitializeOnLoad]
    public static class VT323FontAssetBootstrap
    {
        private const string FontAssetPath = "Assets/Font/VT323-Regular SDF.asset";
        private const string SourceFontPath = "Assets/Font/VT323-Regular.ttf";
        private const string BackupDirectory = "Builds/GRADOMRAZ-Backups/LfsPointers";
        private const string SessionKey = "Karlolegend.Gradomraz.VT323FontAssetBootstrap.Attempted";
        private const string LfsHeader = "version https://git-lfs.github.com/spec/v1";

        static VT323FontAssetBootstrap()
        {
            EditorApplication.delayCall += EnsureFontAsset;
        }

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Repair VT323 LFS Font Asset")]
        public static void RepairFromMenu()
        {
            SessionState.EraseBool(SessionKey);
            EnsureFontAsset();
        }

        private static void EnsureFontAsset()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (HasUsableAtlas(existing))
            {
                return;
            }

            var fullAssetPath = Path.GetFullPath(FontAssetPath);
            if (File.Exists(fullAssetPath) && !IsLfsPointer(fullAssetPath))
            {
                Debug.LogError(
                    $"VT323 font asset exists but is not a valid TMP asset and is not an unresolved Git LFS pointer: {FontAssetPath}. " +
                    "The file was left untouched. Restore it from version control or run a deliberate font rebuild.");
                return;
            }

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
            {
                Debug.LogError($"Cannot recover VT323 TMP asset because its source font is missing: {SourceFontPath}");
                return;
            }

            try
            {
                BackupPointerIfPresent(fullAssetPath);

                if (File.Exists(fullAssetPath))
                {
                    File.Delete(fullAssetPath);
                }

                var generated = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true);

                if (generated == null)
                {
                    throw new InvalidOperationException("TMP_FontAsset.CreateFontAsset returned null.");
                }

                generated.name = "VT323-Regular SDF";
                AssetDatabase.CreateAsset(generated, FontAssetPath);

                if (generated.material != null && !AssetDatabase.Contains(generated.material))
                {
                    generated.material.name = generated.name + " Material";
                    AssetDatabase.AddObjectToAsset(generated.material, generated);
                }

                var atlases = generated.atlasTextures ?? Array.Empty<Texture2D>();
                for (var i = 0; i < atlases.Length; i++)
                {
                    var atlas = atlases[i];
                    if (atlas == null || AssetDatabase.Contains(atlas))
                    {
                        continue;
                    }

                    atlas.name = atlases.Length == 1
                        ? generated.name + " Atlas"
                        : $"{generated.name} Atlas {i}";
                    AssetDatabase.AddObjectToAsset(atlas, generated);
                }

                EditorUtility.SetDirty(generated);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceUpdate);

                var repaired = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
                if (!HasUsableAtlas(repaired))
                {
                    throw new InvalidOperationException("The regenerated VT323 asset still has no usable atlas.");
                }

                Debug.Log(
                    "Recovered VT323-Regular SDF.asset from the source TTF. " +
                    "The unresolved Git LFS pointer was backed up under Builds/GRADOMRAZ-Backups/LfsPointers. " +
                    "Commit the regenerated asset from a Git LFS-enabled checkout.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"VT323 TMP recovery failed: {exception}");
            }
        }

        private static bool HasUsableAtlas(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null || fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
            {
                return false;
            }

            var atlas = fontAsset.atlasTextures[0];
            return atlas != null && atlas.width > 0 && atlas.height > 0;
        }

        private static bool IsLfsPointer(string path)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists || fileInfo.Length > 4096)
            {
                return false;
            }

            using var reader = new StreamReader(path);
            var firstLine = reader.ReadLine();
            return string.Equals(firstLine?.Trim(), LfsHeader, StringComparison.Ordinal);
        }

        private static void BackupPointerIfPresent(string fullAssetPath)
        {
            if (!File.Exists(fullAssetPath) || !IsLfsPointer(fullAssetPath))
            {
                return;
            }

            Directory.CreateDirectory(BackupDirectory);
            var backupPath = Path.Combine(
                BackupDirectory,
                $"VT323-Regular-SDF-{DateTime.Now:yyyyMMdd-HHmmss}.lfs-pointer.txt");
            File.Copy(fullAssetPath, backupPath, false);
        }
    }
}
