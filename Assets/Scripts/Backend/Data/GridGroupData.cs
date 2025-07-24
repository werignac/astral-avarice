using UnityEngine;
using System.Collections.Generic;

public struct GridGroupData
{
	public int GroupId { get; private set; }
	public int TotalPowerProduced { get; private set; }
	public int TotalPowerConsumed { get; private set; }
	public int Maintenace { get; private set; }
	public int CashIncome { get; private set; }
	public int AdvancedMaterialsIncome { get; private set; }

	public GridGroupData(IEnumerable<Building> buildings)
	{
		GroupId = -1;
		TotalPowerProduced = 0;
		TotalPowerConsumed = 0;
		Maintenace = 0;
		CashIncome = 0;
		AdvancedMaterialsIncome = 0;

		foreach (Building building in buildings)
			RecordBuilding(building);
	}

	public void RecordBuilding(Building building)
	{
		if (GroupId == -1)
			GroupId = building.BuildingGroup;

		TotalPowerProduced += building.GetPower();
		TotalPowerConsumed += building.Data.powerRequired;
		Maintenace += building.Data.upkeep;
		// TODO: Create functions for this to get the amount of income.
		CashIncome += building.IsPowered ? building.Data.income : 0;
		AdvancedMaterialsIncome += building.IsPowered ? building.Data.scienceIncome : 0;
	}
}
