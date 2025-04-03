using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Component that maintains a list of WorldToScreenUIElements.
	/// </summary>
    public class WorldToScreenUIManagerComponent : MonoBehaviour
    {
		[SerializeField] private UIDocument uiDocument;

		private List<IWorldToScreenUIElement> elements = new List<IWorldToScreenUIElement>();

		/// <summary>
		/// Adds an element to the world space UI document and creates an
		/// instance of the uiTemplate for the element.
		/// </summary>
		/// <param name="element">The element to add.</param>
		/// <param name="uiTemplate">The ui for this element.</param>
		public void Add(IWorldToScreenUIElement element, VisualTreeAsset uiTemplate)
		{
			VisualElement uiInstance = uiTemplate.Instantiate();
			element.UIElement = uiInstance;

			Add(element);
		}

		/// <summary>
		/// Adds an element to the world space UI document.
		/// Expects element.UIElement to already be filled out.
		/// </summary>
		/// <param name="element">The element to add.</param>
		public void Add(IWorldToScreenUIElement element)
		{
			element.UIElement.style.position = Position.Absolute;
			uiDocument.rootVisualElement.Add(element.UIElement);

			elements.Add(element);
		}

		/// <summary>
		/// Removes an element from world space UI document.
		/// </summary>
		/// <param name="element">The element to remove.</param>
		/// <returns>Whether the element was successfully removed.</returns>
		public bool Remove(IWorldToScreenUIElement element)
		{
			bool removed = elements.Remove(element);
			
			if (removed)
				uiDocument.rootVisualElement.Remove(element.UIElement);
			
			return removed;
		}

		/// <summary>
		/// Update the positions of all the ui elements.
		/// </summary>
		private void LateUpdate()
		{
			Camera mainCamera = Camera.main;

			if (mainCamera == null)
				return;

			// TODO: Implement an update tracking system to avoid calling UpdateComponents multiple times.

			// Iterate backwards in a for loop in case elements move themselves off this 
			// manager. Avoids modifying a collection while looping over it. Assumes only
			// one element could remove itself at a time, and if so that it's the updated
			// element.
			for (int i = elements.Count - 1; i >= 0; i--)
			{
				IWorldToScreenUIElement element = elements[i];
				UpdateComponents(element);
			}

			for (int i = elements.Count - 1; i >= 0; i--)
			{
				IWorldToScreenUIElement element = elements[i];
				PlaceUIElement(element, mainCamera);
			}
		}

		/// <summary>
		/// Runs the Update() function on all the components for an element.
		/// </summary>
		/// <param name="element">The element with the components that must be updated.</param>
		private void UpdateComponents(IWorldToScreenUIElement element)
		{
			foreach (WorldToScreenComponent component in element.Components)
			{
				component.Update();
			}
		}

		/// <summary>
		/// Places an element in the ui space in accordance with it's position
		/// in world space.
		/// </summary>
		/// <param name="element">The element to place.</param>
		/// <param name="mainCamera">Teh camera to use for positioning (the main camera).</param>
		private void PlaceUIElement(IWorldToScreenUIElement element, Camera mainCamera)
		{
			Vector3 worldPosition = element.WorldPosition;

			Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);

			Vector2 uiDocumentPoint = ViewportToUIDocumentPoint(viewportPosition, uiDocument);

			uiDocumentPoint = ApplyUITransformations(element, uiDocumentPoint);

			element.UIElement.style.top = uiDocumentPoint.y;
			element.UIElement.style.left = uiDocumentPoint.x;
		}

		/// <summary>
		/// Turns a point in the viewport into a point in a uiDocument.
		/// Assumes the root element of the uiDocument fills the screen.
		/// </summary>
		/// <param name="viewportPosition">The point to transform.</param>
		/// <param name="uiDocument">The document to transform the point into.</param>
		/// <returns>A point in uiDocument space.</returns>
		private static Vector2 ViewportToUIDocumentPoint(Vector2 viewportPosition, UIDocument uiDocument)
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

		/// <summary>
		/// Get the width and height of a WorldToScreenUIElement in the space
		/// of the uiDocument.
		/// </summary>
		/// <param name="element">The ui element to get the width and height of.</param>
		/// <returns>(width, height)</returns>
		private static Vector2 GetUIDimensions(IWorldToScreenUIElement element)
		{
			if (element.UIElement.resolvedStyle.display == DisplayStyle.Flex)
				return new Vector2(element.UIElement.resolvedStyle.width, element.UIElement.resolvedStyle.height);
			else
				return new Vector2(element.UIElement.style.width.value.value, element.UIElement.style.height.value.value);
		}

		/// <summary>
		/// Apply a WorldToScreenUIElement's transformations (offset + pivot) to
		/// a point in the uiDocument. Used to place ui elements in accordance
		/// with their transformations.
		/// </summary>
		/// <param name="element">The ui element with the transformations we wish to apply.</param>
		/// <param name="uiDocumentPoint">The point in the uiDocument space that we wish to place the ui element at.</param>
		/// <returns>A point in the uiDocument space to use for placing the ui element.</returns>
		private static Vector2 ApplyUITransformations(IWorldToScreenUIElement element, Vector2 uiDocumentPoint)
		{
			Vector2 uiDimensions = GetUIDimensions(element);
			uiDocumentPoint += element.Offset - element.Pivot * uiDimensions;
			return uiDocumentPoint;
		}
	}
}
