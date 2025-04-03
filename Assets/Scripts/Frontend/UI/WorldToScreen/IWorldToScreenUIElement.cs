using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// An object that can control the behaviour of a world to screen ui
	/// element every frame.
	/// </summary>
	public class WorldToScreenComponent
	{
		public interface IWorldToScreenComponentParent
		{
			Vector3 WorldPosition { get; }
			public Vector2 Pivot { get; }

			public Vector2 Offset { get; }

			public VisualElement UIElement { get; }
		}

		private IWorldToScreenComponentParent parent;
		protected Vector3 WorldPosition => parent.WorldPosition;
		protected Vector2 Pivot => parent.Pivot;
		protected Vector2 Offset => parent.Offset;
		protected VisualElement UIElement => parent.UIElement;

		public void AssignParent(IWorldToScreenComponentParent parent)
		{
			this.parent = parent;
		}

		/// <summary>
		/// Override for custom logic that runs every frame prior to moving UI elements.
		/// </summary>
		public virtual void Update() { }
	}

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

		public ICollection<WorldToScreenComponent> Components { get; }
	}
}
