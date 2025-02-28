using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System;

namespace AstralAvarice.VisualData
{
	/// <summary>
	/// Updates a menu element in the build ui to have specific values (icon sprites, tooltips, etc).
	/// Does not handle inspectors when hovering.
	/// 
	/// These are not expected to need to update in realtime.
	/// </summary>
	/// <typeparam name="T">This class houses an identifier. The identifier is passed in event calls. Use this to figure out which element bindings is invoking its event.</typeparam>
    public class BuildUIMenuElementBinding<T>
    {
		private const string SELECTED_CLASS = "selectedBuildingCategory";

		private VisualElement rootElement;
		private Button buttonElement;
		private VisualElement iconElement;

		public T Identifier { get; private set; }

		[HideInInspector] public UnityEvent<T> OnHoverStart = new UnityEvent<T>();
		[HideInInspector] public UnityEvent<T> OnHoverEnd = new UnityEvent<T>();
		[HideInInspector] public UnityEvent<T> OnClick = new UnityEvent<T>();

		public BuildUIMenuElementBinding(T identifier)
		{
			Identifier = identifier;
		}

		public BuildUIMenuElementBinding(VisualElement rootElement, T identifier) : this(identifier)
		{
			Bind(rootElement);
		}

		public void Bind(VisualElement rootElement)
		{
			// Stop listening to the events from the previously bound UI element.
			if (buttonElement != null)
				UnregisterEvents();

			this.rootElement = rootElement;
			buttonElement = rootElement.Q<Button>("ButtonContainer");
			iconElement = rootElement.Q("Icon");

			// Start listening to the events from the newly bound UI element.
			RegisterEvents();
		}

		private void RegisterEvents()
		{
			buttonElement.RegisterCallback<MouseEnterEvent>(Button_OnHoverStart);
			buttonElement.RegisterCallback<MouseLeaveEvent>(Button_OnHoverEnd);
			buttonElement.RegisterCallback<ClickEvent>(Button_OnClick);
		}

		private void UnregisterEvents()
		{
			buttonElement.UnregisterCallback<MouseEnterEvent>(Button_OnHoverStart);
			buttonElement.UnregisterCallback<MouseLeaveEvent>(Button_OnHoverEnd);
			buttonElement.UnregisterCallback<ClickEvent>(Button_OnClick);
		}

		private void Button_OnClick(ClickEvent evt)
		{
			OnClick?.Invoke(Identifier);
		}

		private void Button_OnHoverEnd(MouseLeaveEvent evt)
		{
			OnHoverEnd?.Invoke(Identifier);
		}

		private void Button_OnHoverStart(MouseEnterEvent evt)
		{
			OnHoverStart?.Invoke(Identifier);
		}

		public void Display(IBuildUIMenuElement toDisplay)
		{
			buttonElement.tooltip = FormatTooltip(toDisplay);
			iconElement.style.backgroundImage = new StyleBackground(toDisplay.Icon);
		}

		private static string FormatTooltip(IBuildUIMenuElement toDisplay)
		{
			return $"{toDisplay.Name}: {toDisplay.Description}";
		}

		public void ShowSelectUI()
		{
			buttonElement.AddToClassList(SELECTED_CLASS);
		}

		public void HideSelectUI()
		{
			buttonElement.RemoveFromClassList(SELECTED_CLASS);
		}

	}
}
