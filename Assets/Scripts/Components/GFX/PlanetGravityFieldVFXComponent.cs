using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetGravityFieldVFXComponent : MonoBehaviour
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

	private void Awake()
	{
		planetComponent = GetComponentInParent<PlanetComponent>();
	}

	private void Start()
	{
		planetComponent.OnMassChanged.AddListener(MarkForGravityCircleUpdate);
		initialScale = gravityFieldRenderer.transform.localScale.x;
		MarkForGravityCircleUpdate();
	}

	private void MarkForGravityCircleUpdate()
	{
		markedForGravityCircleUpdate = true;
	}

	private void LateUpdate()
	{
#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
			return;
#endif

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
		
		gravityFieldRenderer.transform.localScale = Vector3.one * outerRadiusObjectSpace;
		gravityFieldRenderer.material.SetFloat(Shader.PropertyToID("_InnerRadius"), innerRadiusUVSpace);
	}
}
