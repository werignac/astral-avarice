using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule.Util;

// This example clears the current active color texture, then renders the scene geometry associated to the m_LayerMask layer.
// Add scene geometry to your own custom layers and experiment switching the layer mask in the render feature UI.
// You can use the frame debugger to inspect the pass output.

// Based on Unity Samples
namespace UnityEngine.Rendering.Universal
{
	public class OutlineStencilRendererFeature : ScriptableRendererFeature
	{
		class OutlineStencilRenderPass : ScriptableRenderPass
		{
			Material m_OutlineMaterial;

			// Layer mask used to filter objects to put in the renderer list
			private LayerMask m_LayerMask;

			// List of shader tags used to build the renderer list
			private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

			public OutlineStencilRenderPass(Material material, LayerMask layerMask)
			{
				m_OutlineMaterial = material;
				m_LayerMask = layerMask;
				requiresIntermediateTexture = true;
			}

			// This class stores the data needed by the pass, passed as parameter to the delegate function that executes the pass
			private class PassData
			{
				public RendererListHandle rendererListHandle;
			}

			[System.Obsolete()]
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				throw new System.NotImplementedException();
			}

			private static void Execute(RasterCommandBuffer cmd, PassData passData)
			{
				cmd.DrawRendererList(passData.rendererListHandle);
			}

			// Sample utility method that showcases how to create a renderer list via the RenderGraph API
			private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
			{
				// Access the relevant frame data from the Universal Render Pipeline
				UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
				UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
				UniversalLightData lightData = frameData.Get<UniversalLightData>();

				var sortFlags = cameraData.defaultOpaqueSortFlags;
				RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
				FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

				ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
				{
					new ShaderTagId("UniversalForwardOnly"),
					new ShaderTagId("UniversalForward"),
					new ShaderTagId("SRPDefaultUnlit"), // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
					new ShaderTagId("LightweightForward"), // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
				};

				m_ShaderTagIdList.Clear();

				foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
					m_ShaderTagIdList.Add(sid);

				DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

				var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
				passData.rendererListHandle = renderGraph.CreateRendererList(param);
			}

			// This static method is used to execute the pass and passed as the RenderFunc delegate to the RenderGraph render pass
			static void ExecutePass(PassData data, RasterGraphContext context)
			{
				context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.green, 1, 0);

				context.cmd.DrawRendererList(data.rendererListHandle);
			}

			// This is where the renderGraph handle can be accessed.
			// Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
				UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
				UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
				UniversalLightData lightData = frameData.Get<UniversalLightData>();

				TextureHandle finalDestination = resourceData.activeColorTexture;

				var destinationDesc = renderGraph.GetTextureDesc(finalDestination);
				destinationDesc.name = $"CameraColor-NormalOutlinePass";
				destinationDesc.clearBuffer = false;

				TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

				using (var builder = renderGraph.AddRasterRenderPass<PassData>("Normal2D Outline Pass", out var passData, new ProfilingSampler("Normal2D Outline Pass")))
				{
					var filterSettings = FilteringSettings.defaultValue;
					filterSettings.renderQueueRange = RenderQueueRange.all;
					filterSettings.layerMask = -1;
					filterSettings.renderingLayerMask = 0xFFFFFFFF;
					filterSettings.sortingLayerRange = new SortingLayerRange();

					var drawSettings = CreateDrawingSettings(new ShaderTagId("NormalsRendering"), renderingData, cameraData, lightData, SortingCriteria.CommonTransparent);
					builder.AllowPassCulling(false);

					builder.SetRenderAttachment(destination, 0);
					//builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

					var param = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
					passData.rendererListHandle = renderGraph.CreateRendererList(param);
					builder.UseRendererList(passData.rendererListHandle);

					builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
					{
						Execute(context.cmd, data);
					});
				}

				RenderGraphUtils.BlitMaterialParameters ditherPara = new(destination, finalDestination, m_OutlineMaterial, 0);
				renderGraph.AddBlitPass(ditherPara, passName: "NormalOutlinePass");
			}
		}

		OutlineStencilRenderPass m_ScriptablePass;

		public LayerMask m_LayerMask;

		public Material outlineMaterial;

		/// <inheritdoc/>
		public override void Create()
		{
			m_ScriptablePass = new OutlineStencilRenderPass(outlineMaterial, m_LayerMask);

			// Configures where the render pass should be injected.
			m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		}

		// Here you can inject one or multiple render passes in the renderer.
		// This method is called when setting up the renderer once per-camera.
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(m_ScriptablePass);
		}
	}
}
