using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;

public class BuildUIComponent : MonoBehaviour, IInspectable
{
	private class BuildButtonBinding
	{
		private VisualElement boundButtonElement;
		private BuildingSettingEntry buildingSettingEntry;

		// Events;
		public UnityEvent<BuildingSettingEntry> OnButtonClick = new UnityEvent<BuildingSettingEntry>();
		public UnityEvent<BuildingSettingEntry> OnButtonHoverStart = new UnityEvent<BuildingSettingEntry>();
		public UnityEvent<BuildingSettingEntry> OnButtonHoverEnd = new UnityEvent<BuildingSettingEntry>();

		public BuildButtonBinding(VisualElement buttonElement, BuildingSettingEntry buildingSettingEntry)
		{
			boundButtonElement = buttonElement;
			this.buildingSettingEntry = buildingSettingEntry;

			Button button = buttonElement.Q<Button>("BuildButton");
			button.RegisterCallback<ClickEvent>(Button_OnClick);
			button.RegisterCallback<PointerEnterEvent>(Button_OnHoverStart);
			button.RegisterCallback<PointerLeaveEvent>(Button_OnHoverEnd);
		}

		private void Button_OnHoverEnd(PointerLeaveEvent evt)
		{
			OnButtonHoverEnd?.Invoke(buildingSettingEntry);
		}

		private void Button_OnHoverStart(PointerEnterEvent evt)
		{
			OnButtonHoverStart?.Invoke(buildingSettingEntry);
		}

		private void Button_OnClick(ClickEvent evt)
		{
			OnButtonClick?.Invoke(buildingSettingEntry);
		}

		/// <summary>
		/// Checks whether this binding sends the specified
		/// building setting entry.
		/// </summary>
		private bool Sends(BuildingSettingEntry buildingSettingEntry)
		{
			return this.buildingSettingEntry == buildingSettingEntry;
		}

		public void ShowSelectUI()
		{
			// TODO
		}

		public void HideSelectUI()
		{
			// TODO
		}
	}

	private UIDocument uiDocument;
	[SerializeField] private VisualTreeAsset buildingButtonTemplate;
	[SerializeField] private Sprite demolishIcon;
	[SerializeField] private Sprite cableIcon;
	[SerializeField] private InspectorUIComponent inspectorUI;

	private VisualElement collapsableBuildMenu;
	private Button collapseMenuButton;
	private VisualElement buildButtonContainer;
	private VisualElement demolishButtonElement;
	private VisualElement cableButtonElement;

	private List<BuildButtonBinding> buildButtonBindings = new List<BuildButtonBinding>();

	private BuildingSettingEntry hoveredBuildingSettingEntry;
	private InspectorUIComponent.InspectorLayer hoveredBuildingLayer;


	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		collapsableBuildMenu = uiDocument.rootVisualElement.Q("CollapsableContainer");
		collapseMenuButton = uiDocument.rootVisualElement.Q("CollapseButton") as Button;
		buildButtonContainer = uiDocument.rootVisualElement.Q("BuildButtonContainer");

		// Add a demolish button.
		demolishButtonElement = CreateDemolishButton();
		buildButtonContainer.Add(demolishButtonElement);

		// Add a cable button
		cableButtonElement = CreateCableButton();
		buildButtonContainer.Add(cableButtonElement);

		// Add a button for each placeable building.
		foreach (BuildingSettingEntry buildingSettingEntry in GlobalBuildingSettings.GetOrCreateSettings().Buildings)
		{
			VisualElement buildButtonElement = CreateButton(buildingSettingEntry.VisualAsset.buildingIcon);
			buildButtonContainer.Add(buildButtonElement);

			BuildButtonBinding binding = new BuildButtonBinding(buildButtonElement, buildingSettingEntry);
			binding.OnButtonClick.AddListener(BuildingButton_OnClick);
			binding.OnButtonHoverStart.AddListener(BuildingButton_OnHoverStart);
			binding.OnButtonHoverEnd.AddListener(BuildingButton_OnHoverEnd);

			buildButtonBindings.Add(binding);
		}

		collapseMenuButton.RegisterCallback<ClickEvent>(BuildManager_OnClickToggle);
		demolishButtonElement.Q<Button>("BuildButton").RegisterCallback<ClickEvent>(DemolishButton_OnClick);
		cableButtonElement.Q<Button>("BuildButton").RegisterCallback<ClickEvent>(CableButton_OnClick);

		// TODO: Unregister listeners on disable / destroy.
		

		// Listen to events to change the selected building to add.
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	protected virtual void CableButton_OnClick(ClickEvent evt)
	{
		BuildManagerComponent.Instance.SetCableState();
	}

	private void BuildingButton_OnHoverEnd(BuildingSettingEntry arg0)
	{
		if(hoveredBuildingSettingEntry == arg0)
		{
			inspectorUI.RemoveLayer(hoveredBuildingLayer);
			hoveredBuildingSettingEntry = null;
			hoveredBuildingLayer = null;
		}
	}

	private void BuildingButton_OnHoverStart(BuildingSettingEntry arg0)
	{
		hoveredBuildingSettingEntry = arg0;
		if (hoveredBuildingLayer == null)
		{
			hoveredBuildingLayer = inspectorUI.AddLayer(this, InspectorUIComponent.InspectorLayerType.UI_HOVER);
		}
	}

    protected virtual void BuildingButton_OnClick(BuildingSettingEntry toBuild)
	{
		BuildManagerComponent.Instance.SetBuildState(toBuild);
	}

    protected virtual void DemolishButton_OnClick(ClickEvent evt)
	{
		BuildManagerComponent.Instance.SetDemolishState();
	}

	private VisualElement CreateDemolishButton()
	{
		return CreateButton(demolishIcon);
	}

	private VisualElement CreateCableButton()
	{
		return CreateButton(cableIcon);
	}

	private VisualElement CreateButton(Sprite icon)
	{
		VisualElement button = buildingButtonTemplate.Instantiate();
		VisualElement iconElement = button.Q("Icon");

		StyleBackground background = new StyleBackground(icon);
		iconElement.style.backgroundImage = background;

		return button;
	}

	private void BuildManager_OnClickToggle(ClickEvent evt)
	{
		switch (collapsableBuildMenu.style.display.value)
		{
			case DisplayStyle.None:
				collapsableBuildMenu.style.display = DisplayStyle.Flex;
				break;
			case DisplayStyle.Flex:
				collapsableBuildMenu.style.display = DisplayStyle.None;
				break;
		}
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		// If the old state was a build or demolish state, unselect the corresponding button.
		if (oldState != null && oldState.GetStateType() != BuildStateType.NONE)
		{
			if (oldState.GetStateType() == BuildStateType.DEMOLISH)
			{
				// Unselect the demolish button.
			}
			else if (oldState.GetStateType() == BuildStateType.CABLE)
			{
				// Unselect the cable button.
			}
			else
			{
				// Unselect the corresponding build button.
			}
		}


		// If the new state is a build or demolish state, select the corresponding button.
		if (newState != null && newState.GetStateType() != BuildStateType.NONE)
		{
			if (newState.GetStateType() == BuildStateType.DEMOLISH)
			{
				// Select the demolish button.
			}
			else if (oldState.GetStateType() == BuildStateType.CABLE)
			{
				// Select the cable button.
			}
			else
			{
				// Select the corresponding build button.
			}
		}
	}

    public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = new BuildingButtonInspectorController(hoveredBuildingSettingEntry);
		return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
	}
}
