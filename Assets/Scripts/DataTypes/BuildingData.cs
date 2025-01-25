using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Data/BuildingData")]
public class BuildingData : ScriptableObject
{
    public int cost;
    public int mass;
    public int maxPowerLines;
    public BuildingType buildingType;
    public int powerProduced;
    public int powerRequired;
    public int income;
    public int upkeep;
    public ResourceType requiredResource;
    public int resourceAmountRequired;
    public GameObject buildingPrefab;
}
