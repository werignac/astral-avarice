using UnityEngine;
using UnityEngine.UIElements;

// TODO: Hide when offscreen.
public class WorldToScreenUIComponent : MonoBehaviour
{
	[SerializeField] protected UIDocument uiDocument;
	/// <summary>
	/// UI document to move to match its position in world space.
	/// Assumed to be in the upper right corner of the ui document normally without adjustment.
	/// Assumes the UI has one child underneath the root.
	/// </summary>
	protected VisualElement ui = null;

	/// <summary>
	/// What point on the UI should match the point in world space.
	/// 0,0 for the upper left corner. 1,1 for the bottom right corner.
	/// Values can go above or below 0 and 1.
	/// </summary>
	[SerializeField] private Vector2 pivot;

	/// <summary>
	/// An offset applied to the position of the UI. Unlike
	/// pivot, it's independednt of the size of the element and is
	/// measured in pixels.
	/// </summary>
	[SerializeField] private Vector2 offset;

	protected virtual void Start()
	{
		if (uiDocument == null)
			return;

		// Fetch the child of the uiDocument. There should only be one.
		if (uiDocument.rootVisualElement.childCount < 1)
			throw new System.Exception($"UIDocument {uiDocument} is missing a child for being placed in the world space.");

		if (uiDocument.rootVisualElement.childCount > 1)
			throw new System.Exception($"UIDocument {uiDocument} has too many children for being placed in the worls space.");

		foreach (VisualElement child in uiDocument.rootVisualElement.Children())
		{
			ui = child;
		}
	}

	private void LateUpdate()
	{
		if (ui == null)
			return;

		if (!uiDocument.isActiveAndEnabled)
			return;

		UpdateUIPosition();
	}

	protected void UpdateUIPosition()
	{
		Camera mainCamera = Camera.main;

		if (mainCamera == null)
			return;

		Vector3 worldPosition = uiDocument.transform.position;


		Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);

		Vector2 uiDocumentPoint = ViewportToUIDocumentPoint(viewportPosition, mainCamera);

		uiDocumentPoint = ApplyUITransformations(uiDocumentPoint);

		ui.style.top = uiDocumentPoint.y;
		ui.style.left = uiDocumentPoint.x;
	}

	private Vector2 ViewportToUIDocumentPoint(Vector2 viewportPosition, Camera camera)
	{
		Vector2 upperLeftOrigin = new Vector2(
				viewportPosition.x,
				1 - viewportPosition.y
			);

		Vector2 uiDocumentDimensions = new Vector2(
			uiDocument.rootVisualElement.resolvedStyle.width,
			uiDocument.rootVisualElement.resolvedStyle.height
			);

		return upperLeftOrigin * uiDocumentDimensions;
	}

	private Vector2 ApplyUITransformations(Vector2 uiDocumentPoint)
	{
		Vector2 uiDimensions = GetUIDimensions();
		uiDocumentPoint += offset - pivot * uiDimensions;
		return uiDocumentPoint;
	}

	private Vector2 GetUIDimensions()
	{
		if (ui.style.display == DisplayStyle.Flex)
			return new Vector2(ui.resolvedStyle.width, ui.resolvedStyle.height);
		else
			return new Vector2(ui.style.width.value.value, ui.style.height.value.value);
	}

}
