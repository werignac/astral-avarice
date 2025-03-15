
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Save Level"))
        {
            ((LevelBuilder)target).SaveLevel();
        }
        if(GUILayout.Button("Orient Buildings"))
        {
            ((LevelBuilder)target).OrientBuildingChildren();
        }
        if (GUILayout.Button("Assign Building Planets"))
        {
            ((LevelBuilder)target).AssignBuildingsParentPlanets();
        }
        if (GUILayout.Button("Find Buildings"))
        {
            ((LevelBuilder)target).FindBuildingComponents();
        }
        if(GUILayout.Button("Create All Cables"))
        {
            ((LevelBuilder)target).CreateAllInRangeCables();
        }
        if (GUILayout.Button("UpdateCableLines"))
        {
            ((LevelBuilder)target).UpdateCableLines();
        }
    }
}
#endif