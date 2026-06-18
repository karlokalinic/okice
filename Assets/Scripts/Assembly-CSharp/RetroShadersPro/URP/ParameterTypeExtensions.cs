using UnityEngine.Rendering.Universal;

namespace RetroShadersPro.URP
{
	public static class ParameterTypeExtensions
	{
		public static RenderPassEvent Convert(this PostProcessRenderPassEvent renderPassEvent)
		{
			if (renderPassEvent == PostProcessRenderPassEvent.BeforeURPPostProcessing)
			{
				return RenderPassEvent.BeforeRenderingPostProcessing;
			}
			return RenderPassEvent.AfterRenderingPostProcessing;
		}
	}
}
