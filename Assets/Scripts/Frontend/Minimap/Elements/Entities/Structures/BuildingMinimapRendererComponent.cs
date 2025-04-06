using UnityEngine;

public class BuildingMinimapRendererComponent : GridGroupMinimapRendererComponent
{
	private BuildingComponent buildingComponent;
	[SerializeField] private SpriteRenderer rendererToMatch;

	private void Awake()
	{
		buildingComponent = GetComponentInParent<BuildingComponent>();
		buildingComponent.OnGridGroupChanged.AddListener(SetGridGroupColor);
	}

	private void Start()
	{
		MatchScale(rendererToMatch);
	}

	private void MatchScale(SpriteRenderer toMatch)
	{
		minimapRenderer.transform.localPosition = toMatch.transform.localPosition;
		minimapRenderer.transform.localScale = toMatch.localBounds.extents * 2 * toMatch.transform.localScale.x;
	}
}
