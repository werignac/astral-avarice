using System;
using UnityEngine;

/// <summary>
/// Component that controls the camera that renders the minimap.
/// </summary>
public class MinimapCameraComponent: MonoBehaviour
{
	private Camera minimapCamera;
	[SerializeField] private GameController gameController;
	[SerializeField] private float margin;

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
}
