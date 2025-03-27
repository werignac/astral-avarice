using System;
using UnityEngine;

/// <summary>
/// Component that moves the camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraMovementComponent : MonoBehaviour
{
	public enum PanSpeedModifier
	{
		NONE, MULTIPLY, DIVIDE
	};

	[SerializeField] private GameController gameController;

	private Vector2 lastMousePosition;
	private Vector2 nextMousePosition;

	private bool isPanning;

	private Camera movingCamera;

	private Vector2 levelBounds = Vector2.one;

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

	// Getters
	// How far the camera can move left or right.
	private float HorizontalBoundMinusCameraWidth { get => Mathf.Max(levelBounds.x / 2 - movingCamera.orthographicSize * movingCamera.aspect, 0); }
	// How far the camera can move left or right when fully zoomed out.
	private float HorizontalBoundMinusCameraWidthMax { get => Mathf.Max(levelBounds.x / 2 - cameraSizeMax * movingCamera.aspect, 0); }
	private float HorizontalBoundMinusCameraWidthMin { get => Mathf.Max(levelBounds.x / 2 - cameraSizeMin * movingCamera.aspect, 0); }

	// How far the camera can move up or down.
	private float VerticalBoundMinusCameraHeight { get => Mathf.Max(levelBounds.y / 2 - movingCamera.orthographicSize, 0); }
	private float VerticalBoundMinusCameraHeightMax { get => Mathf.Max(levelBounds.x / 2 - cameraSizeMax , 0); }
	private float VerticalBoundMinusCameraHeightMin { get => Mathf.Max(levelBounds.x / 2 - cameraSizeMin , 0); }
	public float LevelBoundsAspect { get => levelBounds.x / levelBounds.y; }
	// Accessed by BackgroundComponent.
	public Vector2 LevelBounds => levelBounds;
	// Because of smoothing, these can be above or below 1 or -1.
	/// <summary>
	/// Position of the camera relative to the bounds of the level.
	/// Each axis is between -1 and 1.
	/// </summary>
	public Vector2 NormalizedPositionWithinLevelBounds {
		get => new Vector2(
			transform.position.x / (levelBounds.x / 2),
			transform.position.y / (levelBounds.y / 2)
			);
	}

	public Vector2 NormalizedPositionWithinMoveableArea
	{
		get => new Vector2(
			HorizontalBoundMinusCameraWidth == 0 ? 0 : transform.position.x / HorizontalBoundMinusCameraWidth,
			VerticalBoundMinusCameraHeight == 0 ? 0 : transform.position.y / VerticalBoundMinusCameraHeight
			);
	}

	// Normalized position relative to being fully zoomed out.
	public Vector2 NormalizedPositionMax
	{
		get => new Vector2(
			HorizontalBoundMinusCameraWidth == 0 ? 0 : transform.position.x / HorizontalBoundMinusCameraWidthMax,
			VerticalBoundMinusCameraHeight == 0 ? 0 : transform.position.y / VerticalBoundMinusCameraHeightMax
			);
	}

	// Normalized position relative to being fully zoomed in.
	public Vector2 NormalizedPositionMin
	{
		get => new Vector2(
			HorizontalBoundMinusCameraWidth == 0 ? 0 : transform.position.x / HorizontalBoundMinusCameraWidthMin,
			VerticalBoundMinusCameraHeight == 0 ? 0 : transform.position.y / VerticalBoundMinusCameraHeightMin
			);
	}

	public float CameraSizeMin { get => cameraSizeMin; }
	public float CameraSizeMax { get => cameraSizeMax; }

	/// <summary>
	/// How fast the camera moves when using arrow keys or wasd or a joystick controller.
	/// </summary>
	[SerializeField, Range(0.001f, 20)] private float directionalPanSpeed = 10f;
	/// <summary>
	/// Multiplier for when left shift is held.
	/// </summary>
	[SerializeField, Range(1f, 10f)] private float directionalPanSpeedShiftCoefficient = 2f;
	/// <summary>
	/// Multiplier for when left control is held.
	/// </summary>
	[SerializeField, Range(0.001f, 1f)] private float directionPanSpeedControlCoefficient = 0.5f;


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
		if (isPanning && !gameController.GamePaused)
		{
			PanUpdate();
		}

		ZoomUpdate();

		ApplyLevelBoundsToGoalPosition();

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

	private void ApplyLevelBoundsToGoalPosition()
	{
		goalPosition = ApplyLevelBounds(goalPosition);
	}

	/// <summary>
	/// Forces a position to remain within the level's bounds, including considering the
	/// width and height of the camera.
	/// </summary>
	/// <param name="position">The position to bound.</param>
	/// <returns>The bounded position.</returns>
	private Vector2 ApplyLevelBounds(Vector2 position)
	{
		if (levelBounds.x >= 0)
			position.x = Mathf.Clamp(position.x, -HorizontalBoundMinusCameraWidth, HorizontalBoundMinusCameraWidth);


		if (levelBounds.y >= 0)
			position.y = Mathf.Clamp(position.y, -VerticalBoundMinusCameraHeight, VerticalBoundMinusCameraHeight);

		return position;
	}

	public void SetDirectionalPanInput(Vector2 panDirection, PanSpeedModifier modifier)
	{
		float multiplier = 1;

		switch (modifier)
		{
			case PanSpeedModifier.MULTIPLY:
				multiplier = directionalPanSpeedShiftCoefficient;
				break;
			case PanSpeedModifier.DIVIDE:
				multiplier = directionPanSpeedControlCoefficient;
				break;
		}

		goalPosition += panDirection * directionalPanSpeed * multiplier * Time.unscaledDeltaTime;
	}

	private void ApplySmoothing()
	{
		Vector2 position2D = Vector2.Lerp(transform.position, goalPosition, movementSmoothing * Time.unscaledDeltaTime);
		Vector3 position3D = new Vector3(position2D.x, position2D.y, transform.position.z);

		transform.position = position3D;
		movingCamera.orthographicSize = Mathf.Lerp(movingCamera.orthographicSize, goalCameraSize, zoomSmoothing * Time.unscaledDeltaTime);
	}


	/// <summary>
	/// Moves the camera to the given position.
	/// </summary>
	/// <param name="position">The position in world space to move the camera to.</param>
	/// <param name="useSmoothing">If true, the camera glides to the selected position. If false, the camera teleports immediately.</param>
	/// <param name="restrictToLevelBounds">If true, prior to moving the camera, restrict "position" to be in the level's bounds (including accounting for the width and height of the camera). If false, don't restrict "position" (though the camera may move in bounds afterwards).</param>
	public void MoveTo(Vector2 position, bool useSmoothing = true, bool restrictToLevelBounds = true)
	{
		// Restrict the position if required.
		if (restrictToLevelBounds)
			position = ApplyLevelBounds(position);

		// Set the goal position of the camera to the position we want to move to.
		goalPosition = position;

		// Teleport immediately if requested.
		if (!useSmoothing)
			transform.position = new Vector3(
				position.x,
				position.y,
				transform.position.z
			);
	}
}
