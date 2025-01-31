using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BuildingInspectorController : IInspectorController
{
	protected Label subheading;
	protected VisualElement icon;
	protected VisualElement cost;
	protected Label costValue;
	protected VisualElement advancedMaterialsCost;
	protected Label advancedMaterialsCostValue;
	protected VisualElement income;
	protected Label incomeCurrentValue;
	protected VisualElement incomeDivider;
	protected Label incomePotentialValue;
	protected VisualElement electricityProduction;
	protected Label electricityProductionValue;
	protected VisualElement advancedMaterialsProduction;
	protected Label advancedMaterialProductionValue;
	protected VisualElement electricityConsumption;
	protected Label electricityConsumptionValue;
	protected VisualElement upkeep;
	protected Label upkeepValue;
	protected VisualElement connections;
	protected Label connectionsConsumedValue;
	protected VisualElement connectionsDivider;
	protected Label connectionsTotalValue;
	protected VisualElement mass;
	protected Label massValue;
	protected VisualElement specialResources;
	protected Label specialResourcesLabel;
	protected Label resourcesCurrentValue;
	protected VisualElement resourcesDivider;
	protected Label resourcesPotentialValue;
	protected VisualElement settings;
	protected Button poweredButton;
	protected VisualElement poweredCheckMark;
	protected Label description;

	public virtual void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		subheading = inspectorUI.Q<Label>("Subheading");
		icon = inspectorUI.Q("Icon");
		cost = inspectorUI.Q("Cost");
		costValue = inspectorUI.Q<Label>("CostValue");
		advancedMaterialsCost = inspectorUI.Q("AdvancedMaterialsCost");
		advancedMaterialsCostValue = inspectorUI.Q<Label>("AdvancedMaterialsCostValue");
		income = inspectorUI.Q("Income");
		incomeCurrentValue = inspectorUI.Q<Label>("CurrentIncomeValue");
		incomeDivider = inspectorUI.Q("IncomeDivider");
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
		connectionsDivider = inspectorUI.Q("ConnectionDivider");
		connectionsTotalValue = inspectorUI.Q<Label>("TotalConnectionsValue");
		mass = inspectorUI.Q("Mass");
		massValue = inspectorUI.Q<Label>("MassValue");
		specialResources = inspectorUI.Q("SpecialResources");
		specialResourcesLabel = inspectorUI.Q<Label>("SpecialResourcesLabel");
		resourcesCurrentValue = inspectorUI.Q<Label>("CurrentResourcesValue");
		resourcesDivider = inspectorUI.Q("ResourcesDivider");
		resourcesPotentialValue = inspectorUI.Q<Label>("PotentialResourcesValue");
		settings = inspectorUI.Q("SettingsBackground");
		poweredButton = inspectorUI.Q<Button>("PoweredButton");
		poweredCheckMark = inspectorUI.Q("PoweredCheckMark");
		description = inspectorUI.Q<Label>("Description");
	}

	public abstract void DisconnectInspectorUI();
	public abstract void UpdateUI();
}
