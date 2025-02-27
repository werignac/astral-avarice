using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.VisualData;
using System.Collections.Generic;
using System;

public class BuildMenuUIComponent : MonoBehaviour
{
	private UIDocument uiDocument;

	// TODO: Implement.
	/// <summary>
	/// All the buildings that can be placed by the player in this level.
	/// </summary>
	private BuildingSettingEntry[] AllBuildings { get; set; }

	private BuildSubMenuUIComponent submenu;
	[SerializeField] private GameController gameController;

	[SerializeField] private VisualTreeAsset menuButtonTemplate;

	private VisualElement menuContainer;
	private VisualElement buttonsContainer;
	private List<BuildUIMenuElementBinding<int>> buttonBindings = new List<BuildUIMenuElementBinding<int>>();

	private IBuildUIMenuElement[] displayingMenuElements;
	
	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		submenu = GetComponent<BuildSubMenuUIComponent>();
		gameController.OnLevelLoad?.AddListener(GameController_OnLevelLoad);
	}

	private void GameController_OnLevelLoad()
	{
		AllBuildings = BuildManagerComponent.Instance.PlaceableBuildings;

		Initialize();
	}

	/// <summary>
	/// Find all the relevant visual elements in the UI document, instantiate
	/// buttons etc.
	/// </summary>
	private void Initialize()
	{
		submenu.Hide();
		FetchVisualElements();
		InitializeMenuElements();
	}

	private void FetchVisualElements()
	{
		menuContainer = uiDocument.rootVisualElement.Q("MenuContainer");
		buttonsContainer = menuContainer.Q("ButtonsContainer");
		buttonsContainer.Clear();
	}

	private void InitializeMenuElements()
	{
		displayingMenuElements = GatherMenuElements();

		foreach (IBuildUIMenuElement toDisplay in displayingMenuElements)
			AddMenuElement(toDisplay);
	}

	private void AddMenuElement(IBuildUIMenuElement toAdd)
	{
		// Create a new instance of a button to display the element.
		TemplateContainer buttonInstance = menuButtonTemplate.Instantiate();
		buttonsContainer.Add(buttonInstance);

		// Bind the button to the element.
		int newBindingId = buttonBindings.Count;
		BuildUIMenuElementBinding<int> buttonBinding = new BuildUIMenuElementBinding<int>(buttonInstance, newBindingId);
		buttonBindings.Add(buttonBinding);
		buttonBinding.Display(toAdd);

		buttonBinding.OnClick.AddListener(MenuElement_OnClick);
		buttonBinding.OnHoverStart.AddListener(MenuElement_OnHoverStart);
		buttonBinding.OnHoverEnd.AddListener(MenuElement_OnHoverEnd);


	}

	private void MenuElement_OnClick(int id)
	{
		IBuildUIMenuElement clickedElement = displayingMenuElements[id];
		
		// Multiplex the element that was clicked.
		if (clickedElement is BuildModeVisuals)
		{
			// The element is a build mode.
			BuildModeVisuals clickedMode = clickedElement as BuildModeVisuals;
			PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();

			if (uiSettings.CableModeVisuals == clickedMode)
				OnCableClicked();
			if (uiSettings.MoveModeVisuals == clickedMode)
				OnMoveClicked();
			if (uiSettings.DemolishModeVisuals == clickedMode)
				OnDemolishClicked();
		}
		else
		{
			// The element is a building category.
			BuildCategory clickedCategory = clickedElement as BuildCategory;
			OnCategoryClicked(clickedCategory);
		}
	}

	private void OnCableClicked()
	{
		BuildManagerComponent.Instance.SetCableState();
	}

	private void OnMoveClicked()
	{
		BuildManagerComponent.Instance.SetMoveState(null);
	}

	private void OnDemolishClicked()
	{
		BuildManagerComponent.Instance.SetDemolishState();
	}

	private void OnCategoryClicked(BuildCategory clickedCategory)
	{
		var submenuBuildings = GatherBuildingsOfCategory(clickedCategory);

		submenu.Display(submenuBuildings);
		submenu.Show();
	}

	private void MenuElement_OnHoverStart(int id)
	{
		// TODO: Update inspector.
	}

	private void MenuElement_OnHoverEnd(int id)
	{
		// TODO: Update inspector.
	}

	/// <summary>
	/// Gathers all the build menu elements that should be displayed in the lower left
	/// of the screen.
	/// </summary>
	/// <returns>A list of build menu elements sorted by priority.</returns>
	private IBuildUIMenuElement[] GatherMenuElements()
	{
		List<IBuildUIMenuElement> elementsList = new List<IBuildUIMenuElement>();

		// Get the elements that correspond to "static" modes (cables, move, demolish).
		elementsList.AddRange(GatherBuildModeMenuUIElements());

		// Get the elements that correspond to building types (generators, pylons, utilities, labs).
		elementsList.AddRange(GatherBuildingMenuUIElements());

		// Sort by priority.
		elementsList.Sort(new BuildUIMenuElementPriorityComparer());
		
		return elementsList.ToArray();
	}

	/// <summary>
	/// Get the menu elements for build modes other than building build mode.
	/// </summary>
	private IEnumerable<IBuildUIMenuElement> GatherBuildModeMenuUIElements()
	{
		PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();
		yield return uiSettings.CableModeVisuals;
		yield return uiSettings.MoveModeVisuals;
		yield return uiSettings.DemolishModeVisuals;
	}

	/// <summary>
	/// Get the menu elements for the different building categories.
	/// </summary>
	private IEnumerable<IBuildUIMenuElement> GatherBuildingMenuUIElements()
	{
		HashSet<BuildCategory> uniqueCategories = new HashSet<BuildCategory>();

		// Collect all unique categories.
		foreach (BuildingSettingEntry building in AllBuildings)
			uniqueCategories.UnionWith(building.VisualAsset.categories);

		return uniqueCategories;
	}

	private IEnumerable<BuildingSettingEntry> GatherBuildingsOfCategory(BuildCategory category)
	{
		// Find all buildings with the specified category.
		return System.Array.FindAll(
			AllBuildings,
			(BuildingSettingEntry building) => System.Array.IndexOf(building.VisualAsset.categories, category) >= 0
		);
	}
}
