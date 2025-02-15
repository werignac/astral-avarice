using UnityEngine;

/// <summary>
/// Component that controlls the camera that renders the outline mask.
/// Expects to be put on the GameObject with the Camera component for rendering the outline mask
/// and which is also a child of the main camera.
/// </summary>
public class OutlineCameraComponent : CopyMainCameraSizeComponent
{
	[SerializeField] private RenderTexture outlineRenderTexture;

	protected override void LateUpdate()
	{
		if (mainCamera.pixelWidth != outlineRenderTexture.width || mainCamera.pixelHeight != outlineRenderTexture.height)
		{
			outlineRenderTexture.Release();

			outlineRenderTexture.width = mainCamera.pixelWidth;
			outlineRenderTexture.height = mainCamera.pixelHeight;

			outlineRenderTexture.Create();

			outlineRenderTexture.ApplyDynamicScale();

			selfCamera.aspect = ((float)outlineRenderTexture.width / outlineRenderTexture.height);
		}
		
		selfCamera.orthographicSize = mainCamera.orthographicSize;
	}
}
