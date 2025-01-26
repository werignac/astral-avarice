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

    //Use this if BuildingComponents don't need to have a Building object as a member.
    public void StartMission(MissionData mission)
    {
        cash = 0;
        buildings = new List<Building>();
        timePassed = 0;
        endTime = mission.timeLimit;

        for(int i = 0; i < mission.startingBuildings.Length; ++i)
        {
            buildings.Add(new Building(mission.startingBuildings[i]));
        }

        CalculateIncome();
    }
    //Use this if BuildingComponents need a reference to the Building object being used by the manager.
    public void StartMission(MissionData mission, LevelBuilder level)
    {
        cash = 0;
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
        }
        if(timePassed > endTime)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
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
            for (int i = 0; i < building.NumConnected; ++i)
            {
                Building connectedBuilding = building.GetConnectedBuilding(i);
                if (!buildingsSeen.Contains(connectedBuilding))
                {
                    buildingsSeen.Add(connectedBuilding);
                    if (connectedBuilding.Data.buildingType == BuildingType.PowerConsumer)
                    {
                        AddConsumerToSortedList(connectedBuilding, connectedConsumers);
                    }
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
                float nextPowerToPrice = consumerList[previousIndex].Data.income / consumerList[previousIndex].Data.powerRequired;
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
