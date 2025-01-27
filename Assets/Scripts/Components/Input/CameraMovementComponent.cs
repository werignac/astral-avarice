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

	[Header("Zoom")]
	[SerializeField, Min(0.01f)] private float cameraSizeMin = 3;
	[SerializeField, Min(0.01f)] private float cameraSizeMax = 7;
	[SerializeField] private float zoomRate = 1f;
	private float zoomThisUpdate;

	private Vector2 goalPosition;
	private float goalCameraSize;

	[Header("Smoothing")]
	[SerializeField, Min(1)] private float movementSmoothing = 10f;
	[SerializeField, Min(1)] private float zoomSmoothing = 10f;

	private void Awake()
	{
		movingCamera = GetComponent<Camera>();
		gameController?.OnLevelLoad.AddListener(GameController_OnLevelLoad);
	}

	private void Start()
	{
		goalPosition = transform.position;
		goalCameraSize = movingCamera.orthographicSize;
	}

	private void GameController_OnLevelLoad()
	{
		levelBounds = gameController.LevelBounds;
	}

	// Called Every Frame.
	public void SetHoverInput(Vector2 mousePosition)
	{
		nextMousePosition = mousePosition;
	}

	// Called Every Frame.
	public void SetPanningInput(bool isPanning)
	{
		this.isPanning = isPanning;
	}

	public void SetZoomInput(float zoomThisUpdate)
	{
		this.zoomThisUpdate = zoomThisUpdate;
	}

	private void LateUpdate()
	{
		if (isPanning)
		{
			PanUpdate();
		}

		ZoomUpdate();

		ApplyLevelBounds();

		ApplySmoothing();

		lastMousePosition = nextMousePosition;
	}

	private void PanUpdate()
	{
		Vector2 lastMousePositionWorldSpace = movingCamera.ScreenToWorldPoint(lastMousePosition);
		Vector2 nextMousePositionWorldSpace = movingCamera.ScreenToWorldPoint(nextMousePosition);

		Vector2 worldSpaceDifference = lastMousePositionWorldSpace - nextMousePositionWorldSpace;

		goalPosition += worldSpaceDifference;
	}

	private void ZoomUpdate()
	{
		float deltaZoom = zoomRate * zoomThisUpdate;

		float nextZoom = Mathf.Clamp(goalCameraSize - deltaZoom, cameraSizeMin, cameraSizeMax);

		goalCameraSize = nextZoom;
	}

	private void ApplyLevelBounds()
	{
		Vector2 boundedPosition = goalPosition;

		if (levelBounds.x >= 0)
		{
			float horizontalBound = levelBounds.x / 2;
			float horizontalBoundMinusCameraWidth = horizontalBound - movingCamera.orthographicSize * movingCamera.aspect;
			horizontalBoundMinusCameraWidth = Mathf.Max(horizontalBoundMinusCameraWidth, 0);
			boundedPosition.x = Mathf.Clamp(goalPosition.x, -horizontalBoundMinusCameraWidth, horizontalBoundMinusCameraWidth);
		}

		if (levelBounds.y >= 0)
		{
			float verticalBound = levelBounds.y / 2;
			float verticalboundMinusCameraHeight = verticalBound - movingCamera.orthographicSize;
			verticalboundMinusCameraHeight = Mathf.Max(verticalboundMinusCameraHeight, 0);
			boundedPosition.y = Mathf.Clamp(goalPosition.y, -verticalboundMinusCameraHeight, verticalboundMinusCameraHeight);
		}

		goalPosition = boundedPosition;
	}

	private void ApplySmoothing()
	{
		Vector2 position2D = Vector2.Lerp(transform.position, goalPosition, movementSmoothing * Time.deltaTime);
		Vector3 position3D = new Vector3(position2D.x, position2D.y, transform.position.z);

		transform.position = position3D;
		movingCamera.orthographicSize = Mathf.Lerp(movingCamera.orthographicSize, goalCameraSize, zoomSmoothing * Time.deltaTime);
	}
}
