using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Data/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int cost;
    public int scienceCost;
    public int mass;
    public int maxPowerLines;
    public BuildingType buildingType;
    public int powerProduced;
    public int powerRequired;
    public int income;
    public int upkeep;
    public int scienceIncome;
    public ResourceType requiredResource;
    public int resourceAmountRequired;
    public GameObject buildingPrefab;

    public int TotalIncome
    {
        get { return (income + scienceIncome); }
    }
}
