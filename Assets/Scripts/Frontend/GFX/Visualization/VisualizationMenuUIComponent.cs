using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AstralAvarice.Visualization
{
	/// <summary>
	/// Class that interfaces with the visualization menu.
	/// </summary>
	[RequireComponent(typeof(UIDocument))]
	public class VisualizationMenuComponent : MonoBehaviour
	{
		private const string MENU_BUTTON_ELEMENT_NAME = "VisualizationMenuButton";
		private const string MENU_ELEMENT_NAME = "VisualizationMenu";
		private const string ENTRY_CONTAINER_ELEMENT_NAME = "VisualizationMenuContainer";

		private UIDocument uiDocument;
		private Button menuButtonElement;
		private VisualElement menuElement;
		private VisualElement entryContainerElement;

		/// <summary>
		/// Stores mapping of menu entries and the bindings that they map to.
		/// </summary>
		private Dictionary<VisualizationToggleState_SO, VisualizationStateUIBinding> bindingMap = new Dictionary<VisualizationToggleState_SO, VisualizationStateUIBinding>();

		private void Awake()
		{
			uiDocument = GetComponent<UIDocument>();
		}

		private void Start()
		{
			Bind(uiDocument.rootVisualElement);
			PopulateEntryContainer();
			HideMenu();
		}

		/// <summary>
		/// Called once. Sets up element references & event listeners.
		/// </summary>
		/// <param name="rootElement">The element to bind to.</param>
		private void Bind(VisualElement rootElement)
		{
			menuButtonElement = rootElement.Q<Button>(MENU_BUTTON_ELEMENT_NAME);
			menuElement = rootElement.Q(MENU_ELEMENT_NAME);
			entryContainerElement = rootElement.Q(ENTRY_CONTAINER_ELEMENT_NAME);

			entryContainerElement.Clear();

			RegisterListeners();
		}

		private void RegisterListeners()
		{
			menuButtonElement.RegisterCallback<ClickEvent>(MenuButton_OnClick);
		}

		private void PopulateEntryContainer()
		{
			VisualizationUISettings_SO settings = PtUUISettings.GetOrCreateSettings().VisualizationSettings;

			foreach(VisualizationToggleState_SO state in settings.StatesToDisplay)
			{
				CreateStateUI(state, settings);
			}
		}

		/// <summary>
		/// Create the UI for a toggleable state.
		/// Bind to the new UI so that it updates the state / gets updated when the state changes.
		/// </summary>
		/// <param name="state">The state to bind to.</param>
		/// <param name="settings">The settings that store the reference to the UI asset to instaniate.</param>
		private void CreateStateUI(VisualizationToggleState_SO state, VisualizationUISettings_SO settings)
		{
			TemplateContainer ui = settings.MenuStateElement.Instantiate();
			VisualizationStateUIBinding binding = new VisualizationStateUIBinding(ui, state);
			entryContainerElement.Add(ui);

			bindingMap.Add(state, binding);
		}

		/// <summary>
		/// Toggle the menu on and off when the menu button is pressed.
		/// </summary>
		/// <param name="evt">The click event.</param>
		private void MenuButton_OnClick(ClickEvent evt)
		{
			if (menuElement.style.display == DisplayStyle.Flex)
				HideMenu();
			else
				ShowMenu();
		}

		private void ShowMenu()
		{
			menuElement.style.display = DisplayStyle.Flex;
		}

		private void HideMenu()
		{
			menuElement.style.display = DisplayStyle.None;
		}
	}
}
