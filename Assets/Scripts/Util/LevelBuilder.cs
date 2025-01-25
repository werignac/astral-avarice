using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
    public string levelName;
    public Vector2 levelDimentions;
    public int startingCash;
    public int goalCash;
    public int levelTime;

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
        missionData.cashGoal = goalCash;
        missionData.timeLimit = levelTime;
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
                building.SetPositionAndUpNormal();
            }
        }    
    }
#endif

}