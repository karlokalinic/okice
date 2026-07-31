using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Karlolegend.Gradomraz.Editor
{
    internal sealed class PreserveSerializedShaders : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            var saveablePaths = new List<string>(paths.Length);

            foreach (var path in paths)
            {
                if (!path.EndsWith(".asset", StringComparison.Ordinal) ||
                    AssetDatabase.LoadMainAssetAtPath(path) is not Shader)
                {
                    saveablePaths.Add(path);
                }
            }

            return saveablePaths.ToArray();
        }
    }
}
