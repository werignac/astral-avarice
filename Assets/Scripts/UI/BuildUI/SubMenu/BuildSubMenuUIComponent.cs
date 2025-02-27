using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;

public class BuildSubMenuUIComponent : MonoBehaviour
{
	protected class BuildSubMenuButtonBinding : BuildingButtonBinding<int>
	{

		private VisualElement rootElement;

		public BuildSubMenuButtonBinding(VisualElement buttonElement, int identifier)
			: base(buttonElement, identifier)
		{
			rootElement = buttonElement;
		}

		protected override string GetTitleName() => "BuildingName";
		protected override string GetSpecialResourcesContainerName() => "ConsumedResources";
		protected override string GetAdvancedMaterialsProductionName() => "ScienceProduction";

		public void ShowSelectUI()
		{
			button.style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUISelectedButtonSprite);
		}

		public void HideSelectUI()
		{
			button.style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUIDeselectedButtonSprite);
		}

		public void Show()
		{
			rootElement.style.display = DisplayStyle.Flex;
		}

		public void Hide()
		{
			rootElement.style.display = DisplayStyle.None;
		}
	}

	private class GameControllerCurrencyWrapper : BuildingButtonBinding<int>.ICurrencyCount
	{
		private GameController gameController;

		public GameControllerCurrencyWrapper(GameController gameController)
		{
			this.gameController = gameController;
		}

		public int Cash => gameController.Cash;

		public int AdvancedMaterials => gameController.HeldScience;
	}

	private UIDocument uiDocument;
	[SerializeField] private VisualTreeAsset buildingButtonTemplate;
	[SerializeField] private InspectorUIComponent inspector;
	[SerializeField] private GameController gameController;
	private GameControllerCurrencyWrapper wrappedGameController;

	private VisualElement rootElement;
	private VisualElement buildButtonContainer;

	private BuildingSettingEntry[] displayingBuildingsList;
	private List<BuildSubMenuButtonBinding> buildButtonBindings = new List<BuildSubMenuButtonBinding>();

	private Dictionary<int, InspectorUIComponent.InspectorLayer> inspectorLayers = new Dictionary<int, InspectorUIComponent.InspectorLayer>();

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		wrappedGameController = new GameControllerCurrencyWrapper(gameController);
	}

	private void Start()
	{
		FetchVisualElements();
		// Listen to events to change the selected building to add.
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void FetchVisualElements()
	{
		rootElement = uiDocument.rootVisualElement.Q("SubMenuContainer");
		buildButtonContainer = rootElement.Q("ButtonsContainer");
		buildButtonContainer.Clear();
	}

	internal void Display(IEnumerable<BuildingSettingEntry> submenuBuildings)
	{
		displayingBuildingsList = new List<BuildingSettingEntry>(submenuBuildings).ToArray();

		// TODO: Handle show select when the user switches to a menu where they have a building selected.
		// and when leaving a menu where there was a building selected.

		// Re-use buttons that already exist for the first buildings to display.
		for (int i = 0; i < displayingBuildingsList.Length && i < buildButtonBindings.Count; i++)
		{
			BuildSubMenuButtonBinding binding = buildButtonBindings[i];
			BuildingSettingEntry building = displayingBuildingsList[i];
			binding.Display(building, wrappedGameController);
			binding.Show();
		}

		// Hide buttons if there are more than what is needed.
		for (int i = displayingBuildingsList.Length; i < buildButtonBindings.Count; i++)
		{
			BuildSubMenuButtonBinding binding = buildButtonBindings[i];
			binding.Hide();
		}

		// Add buttons if we are missing them.
		for (int i = buildButtonBindings.Count; i < displayingBuildingsList.Length; i++)
		{
			BuildingSettingEntry buildingSettingEntry = displayingBuildingsList[i];
			AddBuildingButton(buildingSettingEntry, i);
		}

		// TODO: Refresh inspector layers when we change the displaying buildings.

	}

	private void AddBuildingButton(BuildingSettingEntry toAdd, int id)
	{
		VisualElement buildButtonElement = CreateButton();
		buildButtonContainer.Add(buildButtonElement);

		BuildSubMenuButtonBinding binding = new BuildSubMenuButtonBinding(buildButtonElement, id);
		binding.Display(toAdd, wrappedGameController);
		binding.OnClick.AddListener(BuildingButton_OnClick);
		binding.OnHoverStart.AddListener(BuildingButton_OnHoverStart);
		binding.OnHoverEnd.AddListener(BuildingButton_OnHoverEnd);

		buildButtonBindings.Add(binding);
	}

	private BuildSubMenuButtonBinding ButtonIdToBinding(int buttonId)
	{
		if (buttonId < 0)
			return null;
		return buildButtonBindings[buttonId];
	}

	private BuildingSettingEntry ButtonIdToSettingEntry(int buttonId)
	{
		return displayingBuildingsList[buttonId];
	}

	private int SettingEntryToButtonId(BuildingSettingEntry buildingSettingEntry)
	{
		return Array.FindIndex(displayingBuildingsList, (BuildingSettingEntry toCompare) => buildingSettingEntry == toCompare);
	}

	private void BuildingButton_OnHoverStart(int buttonId)
	{
		var buildingSettings = ButtonIdToSettingEntry(buttonId);

		if (!inspectorLayers.ContainsKey(buttonId))
		{
			InspectorUIComponent.InspectorLayer newLayer = inspector.AddLayer(buildingSettings, InspectorUIComponent.InspectorLayerType.UI_HOVER);
			inspectorLayers.Add(buttonId, newLayer);
		}
	}

	private void BuildingButton_OnHoverEnd(int buttonId)
	{
		if (inspectorLayers.TryGetValue(buttonId, out var layer))
		{
			inspector.RemoveLayer(layer);
			inspectorLayers.Remove(buttonId);
		}
	}

	protected virtual void BuildingButton_OnClick(int buttonId)
	{
		var toBuild = ButtonIdToSettingEntry(buttonId);
		BuildManagerComponent.Instance.SetBuildState(toBuild);
	}

	private VisualElement CreateButton()
	{
		VisualElement button = buildingButtonTemplate.Instantiate();
		return button;
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		// If the old state was a build or demolish state, unselect the corresponding button.
		if (oldState != null && oldState.GetStateType() != BuildStateType.NONE)
		{
			if ((oldState.GetStateType() & BuildStateType.BUILDING) != 0)
			{
				BuildingBuildState buildingBuildState = oldState as BuildingBuildState;

				int buttonId = SettingEntryToButtonId(buildingBuildState.toBuild);
				BuildSubMenuButtonBinding toHideButton = ButtonIdToBinding(buttonId);

				if (toHideButton != null)
					toHideButton.HideSelectUI();
			}
		}


		// If the new state is a build or demolish state, select the corresponding button.
		if (newState != null && newState.GetStateType() != BuildStateType.NONE)
		{
			// Select the corresponding build button.
			if ((newState.GetStateType() & BuildStateType.BUILDING) != 0)
			{
				BuildingBuildState buildingBuildState = newState as BuildingBuildState;

				int buttonId = SettingEntryToButtonId(buildingBuildState.toBuild);
				BuildSubMenuButtonBinding toShowButton = ButtonIdToBinding(buttonId);

				if (toShowButton != null)
					toShowButton.ShowSelectUI();
			}
		}
	}

	/// <summary>
	/// Keep updating the bindings' affordability.
	/// </summary>
	private void Update()
	{
		foreach(var binding in buildButtonBindings)
		{
			binding.UpdateUI();
		}
	}

	/// <summary>
	/// Called when the player clicks on a building category.
	/// </summary>
	public void Show()
	{
		rootElement.style.display = DisplayStyle.Flex;
	}

	/// <summary>
	/// Called when the player moves off of a building category.
	/// </summary>
	public void Hide()
	{
		rootElement.style.display = DisplayStyle.None;
	}
}
