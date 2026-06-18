using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RetroShadersPro.URP
{
	[Serializable]
	[VolumeComponentMenu("Retro Shaders Pro/CRT")]
	public class CRTSettings : VolumeComponent, IPostProcessComponent
	{
		public BoolParameter showInSceneView = new BoolParameter(value: true);

		public BoolParameter enabled = new BoolParameter(value: false);

		public RenderPassEventParameter renderPassEvent = new RenderPassEventParameter(PostProcessRenderPassEvent.AfterURPPostProcessing);

		public ClampedFloatParameter distortionStrength = new ClampedFloatParameter(0f, 0f, 1f);

		public ClampedFloatParameter distortionSmoothing = new ClampedFloatParameter(0.01f, 0f, 0.1f);

		public ColorParameter tintColor = new ColorParameter(Color.white);

		public ColorParameter backgroundColor = new ColorParameter(Color.black);

		public BoolParameter scaleParameters = new BoolParameter(value: false);

		public IntParameter verticalReferenceResolution = new IntParameter(1080);

		public BoolParameter forcePointFiltering = new BoolParameter(value: false);

		public TextureParameter rgbTex = new TextureParameter(null);

		public ClampedFloatParameter rgbStrength = new ClampedFloatParameter(0f, 0f, 1f);

		public TextureParameter scanlineTex = new TextureParameter(null);

		public ClampedFloatParameter scanlineStrength = new ClampedFloatParameter(0f, 0f, 1f);

		public ClampedIntParameter scanlineSize = new ClampedIntParameter(8, 1, 64);

		public ClampedFloatParameter scrollSpeed = new ClampedFloatParameter(0f, 0f, 10f);

		public ClampedIntParameter pixelSize = new ClampedIntParameter(1, 1, 256);

		public ClampedFloatParameter randomWear = new ClampedFloatParameter(0.2f, 0f, 5f);

		public ClampedFloatParameter aberrationStrength = new ClampedFloatParameter(0.5f, 0f, 10f);

		public TextureParameter trackingTexture = new TextureParameter(null);

		public ClampedFloatParameter trackingSize = new ClampedFloatParameter(1f, 0.1f, 2f);

		public ClampedFloatParameter trackingStrength = new ClampedFloatParameter(0.1f, 0f, 50f);

		public ClampedFloatParameter trackingSpeed = new ClampedFloatParameter(0.1f, -2.5f, 2.5f);

		public ClampedFloatParameter trackingJitter = new ClampedFloatParameter(0.01f, 0f, 0.1f);

		public ClampedFloatParameter trackingColorDamage = new ClampedFloatParameter(0.05f, 0f, 1f);

		public ClampedFloatParameter trackingLinesThreshold = new ClampedFloatParameter(0.9f, 0f, 1f);

		public ColorParameter trackingLinesColor = new ColorParameter(new Color(1f, 1f, 1f, 0.5f));

		public ClampedFloatParameter brightness = new ClampedFloatParameter(1f, 0f, 3f);

		public ClampedFloatParameter contrast = new ClampedFloatParameter(1f, 0f, 3f);

		public BoolParameter enableInterlacing = new BoolParameter(value: false);

		public bool IsActive()
		{
			if (enabled.value)
			{
				return active;
			}
			return false;
		}

		public bool IsTileCompatible()
		{
			return false;
		}
	}
}
