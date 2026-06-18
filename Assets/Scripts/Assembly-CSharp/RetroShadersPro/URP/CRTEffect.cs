using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RetroShadersPro.URP
{
	public class CRTEffect : ScriptableRendererFeature
	{
		private class CRTRenderPass : ScriptableRenderPass
		{
			private class CopyPassData
			{
				public TextureHandle inputTexture;

				public bool useBilinear;
			}

			private class MainPassData
			{
				public Material material;

				public TextureHandle inputTexture;

				public TextureHandle interlacingTexture;

				public int targetHeight;
			}

			private class InterlacePassData
			{
				public TextureHandle inputTexture;

				public bool useBilinear;
			}

			private Material material;

			private RTHandle tempTexHandle;

			private RTHandle interlaceTexHandle;

			private int frameCounter;

			public CRTRenderPass()
			{
				base.profilingSampler = new ProfilingSampler("CRT Effect");
				base.requiresIntermediateTexture = true;
			}

			private void CreateMaterial()
			{
				Shader shader = Shader.Find("Retro Shaders Pro/Post Processing/CRT");
				if (shader == null)
				{
					Debug.LogError("Cannot find shader: \"Retro Shaders Pro/Post Processing/CRT\".");
				}
				else
				{
					material = new Material(shader);
				}
			}

			private static RenderTextureDescriptor GetCopyPassDescriptor(RenderTextureDescriptor descriptor)
			{
				descriptor.msaaSamples = 1;
				descriptor.depthBufferBits = 0;
				CRTSettings component = VolumeManager.instance.stack.GetComponent<CRTSettings>();
				float num = 1f;
				if (component.scaleParameters.value)
				{
					num = (float)component.verticalReferenceResolution.value / (float)descriptor.height;
				}
				int width = (int)Mathf.Max(4f, (float)descriptor.width / ((float)component.pixelSize.value / num));
				int height = (int)Mathf.Max(4f, (float)descriptor.height / ((float)component.pixelSize.value / num));
				descriptor.width = width;
				descriptor.height = height;
				return descriptor;
			}

			private static RenderTextureDescriptor GetInterlaceDescriptor(RenderTextureDescriptor descriptor)
			{
				descriptor.msaaSamples = 1;
				descriptor.depthBufferBits = 0;
				return descriptor;
			}

			public void CreateInterlacingTexture()
			{
				RenderingUtils.ReAllocateHandleIfNeeded(descriptor: GetInterlaceDescriptor(new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0)), handle: ref interlaceTexHandle, filterMode: FilterMode.Point, wrapMode: TextureWrapMode.Repeat, anisoLevel: 1, mipMapBias: 0f, name: "_CRTInterlacingTexture");
			}

#if !UNITY_6000_0_OR_NEWER
			[Obsolete]
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			{
				ResetTarget();
				RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexHandle, GetCopyPassDescriptor(cameraTextureDescriptor), FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, "_CRTColorCopy");
				RenderingUtils.ReAllocateHandleIfNeeded(ref interlaceTexHandle, GetInterlaceDescriptor(cameraTextureDescriptor), FilterMode.Point, TextureWrapMode.Repeat, 1, 0f, "_CRTInterlacingTexture");
				RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, GetCopyPassDescriptor(cameraTextureDescriptor), FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_CRTColorCopy");
				RenderingUtils.ReAllocateIfNeeded(ref interlaceTexHandle, GetInterlaceDescriptor(cameraTextureDescriptor), FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_CRTInterlacingTexture");
				base.Configure(cmd, cameraTextureDescriptor);
			}
#endif

			private void SetMaterialProperties(RTHandle interlacingTexture, int targetHeight, Material material)
			{
				CRTSettings component = VolumeManager.instance.stack.GetComponent<CRTSettings>();
				base.renderPassEvent = component.renderPassEvent.value.Convert();
				Texture value = ((component.rgbTex.value == null) ? Texture2D.whiteTexture : component.rgbTex.value);
				Texture value2 = ((component.scanlineTex.value == null) ? Texture2D.whiteTexture : component.scanlineTex.value);
				Texture value3 = ((component.trackingTexture.value == null) ? Texture2D.grayTexture : component.trackingTexture.value);
				material.SetColor("_TintColor", component.tintColor.value);
				material.SetColor("_BackgroundColor", component.backgroundColor.value);
				material.SetFloat("_DistortionStrength", component.distortionStrength.value);
				material.SetFloat("_DistortionSmoothing", component.distortionSmoothing.value);
				material.SetTexture("_RGBTex", value);
				material.SetFloat("_RGBStrength", component.rgbStrength.value);
				material.SetTexture("_ScanlineTex", value2);
				material.SetFloat("_ScanlineStrength", component.scanlineStrength.value);
				material.SetFloat("_ScrollSpeed", component.scrollSpeed.value);
				material.SetFloat("_RandomWear", component.randomWear.value);
				material.SetFloat("_AberrationStrength", component.aberrationStrength.value);
				material.SetTexture("_TrackingTex", value3);
				material.SetFloat("_TrackingSize", component.trackingSize.value);
				material.SetFloat("_TrackingStrength", component.trackingStrength.value);
				material.SetFloat("_TrackingSpeed", component.trackingSpeed.value);
				material.SetFloat("_TrackingJitter", component.trackingJitter.value);
				material.SetFloat("_TrackingColorDamage", component.trackingColorDamage.value);
				material.SetFloat("_TrackingLinesThreshold", component.trackingLinesThreshold.value);
				material.SetColor("_TrackingLinesColor", component.trackingLinesColor.value);
				material.SetFloat("_Brightness", component.brightness.value);
				material.SetFloat("_Contrast", component.contrast.value);
				material.SetInt("_Interlacing", frameCounter++ % 2);
				material.SetTexture("_InputTexture", interlacingTexture);
				if (component.scaleParameters.value)
				{
					float num = (float)component.verticalReferenceResolution.value / (float)targetHeight;
					material.SetInt("_Size", (int)((float)component.scanlineSize.value / num));
				}
				else
				{
					material.SetInt("_Size", component.scanlineSize.value);
				}
				if (component.enableInterlacing.value && frameCounter > 1)
				{
					material.EnableKeyword("_INTERLACING_ON");
				}
				else
				{
					material.DisableKeyword("_INTERLACING_ON");
				}
				if (component.forcePointFiltering.value)
				{
					material.EnableKeyword("_POINT_FILTERING_ON");
				}
				else
				{
					material.DisableKeyword("_POINT_FILTERING_ON");
				}
				if (component.aberrationStrength.value > 0.01f)
				{
					material.EnableKeyword("_CHROMATIC_ABERRATION_ON");
				}
				else
				{
					material.DisableKeyword("_CHROMATIC_ABERRATION_ON");
				}
				if (component.trackingTexture.value == null || (component.trackingStrength.value < 0.001f && component.trackingColorDamage.value < 0.001f && component.trackingLinesThreshold.value > 0.999f))
				{
					material.DisableKeyword("_TRACKING_ON");
				}
				else
				{
					material.EnableKeyword("_TRACKING_ON");
				}
			}

#if !UNITY_6000_0_OR_NEWER
			[Obsolete]
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				if (material == null)
				{
					CreateMaterial();
					if (material == null)
					{
						return;
					}
				}
				CRTSettings component = VolumeManager.instance.stack.GetComponent<CRTSettings>();
				if ((renderingData.cameraData.isSceneViewCamera && !component.showInSceneView.value) || renderingData.cameraData.isPreviewCamera)
				{
					return;
				}
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
				SetMaterialProperties(interlaceTexHandle, cameraColorTargetHandle.rt.height, material);
				using (new ProfilingScope(commandBuffer, base.profilingSampler))
				{
					using (new ProfilingScope(commandBuffer, base.profilingSampler))
					{
						Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, tempTexHandle, 0f, !component.forcePointFiltering.value);
						Blitter.BlitCameraTexture(commandBuffer, tempTexHandle, cameraColorTargetHandle, material, 0);
						if (component.enableInterlacing.value)
						{
							Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, interlaceTexHandle, 0f, !component.forcePointFiltering.value);
						}
					}
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				CommandBufferPool.Release(commandBuffer);
			}
#endif

			public void Dispose()
			{
				tempTexHandle?.Release();
			}

			private void ExecuteCopyPass(RasterCommandBuffer cmd, RTHandle source, bool useBilinear)
			{
				Blitter.BlitTexture(cmd, source, new Vector4(1f, 1f, 0f, 0f), 0f, useBilinear);
			}

			private void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle source, RTHandle interlacingTexture, int targetHeight, Material material)
			{
				SetMaterialProperties(interlacingTexture, targetHeight, material);
				Blitter.BlitTexture(cmd, source, new Vector4(1f, 1f, 0f, 0f), material, 0);
			}

			private void ExecuteInterlacePass(RasterCommandBuffer cmd, RTHandle source, bool useBilinear)
			{
				Blitter.BlitTexture(cmd, source, new Vector4(1f, 1f, 0f, 0f), 0f, useBilinear);
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				if (material == null)
				{
					CreateMaterial();
					if (material == null)
					{
						return;
					}
				}
				UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
				UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
				CRTSettings component = VolumeManager.instance.stack.GetComponent<CRTSettings>();
				if ((universalCameraData.isSceneViewCamera && !component.showInSceneView.value) || universalCameraData.isPreviewCamera)
				{
					return;
				}
				RenderTextureDescriptor copyPassDescriptor = GetCopyPassDescriptor(universalCameraData.cameraTargetDescriptor);
				GetInterlaceDescriptor(universalCameraData.cameraTargetDescriptor);
				TextureHandle nullHandle = TextureHandle.nullHandle;
				TextureHandle textureHandle = TextureHandle.nullHandle;
				if (interlaceTexHandle != null)
				{
					textureHandle = renderGraph.ImportTexture(interlaceTexHandle);
				}
				nullHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, copyPassDescriptor, "_CRTColorCopy", clear: false);
				CopyPassData passData;
				using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<CopyPassData>("CRT_CopyColor", out passData, base.profilingSampler, "C:\\Users\\kwark\\OneDrive\\Documenten\\Unity Projects\\AFTERLIVES\\Assets\\Retro Shaders Pro\\Scripts\\Shaders\\CRTEffect.cs", 337))
				{
					passData.inputTexture = universalResourceData.activeColorTexture;
					passData.useBilinear = !component.forcePointFiltering.value;
					rasterRenderGraphBuilder.UseTexture(universalResourceData.activeColorTexture);
					rasterRenderGraphBuilder.SetRenderAttachment(nullHandle, 0);
					rasterRenderGraphBuilder.SetRenderFunc(delegate(CopyPassData data, RasterGraphContext context)
					{
						ExecuteCopyPass(context.cmd, data.inputTexture, data.useBilinear);
					});
				}
				MainPassData passData2;
				using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<MainPassData>("CRT_MainPass", out passData2, base.profilingSampler, "C:\\Users\\kwark\\OneDrive\\Documenten\\Unity Projects\\AFTERLIVES\\Assets\\Retro Shaders Pro\\Scripts\\Shaders\\CRTEffect.cs", 348))
				{
					passData2.material = material;
					passData2.inputTexture = nullHandle;
					passData2.interlacingTexture = textureHandle;
					passData2.targetHeight = universalCameraData.cameraTargetDescriptor.height;
					rasterRenderGraphBuilder2.UseTexture(in nullHandle);
					if (textureHandle.IsValid())
					{
						rasterRenderGraphBuilder2.UseTexture(in textureHandle);
					}
					rasterRenderGraphBuilder2.SetRenderAttachment(universalResourceData.activeColorTexture, 0);
					rasterRenderGraphBuilder2.SetRenderFunc(delegate(MainPassData data, RasterGraphContext context)
					{
						ExecuteMainPass(context.cmd, data.inputTexture, data.interlacingTexture, data.targetHeight, data.material);
					});
				}
				if (!component.enableInterlacing.value || !textureHandle.IsValid())
				{
					return;
				}
				InterlacePassData passData3;
				using IRasterRenderGraphBuilder rasterRenderGraphBuilder3 = renderGraph.AddRasterRenderPass<InterlacePassData>("CRT_CopyInterlacingTexture", out passData3, base.profilingSampler, "C:\\Users\\kwark\\OneDrive\\Documenten\\Unity Projects\\AFTERLIVES\\Assets\\Retro Shaders Pro\\Scripts\\Shaders\\CRTEffect.cs", 367);
				passData3.inputTexture = universalResourceData.activeColorTexture;
				passData3.useBilinear = !component.forcePointFiltering.value;
				rasterRenderGraphBuilder3.UseTexture(universalResourceData.activeColorTexture);
				rasterRenderGraphBuilder3.SetRenderAttachment(textureHandle, 0);
				rasterRenderGraphBuilder3.SetRenderFunc(delegate(InterlacePassData data, RasterGraphContext context)
				{
					ExecuteInterlacePass(context.cmd, data.inputTexture, data.useBilinear);
				});
			}
		}

		private CRTRenderPass pass;

		public override void Create()
		{
			pass = new CRTRenderPass();
			base.name = "CRT";
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			CRTSettings component = VolumeManager.instance.stack.GetComponent<CRTSettings>();
			if (component != null && component.IsActive())
			{
				pass.CreateInterlacingTexture();
				renderer.EnqueuePass(pass);
			}
		}

		protected override void Dispose(bool disposing)
		{
			pass.Dispose();
			base.Dispose(disposing);
		}
	}
}
