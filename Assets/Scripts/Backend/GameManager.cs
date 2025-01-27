using UnityEngine;
using System.Collections.Generic;

public class GameManager
{
    private readonly int paymentInterval = 30;

    private List<Building> buildings;
    private int income;
    private int cash;
    private float timePassed;
    private int endTime;
    private GameController controller;
    private MissionData currentMission;

    public int Income
    {
        get { return (income); }
    }
    public int Cash
    {
        get { return (cash); }
    }

    public GameManager(GameController controller)
    {
        this.controller = controller;
    }

    public void SpendMoney(int spentAmount)
    {
        cash -= spentAmount;
        if (controller != null)
        {
            controller.UpdateCashAndIncome(cash, income);
        }
    }

    //Use this if buildings don't need to be instantiated at start.
    public void StartMission(MissionData mission)
    {
        currentMission = mission;
        cash = mission.startingCash;
        buildings = new List<Building>();
        timePassed = 0;
        endTime = mission.timeLimit;
        CalculateIncome();
    }
    //Use this if BuildingComponents need a reference to the Building object being used by the manager.
    public void StartMission(MissionData mission, LevelBuilder level)
    {
        currentMission = mission;
        cash = mission.startingCash;
        buildings = new List<Building>();
        timePassed = 0;
        endTime = mission.timeLimit;

        for (int i = 0; i < level.buildings.Length; ++i)
        {
            Building building = new Building(level.buildings[i].Data);
            buildings.Add(building);
            level.buildings[i].SetGameBuilding(building);
        }

        CalculateIncome();
    }

    public void Update(float deltaTime)
    {
        float oldTime = timePassed;
        timePassed += deltaTime;
        int numPayouts = Mathf.FloorToInt(timePassed / paymentInterval) - Mathf.FloorToInt(oldTime / paymentInterval);
        for(int i = 0; i < numPayouts; ++i)
        {
            cash += income;
            if (controller != null)
            {
                controller.UpdateCashAndIncome(cash, income);
            }
        }
        if(timePassed > endTime)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        if(cash >= currentMission.cashGoal)
        {
            Debug.Log("Earned enough money! Victory!");
        }
        if(controller != null)
        {
            controller.EndGame();
        }
    }

    public void AddBuilding(Building building)
    {
        buildings.Add(building);
        income -= building.Data.upkeep;
        AdjustIncomeForConnected(building);
    }

    public void RemoveBuilding(Building building)
    {
        if(buildings.Remove(building))
        {
            income += building.Data.upkeep;
            for(int i = 0; i < building.NumConnected; ++i)
            {
                building.GetConnectedBuilding(i).RemoveConnection(building);
            }
            for(int i = 0; i < building.NumConnected; ++i)
            {
                AdjustIncomeForConnected(building.GetConnectedBuilding(i));
            }
        }
    }

    public void AddConnection(Building first, Building second)
    {
        first.AddConnection(second);
        second.AddConnection(first);
        AdjustIncomeForConnected(first);
    }

	public void RemoveConnection(Building first, Building second)
	{
		bool removedOne = first.RemoveConnection(second);
		removedOne = second.RemoveConnection(first) || removedOne;
		if (removedOne)
		{
			AdjustIncomeForConnected(first);
			AdjustIncomeForConnected(second);
		}

	}

    public void AdjustIncomeForConnected(Building startingBuilding)
    {
        HashSet<Building> buildingsSeen = new HashSet<Building>();
        List<Building> connectedBuildings = new List<Building>();
        List<Building> connectedConsumers = new List<Building>();
        int totalPower = 0;
        buildingsSeen.Add(startingBuilding);
        connectedBuildings.Add(startingBuilding);
        while (connectedBuildings.Count > 0)
        {
            Building building = connectedBuildings[0];
            connectedBuildings.RemoveAt(0);
            if (building.Data.buildingType == BuildingType.PowerProducer)
            {
                totalPower += building.Data.powerProduced;
                building.PowerToGive = 0;
			}
			if (building.Data.buildingType == BuildingType.PowerConsumer)
			{
				AddConsumerToSortedList(building, connectedConsumers);
			}
			for (int i = 0; i < building.NumConnected; ++i)
            {
                Building connectedBuilding = building.GetConnectedBuilding(i);
                if (!buildingsSeen.Contains(connectedBuilding))
                {
                    buildingsSeen.Add(connectedBuilding);
					connectedBuildings.Add(connectedBuilding);
                }
            }
        }
        foreach (Building consumer in connectedConsumers)
        {
            if (totalPower > consumer.Data.powerRequired)
            {
                totalPower -= consumer.Data.powerRequired;
                if (!consumer.IsPowered)
                {
                    income += consumer.Data.income;
                    consumer.IsPowered = true;
                }
            }
            else
            {
                if (consumer.IsPowered)
                {
                    income -= consumer.Data.income;
                    consumer.IsPowered = false;
                }
            }
        }
        if(controller != null)
        {
            controller.UpdateCashAndIncome(cash, income);
        }
    }

    public void CalculateIncome()
    {
        List<Building> producers = new List<Building>();
        income = 0;

        foreach(Building building in buildings)
        {
            income -= building.Data.upkeep;
            if(building.Data.buildingType == BuildingType.PowerConsumer)
            {
                building.IsPowered = false;
            }
            else if(building.Data.buildingType == BuildingType.PowerProducer)
            {
                building.PowerToGive = building.Data.powerProduced;
                producers.Add(building);
            }
        }

        foreach(Building producer in producers)
        {
            if(producer.PowerToGive > 0)
            {
                AdjustIncomeForConnected(producer);
            }
        }
        if(producers.Count <= 0)
        {
            if (controller != null)
            {
                controller.UpdateCashAndIncome(cash, income);
            }
        }
    }

    private void AddConsumerToSortedList(Building consumerBuilding, List<Building> consumerList)
    {
        float powerToPrice = consumerBuilding.Data.income / consumerBuilding.Data.powerRequired;
        int max = consumerList.Count;
        int min = 0;
        int index = 0;
        while(max > min)
        {
            index = min + ((max - min) / 2);
            bool lessThanPrev = true;
            bool greaterThanNext = true;
            int previousIndex = index - 1;
            int nextIndex = index;
            if(previousIndex >= 0)
            {
                float prevPowerToPrice = consumerList[previousIndex].Data.income / consumerList[previousIndex].Data.powerRequired;
                if(prevPowerToPrice < powerToPrice)
                {
                    lessThanPrev = false;
                }
            }
            if(nextIndex < consumerList.Count)
            {
                float nextPowerToPrice = consumerList[nextIndex].Data.income / consumerList[nextIndex].Data.powerRequired;
                if(nextPowerToPrice > powerToPrice)
                {
                    greaterThanNext = false;
                }
            }
            if(greaterThanNext && lessThanPrev)
            {
                break;
            }
            else if(greaterThanNext)
            {
                max = index;
            }    
            else
            {
                min = index + 1;
            }
        }
        consumerList.Insert(index, consumerBuilding);
    }
}
