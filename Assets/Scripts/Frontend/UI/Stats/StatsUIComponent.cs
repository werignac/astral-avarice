using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.Frontend;

[RequireComponent(typeof(UIDocument))]
public class StatsUIComponent : MonoBehaviour
{
	private const string INCOME_PIE_CHART_CONTAINER_ELEMENT_NAME = "IncomePieChartContainer";

	private UIDocument statsDocument;

	private Button incrementGameSpeedButton;
	private Button decrementGameSpeedButton;
	private Button redistributeElectricityButton;
	private Button pauseButtion;
	private VisualElement rankImage;
	private Label rankTime;

	[SerializeField] private GameController gameController;
	[SerializeField] private PauseManager pauseManager;
	[SerializeField] private Sprite[] missionRankSprites;

	private IncomePieChartController pieChartController = new IncomePieChartController();

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
		rankImage = statsDocument.rootVisualElement.Q("RankImage");
		rankTime = statsDocument.rootVisualElement.Q<Label>("RankTime");

		incrementGameSpeedButton.RegisterCallback<ClickEvent>(IncrementTimeButton_OnClick);
		decrementGameSpeedButton.RegisterCallback<ClickEvent>(DecrementTimeButton_OnClick);
		redistributeElectricityButton.RegisterCallback<ClickEvent>(RedistributeElectricityButton_OnClick);
		pauseButtion.RegisterCallback<ClickEvent>(PauseButton_OnClick);

		pieChartController.Bind(statsDocument.rootVisualElement.Q(INCOME_PIE_CHART_CONTAINER_ELEMENT_NAME));
	}

    private void Update()
    {
		pieChartController.SetIncomeAndGoal(gameController.Income, gameController.TargetIncome);

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
