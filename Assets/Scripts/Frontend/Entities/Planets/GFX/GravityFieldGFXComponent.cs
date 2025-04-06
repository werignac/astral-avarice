using UnityEngine;
using UnityEngine.SceneManagement;
using AstralAvarice.Visualization;
using System;

public class GravityFieldGFXComponent : MonoBehaviour
{
	private PlanetComponent planetComponent;

	[SerializeField] private SpriteRenderer gravityFieldRenderer;

	private float initialScale = 0f;
	// Draw a little bit into the planet to make up for potential GFX inaccuracies
	// e.g. stars wiggle on the edge.
	[SerializeField, Min(0)] private float marginIntoPlanet = 0.1f;
	// The minimum radius to draw.
	[SerializeField, Min(0)] private float minimumFieldDrawRadius = 0.3f;

	// When true, update the circle on the next LateUpdate.
	// When false, don't update.
	private bool markedForGravityCircleUpdate;

	[SerializeField] private VisualizationToggleState_SO gravityToggleState;

	// Are we expecting the planet to change mass due to the player building or demolishing on the planet?
	// The player could be hovering over a building on the planet, or near the planet depending on the
	// build mode.
	private bool isPlanetProspectingMassChange;

	// Getters
	private bool ShowField
	{
		get
		{
			return gravityToggleState.Value || isPlanetProspectingMassChange;
		}
	}

	private void Awake()
	{
		planetComponent = GetComponentInParent<PlanetComponent>();
	}

	private void Start()
	{
		planetComponent.OnMassChanged.AddListener(MarkForGravityCircleUpdate);
		initialScale = gravityFieldRenderer.transform.localScale.x;
		MarkForGravityCircleUpdate();

		// Hide and show the gravity field based on the visualization settings.
		gravityToggleState.AddStateChangeListener(GravityToggleState_OnChange);
		// Hide and show based on whether this field's planet mass will be changed.
		planetComponent.OnStartProspectingMassChange.AddListener(Planet_OnStartProspectingMassChange);
		planetComponent.OnStopProspectingMassChange.AddListener(Planet_OnStopProspectingMassChange);

		// Show the gravity field based on current game state.
		SetShowField(ShowField);
	}

	private void MarkForGravityCircleUpdate()
	{
		markedForGravityCircleUpdate = true;
	}

	private void LateUpdate()
	{
		if (markedForGravityCircleUpdate)
			DrawGravityField();
		
		markedForGravityCircleUpdate = false;
	}

	private void DrawGravityField()
	{
		float outerRadius = planetComponent.GravityRadius;
		float innerRadius = planetComponent.Radius * (1 - marginIntoPlanet);

		outerRadius = Mathf.Max(outerRadius, planetComponent.Radius + minimumFieldDrawRadius);

		DrawTorus(innerRadius, outerRadius);
	}

	private void DrawTorus(float innerRadius, float outerRadius)
	{
		float outerRadiusObjectSpace = outerRadius * initialScale * 2;
		float innerRadiusUVSpace = innerRadius / outerRadius;

		Transform parent = gravityFieldRenderer.transform.parent;
		gravityFieldRenderer.transform.parent = null;
		gravityFieldRenderer.transform.localScale = Vector3.one * outerRadiusObjectSpace;
		gravityFieldRenderer.material.SetFloat(Shader.PropertyToID("_InnerRadius"), innerRadiusUVSpace);
		gravityFieldRenderer.transform.parent = parent;
	}

	private void UpdateShowField()
	{
		SetShowField(ShowField);
	}

	private void SetShowField(bool showField)
	{
		gravityFieldRenderer.enabled = showField;
	}

	private void GravityToggleState_OnChange(bool arg0)
	{
		UpdateShowField();
	}

	/// <summary>
	/// Whilst in the building build state, if the player hovers over a new planet
	/// (or stops hovering over a planet), show or hide this field when relevant.
	/// </summary>
	private void Planet_OnStartProspectingMassChange()
	{
		isPlanetProspectingMassChange = true;
		UpdateShowField();
	}

	private void Planet_OnStopProspectingMassChange()
	{
		isPlanetProspectingMassChange = false;
		UpdateShowField();
	}

	private void OnDestroy()
	{
		gravityToggleState.RemoveStateChangeListener(GravityToggleState_OnChange);
	}
}
