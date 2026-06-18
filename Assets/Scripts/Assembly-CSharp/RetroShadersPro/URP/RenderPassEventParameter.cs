using System;
using UnityEngine.Rendering;

namespace RetroShadersPro.URP
{
	[Serializable]
	public sealed class RenderPassEventParameter : VolumeParameter<PostProcessRenderPassEvent>
	{
		public RenderPassEventParameter(PostProcessRenderPassEvent value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}
}
