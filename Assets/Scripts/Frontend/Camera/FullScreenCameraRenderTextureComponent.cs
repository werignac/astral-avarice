using UnityEngine;

/// <summary>
/// Component that controlls the camera that renders the outline mask.
/// Expects to be put on the GameObject with the Camera component for rendering the outline mask
/// and which is also a child of the main camera.
/// </summary>
public class FullScreenCameraRenderTextureComponent : CopyMainCameraSizeComponent
{
	[SerializeField] private RenderTexture renderTexture;

	protected override void LateUpdate()
	{
		if (mainCamera.pixelWidth != renderTexture.width || mainCamera.pixelHeight != renderTexture.height)
		{
			renderTexture.Release();

			renderTexture.width = mainCamera.pixelWidth;
			renderTexture.height = mainCamera.pixelHeight;

			renderTexture.Create();

			renderTexture.ApplyDynamicScale();

			selfCamera.aspect = ((float)renderTexture.width / renderTexture.height);
		}
		
		selfCamera.orthographicSize = mainCamera.orthographicSize;
	}
}
