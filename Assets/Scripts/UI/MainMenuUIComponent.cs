using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIComponent : MonoBehaviour
{
    [SerializeField] private DataSet gameData;
    [SerializeField] private UIDocument mainMenuDocument;
    [SerializeField] private UIDocument missionSelectDocument;

    private VisualElement missionsContent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        missionsContent = missionSelectDocument.rootVisualElement.Q("MissionsContent");
        if(missionsContent == null)
        {
            Debug.Log("No missions content found");
        }
        Button missionMenuButton = mainMenuDocument.rootVisualElement.Q("MissionSelectButton") as Button;
        missionMenuButton.RegisterCallback<ClickEvent>(OpenMissionPanel);
        missionSelectDocument.sortingOrder = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateMissions()
    {
        missionsContent.Clear();
        for(int i = 0; i < gameData.missionDatas.Length; ++i)
        {
            MissionData mission = gameData.missionDatas[i];
            Button button = new Button();
            button.text = mission.name;
            button.AddToClassList("missionButton");
            button.RegisterCallback<ClickEvent, MissionData>(StartMission, mission);
            missionsContent.Add(button);
        }
    }

    public void OpenMissionPanel(ClickEvent click)
    {
        missionSelectDocument.sortingOrder = 2;

        PopulateMissions();
    }

    public void StartMission(ClickEvent click, MissionData mission)
    {
        Debug.Log("Start mission " + mission.name);
        Data.selectedMission = mission;
        SceneManager.LoadScene("MainGame");
    }
}
