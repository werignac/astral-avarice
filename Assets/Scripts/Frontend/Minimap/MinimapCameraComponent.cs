using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component that controls the camera that renders the minimap.
/// </summary>
public class MinimapCameraComponent: MonoBehaviour
{
	private Camera minimapCamera;
	[SerializeField] private GameController gameController;
	[SerializeField] private float margin;

	[HideInInspector] public UnityEvent OnResizeForLevelBounds = new UnityEvent();

	private float renderTextureAspect;

	private void Awake()
	{
		minimapCamera = GetComponent<Camera>();

		// TODO: Change render texture resolution?

		renderTextureAspect = minimapCamera.targetTexture.width / minimapCamera.targetTexture.height;
		gameController.OnLevelLoad.AddListener(GameController_OnLevelLoad);
	}

	// Fits the minimap camera to the level bounds.
	private void GameController_OnLevelLoad()
	{
		Vector2 levelBounds = gameController.LevelBounds;

		float minHeightFromLevelHeight = levelBounds.y / 2;
		float minHeightFromLevelWidth = levelBounds.x / 2 / renderTextureAspect;

		float minHeight = Mathf.Max(minHeightFromLevelHeight, minHeightFromLevelWidth) + margin;

		minimapCamera.orthographicSize = minHeight;

		OnResizeForLevelBounds.Invoke();
	}

	/// <summary>
	/// Takes a normalized Vector2 in the space of the minimap and converts it
	/// into a position in world space.
	/// </summary>
	/// <param name="minimapPosition">Normalized Vector where 0, 0 is the bottom left of the camera.</param>
	/// <returns>A Vector2 in world space from projecting the minimap position through the camera.</returns>
	public Vector2 MinimapSpaceToWorldSpace(Vector2 minimapPosition)
	{
		return minimapCamera.ViewportToWorldPoint(minimapPosition);
	}

	/// <summary>
	/// Takes a Vector2 in the world space and converts it into a position
	/// in the minimap render texture.
	/// </summary>
	/// <param name="worldPosition">The position in world space to convert.</param>
	/// <returns>A position on the  minimap. Normalized Vector if the world point is in the camera's view.</returns>
	public Vector2 WorldSpaceToMinimapSpace(Vector2 worldPosition)
	{
		return minimapCamera.WorldToViewportPoint(worldPosition);
	}
}
