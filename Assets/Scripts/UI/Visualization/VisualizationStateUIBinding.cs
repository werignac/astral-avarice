using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace AstralAvarice.Visualization
{
	/// <summary>
	/// Binds to an instance of the visualization state UI to control
	/// UI elements and receive input.
	/// </summary>
	public class VisualizationStateUIBinding
	{
		private const string TOGGLE_BUTTON_ELEMENT_NAME = "ToggleButton";
		private const string LABEL_ELEMENT_NAME = "StateLabel";

		private const string ON_STATE_CLASS = "visualizationMenuState_On";
		private const string OFF_STATE_CLASS = "visualizationMenuState_Off";

		private VisualElement rootElement = null;
		private Button toggleButtonElement;
		private Label labelElement;

		private VisualizationToggleState_SO displayingState = null;

#region Constructors
		public VisualizationStateUIBinding() { }

		public VisualizationStateUIBinding(VisualElement elementToBindTo)
		{
			Bind(elementToBindTo);
		}

		public VisualizationStateUIBinding(VisualizationToggleState_SO displayingState)
		{
			Display(displayingState);
		}

		public VisualizationStateUIBinding(
			VisualElement elementToBindTo,
			VisualizationToggleState_SO displayingState
			)
		{
			Bind(elementToBindTo);
			Display(displayingState);
		}
#endregion Constructors

		public void Bind(VisualElement elementToBindTo)
		{
			if (rootElement != null)
				DeregisterEvents();

			Debug.Assert(elementToBindTo != null, $"Cannot bind to null element.");

			rootElement = elementToBindTo;
			toggleButtonElement = elementToBindTo.Q<Button>(TOGGLE_BUTTON_ELEMENT_NAME);
			labelElement = elementToBindTo.Q<Label>(LABEL_ELEMENT_NAME);

			RegisterEvents();

			if (displayingState != null)
				UpdateDisplay();
		}

		private void RegisterEvents()
		{
			toggleButtonElement.RegisterCallback<ClickEvent>(ToggleButton_OnClick);
		}

		private void DeregisterEvents()
		{
			toggleButtonElement.UnregisterCallback<ClickEvent>(ToggleButton_OnClick);
		}

		public void Display(VisualizationToggleState_SO state)
		{
			if (displayingState != null)
				displayingState.RemoveStateChangeListener(DisplayingState_OnChange);
			
			displayingState = state;
			displayingState.AddStateChangeListener(DisplayingState_OnChange);

			if (rootElement != null)
				UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			// TODO: Change to proper display name.
			labelElement.text = displayingState.name;

			if (displayingState.Value)
			{
				rootElement.AddToClassList(ON_STATE_CLASS);
				rootElement.RemoveFromClassList(OFF_STATE_CLASS);
			}
			else
			{
				rootElement.RemoveFromClassList(ON_STATE_CLASS);
				rootElement.AddToClassList(OFF_STATE_CLASS);
			}
		}
		private void ToggleButton_OnClick(ClickEvent evt)
		{
			displayingState.Toggle();
		}

		private void DisplayingState_OnChange(bool arg0)
		{
			UpdateDisplay();
		}
	}
}
