using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIComponent : MonoBehaviour
{
    [SerializeField] private DataSet gameData;
    [SerializeField] private UIDocument mainMenuDocument;
    [SerializeField] private UIDocument missionSelectDocument;
    [SerializeField] private UIDocument settingsDocument;
    [SerializeField] private UIDocument creditsDocument;
    [SerializeField] private AudioMixer audioMixer;

    private VisualElement missionsContent;
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;

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

        Button settingsButton = mainMenuDocument.rootVisualElement.Q<Button>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(OpenSettingsMenu);

        Button creditsButton = mainMenuDocument.rootVisualElement.Q<Button>("CreditsButton");
        creditsButton.RegisterCallback<ClickEvent>(OpenCreditsPage);

		Button missionBackButton = missionSelectDocument.rootVisualElement.Q<Button>("BackButton");
		missionBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        Button settingsBackButton = settingsDocument.rootVisualElement.Q<Button>("BackButton");
        settingsBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        Button creditsBackButton = creditsDocument.rootVisualElement.Q<Button>("BackButton");
        creditsBackButton.RegisterCallback<ClickEvent>(OpenMainMenu);

        masterVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("MasterSlider");
        float masterVolume;
        audioMixer.GetFloat("Master", out masterVolume);
        masterVolumeSlider.value = masterVolume;
        masterVolumeSlider.RegisterValueChangedCallback<float>(MasterVolumeChanged);

        musicVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("MusicSlider");
        float musicVolume;
        audioMixer.GetFloat("BGM", out musicVolume);
        musicVolumeSlider.value = musicVolume;
        musicVolumeSlider.RegisterValueChangedCallback<float>(MusicVolumeChanged);

        sfxVolumeSlider = settingsDocument.rootVisualElement.Q<Slider>("SFXSlider");
        float sfxVolume;
        audioMixer.GetFloat("SFX", out sfxVolume);
        sfxVolumeSlider.value = sfxVolume;
        sfxVolumeSlider.RegisterValueChangedCallback<float>(SFXVolumeChanged);

        missionSelectDocument.sortingOrder = 0;
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.None;

        settingsDocument.rootVisualElement.style.display = DisplayStyle.None;
        creditsDocument.rootVisualElement.style.display = DisplayStyle.None;
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
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
		
		missionSelectDocument.sortingOrder = 2;

        PopulateMissions();
    }

    public void StartMission(ClickEvent click, MissionData mission)
    {
        Debug.Log("Start mission " + mission.name);
        Data.selectedMission = mission;
        if(mission.tutorialScene != "")
        {
            SceneManager.LoadScene(mission.tutorialScene);
        }
        else
        {
            SceneManager.LoadScene("MainGame");
        }
    }

	private void OpenMainMenu(ClickEvent click)
	{
		mainMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		missionSelectDocument.rootVisualElement.style.display = DisplayStyle.None;
        settingsDocument.rootVisualElement.style.display = DisplayStyle.None;
        creditsDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void OpenSettingsMenu(ClickEvent click)
    {
        settingsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OpenCreditsPage(ClickEvent click)
    {
        creditsDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        mainMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void MasterVolumeChanged(ChangeEvent<float> newValue)
    {
        if (newValue.newValue < -29f)
        {
            audioMixer.SetFloat("Master", -80);
        }
        else
        {
            audioMixer.SetFloat("Master", newValue.newValue);
        }
    }

    private void MusicVolumeChanged(ChangeEvent<float> newValue)
    {
        if (newValue.newValue < -49f)
        {
            audioMixer.SetFloat("BGM", -80);
        }
        else
        {
            audioMixer.SetFloat("BGM", newValue.newValue);
        }
    }

    private void SFXVolumeChanged(ChangeEvent<float> newValue)
    {
        if (newValue.newValue < -49f)
        {
            audioMixer.SetFloat("SFX", -80);
        }
        else
        {
            audioMixer.SetFloat("SFX", newValue.newValue);
        }
    }
}
