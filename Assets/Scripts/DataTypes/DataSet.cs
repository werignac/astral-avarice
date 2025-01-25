using UnityEngine;

[CreateAssetMenu(fileName = "DataSet", menuName = "Data/DataSet")]
public class DataSet : ScriptableObject
{
    public BuildingData[] buildingDatas;
    public MissionData[] missionDatas;
}
