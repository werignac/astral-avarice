using System;
using UnityEngine;

public class CableMinimapRendererComponent : GridGroupMinimapRendererComponent
{
	private const float CABLE_WIDTH_MULTIPLIER = 5f;

	private CableComponent cableComponent;
	[SerializeField] private LineRenderer rendererToMatch;

	private void Awake()
	{
		cableComponent = GetComponent<CableComponent>();
		cableComponent.OnCableMoved.AddListener(Cable_OnMove);
		cableComponent.OnGridGroupChanged.AddListener(SetGridGroupColor);
	}

	private void Cable_OnMove()
	{
		MatchScale(rendererToMatch);
	}

	private void MatchScale(LineRenderer toMatch)
	{
		CableComponent.GetBoxFromPoints(
			toMatch.GetPosition(0),
			toMatch.GetPosition(1),
			out Vector2 center,
			out Vector2 size,
			out float angle
		);

		// Increase the width of the cable to see it in the minimap (too thin and it won't be picked up).
		size.y *= CABLE_WIDTH_MULTIPLIER;

		minimapRenderer.transform.position = center;
		minimapRenderer.transform.localScale = size;
		minimapRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
	}
}
