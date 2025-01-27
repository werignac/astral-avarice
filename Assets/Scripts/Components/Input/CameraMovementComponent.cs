using System;
using UnityEngine;

/// <summary>
/// Component that moves the camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraMovementComponent : MonoBehaviour
{
	[SerializeField] private GameController gameController;

	private Vector2 lastMousePosition;
	private Vector2 nextMousePosition;

	private bool isPanning;

	private Camera movingCamera;

	private Vector2 levelBounds = Vector2.one * 1;

	private void Awake()
	{
		movingCamera = GetComponent<Camera>();
		gameController?.OnLevelLoad.AddListener(GameController_OnLevelLoad);
	}

	private void GameController_OnLevelLoad()
	{
		levelBounds = gameController.LevelBounds;
	}

	// Called Every Frame.
	public void HoverInput(Vector2 mousePosition)
	{
		nextMousePosition = mousePosition;
	}

	// Called Every Frame.
	public void SetPanningInput(bool isPanning)
	{
		this.isPanning = isPanning;
	}

	private void LateUpdate()
	{
		if (isPanning)
		{
			PanUpdate();
		}

		lastMousePosition = nextMousePosition;
	}

	private void PanUpdate()
	{
		Vector2 lastMousePositionWorldSpace = movingCamera.ScreenToWorldPoint(lastMousePosition);
		Vector2 nextMousePositionWorldSpace = movingCamera.ScreenToWorldPoint(nextMousePosition);

		Vector2 worldSpaceDifference = lastMousePositionWorldSpace - nextMousePositionWorldSpace;

		transform.Translate(worldSpaceDifference);

		ApplyLevelBounds();
	}

	private void ApplyLevelBounds()
	{
		Vector3 boundedPosition = transform.position;

		if (levelBounds.x >= 0)
			boundedPosition.x = Mathf.Clamp(transform.position.x, -levelBounds.x / 2, levelBounds.x / 2);

		if (levelBounds.y >= 0)
			boundedPosition.y = Mathf.Clamp(transform.position.y, -levelBounds.y / 2, levelBounds.y / 2);


		transform.position = boundedPosition;
	}
}
