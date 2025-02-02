using UnityEngine;

public class BackgroundComponent : MonoBehaviour
{
	private new Camera camera;
	private CameraMovementComponent cameraMovementComponent;
	private SpriteRenderer backgroundRenderer;

	private Sprite backgroundSprite;
	[SerializeField] private bool enableCounterCameraMovement = false;
	[SerializeField] private bool invertMovementDirection = false;
	// When zooming the camera in, don't move the background.
	[SerializeField] private bool straightScale = false;

	[SerializeField, Range(0, 0.5f)]private float margin;
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
			FitToCamera(margin);
		else
			CounterMoveCamera();
	}

	private void FitToCamera(float m)
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

		float widthRatio = width / (UnscaledWidth * (1 - 2 * m));
		float heightRatio = height / (UnscaledHeight * (1 - 2 * m));

		transform.localScale = Vector3.one * Mathf.Max(widthRatio, heightRatio);
	}

	// Move BG in opposite direction that player is moving the camera.
	private void CounterMoveCamera()
	{
		FitToCamera(margin + moveMargin);

		Vector2 normalizedCameraPosition;

		if (straightScale)
			normalizedCameraPosition = cameraMovementComponent.NormalizedPositionMax;
		else
			normalizedCameraPosition = cameraMovementComponent.NormalizedPosition;

		if (invertMovementDirection)
			normalizedCameraPosition = -normalizedCameraPosition;

		// Normalized position, but scaled proportional to the level bounds.
		Vector2 relativeNormalizedCameraPosition;
		if (cameraMovementComponent.LevelBoundsAspect > 1)
			relativeNormalizedCameraPosition = new Vector2(normalizedCameraPosition.x / cameraMovementComponent.LevelBoundsAspect, normalizedCameraPosition.y);
		else
			relativeNormalizedCameraPosition = new Vector2(normalizedCameraPosition.x, normalizedCameraPosition.y * cameraMovementComponent.LevelBoundsAspect);

		float maxHorizontalMovement = ScaledWidth * moveMargin;
		float maxVerticalMovement = ScaledHeight * moveMargin;

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
