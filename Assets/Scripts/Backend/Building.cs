using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Building
{
    private BuildingData data;
    private List<Building> connectedBuildings;
    private bool isPowered;
    private GameManager manager;

	private int gridGroup = -1;

	public UnityEvent<int> onGridGroupChanged = new UnityEvent<int>();

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
	/// <summary>
	/// Number of this building's special resources that are being provided by the planet it is on.
	/// </summary>
    public int ResourcesProvided { get; set; }
    public int BuildingGroup {
		get => gridGroup;
		set
		{
			bool changed = value != gridGroup;

			if (!changed)
				return;

			gridGroup = value;
			onGridGroupChanged.Invoke(value);
		}
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
    public bool HasConnection(Building other)
    {
        return (connectedBuildings.Contains(other));
    }
	/// <summary>
	/// Returns whether this building can accept more power lines / cables.
	/// </summary>
	public bool CanAcceptNewConnections()
	{
		return NumConnected < data.maxPowerLines;
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
