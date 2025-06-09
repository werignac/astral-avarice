using UnityEngine;

public class GravityFieldCursorComponent : MonoBehaviour
{
	private const string INVERTED_PROPERTY = "_InvertGradient";

	private SpriteRenderer gravityFieldRenderer;
	private float initialSize;


	private void Awake()
	{
		gravityFieldRenderer = GetComponent<SpriteRenderer>();
		initialSize = transform.localScale.x;
	}

	/// <summary>
	/// Sets the gravity cursor's properties to show the change in mass on a planet.
	/// </summary>
	/// <param name="onPlanet">The planet to show the change in mass.</param>
	/// <param name="deltaMass">How much the mass will change.</param>
	public void SetGravityCursor(PlanetComponent onPlanet, int deltaMass)
	{
		float planetRadius = onPlanet.Radius;
		int currentMass = onPlanet.GetTotalMass();
		float currentGravityRadius = PlanetComponent.MassToGravityRadius(currentMass);
		float nextGravityRadius = PlanetComponent.MassToGravityRadius(currentMass + deltaMass);
		Vector3 gravityCursorPosition = onPlanet.transform.position;
		float outerGravityRadius = Mathf.Max(currentGravityRadius, nextGravityRadius, planetRadius + 0.3f);
		float innerGravityRadius = Mathf.Max(Mathf.Min(currentGravityRadius, nextGravityRadius), planetRadius + 0.3f);
		SetPosition(gravityCursorPosition);
		SetRadii(outerGravityRadius, innerGravityRadius);
		SetGradientIsInverted(deltaMass < 0);
	}

	private void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	private void SetRadii(float outerRadius, float innerRadius)
	{
		transform.localScale = Vector3.one * outerRadius * initialSize * 2;
		gravityFieldRenderer.material.SetFloat(Shader.PropertyToID("_InnerRadius"), innerRadius / outerRadius);
	}

	private void SetGradientIsInverted(bool isInverted)
	{
		gravityFieldRenderer.material.SetFloat(Shader.PropertyToID(INVERTED_PROPERTY), isInverted ? 1 : 0);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public bool GetIsShowing()
	{
		return gameObject.activeSelf;
	}
}
