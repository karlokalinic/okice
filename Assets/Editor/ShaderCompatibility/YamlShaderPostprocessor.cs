using System;
using UnityEditor;
using UnityEngine;

namespace Karlolegend.Gradomraz.Editor
{
    internal sealed class SerializedShaderPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (path.EndsWith(".asset", StringComparison.Ordinal) &&
                    AssetDatabase.LoadMainAssetAtPath(path) is Shader shader)
                {
                    ShaderUtil.RegisterShader(shader);
                }
            }
        }
    }
}
