#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Safe, idempotent memory tuning for already-mipmapped 3D textures and
/// long decoded audio clips. No visual resolution, sample rate, stereo
/// channel count or authored dialogue/PlayMaker references are changed.
/// </summary>
public static class GradomrazWebImporterTuner
{
    [MenuItem("Tools/GRADOMRAZ/Web Release/0 - Tune memory imports (3D + long audio)")]
    public static void TuneMemoryImporters()
    {
        int visitedTextures = 0, enabledStreaming = 0;
        int visitedAudio = 0, compressedAudio = 0;
        var report = new StringBuilder();
        report.AppendLine("GRADOMRAZ Web memory importer tuning / Unity " + Application.unityVersion);

        // Mipmaps remain sharp near the camera; lower-resolution distant
        // levels can be discarded instead of keeping all mip levels in RAM.
        // Do not alter UI/sprite/cursor assets or textures without mipmaps.
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;
            visitedTextures++;
            if (importer.textureType != TextureImporterType.Default &&
                importer.textureType != TextureImporterType.NormalMap)
                continue;
            if (!importer.mipmapEnabled || importer.streamingMipmaps)
                continue;

            importer.streamingMipmaps = true;
            importer.streamingMipmapsPriority = 0;
            importer.SaveAndReimport();
            enabledStreaming++;
            report.AppendLine("STREAM_MIPS " + path);
        }

        // Unity Web does not support conventional streamed AudioClips.
        // Decode-on-load 10+ second OGG/WAV clips can consume substantial
        // PCM memory. Keep them compressed in memory while retaining audio
        // quality, stereo channels, preload semantics and short SFX latency.
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                continue;
            visitedAudio++;
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null || clip.length < 10.0f)
                continue;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad)
                continue;

            settings.loadType = AudioClipLoadType.CompressedInMemory;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = true;
            importer.SaveAndReimport();
            compressedAudio++;
            report.AppendLine("COMPRESSED_LONG_AUDIO " + path +
                              " durationSeconds=" + clip.length.ToString("F1"));
        }

        AssetDatabase.SaveAssets();
        report.AppendLine("Textures inspected: " + visitedTextures);
        report.AppendLine("Textures enabled for mip streaming: " + enabledStreaming);
        report.AppendLine("AudioClips inspected: " + visitedAudio);
        report.AppendLine("Long audio clips switched to CompressedInMemory: " + compressedAudio);
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string output = Path.Combine(root, "Builds", "GRADOMRAZ-Web6");
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "importer-memory-audit.txt"), report.ToString());
        Debug.Log("[GRADOMRAZ-WEB-IMPORT] COMPLETE\n" + report);
    }
}
#endif
