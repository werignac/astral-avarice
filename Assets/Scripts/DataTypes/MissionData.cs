using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Data/MissionData")]
public class MissionData : ScriptableObject
{
    public int startingCash;
    public int cashGoal;
    public int[] goalTimes;
    public string missionName;
    public int startingScience;
    public BuildingData[] startingBuildings;
    public string tutorialScene;
}
