using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class StatsUIComponent : MonoBehaviour
{
	private UIDocument statsDocument;

	private Button incrementGameSpeedButton;
	private Button decrementGameSpeedButton;
	private Button redistributeElectricityButton;
	private Button pauseButtion;
	private ProgressBar incomeProgress;
	private VisualElement rankImage;
	private Label rankTime;

	[SerializeField] private GameController gameController;
	[SerializeField] private PauseManager pauseManager;
	[SerializeField] private Sprite[] missionRankSprites;

	private void Awake()
	{
		statsDocument = GetComponent<UIDocument>();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		incrementGameSpeedButton = statsDocument.rootVisualElement.Q<Button>("IncreaseButton");
		decrementGameSpeedButton = statsDocument.rootVisualElement.Q<Button>("DecreaseButton");
		redistributeElectricityButton = statsDocument.rootVisualElement.Q<Button>("RedistributeButton");
		pauseButtion = statsDocument.rootVisualElement.Q<Button>("PauseButton");
		incomeProgress = statsDocument.rootVisualElement.Q<ProgressBar>("IncomeProgress");
		rankImage = statsDocument.rootVisualElement.Q("RankImage");
		rankTime = statsDocument.rootVisualElement.Q<Label>("RankTime");

		incrementGameSpeedButton.RegisterCallback<ClickEvent>(IncrementTimeButton_OnClick);
		decrementGameSpeedButton.RegisterCallback<ClickEvent>(DecrementTimeButton_OnClick);
		redistributeElectricityButton.RegisterCallback<ClickEvent>(RedistributeElectricityButton_OnClick);
		pauseButtion.RegisterCallback<ClickEvent>(PauseButton_OnClick);
    }

    private void Update()
    {
		incomeProgress.title = "Income: " + gameController.Income + "/" + gameController.TargetIncome;
		incomeProgress.value = (gameController.Income * 100) / gameController.TargetIncome;

		int rank = gameController.GetRank();
		string timeText = "(";
        if (rank == 0)
        {
			timeText = "WARNING!";
			rankTime.style.color = Color.red;
        }
		else if(rank == 1)
        {
			if (gameController.Winning)
			{
				rankTime.style.color = Color.green;
			}
			else
			{
				rankTime.style.color = Color.white;
			}
            timeText += "--:--)";
		}
		else
        {
            if (gameController.Winning)
            {
                rankTime.style.color = Color.green;
            }
            else
            {
                rankTime.style.color = Color.white;
            }
            int time = Data.selectedMission.goalTimes[5 - rank];
            timeText += Mathf.FloorToInt(time / 60).ToString("00");
            timeText += ":" + Mathf.FloorToInt((time % 60)).ToString("00");
			timeText += ")";
        }
        rankTime.text = timeText;


        rankImage.style.display = DisplayStyle.Flex;
        rankImage.style.backgroundImage = new StyleBackground(missionRankSprites[rank]);
        rankImage.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleToFit);
    }

    private void PauseButton_OnClick(ClickEvent evt)
	{
		pauseManager.PauseGame();
	}

	private void RedistributeElectricityButton_OnClick(ClickEvent evt)
	{
		gameController.RecomputeIncome();
	}

	private void DecrementTimeButton_OnClick(ClickEvent evt)
	{
		gameController.DecrementGameSpeed();
	}

	private void IncrementTimeButton_OnClick(ClickEvent evt)
	{
		gameController.IncrementGameSpeed();
	}
}
