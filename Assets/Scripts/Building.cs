using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building
{
    private BuildingData data;
    private List<Building> connectedBuildings;
    private bool isPowered;

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
}
