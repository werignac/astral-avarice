using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SolarFieldComponent : MonoBehaviour
{
	private const string SOLAR_FIELD_INNER_RADIUS_PROPERTY_NAME = "_InnerRadius";
	private const string SOLAR_FIELD_STEPS_PROPERTY_NAME = "_StepCount";


	private PlanetComponent planet;
	// Assumed to be spot light with no angle (point light).
	// This light is set to have the radius of planet.SolarOutput.
	[SerializeField] private Light2D starLight;
	[SerializeField, Min(0)] private float lightFalloffRadius = 0.5f;

	// Sprite renderer that shows where planets increase / decrease in solar energy.
	[SerializeField] private Renderer solarFieldRenderer;


	private void Awake()
	{
		planet = GetComponentInParent<PlanetComponent>();
	}

	private void Start()
	{
		SetLightRadii();
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

}
