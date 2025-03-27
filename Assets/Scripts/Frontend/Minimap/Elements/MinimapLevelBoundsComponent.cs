using System;
using UnityEngine;

/// <summary>
/// Component that manipulates a LineRenderer to show the bounds of a level
/// in the minimap.
/// </summary>
public class MinimapLevelBoundsComponent : MinimapWireframeBox
{
	[SerializeField] private GameController gameController;

	private void Start()
	{
		gameController.OnLevelLoad.AddListener(GameController_OnLevelLoad);
	}

	private void GameController_OnLevelLoad()
	{
		Vector2 levelDimensions = gameController.LevelBounds;

		Bounds boxBounds = new Bounds(Vector3.zero, levelDimensions);

		SetBox(boxBounds);
	}
}
