using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

// From: https://youtu.be/U8PygjYAF7A?si=4Kmrqk_Fee9Xe5m-
public class DitherEffectRendererFeature : ScriptableRendererFeature
{
    class DitherEffectPass : ScriptableRenderPass
    {
		const string m_PassName = "DitherEffectPass";
		Material m_BlitMaterial;

		public void Setup(Material material)
		{
			m_BlitMaterial = material;
			// Automatically create a texture to write to?
			requiresIntermediateTexture = true;
		}

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
			// TODO: Create PostProcess and Access here to decide color etc.

			// Access point to all render textures.
			var resourceData = frameData.Get<UniversalResourceData>();

			if (resourceData.isActiveTargetBackBuffer)
			{
				Debug.LogError($"Skipping render pass. ditherEffectRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
				return;
			}

			TextureHandle source = resourceData.activeColorTexture;

			var destinationDesc = renderGraph.GetTextureDesc(source);
			destinationDesc.name = $"CameraColor-{m_PassName}";
			destinationDesc.clearBuffer = false;

			TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

			RenderGraphUtils.BlitMaterialParameters ditherPara = new(source, destination, m_BlitMaterial, 0);
			renderGraph.AddBlitPass(ditherPara, passName: m_PassName);
			renderGraph.AddCopyPass(destination, source, passName: "CopyIntoCameraColor");
        }
    }

	public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
	public Material material;

    DitherEffectPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new DitherEffectPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
		if (material == null)
		{
			Debug.LogWarning("DitherEffectRendererFeature material is null and will be skipped.");
			return;
		}

		m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
