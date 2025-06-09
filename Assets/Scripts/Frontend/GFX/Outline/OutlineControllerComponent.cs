using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using AstralAvarice.Frontend;

public class OutlineControllerComponent : MonoBehaviour
{
	private const string OUTLINE_RENDERER_FEATURE_NAME = "OutlinePostProcessRendererFeature";

	// Renderer with the outline feature. This is the main renderer. Not the renderer for rendering the stencil of the outline.
	[SerializeField] private new Renderer2DData renderer;
	private FullScreenPassRendererFeature outlineRendererFeature;

	private void Awake()
	{
		ScriptableRendererFeature feature = renderer.rendererFeatures.Find((ScriptableRendererFeature feature) => feature.name == OUTLINE_RENDERER_FEATURE_NAME);

		if (feature == null)
			throw new System.Exception($"Could not find scriptable render feature {OUTLINE_RENDERER_FEATURE_NAME} in renderer settings.");

		if (!(feature is FullScreenPassRendererFeature))
			throw new System.Exception($"Scriptable render feature {OUTLINE_RENDERER_FEATURE_NAME} is not of type {typeof(FullScreenPassRendererFeature)}");

		outlineRendererFeature = feature as FullScreenPassRendererFeature;
	}

	private void Start()
	{
		// By default the outline should be blue for select.
		SetOutlineSelect();

		BuildManagerComponent buildManager = BuildManagerComponent.Instance;

		// When we enter demolish mode, the outline should be red for demolish.
		buildManager.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void BuildManager_OnStateChanged(IBuildState oldState, IBuildState newState)
	{
		if (oldState == null || oldState.GetStateType() == BuildStateType.DEMOLISH)
			SetOutlineSelect();

		if (newState.GetStateType() == BuildStateType.DEMOLISH)
			SetOutlineDemolish();
	}

	private void SetOutlineDemolish()
	{
		SetOutlineMaterial(PtUUISettings.GetOrCreateSettings().DemolishOutlineMaterial);
	}

	private void SetOutlineSelect()
	{
		SetOutlineMaterial(PtUUISettings.GetOrCreateSettings().SelectOutlineMaterial);
	}

	private void SetOutlineMaterial(Material outlineMaterial)
	{
		outlineRendererFeature.passMaterial = outlineMaterial;
	}
}
