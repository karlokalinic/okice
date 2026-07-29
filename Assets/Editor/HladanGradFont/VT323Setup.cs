// GRADOMRAZ — Font Wizard
// This editor tool builds TMP font assets from imported TTF/OTF fonts and applies
// them to the game. VT323-Regular is the default recommended font for the project,
// but you can choose any duplicated font source from the project.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace HladanGrad.Fonts
{
    public class VT323Setup : EditorWindow
    {
        private const string DefaultSourceName = "VT323-Regular";
        private const string PublicPixelPath = "Assets/MonoBehaviour/PublicPixel SDF.asset";

        private string[] fontNames = Array.Empty<string>();
        private string[] fontSourcePaths = Array.Empty<string>();
        private int selectedFontIndex;
        private TMP_FontAsset builtFontAsset;
        private string statusMessage = string.Empty;

        [MenuItem("Tools/GRADOMRAZ/Font Wizard", false, 1000)]
        public static void ShowWindow()
        {
            var window = GetWindow<VT323Setup>("Font Wizard");
            window.RefreshFontSources();
            window.Show();
        }

        private void OnEnable()
        {
            RefreshFontSources();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GRADOMRAZ Font Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose a font source from the project, build its TMP font asset, and apply it safely to the game.", MessageType.Info);
            EditorGUILayout.Space();

            if (fontNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No imported font assets were found. Import TTF/OTF files under Assets/Font or Assets/UFONTS and then refresh.", MessageType.Warning);
            }
            else
            {
                selectedFontIndex = EditorGUILayout.Popup("Font source", selectedFontIndex, fontNames);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh font list"))
            {
                RefreshFontSources();
            }
            if (GUILayout.Button("Select VT323"))
            {
                SelectDefaultFont();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (fontNames.Length > 0)
            {
                if (GUILayout.Button("Build TMP font asset"))
                {
                    BuildSelectedFontAsset();
                    // Add black text borders to the font
                    if (builtFontAsset != null)
                    {
                        builtFontAsset.material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                        builtFontAsset.material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                        EditorUtility.SetDirty(builtFontAsset.material);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        statusMessage = $"Built TMP font asset from {Path.GetFileName(fontSourcePaths[selectedFontIndex])} with black outline.";
                    }
                    // In case the font asset is already built, we can still add the outline to it
                    else if (builtFontAsset == null)
                    {
                        string sourceFontPath = fontSourcePaths[selectedFontIndex];
                        string targetPath = GetTargetFontAssetPath(sourceFontPath);
                        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(targetPath);
                        if (existing != null)
                        {
                            builtFontAsset = existing;
                            builtFontAsset.material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
                            builtFontAsset.material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                            EditorUtility.SetDirty(builtFontAsset.material);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            statusMessage = $"Added black outline to existing TMP font asset {Path.GetFileName(targetPath)}.";
                        }
                    }
                    // Final check to ensure the font asset is built and has the outline applied
                    if (builtFontAsset != null)
                    {
                        statusMessage = $"Built TMP font asset from {Path.GetFileName(fontSourcePaths[selectedFontIndex])} with black outline.";
                    }
                    else
                    {
                        statusMessage = $"Failed to build TMP font asset from {Path.GetFileName(fontSourcePaths[selectedFontIndex])}.";
                    }
                }

                if (GUILayout.Button("Build + add as fallback on PublicPixel"))
                {
                    if (BuildSelectedFontAsset() is TMP_FontAsset fontAsset)
                    {
                        AddFontToPublicPixelFallback(fontAsset);
                    }
                }

                if (GUILayout.Button("Apply selected font to all TMP and legacy text components"))
                {
                    if (BuildSelectedFontAsset() is TMP_FontAsset fontAsset)
                    {
                        ApplyFontToAllText(fontAsset);
                    }
                }

                if (GUILayout.Button("Scan all TMP text components"))
                {
                    ReportAllTMPTextCounts();
                }

                if (GUILayout.Button("Reimport CRT shader"))
                {
                    ReimportCrtShader();
                }
            }

            EditorGUILayout.Space();

            if (builtFontAsset != null)
            {
                EditorGUILayout.ObjectField("Last built font asset", builtFontAsset, typeof(TMP_FontAsset), false);
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, MessageType.None);
            }
        }

        private void RefreshFontSources()
        {
            var fontGuids = AssetDatabase.FindAssets("t:Font", new[] { "Assets" });
            var sources = new List<string>(fontGuids.Length);
            foreach (var guid in fontGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".ttf" || ext == ".otf")
                {
                    sources.Add(path);
                }
            }

            fontSourcePaths = sources.Distinct().OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase).ToArray();
            fontNames = fontSourcePaths.Select(Path.GetFileNameWithoutExtension).ToArray();
            if (fontSourcePaths.Length == 0)
            {
                selectedFontIndex = 0;
            }
            else
            {
                selectedFontIndex = Math.Min(selectedFontIndex, fontSourcePaths.Length - 1);
                SelectDefaultFontIfMissing();
            }

            statusMessage = $"Found {fontSourcePaths.Length} font source(s).";
        }

        private void SelectDefaultFontIfMissing()
        {
            if (fontSourcePaths.Length == 0) return;
            if (selectedFontIndex < fontSourcePaths.Length && Path.GetFileNameWithoutExtension(fontSourcePaths[selectedFontIndex]).Equals(DefaultSourceName, StringComparison.OrdinalIgnoreCase))
                return;

            for (int i = 0; i < fontSourcePaths.Length; i++)
            {
                if (Path.GetFileNameWithoutExtension(fontSourcePaths[i]).Equals(DefaultSourceName, StringComparison.OrdinalIgnoreCase))
                {
                    selectedFontIndex = i;
                    return;
                }
            }
        }

        private void SelectDefaultFont()
        {
            for (int i = 0; i < fontSourcePaths.Length; i++)
            {
                if (Path.GetFileNameWithoutExtension(fontSourcePaths[i]).Equals(DefaultSourceName, StringComparison.OrdinalIgnoreCase))
                {
                    selectedFontIndex = i;
                    statusMessage = $"Selected default font: {DefaultSourceName}.";
                    return;
                }
            }

            statusMessage = $"Default font source '{DefaultSourceName}' was not found.";
        }

        private TMP_FontAsset BuildSelectedFontAsset()
        {
            if (fontSourcePaths.Length == 0)
            {
                statusMessage = "No source fonts are available to build.";
                return null;
            }

            string sourceFontPath = fontSourcePaths[selectedFontIndex];
            var fontAsset = BuildFontAsset(sourceFontPath);
            if (fontAsset != null)
            {
                builtFontAsset = fontAsset;
                statusMessage = $"Built TMP font asset from {Path.GetFileName(sourceFontPath)}.";
            }
            return fontAsset;
        }

        private TMP_FontAsset BuildFontAsset(string sourceFontPath)
        {
            if (!EnsureTMPShadersAvailable())
            {
                return null;
            }

            string targetPath = GetTargetFontAssetPath(sourceFontPath);
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(targetPath);
            if (existing != null)
            {
                return existing;
            }

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            if (sourceFont == null)
            {
                statusMessage = $"Missing source font at {sourceFontPath}.";
                return null;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);

            if (fontAsset == null)
            {
                statusMessage = $"Failed to create TMP font asset from {Path.GetFileName(sourceFontPath)}.";
                return null;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(targetPath));
            AssetDatabase.CreateAsset(fontAsset, targetPath);

            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    if (fontAsset.atlasTextures[i] == null) continue;
                    fontAsset.atlasTextures[i].name = fontAsset.name + " Atlas " + i;
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[i], fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            return fontAsset;
        }

        private bool EnsureTMPShadersAvailable()
        {
            bool hasDistanceField = Shader.Find("TextMeshPro/Distance Field") != null;
            bool hasMobileBitmap = Shader.Find("TextMeshPro/Mobile/Bitmap") != null;
            bool hasSprite = Shader.Find("TextMeshPro/Sprite") != null;
            if (hasDistanceField && hasMobileBitmap && hasSprite)
            {
                return true;
            }

            string packagePath = FindTMPResourcesPackage();
            if (string.IsNullOrEmpty(packagePath))
            {
                statusMessage = "TextMeshPro shaders are missing and TMP Essential Resources could not be found.";
                return false;
            }

            statusMessage = "Importing TMP Essential Resources to restore TextMeshPro shaders...";
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh();

            hasDistanceField = Shader.Find("TextMeshPro/Distance Field") != null;
            hasMobileBitmap = Shader.Find("TextMeshPro/Mobile/Bitmap") != null;
            hasSprite = Shader.Find("TextMeshPro/Sprite") != null;
            if (!hasDistanceField || !hasMobileBitmap || !hasSprite)
            {
                statusMessage = "TextMeshPro shaders are still missing after importing TMP Essential Resources.";
                return false;
            }

            statusMessage = "TextMeshPro shaders were restored successfully.";
            return true;
        }

        private string FindTMPResourcesPackage()
        {
            string packageCacheRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "PackageCache"));
            if (!Directory.Exists(packageCacheRoot))
            {
                return null;
            }

            var candidates = Directory.GetFiles(packageCacheRoot, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
            return candidates.FirstOrDefault();
        }

        private string GetTargetFontAssetPath(string sourceFontPath)
        {
            string sourceDirectory = Path.GetDirectoryName(sourceFontPath) ?? "Assets/Font";
            string sourceName = Path.GetFileNameWithoutExtension(sourceFontPath);
            string targetFile = sourceName + " SDF.asset";
            return Path.Combine(sourceDirectory, targetFile).Replace("\\", "/");
        }

        private void AddFontToPublicPixelFallback(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                statusMessage = "No font asset is available to add to fallback.";
                return;
            }

            var publicPixel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PublicPixelPath);
            if (publicPixel == null)
            {
                statusMessage = $"Could not find the game font asset at {PublicPixelPath}.";
                return;
            }

            publicPixel.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (!publicPixel.fallbackFontAssetTable.Contains(fontAsset))
            {
                publicPixel.fallbackFontAssetTable.Add(fontAsset);
                EditorUtility.SetDirty(publicPixel);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                statusMessage = $"Added {fontAsset.name} as a fallback on PublicPixel.";
            }
            else
            {
                statusMessage = $"{fontAsset.name} is already registered as a fallback on PublicPixel.";
            }
        }

        private void ApplyFontToAllText(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                statusMessage = "No font asset is available to apply.";
                return;
            }

            Font legacyFont = fontAsset.sourceFontFile;
            if (legacyFont == null && fontSourcePaths.Length > selectedFontIndex)
            {
                legacyFont = AssetDatabase.LoadAssetAtPath<Font>(fontSourcePaths[selectedFontIndex]);
            }

            int changed = 0;
            var publicPixel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PublicPixelPath);
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar("Applying font", path, (float)i / prefabGuids.Length);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    bool dirty = false;
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (ShouldSwap(text, publicPixel, fontAsset))
                        {
                            Apply(text, fontAsset);
                            dirty = true;
                            changed++;
                        }
                    }

                    if (legacyFont != null)
                    {
                        foreach (var legacyText in root.GetComponentsInChildren<Text>(true))
                        {
                            if (ShouldSwapLegacyText(legacyText, legacyFont))
                            {
                                Apply(legacyText, legacyFont);
                                dirty = true;
                                changed++;
                            }
                        }

                        foreach (var legacyMesh in root.GetComponentsInChildren<TextMesh>(true))
                        {
                            if (ShouldSwapLegacyTextMesh(legacyMesh, legacyFont))
                            {
                                Apply(legacyMesh, legacyFont);
                                dirty = true;
                                changed++;
                            }
                        }
                    }

                    if (dirty)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Font Wizard] Skipped prefab {path}: {ex.Message}");
                }
                finally
                {
                    if (root != null)
                        PrefabUtility.UnloadPrefabContents(root);
                }
            }

            var currentSetup = EditorSceneManager.GetSceneManagerSetup();
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar("Applying font", path, 0.9f + 0.1f * i / sceneGuids.Length);
                try
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    bool dirty = false;
                    foreach (var rootGo in scene.GetRootGameObjects())
                    {
                        foreach (var text in rootGo.GetComponentsInChildren<TMP_Text>(true))
                        {
                            if (ShouldSwap(text, publicPixel, fontAsset))
                            {
                                Apply(text, fontAsset);
                                dirty = true;
                                changed++;
                            }
                        }

                        if (legacyFont != null)
                        {
                            foreach (var legacyText in rootGo.GetComponentsInChildren<Text>(true))
                            {
                                if (ShouldSwapLegacyText(legacyText, legacyFont))
                                {
                                    Apply(legacyText, legacyFont);
                                    dirty = true;
                                    changed++;
                                }
                            }

                            foreach (var legacyMesh in rootGo.GetComponentsInChildren<TextMesh>(true))
                            {
                                if (ShouldSwapLegacyTextMesh(legacyMesh, legacyFont))
                                {
                                    Apply(legacyMesh, legacyFont);
                                    dirty = true;
                                    changed++;
                                }
                            }
                        }
                    }

                    if (dirty)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Font Wizard] Skipped scene {path}: {ex.Message}");
                }
            }

            EditorUtility.ClearProgressBar();
            if (currentSetup != null && currentSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(currentSetup);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            statusMessage = $"Applied {fontAsset.name} to {changed} text components.";
        }

        private void ReportAllTMPTextCounts()
        {
            int sceneCount = 0;
            int prefabCount = 0;
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    prefabCount += root.GetComponentsInChildren<TMP_Text>(true).Length;
                }
                catch
                {
                }
                finally
                {
                    if (root != null)
                        PrefabUtility.UnloadPrefabContents(root);
                }
            }

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            foreach (var guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (var rootGo in scene.GetRootGameObjects())
                    {
                        sceneCount += rootGo.GetComponentsInChildren<TMP_Text>(true).Length;
                    }
                }
                catch
                {
                }
            }

            statusMessage = $"Found {prefabCount} TMP texts in prefabs and {sceneCount} TMP texts in scenes.";
        }

        private void ReimportCrtShader()
        {
            const string shaderPath = "Assets/Resources/shaders/crt.shader";
            if (!AssetDatabase.LoadAssetAtPath<Shader>(shaderPath))
            {
                statusMessage = $"CRT shader not found at {shaderPath}.";
                return;
            }

            AssetDatabase.ImportAsset(shaderPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.Refresh();
            statusMessage = "Reimported CRT shader and refreshed the asset database.";
        }

        private static bool ShouldSwap(TMP_Text text, TMP_FontAsset publicPixel, TMP_FontAsset chosenFont)
        {
            if (text == null) return false;
            if (text.font == chosenFont) return false;
            if (text.font == null) return true;

            return true;
        }

        private static void Apply(TMP_Text text, TMP_FontAsset chosenFont)
        {
            text.font = chosenFont;
            if (chosenFont.material != null)
            {
                text.fontSharedMaterial = chosenFont.material;
            }
            EditorUtility.SetDirty(text);
        }

        private static bool ShouldSwapLegacyText(Text text, Font legacyFont)
        {
            if (text == null) return false;
            if (text.font == legacyFont) return false;
            return true;
        }

        private static void Apply(Text text, Font legacyFont)
        {
            text.font = legacyFont;
            EditorUtility.SetDirty(text);
        }

        private static bool ShouldSwapLegacyTextMesh(TextMesh textMesh, Font legacyFont)
        {
            if (textMesh == null) return false;
            if (textMesh.font == legacyFont) return false;
            return true;
        }

        private static void Apply(TextMesh textMesh, Font legacyFont)
        {
            textMesh.font = legacyFont;
            EditorUtility.SetDirty(textMesh);
        }
    }
}
