using System;
using UnityEngine;

/// <summary>
/// Component that manipulates a LineRenderer to show the bounds of a level
/// in the minimap.
/// </summary>
public class MinimapLevelBoundsComponent : MinimapWireframeBox
{
	private GameController gameController;

	protected override void Awake()
	{
		base.Awake();

		gameController = GetComponentInParent<GameController>();
		gameController.OnLevelLoad.AddListener(GameController_OnLevelLoad);
	}

	private void GameController_OnLevelLoad()
	{
		Vector2 levelDimensions = gameController.LevelBounds;

		Bounds boxBounds = new Bounds(Vector3.zero, levelDimensions);

		SetBox(boxBounds);
	}
}
