using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Finds a bunch of UI elements that pertain to a buliding's stats and state.
/// Some of these may be null depending on the UI.
/// </summary>
public class BuildingUIBinding
{
	protected Label Title { get; set; } = null;
	protected VisualElement Icon { get; set; } = null;
	protected BuildingValueUIBinding Cost { get; set; } = null;
	protected BuildingValueUIBinding AdvancedMaterialsCost { get; set; } = null;
	protected BuildingValueUIBinding Income { get; set; } = null;
	protected BuildingValueUIBinding ElectricityProduction { get; set; } = null;
	protected BuildingValueUIBinding AdvancedMaterialsProduction { get; set; } = null;
	protected BuildingValueUIBinding ElectricityConsumption { get; set; } = null;
	protected BuildingValueUIBinding Upkeep { get; set; } = null;
	protected BuildingValueUIBinding Connections { get; set; } = null;
	protected BuildingValueUIBinding Mass { get; set; } = null;
	protected SpecialResourcesContainerUIBinding ResourcesBinding { get; set; } = null;
	protected VisualElement Settings { get; set; } = null;
	protected Button PoweredButton { get; set; } = null;
	protected VisualElement PoweredCheckMark { get; set; } = null;
	protected Label Description { get; set; } = null;

	// These names can be overriden for UI that uses different names.
	protected virtual string GetTitleName() => "Title";
	protected virtual string GetIconName() => "Icon";
	protected virtual string GetCostName() => "Cost";
	protected virtual string GetAdvancedMaterialsCostName() => "AdvancedMaterialsCost";
	protected virtual string GetIncomeName() => "Income";
	protected virtual string GetElectricityProductionName() => "ElectricityProduction";
	protected virtual string GetAdvancedMaterialsProductionName() => "AdvancedMaterialsProduction";
	protected virtual string GetElectricityConsumptionName() => "ElectricityConsumption";
	protected virtual string GetUpkeepName() => "Upkeep";
	protected virtual string GetConnectionsName() => "Connections";
	protected virtual string GetMassName() => "Mass";
	protected virtual string GetSpecialResourcesContainerName() => "SpecialResourcesContainer";
	protected virtual string GetSettingsName() => "SettingsBackground";
	protected virtual string GetPoweredButtonName() => "PoweredButton";
	protected virtual string GetPoweredCheckMarkName() => "PoweredCheckMark";
	protected virtual string GetDescriptionName() => "Description";

	public BuildingUIBinding() { }

	public BuildingUIBinding(VisualElement rootContainer)
	{
		Bind(rootContainer);
	}

	public void Bind(VisualElement rootContainer)
	{
		Title = rootContainer.Q<Label>(GetTitleName());
		Icon = rootContainer.Q(GetIconName());
		Cost = FindValueContainer(rootContainer, GetCostName());
		AdvancedMaterialsCost = FindValueContainer(rootContainer, GetAdvancedMaterialsCostName());
		Income = FindValueContainer(rootContainer, GetIncomeName());
		ElectricityProduction = FindValueContainer(rootContainer, GetElectricityProductionName());
		AdvancedMaterialsProduction = FindValueContainer(rootContainer, GetAdvancedMaterialsProductionName());
		ElectricityConsumption = FindValueContainer(rootContainer, GetElectricityConsumptionName());
		Upkeep = FindValueContainer(rootContainer, GetUpkeepName());
		Connections = FindValueContainer(rootContainer, GetConnectionsName());
		Mass = FindValueContainer(rootContainer, GetMassName());
		ResourcesBinding = new SpecialResourcesContainerUIBinding(rootContainer.Q(GetSpecialResourcesContainerName()));
		Settings = rootContainer.Q(GetSettingsName());
		PoweredButton = rootContainer.Q<Button>(GetPoweredButtonName());
		PoweredCheckMark = rootContainer.Q(GetPoweredCheckMarkName());
		Description = rootContainer.Q<Label>(GetDescriptionName());
	}

	protected static BuildingValueUIBinding FindValueContainer(VisualElement rootContainer, string valueName)
	{
		VisualElement container = rootContainer.Q($"{valueName}Container");

		if (container == null)
			return null;

		return new BuildingValueUIBinding(container, valueName: valueName);
	}
}
