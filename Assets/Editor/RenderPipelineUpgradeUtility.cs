using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class RenderPipelineUpgradeUtility
{
    private const string HdrpFolder = "Assets/HDRPDefaultResources";
    private const string HdrpAssetPath = HdrpFolder + "/HDRPAsset.asset";

    [MenuItem("Tools/Repair/Upgrade Project to HDRP Safely")]
    public static void UpgradeProjectToHDRP()
    {
        if (!TryGetHdrpAsset(out RenderPipelineAsset hdrpAsset))
        {
            if (!CreateHdrpAsset(out hdrpAsset))
            {
                Debug.LogError("HDRP upgrade failed: HDRP package or asset type is not available.");
                return;
            }
        }

        if (!AssignRenderPipelineAsset(hdrpAsset))
        {
            Debug.LogError("HDRP upgrade failed: could not assign the HDRP asset to graphics/quality settings.");
            return;
        }

        SetLinearColorSpace();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("HDRP upgrade helper completed. Open the Editor and verify scene/material compatibility.");
    }

    [MenuItem("Tools/Repair/Restore URP Project")]
    public static void RestoreUrpProject()
    {
        if (!TryGetUrpAsset(out RenderPipelineAsset urpAsset))
        {
            Debug.LogError("URP restore failed: no UniversalRenderPipelineAsset found in the project.");
            return;
        }

        if (!AssignRenderPipelineAsset(urpAsset))
        {
            Debug.LogError("URP restore failed: could not assign the URP asset to graphics/quality settings.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Restored URP asset: {AssetDatabase.GetAssetPath(urpAsset)}");
    }

    [MenuItem("Tools/Repair/Print Current Render Pipeline")]
    public static void PrintCurrentRenderPipeline()
    {
        var currentRp = GraphicsSettings.renderPipelineAsset;
        Debug.Log($"GraphicsSettings.renderPipelineAsset = {(currentRp ? currentRp.name : "null")} ({currentRp?.GetType().FullName ?? "none"})");

        var qualityRp = GetQualitySettingsRenderPipeline();
        Debug.Log($"QualitySettings.renderPipeline = {(qualityRp ? qualityRp.name : "null")} ({qualityRp?.GetType().FullName ?? "none"})");

        Debug.Log($"PlayerSettings.colorSpace = {PlayerSettings.colorSpace}");
    }

    private static bool TryGetHdrpAsset(out UnityEngine.Rendering.RenderPipelineAsset asset)
    {
        asset = null;
        var hdrpType = GetHdrpType();
        if (hdrpType == null)
            return false;

        foreach (var guid in AssetDatabase.FindAssets("t:HDRenderPipelineAsset"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var loaded = AssetDatabase.LoadAssetAtPath(path, hdrpType) as UnityEngine.Rendering.RenderPipelineAsset;
            if (loaded != null)
            {
                asset = loaded;
                Debug.Log($"Found existing HDRP asset at {path}");
                return true;
            }
        }

        return false;
    }

    private static bool TryGetUrpAsset(out UnityEngine.Rendering.RenderPipelineAsset asset)
    {
        asset = null;
        var urpType = GetUrpType();
        if (urpType == null)
            return false;

        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var loaded = AssetDatabase.LoadAssetAtPath(path, urpType) as UnityEngine.Rendering.RenderPipelineAsset;
            if (loaded != null)
            {
                asset = loaded;
                Debug.Log($"Found existing URP asset at {path}");
                return true;
            }
        }

        return false;
    }

    private static bool CreateHdrpAsset(out UnityEngine.Rendering.RenderPipelineAsset asset)
    {
        asset = null;
        var hdrpType = GetHdrpType();
        if (hdrpType == null)
            return false;

        if (!AssetDatabase.IsValidFolder(HdrpFolder))
        {
            AssetDatabase.CreateFolder("Assets", "HDRPDefaultResources");
        }

        asset = ScriptableObject.CreateInstance(hdrpType) as UnityEngine.Rendering.RenderPipelineAsset;
        if (asset == null)
        {
            return false;
        }

        AssetDatabase.CreateAsset(asset, HdrpAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(HdrpAssetPath);
        Debug.Log($"Created HDRP asset at {HdrpAssetPath}");
        return true;
    }

    private static bool AssignRenderPipelineAsset(UnityEngine.Rendering.RenderPipelineAsset asset)
    {
        if (asset == null)
            return false;

        bool success = false;

        try
        {
            GraphicsSettings.renderPipelineAsset = asset;
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to assign GraphicsSettings.renderPipelineAsset: {e.Message}");
        }

        try
        {
            var qualityRpProp = typeof(QualitySettings).GetProperty("renderPipeline", BindingFlags.Static | BindingFlags.Public);
            if (qualityRpProp != null)
            {
                qualityRpProp.SetValue(null, asset);
            }
            else
            {
                Debug.LogWarning("QualitySettings.renderPipeline property not found.");
            }
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to assign QualitySettings.renderPipeline: {e.Message}");
        }

        return success;
    }

    private static ScriptableObject GetQualitySettingsRenderPipeline()
    {
        try
        {
            var qualityRpProp = typeof(QualitySettings).GetProperty("renderPipeline", BindingFlags.Static | BindingFlags.Public);
            if (qualityRpProp != null)
            {
                return qualityRpProp.GetValue(null) as ScriptableObject;
            }
        }
        catch
        {
        }

        return null;
    }

    private static void SetLinearColorSpace()
    {
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            Debug.Log("Set PlayerSettings.colorSpace = Linear.");
        }
    }

    private static Type GetHdrpType()
    {
        return Type.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset, Unity.RenderPipelines.HighDefinition.Runtime");
    }

    private static Type GetUrpType()
    {
        return Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
    }
}
