using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

/// <summary>
/// A binding for a button displaying a building that can be constructed by the player.
/// Used in the build menu.
/// </summary>
/// <typeparam name="T">The type to use as an identifier to return when a click callback occurs.</typeparam>
public class BuildingButtonBinding<T>: BuildingUIBinding
{
	private class BuildingDisplayData
	{
		public string BuildingName { get; private set; }
		public Sprite BuildingIcon { get; private set; }
		public int BuildingCost { get; private set; }
		public bool BuildingHasScienceCost { get; private set; }
		public int BuildingScienceCost { get; private set; }
		public bool BuildingHasIncome { get; private set; } 
		public int BuildingIncome { get; private set; } 
		public bool BuildingProducesPower { get; private set; } 
		public int BuildingProducedPower { get; private set; } 
		public bool BuildingConsumesElectricity { get; private set; } 
		public int BuildingConsumedElectricity { get; private set; } 
		public bool BuildingHasUpkeep { get; private set; } 
		public int BuildingUpkeep { get; private set; } 
		public int BuildingConnectionsTotal { get; private set; } 
		public int BuildingMass { get; private set; } 
		public string BuildingDescription { get; private set; } 
		public bool BuildingHasScienceIncome { get; private set; }
		public int BuildingScienceIncome { get; private set; }

		public BuildingData BuildingData { get; private set; }

		public BuildingDisplayData(BuildingSettingEntry toDisplay)
		{
			BuildingName = toDisplay.VisualAsset.buildingName;
			BuildingIcon = toDisplay.VisualAsset.buildingIcon;
			BuildingCost = toDisplay.BuildingDataAsset.cost;
			BuildingHasScienceCost = toDisplay.BuildingDataAsset.scienceCost > 0;
			BuildingScienceCost = toDisplay.BuildingDataAsset.scienceCost;
			BuildingHasIncome = toDisplay.BuildingDataAsset.income > 0;
			BuildingIncome = toDisplay.BuildingDataAsset.income;
			BuildingProducesPower = toDisplay.BuildingDataAsset.powerProduced > 0;
			BuildingProducedPower = toDisplay.BuildingDataAsset.powerProduced;
			BuildingConsumesElectricity = toDisplay.BuildingDataAsset.powerRequired > 0;
			BuildingConsumedElectricity = toDisplay.BuildingDataAsset.powerRequired;
			BuildingHasUpkeep = toDisplay.BuildingDataAsset.upkeep > 0;
			BuildingUpkeep = toDisplay.BuildingDataAsset.upkeep;
			BuildingConnectionsTotal = toDisplay.BuildingDataAsset.maxPowerLines;
			BuildingMass = toDisplay.BuildingDataAsset.mass;
			BuildingDescription = toDisplay.VisualAsset.buildingDescription;
			BuildingHasScienceIncome = toDisplay.BuildingDataAsset.scienceIncome > 0;
			BuildingScienceIncome = toDisplay.BuildingDataAsset.scienceIncome;

			BuildingData = toDisplay.BuildingDataAsset;
		}
	}

	public interface ICurrencyCount
	{
		public int Cash { get; }
		public int AdvancedMaterials { get; }
	}

	private class RealtimeBuildingDisplayData : BuildingDisplayData
	{
		private ICurrencyCount currencyCount;

		public RealtimeBuildingDisplayData(BuildingSettingEntry toDisplay, ICurrencyCount currencyCount) :
			base(toDisplay)
		{
			this.currencyCount = currencyCount;
		}

		public bool GetCanAffordCash()
		{
			return currencyCount.Cash >= BuildingCost;
		}

		public bool GetCanAffordAdvancedMaterials()
		{
			return currencyCount.AdvancedMaterials >= BuildingScienceCost;
		}
	}

	private RealtimeBuildingDisplayData displayData;
	protected VisualElement button;
	public T Identifier { get; protected set; }

	public UnityEvent<T> OnHoverStart = new UnityEvent<T>();
	public UnityEvent<T> OnClick = new UnityEvent<T>();
	public UnityEvent<T> OnHoverEnd = new UnityEvent<T>();

	private BuildingCostUIBinding AffordableCost { get => Cost as BuildingCostUIBinding; }
	private BuildingCostUIBinding AffordableAdvancedMaterialsCost { get => AdvancedMaterialsCost as BuildingCostUIBinding; }

	public BuildingButtonBinding(VisualElement button, T identifier) : base(button)
	{
		this.button = button.Q("ButtonContainer");
		this.Identifier = identifier;

		RegisterEvents();
	}

	private void RegisterEvents()
	{
		button.RegisterCallback<ClickEvent>(Button_OnClick);
		button.RegisterCallback<MouseEnterEvent>(Button_OnMouseEnter);
		button.RegisterCallback<MouseLeaveEvent>(Button_OnMouseLeave);
	}

	private void Button_OnMouseLeave(MouseLeaveEvent evt)
	{
		OnHoverEnd?.Invoke(Identifier);
	}

	private void Button_OnMouseEnter(MouseEnterEvent evt)
	{
		OnHoverStart?.Invoke(Identifier);
	}

	private void Button_OnClick(ClickEvent evt)
	{
		OnClick?.Invoke(Identifier);
	}

	// Overrides to display cost affordability.

	protected override BuildingValueUIBinding FindCost(VisualElement rootContainer)
	{
		VisualElement costContainer = rootContainer.Q("CashCostContainer");

		if (costContainer == null)
			return null;

		return new BuildingCostUIBinding(costContainer); 
	}

	protected override BuildingValueUIBinding FindAdvancedMaterialsCost(VisualElement rootContainer)
	{
		VisualElement advMatContainer = rootContainer.Q("ScienceCostContainer");

		if (advMatContainer == null)
			return null;

		return new BuildingCostUIBinding(advMatContainer);
	}

	/// <summary>
	/// Sets up the button to display the provided building, and a refernece to a
	/// class that gets the current currency amounts for updating affordability
	/// in realtime.
	/// </summary>
	/// <param name="building"></param>
	/// <param name="currencyCount"></param>
	public virtual void Display(BuildingSettingEntry building, ICurrencyCount currencyCount)
	{
		displayData = new RealtimeBuildingDisplayData(building, currencyCount);
		SetUI(displayData);
	}

	// Called once after Display is called.
	private void SetUI(BuildingDisplayData displayData)
	{
		SetName(displayData);
		SetIcon(displayData);
		SetCost(displayData);
		SetAdvancedMaterialsCost(displayData);
		SetUpkeep(displayData);
		SetConsumedElectricity(displayData);
		SetSpecialResources(displayData);
		SetIncome(displayData);
		SetElectricityProduction(displayData);
		SetAdvancedMaterialsProduction(displayData);
		SetMass(displayData);
		SetConnections(displayData);

		UpdateUI();
	}

	private void SetName(BuildingDisplayData displayData)
	{
		Title.text = displayData.BuildingName;
	}

	private void SetIcon(BuildingDisplayData displayData)
	{
		Icon.style.backgroundImage = new StyleBackground(displayData.BuildingIcon);
	}

	private void SetCost(BuildingDisplayData displayData)
	{
		Cost.Show();
		Cost.SetValue(displayData.BuildingCost.ToString());
	}

	private void SetAdvancedMaterialsCost(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingHasScienceCost,
			displayData.BuildingScienceCost,
			AdvancedMaterialsCost
		);
	}

	private void SetUpkeep(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingHasUpkeep,
			displayData.BuildingUpkeep,
			Upkeep
		);
	}

	private void SetConsumedElectricity(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingConsumesElectricity,
			displayData.BuildingConsumedElectricity,
			ElectricityConsumption
		);
	}

	private void SetSpecialResources(BuildingDisplayData displayData)
	{
		ResourcesBinding.ShowBuildingTypeResources(displayData.BuildingData);
	}

	private void SetIncome(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingHasIncome,
			displayData.BuildingIncome,
			Income
		);
	}

	private void SetElectricityProduction(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingProducesPower,
			displayData.BuildingProducedPower,
			ElectricityProduction
		);
	}

	private void SetAdvancedMaterialsProduction(BuildingDisplayData displayData)
	{
		IfHasShowValue(
			displayData.BuildingHasScienceIncome,
			displayData.BuildingScienceIncome,
			AdvancedMaterialsProduction
		);
	}

	private void SetMass(BuildingDisplayData displayData)
	{
		Mass.Show();
		Mass.SetValue(displayData.BuildingMass.ToString());
	}

	private void SetConnections(BuildingDisplayData displayData)
	{
		Connections.Show();
		Connections.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
		Connections.SetValue(displayData.BuildingConnectionsTotal.ToString());
	}


	private static void IfHasShowValue(bool has, int value, BuildingValueUIBinding valueBinding)
	{
		IfHasShowValue(has, value.ToString(), valueBinding);
	}

	/// <summary>
	/// Shows and updates a value binding if "has" is true.
	/// </summary>
	/// <param name="has">Whether the UI for this value should be shown.</param>
	/// <param name="value">The new value of the UI.</param>
	/// <param name="valueBinding">The UI binding.</param>
	private static void IfHasShowValue(bool has, string value, BuildingValueUIBinding valueBinding)
	{
		if (has)
		{
			valueBinding.SetDividerMode(BuildingValueUIBinding.DividerStyle.WITHOUT_DIVIDER);
			valueBinding.Show();
			valueBinding.SetValue(value);
		}
		else
			valueBinding.Hide();
	}

	// Called every frame.
	public void UpdateUI()
	{
		UpdateCost(displayData);
		UpdateAdvancedMaterialsCost(displayData);
	}

	private void UpdateCost(RealtimeBuildingDisplayData displayData)
	{
		bool canAfford = displayData.GetCanAffordCash();
		AffordableCost?.SetAffordability(canAfford);
	}

	private void UpdateAdvancedMaterialsCost(RealtimeBuildingDisplayData displayData)
	{
		bool canAfford = displayData.GetCanAffordAdvancedMaterials();
		AffordableAdvancedMaterialsCost?.SetAffordability(canAfford);
	}

	// Called on destruction
	public void CleanUp()
	{
		UnregisterEvents();
	}

	private void UnregisterEvents()
	{
		button.UnregisterCallback<MouseEnterEvent>(Button_OnMouseEnter);
		button.UnregisterCallback<ClickEvent>(Button_OnClick);
		button.UnregisterCallback<MouseLeaveEvent>(Button_OnMouseLeave);
	}
}
