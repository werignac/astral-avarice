using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    public interface IWorldToScreenUIElement
    {
		public Vector3 WorldPosition { get; }

		/// <summary>
		/// What point on the UI should match the point in world space.
		/// 0,0 for the upper left corner. 1,1 for the bottom right corner.
		/// Values can go above or below 0 and 1.
		/// </summary>
		public Vector2 Pivot { get; }

		/// <summary>
		/// An offset applied to the position of the UI. Unlike
		/// pivot, it's independent of the size of the element and is
		/// measured in pixels.
		/// </summary>
		public Vector2 Offset { get; }

		public VisualElement UIElement { get; set; }
	}
}
