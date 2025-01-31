using UnityEngine;
using UnityEngine.UIElements;

public class BuildingButtonInspectorController : BuildingInspectorController
{
	private BuildingSettingEntry buildSettings;

	private string BuildingName
	{
		get => buildSettings.VisualAsset.buildingName;
	}

	private Sprite BuildingIcon
	{
		get => buildSettings.VisualAsset.buildingIcon;
	}

	private float BuildingCost
	{
		get => buildSettings.BuildingDataAsset.cost;
	}

	private int BuildingScienceCost
	{
		get => buildSettings.BuildingDataAsset.scienceCost;
	}

	private bool BuildingHasIncome
	{
		get => buildSettings.BuildingDataAsset.income > 0;
	}

	private float BuildingIncome
	{
		get => buildSettings.BuildingDataAsset.income;
	}

	private bool BuildingProducesPower
	{
		get => buildSettings.BuildingDataAsset.powerProduced > 0;
	}

	private float BuildingProducedPower
	{
		get => buildSettings.BuildingDataAsset.powerProduced;
	}

	private bool BuildingConsumesElectricity
	{
		get => buildSettings.BuildingDataAsset.powerRequired > 0;
	}

	private float BuildingConsumedElectricity
	{
		get => buildSettings.BuildingDataAsset.powerRequired;
	}

	private bool BuildingHasUpkeep
	{
		get => buildSettings.BuildingDataAsset.upkeep > 0;
	}

	private float BuildingUpkeep
	{
		get => buildSettings.BuildingDataAsset.upkeep;
	}

	private int BuildingConnectionsTotal
	{
		get => buildSettings.BuildingDataAsset.maxPowerLines;
	}

	private float BuildingMass
	{
		get => buildSettings.BuildingDataAsset.mass;
	}

	private string BuildingDescription
	{
		get => buildSettings.VisualAsset.buildingDescription;
	}

	private int BuildingScienceIncome
	{
		get => buildSettings.BuildingDataAsset.scienceIncome;
	}
	private ResourceType BuildingRequiredResource
	{
		get => buildSettings.BuildingDataAsset.requiredResource;
	}
	private int BuildingResourcesAmountRequired
	{
		get => buildSettings.BuildingDataAsset.resourceAmountRequired;
	}

	public BuildingButtonInspectorController(BuildingSettingEntry toDisplay)
	{
		buildSettings = toDisplay;
	}

	public override void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		base.ConnectInspectorUI(inspectorUI);
	}

	public override void UpdateUI()
	{
		subheading.text = BuildingName;
		icon.style.backgroundImage = new StyleBackground(BuildingIcon);

		costValue.text = "$" + BuildingCost.ToString("0.00");

		if (BuildingScienceCost > 0)
		{
			advancedMaterialsCostValue.text = BuildingScienceCost.ToString();
		}
		else
		{
			advancedMaterialsCost.style.display = DisplayStyle.None;
		}

		if (BuildingHasIncome)
		{
			incomeCurrentValue.style.display = DisplayStyle.None;
			incomeDivider.style.display = DisplayStyle.None;
			incomePotentialValue.text = "$" + BuildingIncome.ToString("0.00");
		}
		else
			income.style.display = DisplayStyle.None;

		if (BuildingProducesPower)
			electricityProductionValue.text = BuildingProducedPower.ToString("0.00");
		else
			electricityProduction.style.display = DisplayStyle.None;

		if(BuildingScienceIncome > 0)
		{
			advancedMaterialProductionValue.text = BuildingScienceIncome.ToString();
		}
		else
		{
			advancedMaterialsProduction.style.display = DisplayStyle.None;
		}

		if (BuildingConsumesElectricity)
			electricityConsumptionValue.text = BuildingConsumedElectricity.ToString("0.00");
		else
			electricityConsumption.style.display = DisplayStyle.None;

		if (BuildingHasUpkeep)
			upkeepValue.text = BuildingUpkeep.ToString("0.00");
		else
			upkeep.style.display = DisplayStyle.None;

		connectionsConsumedValue.style.display = DisplayStyle.None;
		connectionsDivider.style.display = DisplayStyle.None;
		connectionsTotalValue.text = BuildingConnectionsTotal.ToString();

		massValue.text = BuildingMass.ToString("0.00");

		if(BuildingRequiredResource != ResourceType.Resource_Count)
		{
			specialResourcesLabel.text = BuildingRequiredResource.ToString() + " required:";
			resourcesCurrentValue.text = BuildingResourcesAmountRequired.ToString();
			resourcesDivider.style.display = DisplayStyle.None;
			resourcesPotentialValue.style.display = DisplayStyle.None;
		}
		else
		{
			specialResources.style.display = DisplayStyle.None;
		}

		settings.style.display = DisplayStyle.None;

		description.text = BuildingDescription;
	}

	public override void DisconnectInspectorUI()
	{
	}
}
