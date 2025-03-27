using UnityEngine;

public class BackgroundComponent : MonoBehaviour
{
	private new Camera camera;
	private CameraMovementComponent cameraMovementComponent;
	private SpriteRenderer backgroundRenderer;

	private Sprite backgroundSprite;
	/// <summary>
	/// Whether the background is moving to give a parallax effect.
	/// When false, the background fits the screen.
	/// </summary>
	[SerializeField] private bool enableCounterCameraMovement = false;
	/// <summary>
	/// Inverts the direction that the background moves in.
	/// </summary>
	[SerializeField] private bool invertMovementDirection = false;
	/// <summary>
	/// When zooming the camera in, don't translate the background relative to the camera.
	/// </summary>
	[SerializeField] private bool straightScale = false;

	/// <summary>
	/// A margin between the edge of the camera and the edge of the background.
	/// Relative to the size of the background (between 0 and 0.5).
	/// </summary>
	[SerializeField, Range(0, 0.5f)]private float staticMargin;
	/// <summary>
	/// A margin between the edge of the camera and the edge of the background.
	/// Relative to the size of the background (between 0 and 0.5).
	/// This is the space in which the background moves. Everything smaller than this margin
	/// will always be on screen.
	/// </summary>
	[SerializeField, Range(0, 0.5f)] private float moveMargin;

	private float UnscaledWidth { get => backgroundSprite.texture.width / backgroundSprite.pixelsPerUnit ; }
	private float ScaledWidth { get => UnscaledWidth * transform.localScale.x; }
	private float UnscaledHeight { get => backgroundSprite.texture.height / backgroundSprite.pixelsPerUnit; }
	private float ScaledHeight { get => UnscaledHeight * transform.localScale.y; }
	private float CameraWidth { get => camera.orthographicSize * camera.aspect * 2; }
	private float CameraWidthMax { get => cameraMovementComponent.CameraSizeMax * camera.aspect * 2; }
	private float CameraWidthMin { get => cameraMovementComponent.CameraSizeMin * camera.aspect * 2; }
	private float CameraHeight { get => camera.orthographicSize * 2; }
	private float CameraHeightMax { get => cameraMovementComponent.CameraSizeMax * 2; }
	private float CameraHeightMin { get => cameraMovementComponent.CameraSizeMin * 2; }

	private void Awake()
	{
		camera = GetComponentInParent<Camera>();
		cameraMovementComponent = GetComponentInParent<CameraMovementComponent>();
		backgroundRenderer = GetComponent<SpriteRenderer>();
		backgroundSprite = backgroundRenderer.sprite;
	}

	private void LateUpdate()
	{
		if (!enableCounterCameraMovement || cameraMovementComponent == null)
			FitToCamera();
		else
			CounterMoveCamera();
	}

	private void FitToCamera()
	{
		float width;
		float height;

		if (!enableCounterCameraMovement || cameraMovementComponent == null || !straightScale)
		{
			width = CameraWidth;
			height = CameraHeight;
		}
		else
		{
			width = CameraWidthMax;
			height = CameraHeightMax;
		}
		/*
		// Scaling beyond level bounds causes background placement issues.
		if (cameraMovementComponent != null)
		{
			width = Mathf.Min(width, cameraMovementComponent.LevelBounds.x);
			height = Mathf.Min(height, cameraMovementComponent.LevelBounds.y);
		}*/

		if (enableCounterCameraMovement)
		{
			width /= 1 - 2 * moveMargin;
			height /= 1 - 2 * moveMargin;

			if (cameraMovementComponent != null)
			{
				width = Mathf.Min(width, Mathf.Max(cameraMovementComponent.LevelBounds.x, CameraWidth));
				height = Mathf.Min(height, Mathf.Max(cameraMovementComponent.LevelBounds.y, CameraHeight));
			}
		}

		float widthRatio = width / (UnscaledWidth * (1 - 2 * staticMargin));
		float heightRatio = height / (UnscaledHeight * (1 - 2 * staticMargin));

		transform.localScale = Vector3.one * Mathf.Min(widthRatio, heightRatio);
	}

	// Move BG in opposite direction that player is moving the camera.
	private void CounterMoveCamera()
	{
		FitToCamera();

		Vector2 normalizedCameraPosition;

		if (straightScale)
			normalizedCameraPosition = cameraMovementComponent.NormalizedPositionMax;
		else
			normalizedCameraPosition = cameraMovementComponent.NormalizedPositionWithinMoveableArea;

		if (invertMovementDirection)
			normalizedCameraPosition = -normalizedCameraPosition;

		// Normalized position, but scaled proportional to the level bounds.
		Vector2 relativeNormalizedCameraPosition;
		if (cameraMovementComponent.LevelBoundsAspect > 1)
			relativeNormalizedCameraPosition = new Vector2(normalizedCameraPosition.x / cameraMovementComponent.LevelBoundsAspect, normalizedCameraPosition.y);
		else
			relativeNormalizedCameraPosition = new Vector2(normalizedCameraPosition.x, normalizedCameraPosition.y * cameraMovementComponent.LevelBoundsAspect);

		// Not using moveMargin because sometimes there's less margin than moveMargin.
		float maxHorizontalMovement = Mathf.Max(((ScaledWidth * (1 - 2 * staticMargin)) - CameraWidth) / 2, 0);
		float maxVerticalMovement = Mathf.Max(((ScaledHeight * (1 - 2 * staticMargin)) - CameraHeight) / 2, 0);

		Vector2 posXY = relativeNormalizedCameraPosition * new Vector2(maxHorizontalMovement, maxVerticalMovement);

		// Because the normalized camera position can occationally be > 1, we need to make sure the BG
		// never goes off screen.
		float safetyBoundHorizontal = (ScaledWidth - CameraWidth) / 2;
		float safetyBoundVertical = (ScaledHeight - CameraHeight) / 2;
		posXY = new Vector2(
			Mathf.Clamp(posXY.x, -safetyBoundHorizontal, safetyBoundHorizontal),
			Mathf.Clamp(posXY.y, -safetyBoundVertical, safetyBoundVertical)
			);

		Vector3 posXYZ = new Vector3(posXY.x, posXY.y, transform.localPosition.z);

		transform.localPosition = posXYZ;
	}
}
