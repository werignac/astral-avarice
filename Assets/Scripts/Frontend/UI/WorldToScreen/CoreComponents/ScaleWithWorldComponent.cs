using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class ScaleWithWorldComponent : WorldToScreenComponent
	{
		/// <summary>
		/// How (wide, tall) the ui element is in world space.
		/// </summary>
		public Vector2 WorldSize { get; set; } = Vector2.one;

		public bool Enabled { get; set; } = true;

		public override void Update()
		{
			if (!Enabled)
				return;

			Camera mainCamera = Camera.main;
			float cameraHeight = mainCamera.orthographicSize * 2;
			// Size of ui element relative to screen.
			Vector2 screenRelativeSize = WorldSize / cameraHeight;

			Vector2 pixelSize = new Vector2(
				screenRelativeSize.x * Screen.height,
				screenRelativeSize.y * Screen.height
			);

			UIElement.style.width = pixelSize.x;
			UIElement.style.height = pixelSize.y;
		}
	}
}
