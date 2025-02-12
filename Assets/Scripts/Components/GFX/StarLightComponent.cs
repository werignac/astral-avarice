using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StarLightComponent : MonoBehaviour
{

	private PlanetComponent planet;
	// Assumed to be spot light with no angle (point light).
	[SerializeField] private Light2D starLight;
	[SerializeField, Min(0)] private float lightFalloffRadius = 0.5f;


	private void Awake()
	{
		planet = GetComponentInParent<PlanetComponent>();
	}

	private void Start()
	{
		SetLightRadii();
	}

	private void SetLightRadii()
	{
		if (starLight == null)
			return;

		starLight.pointLightOuterRadius = planet.SolarOutput;
		starLight.pointLightInnerRadius = starLight.pointLightOuterRadius - lightFalloffRadius;
	}

#if UNITY_EDITOR

	// Show the radius in-editor.
	private void OnValidate()
	{
		planet = GetComponentInParent<PlanetComponent>();

		if (planet != null)
			SetLightRadii();
	}
#endif

}
