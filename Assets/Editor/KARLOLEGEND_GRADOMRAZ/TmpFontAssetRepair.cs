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
        private const string RequiredCroatianCharacters = "ČĆŽŠĐčćžšđ";

        private readonly struct RepairTarget
        {
            public readonly string FontAssetPath;
            public readonly string SourceFontPath;
            public readonly bool RequireCroatianCharacters;

            public RepairTarget(string fontAssetPath, string sourceFontPath, bool requireCroatianCharacters = false)
            {
                FontAssetPath = fontAssetPath;
                SourceFontPath = sourceFontPath;
                RequireCroatianCharacters = requireCroatianCharacters;
            }
        }

        private static readonly RepairTarget[] Targets =
        {
            new("Assets/Resources/fonts & materials/Bangers SDF.asset", "Assets/Font/Bangers.ttf"),
            new("Assets/Resources/fonts & materials/Electronic Highway Sign SDF.asset", "Assets/Font/Electronic Highway Sign.ttf"),
            new("Assets/Resources/fonts & materials/LiberationSans SDF - Fallback.asset", "Assets/Font/LiberationSans.ttf", true),
            new("Assets/Resources/fonts & materials/Oswald Bold SDF.asset", "Assets/Font/Oswald-Bold.ttf"),
            new("Assets/Resources/fonts & materials/Roboto-Bold SDF.asset", "Assets/Font/Roboto-Bold.ttf"),
        };

        [MenuItem("KARLOLEGEND/GRADOMRAZ/Repair TMP Font Assets")]
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

                if (IsBuildReady(existing, target.RequireCroatianCharacters))
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

                if (generated == null)
                {
                    Debug.LogError($"TMP repair failed to create font asset for {target.FontAssetPath}");
                    continue;
                }

                if (target.RequireCroatianCharacters &&
                    !generated.TryAddCharacters(RequiredCroatianCharacters, out var missingCharacters))
                {
                    Debug.LogError(
                        $"TMP repair cannot provide required Croatian glyphs for {target.FontAssetPath}. Missing: {missingCharacters}");
                    DestroyGeneratedFontAsset(generated);
                    continue;
                }

                if (!HasUsableAtlas(generated))
                {
                    Debug.LogError($"TMP repair failed to generate atlas for {target.FontAssetPath}");
                    DestroyGeneratedFontAsset(generated);
                    continue;
                }

                var generatedMaterial = generated.material;
                var generatedAtlases = generated.atlasTextures ?? Array.Empty<Texture2D>();

                EditorUtility.CopySerialized(generated, existing);
                existing.name = originalName;

                if (originalMaterial != null)
                {
                    existing.material = originalMaterial;
                }
                else if (generatedMaterial != null && !AssetDatabase.Contains(generatedMaterial))
                {
                    generatedMaterial.name = originalName + " Material";
                    AssetDatabase.AddObjectToAsset(generatedMaterial, existing);
                    existing.material = generatedMaterial;
                }

                for (var i = 0; i < generatedAtlases.Length; i++)
                {
                    var atlas = generatedAtlases[i];
                    if (atlas == null)
                    {
                        continue;
                    }

                    atlas.name = generatedAtlases.Length == 1
                        ? originalName + " Atlas"
                        : $"{originalName} Atlas {i}";

                    if (!AssetDatabase.Contains(atlas))
                    {
                        AssetDatabase.AddObjectToAsset(atlas, existing);
                    }
                }

                var primaryAtlas = existing.atlasTextures != null && existing.atlasTextures.Length > 0
                    ? existing.atlasTextures[0]
                    : null;

                if (primaryAtlas != null)
                {
                    UpdateRelatedMaterials(originalName, primaryAtlas, existing.material);
                }

                EditorUtility.SetDirty(existing);
                DestroyGeneratedFontAsset(generated);
                repaired++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"TMP font asset repair completed. Repaired={repaired}");
        }

        public static bool EnsureBuildReady(out string error)
        {
            if (ValidateTargets(out error))
            {
                return true;
            }

            Debug.LogWarning($"TMP validation failed before build. Attempting one repair pass. {error}");
            Run();
            return ValidateTargets(out error);
        }

        private static bool ValidateTargets(out string error)
        {
            foreach (var target in Targets)
            {
                var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target.FontAssetPath);
                if (fontAsset == null)
                {
                    error = $"Missing TMP font asset: {target.FontAssetPath}";
                    return false;
                }

                if (!HasUsableAtlas(fontAsset))
                {
                    error = $"TMP font asset has no usable atlas: {target.FontAssetPath}";
                    return false;
                }

                if (target.RequireCroatianCharacters && !HasCroatianCharacters(fontAsset))
                {
                    error = $"TMP fallback font is missing Croatian glyphs ({RequiredCroatianCharacters}): {target.FontAssetPath}";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsBuildReady(TMP_FontAsset fontAsset, bool requireCroatianCharacters)
        {
            return HasUsableAtlas(fontAsset) && (!requireCroatianCharacters || HasCroatianCharacters(fontAsset));
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

        private static bool HasCroatianCharacters(TMP_FontAsset fontAsset)
        {
            foreach (var character in RequiredCroatianCharacters)
            {
                if (!fontAsset.HasCharacter(character))
                {
                    return false;
                }
            }

            return true;
        }

        private static void DestroyGeneratedFontAsset(TMP_FontAsset generated)
        {
            if (generated == null)
            {
                return;
            }

            var transientMaterial = generated.material;
            var transientAtlases = generated.atlasTextures ?? Array.Empty<Texture2D>();

            generated.atlasPopulationMode = AtlasPopulationMode.Static;
            var serializedGenerated = new SerializedObject(generated);

            var materialProperty = serializedGenerated.FindProperty("m_Material");
            if (materialProperty != null)
            {
                materialProperty.objectReferenceValue = null;
            }

            var atlasTextureProperty = serializedGenerated.FindProperty("m_AtlasTexture");
            if (atlasTextureProperty != null)
            {
                atlasTextureProperty.objectReferenceValue = null;
            }

            var atlasTexturesProperty = serializedGenerated.FindProperty("m_AtlasTextures");
            if (atlasTexturesProperty != null && atlasTexturesProperty.isArray)
            {
                atlasTexturesProperty.arraySize = 0;
            }

            serializedGenerated.ApplyModifiedPropertiesWithoutUndo();
            UnityEngine.Object.DestroyImmediate(generated);

            if (transientMaterial != null && !AssetDatabase.Contains(transientMaterial))
            {
                UnityEngine.Object.DestroyImmediate(transientMaterial);
            }

            foreach (var atlas in transientAtlases)
            {
                if (atlas != null && !AssetDatabase.Contains(atlas))
                {
                    UnityEngine.Object.DestroyImmediate(atlas);
                }
            }
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
                if (!material.HasProperty("_MainTex"))
                {
                    continue;
                }

                material.SetTexture("_MainTex", atlas);
                EditorUtility.SetDirty(material);
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
