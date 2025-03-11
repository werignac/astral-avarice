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
    public bool hasPrereq;
    public string prereqMission;
    public int prereqRank;

    public int GetRank(float time)
    {
        if(time < 0)
        {
            return (0);
        }
        int rank = 5;
        for (int i = 0; i < goalTimes.Length; ++i)
        {
            if (time > goalTimes[i])
            {
                --rank;
            }
            else
            {
                break;
            }
        }
        return (rank);
    }
}
