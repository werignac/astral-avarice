using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Finds a bunch of UI elements that pertain to a buliding's stats and state.
/// Some of these may be null depending on the UI.
/// </summary>
public class BuildingUIBinding
{
	private const string TOGGLE_ON_CLASS = "toggleOn";
	private const string TOGGLE_OFF_CLASS = "toggleOff";

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
	protected Button PoweredToggleButton { get; set; } = null;
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
	protected virtual string GetPoweredToggleContainerName() => "PoweredToggleContainer";
	protected virtual string GetPoweredToggleButtonName() => "ToggleButton";
	protected virtual string GetDescriptionName() => "Description";

	public BuildingUIBinding() { }

	public BuildingUIBinding(VisualElement rootContainer)
	{
		Bind(rootContainer);
	}

	public void Bind(VisualElement rootContainer)
	{
		Title = FindTitle(rootContainer);
		Icon = FindIcon(rootContainer);
		Cost = FindCost(rootContainer);
		AdvancedMaterialsCost = FindAdvancedMaterialsCost(rootContainer);
		Income = FindIncome(rootContainer);
		ElectricityProduction = FindElectricityProduction(rootContainer);
		AdvancedMaterialsProduction = FindAdvancedMAterialsProduction(rootContainer);
		ElectricityConsumption = FindElectricityConsumption(rootContainer);
		Upkeep = FindUpkeep(rootContainer);
		Connections = FindConnections(rootContainer);
		Mass = FindMass(rootContainer);
		ResourcesBinding = FindSpecialResources(rootContainer);
		Settings = FindSettings(rootContainer);
		PoweredToggleButton = FindPoweredToggleButton(rootContainer);
		Description = FindDescription(rootContainer);
	}

	protected virtual Label FindTitle(VisualElement rootContainer)
	{
		return rootContainer.Q<Label>(GetTitleName());
	}

	protected virtual VisualElement FindIcon(VisualElement rootContainer)
	{
		return rootContainer.Q(GetIconName());
	}

	protected virtual BuildingValueUIBinding FindCost(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetCostName());
	}

	protected virtual BuildingValueUIBinding FindAdvancedMaterialsCost(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetAdvancedMaterialsCostName());
	}

	protected virtual BuildingValueUIBinding FindIncome(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetIncomeName());
	}

	protected virtual BuildingValueUIBinding FindElectricityProduction(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetElectricityProductionName());
	}

	protected virtual BuildingValueUIBinding FindAdvancedMAterialsProduction(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetAdvancedMaterialsProductionName());
	}

	protected virtual BuildingValueUIBinding FindElectricityConsumption(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetElectricityConsumptionName());
	}

	protected virtual BuildingValueUIBinding FindUpkeep(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetUpkeepName());
	}

	protected virtual BuildingValueUIBinding FindConnections(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetConnectionsName());
	}

	protected virtual BuildingValueUIBinding FindMass(VisualElement rootContainer)
	{
		return FindValueContainer(rootContainer, GetMassName());
	}

	protected virtual SpecialResourcesContainerUIBinding FindSpecialResources(VisualElement rootContainer)
	{
		return new SpecialResourcesContainerUIBinding(rootContainer.Q(GetSpecialResourcesContainerName()));
	}

	protected virtual VisualElement FindSettings(VisualElement rootContainer)
	{
		return rootContainer.Q(GetSettingsName());
	}

	protected virtual Button FindPoweredToggleButton(VisualElement rootContainer)
	{
		VisualElement toggleContainer = rootContainer.Q(GetPoweredToggleContainerName());
		if (toggleContainer == null)
			return null;

		return toggleContainer.Q<Button>(GetPoweredToggleButtonName());
	}

	protected virtual Label FindDescription(VisualElement rootContainer)
	{
		return rootContainer.Q<Label>(GetDescriptionName());
	}

	protected static BuildingValueUIBinding FindValueContainer(VisualElement rootContainer, string valueName)
	{
		VisualElement container = rootContainer.Q($"{valueName}Container");

		if (container == null)
			return null;

		return new BuildingValueUIBinding(container, valueName: valueName);
	}

	protected void SetPoweredToggle(bool toggledOn)
	{
		if (toggledOn)
		{
			PoweredToggleButton.AddToClassList(TOGGLE_ON_CLASS);
			PoweredToggleButton.RemoveFromClassList(TOGGLE_OFF_CLASS);
		}
		else
		{
			PoweredToggleButton.RemoveFromClassList(TOGGLE_ON_CLASS);
			PoweredToggleButton.AddToClassList(TOGGLE_OFF_CLASS);
		}
	}
}
