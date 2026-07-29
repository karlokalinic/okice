using System;
using UnityEngine;
using UnityEditor;

namespace Karlolegend.Gradomraz.Editor
{
	/// <summary>
	/// Registers serialized shader assets so they can be found by name at edit time.
	/// </summary>
	public class SerializedShaderPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (var importedAsset in importedAssets)
			{
				if (!importedAsset.EndsWith(".asset", StringComparison.Ordinal)) continue;
				Shader yamlShader = AssetDatabase.LoadMainAssetAtPath(importedAsset) as Shader;
				if (yamlShader == null) continue;
				ShaderUtil.RegisterShader(yamlShader);
				Debug.Log($"Registered shader \"{yamlShader.name}\" from {importedAsset}");
			}
		}
	}
}