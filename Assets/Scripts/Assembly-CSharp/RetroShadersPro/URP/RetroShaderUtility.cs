using System.Reflection;
using UnityEngine.Rendering.Universal;

namespace RetroShadersPro.URP
{
	public static class RetroShaderUtility
	{
		public static ScriptableRendererData GetForwardRenderer()
		{
			ScriptableRendererData[] obj = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(UniversalRenderPipeline.asset);
			int num = (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(UniversalRenderPipeline.asset);
			return obj[num];
		}

		public static bool CheckEffectEnabled<T>() where T : ScriptableRendererFeature
		{
			if (UniversalRenderPipeline.asset == null)
			{
				return false;
			}
			foreach (ScriptableRendererFeature rendererFeature in ((ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(UniversalRenderPipeline.asset))[0].rendererFeatures)
			{
				if (rendererFeature?.GetType() == typeof(T))
				{
					return true;
				}
			}
			return false;
		}

		public static void AddEffectToPipelineAsset<T>() where T : ScriptableRendererFeature
		{
		}
	}
}
