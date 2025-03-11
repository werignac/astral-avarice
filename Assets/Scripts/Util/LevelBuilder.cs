using UnityEngine;
using UnityEditor;
using werignac.Utils;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
    public string levelName;
    public Vector2 levelDimentions;
    public int startingCash;
    public int startingScience;
    public int goalCash;
    public int[] goalTimes;
    public bool hasPrereq;
    public string prereqMission;
    public int prereqRank;
    public BuildingComponent[] buildings;

    public void Update()
    {
        Debug.DrawLine(transform.position + (new Vector3(levelDimentions.x / -2, levelDimentions.y / -2, 0)), transform.position + (new Vector3(levelDimentions.x / 2, levelDimentions.y / -2, 0)));
        Debug.DrawLine(transform.position + (new Vector3(levelDimentions.x / 2, levelDimentions.y / -2, 0)), transform.position + (new Vector3(levelDimentions.x / 2, levelDimentions.y / 2, 0)));
        Debug.DrawLine(transform.position + (new Vector3(levelDimentions.x / 2, levelDimentions.y / 2, 0)), transform.position + (new Vector3(levelDimentions.x / -2, levelDimentions.y / 2, 0)));
        Debug.DrawLine(transform.position + (new Vector3(levelDimentions.x / -2, levelDimentions.y / 2, 0)), transform.position + (new Vector3(levelDimentions.x / -2, levelDimentions.y / -2, 0)));
        gameObject.name = levelName;
    }

#if UNITY_EDITOR
    public void SaveLevel()
    {
        PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/Resources/Levels/" + levelName + ".prefab");
        MissionData missionData = (MissionData)MissionData.CreateInstance("MissionData");
        missionData.missionName = levelName;
        missionData.startingCash = startingCash;
        missionData.startingScience = startingScience;
        missionData.cashGoal = goalCash;
        missionData.goalTimes = goalTimes;
        missionData.hasPrereq = hasPrereq;
        missionData.prereqMission = prereqMission;
        missionData.prereqRank = prereqRank;

        buildings = GameObject.FindObjectsByType<BuildingComponent>(FindObjectsSortMode.None);
        missionData.startingBuildings = new BuildingData[buildings.Length];
        for(int i = 0; i < buildings.Length; ++i)
        {
            missionData.startingBuildings[i] = buildings[i].Data;
        }

        AssetDatabase.CreateAsset(missionData, "Assets/ScriptableObjects/Missions/" + levelName + ".asset");
        AssetDatabase.SaveAssets();
    }

    public void OrientBuildingChildren()
    {
        for(int i = 0; i < transform.childCount; ++i)
        {
            BuildingComponent building = transform.GetChild(i).gameObject.GetComponent<BuildingComponent>();
            if(building != null)
            {
                if (building.SetPositionAndUpNormal())
                {
                    --i;
                }

				if (!building.TryGetComponentInParent(out PlanetComponent _))
				{
					Debug.LogError("Failed to parent building to planet. Is the root prefab unpacked?");
					i++;
				}
            }
        }    
    }

    public void FindBuildingComponents()
    {
        buildings = GameObject.FindObjectsByType<BuildingComponent>(FindObjectsSortMode.None);
    }
#endif

}