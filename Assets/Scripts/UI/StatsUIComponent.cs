using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class StatsUIComponent : MonoBehaviour
{
	private UIDocument statsDocument;

	private Button incrementGameSpeedButton;
	private Button decrementGameSpeedButton;
	private Button redistributeElectricityButton;

	[SerializeField] private GameController gameController;

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

		incrementGameSpeedButton.RegisterCallback<ClickEvent>(IncrementTimeButton_OnClick);
		decrementGameSpeedButton.RegisterCallback<ClickEvent>(DecrementTimeButton_OnClick);
		redistributeElectricityButton.RegisterCallback<ClickEvent>(RedistributeElectricityButton_OnClick);
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
