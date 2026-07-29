using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Karlolegend.Gradomraz.Editor
{
    public static class TmpFontAssetRepair
    {
        private const string BackupRoot = "Builds/GRADOMRAZ-Backups/TmpFontAssetsBeforeRepair";

        private readonly struct RepairTarget
        {
            public readonly string FontAssetPath;
            public readonly string SourceFontPath;

            public RepairTarget(string fontAssetPath, string sourceFontPath)
            {
                FontAssetPath = fontAssetPath;
                SourceFontPath = sourceFontPath;
            }
        }

        private static readonly RepairTarget[] Targets =
        {
            new("Assets/Resources/fonts & materials/Bangers SDF.asset", "Assets/Font/Bangers.ttf"),
            new("Assets/Resources/fonts & materials/Electronic Highway Sign SDF.asset", "Assets/Font/Electronic Highway Sign.ttf"),
            new("Assets/Resources/fonts & materials/LiberationSans SDF - Fallback.asset", "Assets/Font/LiberationSans.ttf"),
            new("Assets/Resources/fonts & materials/Oswald Bold SDF.asset", "Assets/Font/Oswald-Bold.ttf"),
            new("Assets/Resources/fonts & materials/Roboto-Bold SDF.asset", "Assets/Font/Roboto-Bold.ttf"),
        };

        public static void Run()
        {
            Directory.CreateDirectory(BackupRoot);

            var repaired = 0;
            foreach (var target in Targets)
            {
                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target.FontAssetPath);
                if (existing == null)
                {
                    Debug.LogError($"TMP repair skipped missing font asset: {target.FontAssetPath}");
                    continue;
                }

                if (HasUsableAtlas(existing))
                {
                    Debug.Log($"TMP repair skipped valid font asset: {target.FontAssetPath}");
                    continue;
                }

                var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(target.SourceFontPath);
                if (sourceFont == null)
                {
                    Debug.LogError($"TMP repair skipped missing source font: {target.SourceFontPath}");
                    continue;
                }

                BackupAssetFile(target.FontAssetPath);

                var originalName = existing.name;
                var originalMaterial = existing.material;
                var generated = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true);

                if (!HasUsableAtlas(generated))
                {
                    Debug.LogError($"TMP repair failed to generate atlas for {target.FontAssetPath}");
                    UnityEngine.Object.DestroyImmediate(generated);
                    continue;
                }

                EditorUtility.CopySerialized(generated, existing);
                existing.name = originalName;
                if (originalMaterial != null)
                {
                    existing.material = originalMaterial;
                }

                var atlas = existing.atlasTextures[0];
                atlas.name = originalName + " Atlas";
                if (!AssetDatabase.Contains(atlas))
                {
                    AssetDatabase.AddObjectToAsset(atlas, existing);
                }

                UpdateRelatedMaterials(originalName, atlas, existing.material);

                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(generated);
                repaired++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"TMP font asset repair completed. Repaired={repaired}");
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

        private static void UpdateRelatedMaterials(string fontAssetName, Texture2D atlas, Material primaryMaterial)
        {
            var materialPaths = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Resources/fonts & materials" });
            var candidates = new List<Material>();

            foreach (var guid in materialPaths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                if (material.name.StartsWith(fontAssetName.Replace(" - Fallback", string.Empty), StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(material);
                }
            }

            if (primaryMaterial != null && !candidates.Contains(primaryMaterial))
            {
                candidates.Add(primaryMaterial);
            }

            foreach (var material in candidates)
            {
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", atlas);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        private static void BackupAssetFile(string assetPath)
        {
            var fullPath = Path.GetFullPath(assetPath);
            var backupPath = Path.GetFullPath(Path.Combine(BackupRoot, assetPath));
            var backupDir = Path.GetDirectoryName(backupPath);
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            if (!File.Exists(backupPath))
            {
                File.Copy(fullPath, backupPath);
            }
        }
    }
}
