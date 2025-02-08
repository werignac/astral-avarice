using UnityEngine;

/// <summary>
/// Manipulates a LineRenderer component to draw the current state of the camera
/// in the minimap.
/// </summary>
public class MinimapCameraBoundsComponent : MinimapWireframeBox
{
	private Camera mainCamera;

	protected override void Awake()
	{
		base.Awake();
		mainCamera = GetComponentInParent<Camera>();
	}

	private void LateUpdate()
	{
		Vector2 cameraDimensions = new Vector2(mainCamera.orthographicSize * 2 * mainCamera.aspect, mainCamera.orthographicSize * 2);
		// Assumes this line renderer uses local space and is on camera object.
		Bounds cameraBounds = new Bounds(Vector3.zero, cameraDimensions);
		SetBox(cameraBounds);
	}
}
