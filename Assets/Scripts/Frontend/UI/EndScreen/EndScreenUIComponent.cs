using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
    public class EndScreenUIComponent : MonoBehaviour
    {
		private const string MAIN_MENU_BUTTON_ELEMENT_NAME = "MainMenuButton";
		private const string PLAY_AGAIN_BUTTON_ELEMENT_NAME = "PlayAgainButton";
		private const string TIME_BAR_ELEMENT_NAME = "TimeBar";


		[SerializeField] private UIDocument uiDocument;

		[HideInInspector] public UnityEvent OnMainMenuButtonClicked = new UnityEvent();
		[HideInInspector] public UnityEvent OnPlayAgainButtonClicked = new UnityEvent();

		private Button mainMenuButtonElement;
		private Button playAgainButtonElement;
		private TimeBarBinding timeBar;

		public void Initialize(int[] missionRankTimes)
		{
			mainMenuButtonElement = uiDocument.rootVisualElement.Q<Button>(MAIN_MENU_BUTTON_ELEMENT_NAME);
			playAgainButtonElement = uiDocument.rootVisualElement.Q<Button>(PLAY_AGAIN_BUTTON_ELEMENT_NAME);

			mainMenuButtonElement.RegisterCallback<ClickEvent>(MainMenuButton_OnClick);
			playAgainButtonElement.RegisterCallback<ClickEvent>(PlayAgainButton_OnClick);

			// Assumes the first time is the shortest.
			int shortestTime = missionRankTimes[0];

			// Assumes the last time is the longest.
			int longestTime = missionRankTimes[missionRankTimes.Length - 1];

			// Add more time for the D rank.
			int longestTimeDisplay = Mathf.CeilToInt(shortestTime + (longestTime - shortestTime) * 1.3f);

			timeBar = new TimeBarBinding(uiDocument.rootVisualElement.Q(TIME_BAR_ELEMENT_NAME),
				shortestTime,
				longestTimeDisplay);

			RankUIData[] rankSettings = PtUUISettings.GetOrCreateSettings().RankSettings;

			// Don't include x-rank on the time bar.
			for (int i = rankSettings.Length - 1; i >= 1; i--)
			{
				int rankTimeIndex = rankSettings.Length - i - 1;

				if (rankTimeIndex > missionRankTimes.Length)
				{
					timeBar.HideTick(i);
					continue;
				}

				// Last rank is handled differently from the other ranks.
				if (rankTimeIndex == missionRankTimes.Length)
				{
					timeBar.SetTick(i, -1);
					continue;
				}

				int rankTime = missionRankTimes[rankTimeIndex];
				timeBar.SetTick(i, rankTime);
			}
		}

		private void PlayAgainButton_OnClick(ClickEvent evt)
		{
			OnPlayAgainButtonClicked.Invoke();
		}

		private void MainMenuButton_OnClick(ClickEvent evt)
		{
			OnMainMenuButtonClicked.Invoke();
		}

		public void Show()
		{
			uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		}

		public void Hide()
		{
			uiDocument.rootVisualElement.style.display = DisplayStyle.None;
		}
    }
}
