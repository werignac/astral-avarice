using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;

public class BuildUIComponent : MonoBehaviour
{
	protected class BuildButtonBinding : IInspectable
	{
		private VisualElement boundButtonElement;
		private BuildingSettingEntry buildingSettingEntry;

		// Events;
		public UnityEvent<BuildingSettingEntry, BuildButtonBinding> OnButtonClick = new UnityEvent<BuildingSettingEntry, BuildButtonBinding>();
		public UnityEvent<BuildingSettingEntry, BuildButtonBinding> OnButtonHoverStart = new UnityEvent<BuildingSettingEntry, BuildButtonBinding>();
		public UnityEvent<BuildingSettingEntry, BuildButtonBinding> OnButtonHoverEnd = new UnityEvent<BuildingSettingEntry, BuildButtonBinding>();

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
			OnButtonHoverEnd?.Invoke(buildingSettingEntry, this);
		}

		private void Button_OnHoverStart(PointerEnterEvent evt)
		{
			OnButtonHoverStart?.Invoke(buildingSettingEntry, this);
		}

		private void Button_OnClick(ClickEvent evt)
		{
			OnButtonClick?.Invoke(buildingSettingEntry, this);
		}

		/// <summary>
		/// Checks whether this binding sends the specified
		/// building setting entry.
		/// </summary>
		public bool Sends(BuildingSettingEntry buildingSettingEntry)
		{
			return this.buildingSettingEntry == buildingSettingEntry;
		}

		public void ShowSelectUI()
		{
			boundButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUISelectedButtonSprite);
		}

		public void HideSelectUI()
		{
			boundButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUIDeselectedButtonSprite);
		}

		public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
		{
			inspectorController = new BuildingButtonInspectorController(buildingSettingEntry);
			return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
		}
	}

	private static InspectableDemolish demolishInspector = new InspectableDemolish();
	private static InspectableCable cableInspector = new InspectableCable();

	private UIDocument uiDocument;
	[SerializeField] private VisualTreeAsset buildingButtonTemplate;
	[SerializeField] private Sprite collapseButtonMenuOpenedSprite;
	[SerializeField] private Sprite collapseButtonMenuClosedSprite;
	[SerializeField] private Sprite demolishIcon;
	[SerializeField] private Sprite cableIcon;
	[SerializeField] private InspectorUIComponent inspectorUI;

	private VisualElement collapsableBuildMenu;
	private Button collapseMenuButton;
	private VisualElement buildButtonContainer;
	private VisualElement demolishButtonElement;
	private VisualElement cableButtonElement;

	private List<BuildButtonBinding> buildButtonBindings = new List<BuildButtonBinding>();

	private Dictionary<System.Object, InspectorUIComponent.InspectorLayer> inspectorLayers = new Dictionary<System.Object, InspectorUIComponent.InspectorLayer>();


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
		foreach (BuildingSettingEntry buildingSettingEntry in BuildManagerComponent.Instance.PlaceableBuildings)
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
		
		Button demolishButton = demolishButtonElement.Q<Button>("BuildButton");
		demolishButton.RegisterCallback<ClickEvent>(DemolishButton_OnClick);
		demolishButton.RegisterCallback<PointerEnterEvent>(DemolishButton_OnHoverStart);
		demolishButton.RegisterCallback<PointerLeaveEvent>(DemolishButton_OnHoverEnd);

		Button cableButton = cableButtonElement.Q<Button>("BuildButton");
		cableButton.RegisterCallback<ClickEvent>(CableButton_OnClick);
		cableButton.RegisterCallback<PointerEnterEvent>(CableButton_OnHoverStart);
		cableButton.RegisterCallback<PointerLeaveEvent>(CableButton_OnHoverEnd);

		// TODO: Unregister listeners on disable / destroy.
		

		// Listen to events to change the selected building to add.
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void DemolishButton_OnHoverEnd(PointerLeaveEvent evt)
	{
		if (inspectorLayers.ContainsKey(demolishInspector))
		{
			var layer = inspectorLayers[demolishInspector];
			inspectorUI.RemoveLayer(layer);
			inspectorLayers.Remove(demolishInspector);
		}
	}

	private void CableButton_OnHoverEnd(PointerLeaveEvent evt)
	{
		if (inspectorLayers.ContainsKey(cableInspector))
		{
			var layer = inspectorLayers[cableInspector];
			inspectorUI.RemoveLayer(layer);
			inspectorLayers.Remove(cableInspector);
		}
	}

	private void CableButton_OnHoverStart(PointerEnterEvent evt)
	{
		if (!inspectorLayers.ContainsKey(cableInspector))
		{
			InspectorUIComponent.InspectorLayer layer = inspectorUI.AddLayer(cableInspector, InspectorUIComponent.InspectorLayerType.UI_HOVER);
			inspectorLayers.Add(cableInspector, layer);
		}
	}

	private void DemolishButton_OnHoverStart(PointerEnterEvent evt)
	{
		if (! inspectorLayers.ContainsKey(demolishInspector))
		{
			InspectorUIComponent.InspectorLayer layer = inspectorUI.AddLayer(demolishInspector, InspectorUIComponent.InspectorLayerType.UI_HOVER);
			inspectorLayers.Add(demolishInspector, layer);
		}
	}

	protected virtual void CableButton_OnClick(ClickEvent evt)
	{
		BuildManagerComponent.Instance.SetCableState();
	}

	private void BuildingButton_OnHoverEnd(BuildingSettingEntry buildingSettings, BuildButtonBinding _)
	{
		if (inspectorLayers.ContainsKey(buildingSettings))
		{
			InspectorUIComponent.InspectorLayer layer = inspectorLayers[buildingSettings];
			inspectorUI.RemoveLayer(layer);
			inspectorLayers.Remove(buildingSettings);
		}
	}

	private void BuildingButton_OnHoverStart(BuildingSettingEntry buildingSettings, BuildButtonBinding button)
	{
		if (!inspectorLayers.ContainsKey(buildingSettings))
		{
			InspectorUIComponent.InspectorLayer newLayer = inspectorUI.AddLayer(button, InspectorUIComponent.InspectorLayerType.UI_HOVER);
			inspectorLayers.Add(buildingSettings, newLayer);
		}
	}

    protected virtual void BuildingButton_OnClick(BuildingSettingEntry toBuild, BuildButtonBinding _)
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
				collapseMenuButton.style.backgroundImage = new StyleBackground(collapseButtonMenuOpenedSprite);
				break;
			case DisplayStyle.Flex:
				collapsableBuildMenu.style.display = DisplayStyle.None;
				collapseMenuButton.style.backgroundImage = new StyleBackground(collapseButtonMenuClosedSprite);
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
				demolishButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUIDeselectedButtonSprite);
			}
			else 
			{
				// Unselect the cable button.
				if ((oldState.GetStateType() & BuildStateType.CABLE) != 0)
					cableButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUIDeselectedButtonSprite);

				// Unselect the corresponding build button.
				if ((oldState.GetStateType() & BuildStateType.BUILDING) != 0)
				{
					BuildingBuildState buildingBuildState = oldState as BuildingBuildState;

					BuildButtonBinding toHideButton = buildButtonBindings.Find(
						(BuildButtonBinding button) => button.Sends(buildingBuildState.toBuild)
					);

					if (toHideButton != null)
						toHideButton.HideSelectUI();
				}
			}
		}


		// If the new state is a build or demolish state, select the corresponding button.
		if (newState != null && newState.GetStateType() != BuildStateType.NONE)
		{
			if (newState.GetStateType() == BuildStateType.DEMOLISH)
			{
				// Select the demolish button.
				demolishButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUISelectedButtonSprite);
			}
			else 
			{
				// Select the cable button.
				if ((newState.GetStateType() & BuildStateType.CABLE) != 0)
					cableButtonElement.Q("BuildButton").style.backgroundImage = new StyleBackground(PtUUISettings.GetOrCreateSettings().BuildUISelectedButtonSprite);

				// Select the corresponding build button.
				if ((newState.GetStateType() & BuildStateType.BUILDING) != 0)
				{
					BuildingBuildState buildingBuildState = newState as BuildingBuildState;

					BuildButtonBinding toShowButton = buildButtonBindings.Find(
						(BuildButtonBinding button) => button.Sends(buildingBuildState.toBuild)
					);

					if (toShowButton != null)
						toShowButton.ShowSelectUI();
				}
			}
		}
	}
}
