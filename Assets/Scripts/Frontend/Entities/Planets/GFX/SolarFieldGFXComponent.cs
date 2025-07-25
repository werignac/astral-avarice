using UnityEngine;
using UnityEngine.Rendering.Universal;
using AstralAvarice.Visualization;
using AstralAvarice.UI.Tooltips;

public class SolarFieldGFXComponent : MonoBehaviour
{
	private const string SOLAR_FIELD_INNER_RADIUS_PROPERTY_NAME = "_InnerRadius";
	private const string SOLAR_FIELD_STEPS_PROPERTY_NAME = "_StepCount";
	private const string SOLAR_FIELD_HIGHLIGHTED_RADIUS_PROPERTY_NAME = "_HighlightedRadius";


	private PlanetComponent planet;
	// Assumed to be spot light with no angle (point light).
	// This light is set to have the radius of planet.SolarOutput.
	[SerializeField] private Light2D starLight;
	[SerializeField, Min(0)] private float lightFalloffRadius = 0.5f;

	// Sprite renderer that shows where planets increase / decrease in solar energy.
	[SerializeField] private Renderer solarFieldRenderer;

	// State of whether solar fields should be visualized or not.
	[SerializeField] private VisualizationToggleState_SO solarFieldVisualizationState;

	[SerializeField] private SolarFieldHoverComponent _hoverComponent;


	private void Awake()
	{
		planet = GetComponentInParent<PlanetComponent>();
	}

	private void Start()
	{
		SetLightRadii();

		// Show or hide the field based on the current state.
		SetShowField(solarFieldVisualizationState.Value);
		// Listen to when the state of whether fields should be shown or hidden changes in the future.
		solarFieldVisualizationState.AddStateChangeListener(SetShowField);

		_hoverComponent.OnHoverRadius.AddListener(SetHighlightedRadius);
	}

	private void SetLightRadii(bool setMaterialProperties = true)
	{
		if (starLight == null)
			return;

		starLight.pointLightOuterRadius = planet.SolarOutput;
		starLight.pointLightInnerRadius = starLight.pointLightOuterRadius - lightFalloffRadius;


		solarFieldRenderer.transform.localScale = Vector3.one * planet.SolarOutput * 2 / transform.lossyScale.x;

		if (setMaterialProperties)
		{
			solarFieldRenderer.material.SetFloat(Shader.PropertyToID(SOLAR_FIELD_INNER_RADIUS_PROPERTY_NAME), 0);
			solarFieldRenderer.material.SetFloat(Shader.PropertyToID(SOLAR_FIELD_STEPS_PROPERTY_NAME), planet.SolarOutput);
		}
	}

#if UNITY_EDITOR

	// Show the radius in-editor.
	private void OnValidate()
	{
		planet = GetComponentInParent<PlanetComponent>();

		if (planet != null)
			SetLightRadii(false);
	}
#endif

	private void SetShowField(bool showField)
	{
		solarFieldRenderer.enabled = showField;
		_hoverComponent.enabled = showField;
	}

	private void SetHighlightedRadius(int highlightedRadius)
	{
		solarFieldRenderer.material.SetFloat(Shader.PropertyToID(SOLAR_FIELD_HIGHLIGHTED_RADIUS_PROPERTY_NAME), highlightedRadius);
	}

	/// <summary>
	/// Cleanup listeners to solar field visualization state (SOs exist beyond the scope
	/// / lifetime of this component).
	/// </summary>
	private void OnDestroy()
	{
		solarFieldVisualizationState.RemoveStateChangeListener(SetShowField);
	}
}
