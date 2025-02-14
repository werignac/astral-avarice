using UnityEngine;

/// <summary>
/// Component that controlls the camera that renders the outline mask.
/// Expects to be put on the GameObject with the Camera component for rendering the outline mask
/// and which is also a child of the main camera.
/// </summary>
public class OutlineCameraComponent : MonoBehaviour
{
	private Camera mainCamera;
	private Camera outlineCamera;
	[SerializeField] private RenderTexture outlineRenderTexture;


	private void Awake()
	{
		outlineCamera = GetComponent<Camera>();
		mainCamera = Camera.main;
	}

	private void LateUpdate()
	{
		if (mainCamera.pixelWidth != outlineRenderTexture.width || mainCamera.pixelHeight != outlineRenderTexture.height)
		{
			outlineRenderTexture.Release();

			outlineRenderTexture.width = mainCamera.pixelWidth;
			outlineRenderTexture.height = mainCamera.pixelHeight;

			outlineRenderTexture.Create();

			outlineRenderTexture.ApplyDynamicScale();

			outlineCamera.aspect = ((float)outlineRenderTexture.width / outlineRenderTexture.height);
		}
		
		outlineCamera.orthographicSize = mainCamera.orthographicSize;
	}
}
