using System;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingInspectorController : IInspectorController
{
	private BuildingComponent displayingBuilding;

	private string BuildingName
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
			{
				return displayingBuilding.BuildingVisuals.buildingName;
			}
			return displayingBuilding.Data.buildingName;
		}
	}

	private Sprite BuildingIcon
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
				return displayingBuilding.BuildingVisuals.buildingIcon;

			return null;
		}
	}

	private bool BuildingHasIncome
	{
		get => displayingBuilding.Data.income > 0;
	}

	private float BuildingIncome
	{
		get => displayingBuilding.Data.income;
	}

	private bool BuildingProducesPower
	{
		get => displayingBuilding.Data.powerProduced > 0;
	}

	private float BuildingProducedPower
	{
		get => displayingBuilding.Data.powerProduced;
	}
	
	private bool BuildingConsumesElectricity
	{
		get => displayingBuilding.Data.powerRequired > 0;
	}

	private float BuildingConsumedElectricity
	{
		get => displayingBuilding.Data.powerRequired;
	}

	private bool BuildingHasUpkeep
	{
		get => displayingBuilding.Data.upkeep > 0;
	}

	private float BuildingUpkeep
	{
		get => displayingBuilding.Data.upkeep;
	}

	private int BuildingConnectionsConsumed
	{
		get => displayingBuilding.BackendBuilding.NumConnected;
	}

	private int BuildingConnectionsTotal
	{
		get => displayingBuilding.Data.maxPowerLines;
	}


	private bool BuildingIsPowered
	{
		get => displayingBuilding.BackendBuilding.IsPowered;
	}

	private string BuildingDescription
	{
		get
		{
			if (displayingBuilding.BuildingVisuals != null)
				return displayingBuilding.BuildingVisuals.buildingDescription
		;
			return "";
		}
	}

	private Label subheading;
	private VisualElement icon;
	private VisualElement income;
	private Label incomeCurrentValue;
	private Label incomePotentialValue;
	private VisualElement electricityProduction;
	private Label electricityProductionValue;
	private VisualElement advancedMaterialsProduction;
	private Label advancedMaterialProductionValue;
	private VisualElement electricityConsumption;
	private Label electricityConsumptionValue;
	private VisualElement upkeep;
	private Label upkeepValue;
	private VisualElement connections;
	private Label connectionsConsumedValue;
	private Label connectionsTotalValue;
	private VisualElement specialResources;
	private VisualElement settings;
	private Button poweredButton;
	private VisualElement poweredCheckMark;
	private Label description;


	public BuildingInspectorController(BuildingComponent toDisplay)
	{
		displayingBuilding = toDisplay;
	}

	public void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		subheading = inspectorUI.Q<Label>("Subheading");
		icon = inspectorUI.Q("Icon");
		income = inspectorUI.Q("Income");
		incomeCurrentValue = inspectorUI.Q<Label>("CurrentIncomeValue");
		incomePotentialValue = inspectorUI.Q<Label>("PotentialIncomeValue");
		electricityProduction = inspectorUI.Q("ElectricityProduction");
		electricityProductionValue = inspectorUI.Q<Label>("ElectricityProductionValue");
		advancedMaterialsProduction = inspectorUI.Q("AdvancedMaterialsProduction");
		advancedMaterialProductionValue = inspectorUI.Q<Label>("AdvancedMaterialsProductionValue");
		electricityConsumption = inspectorUI.Q("ElectricityConsumption");
		electricityConsumptionValue = inspectorUI.Q<Label>("ElectricityConsumptionValue");
		upkeep = inspectorUI.Q("Upkeep");
		upkeepValue = inspectorUI.Q<Label>("UpkeepValue");
		connections = inspectorUI.Q("Connections");
		connectionsConsumedValue = inspectorUI.Q<Label>("ConsumedConnectionsValue");
		connectionsTotalValue = inspectorUI.Q<Label>("TotalConnectionsValue");
		specialResources = inspectorUI.Q("SpecialResources");
		settings = inspectorUI.Q("SettingsBackground");
		poweredButton = inspectorUI.Q<Button>("PoweredButton");
		poweredCheckMark = inspectorUI.Q("PoweredCheckMark");
		description = inspectorUI.Q<Label>("Description");

		RegisterListeners();
		UpdateUI();
	}

	private void RegisterListeners()
	{
		poweredButton.RegisterCallback<ClickEvent>(PoweredButton_OnClick);
	}

	private void UnregisterListeners()
	{
		poweredButton.UnregisterCallback<ClickEvent>(PoweredButton_OnClick);
	}

	private void PoweredButton_OnClick(ClickEvent evt)
	{
		displayingBuilding.BackendBuilding.TogglePower();
	}

	public void UpdateUI()
	{
		subheading.text = BuildingName;
		icon.style.backgroundImage = new StyleBackground(BuildingIcon);

		if (BuildingHasIncome)
		{
			incomeCurrentValue.text = "$" + (BuildingIncome * (BuildingIsPowered ? 1 : 0)).ToString("0.00");
			incomePotentialValue.text = BuildingIncome.ToString("0.00");
		}
		else
			income.style.display = DisplayStyle.None;

		if (BuildingProducesPower)
			electricityProductionValue.text = BuildingProducedPower.ToString("0.00");
		else
			electricityProduction.style.display = DisplayStyle.None;

		// TODO: AdvancedMaterials

		if (BuildingConsumesElectricity)
			electricityConsumptionValue.text = BuildingConsumedElectricity.ToString("0.00");
		else
			electricityConsumption.style.display = DisplayStyle.None;

		if (BuildingHasUpkeep)
			upkeepValue.text = BuildingUpkeep.ToString("0.00");
		else
			upkeep.style.display = DisplayStyle.None;

		connectionsConsumedValue.text = BuildingConnectionsConsumed.ToString();
		connectionsTotalValue.text = BuildingConnectionsTotal.ToString();

		if (BuildingConsumesElectricity)
			poweredCheckMark.style.display = BuildingIsPowered ? DisplayStyle.Flex : DisplayStyle.None;
		else
			settings.style.display = DisplayStyle.None;

		description.text = BuildingDescription;
	}

	public void DisconnectInspectorUI()
	{
		UnregisterListeners();
	}
}
