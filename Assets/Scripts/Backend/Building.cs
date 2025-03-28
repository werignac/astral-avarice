using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building
{
    private BuildingData data;
    private List<Building> connectedBuildings;
    private bool isPowered;
    private GameManager manager;

    public BuildingData Data
    {
        get { return (data); }
    }
    public bool IsPowered
    {
        get { return (isPowered); }
        set { isPowered = value; }
    }
    public int NumConnected
    {
        get { return (connectedBuildings.Count); }
    }
    public int PowerToGive
    {
        get;
        set;
    }
    public int ResourcesProvided { get; set; }
    public int BuildingGroup { get; set; }

    public Building(BuildingData data)
    {
        this.data = data;
        connectedBuildings = new List<Building>();
        isPowered = false;
    }

    public Building GetConnectedBuilding(int index)
    {
        if(index < 0 || index >= connectedBuildings.Count)
        {
            return (null);
        }
        return (connectedBuildings[index]);
    }
    public void AddConnection(Building other)
    {
        connectedBuildings.Add(other);
    }
    public bool RemoveConnection(Building other)
    {
        return (connectedBuildings.Remove(other));
    }
    public bool HasConnection(Building other)
    {
        return (connectedBuildings.Contains(other));
    }
    public void SetManager(GameManager m)
    {
        manager = m;
    }

    public bool TogglePower()
    {
        return (manager.ToggleBuildingPower(this));
    }

    public int GetPower()
    {
        if (Data.resourceAmountRequired <= 0)
        {
            return (Data.powerProduced);
        }
        return (Mathf.CeilToInt(((float)ResourcesProvided) / Data.resourceAmountRequired * Data.powerProduced));
    }
}
