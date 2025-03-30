using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    public static class IWorldToScreenUIElementExtentions
    {
		/// <summary>
		/// Wraps a IWorldToScreenUIElement into a WorldToScreenComponent.IWorldToScreenComponentParent
		/// </summary>
		private class WorldToScreenUIElementComponentWrapper : WorldToScreenComponent.IWorldToScreenComponentParent
		{
			private IWorldToScreenUIElement element;

			public Vector3 WorldPosition => element.WorldPosition;

			public Vector2 Pivot => element.Pivot;

			public Vector2 Offset => element.Offset;

			public VisualElement UIElement => element.UIElement;

			public WorldToScreenUIElementComponentWrapper(IWorldToScreenUIElement element)
			{
				this.element = element;
			}
		}

		/// <summary>
		/// Creates and adds a component to this worlds screen ui element.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <param name="self">The world screen ui element to add the component to.</param>
		/// <returns>The newly created component.</returns>
		public static T AddComponent<T>(this IWorldToScreenUIElement self) where T : WorldToScreenComponent, new()
		{
			T component = new T();
			WorldToScreenUIElementComponentWrapper wrapper = new WorldToScreenUIElementComponentWrapper(self);
			component.AssignParent(wrapper);
			self.Components.Add(component);
			return component;
		}
    }
}
