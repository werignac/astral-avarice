using UnityEngine;
using UnityEngine.UIElements;

public class BuildingInstanceInspectorController : BuildingInspectorController
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

	private float BuildingMass
	{
		get => displayingBuilding.Data.mass;
	}

	private bool BuildingIsPowered
	{
		get => displayingBuilding.BackendBuilding.IsPowered;
    }

    private int BuildingScienceIncome
    {
        get => displayingBuilding.Data.scienceIncome;
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

	public BuildingInstanceInspectorController(BuildingComponent toDisplay)
	{
		displayingBuilding = toDisplay;
	}

	public override void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		base.ConnectInspectorUI(inspectorUI);

		RegisterListeners();
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

	public override void UpdateUI()
	{
		subheading.text = BuildingName;
		icon.style.backgroundImage = new StyleBackground(BuildingIcon);

		cost.style.display = DisplayStyle.None;
		advancedMaterialsCost.style.display = DisplayStyle.None;

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



        if (BuildingScienceIncome > 0)
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

		connectionsConsumedValue.text = BuildingConnectionsConsumed.ToString();
		connectionsTotalValue.text = BuildingConnectionsTotal.ToString();

		massValue.text = BuildingMass.ToString("0.00");

		resourcesBinding.ShowBuildingInstanceResources(displayingBuilding);

        if (BuildingConsumesElectricity)
			poweredCheckMark.style.display = BuildingIsPowered ? DisplayStyle.Flex : DisplayStyle.None;
		else
			settings.style.display = DisplayStyle.None;

		description.text = BuildingDescription;
	}

	public override void DisconnectInspectorUI()
	{
		UnregisterListeners();
	}

}
