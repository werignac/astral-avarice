using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.Utils;


namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Controls the time bar in the end screen.
	/// </summary>
    public class TimeBarBinding
    {
		/// <summary>
		/// Controls one of the ticks on the time bar.
		/// </summary>
		private class TimeBarTickBinding
		{
			private const string ILLUMINATE_ELEMENT_NAME = "Container";
			private const string RANK_ELEMENT_NAME = "Rank";
			private const string TIME_ELEMENT_NAME = "Time";

			private const string ON_CLASS_NAME = "timeTickOn"; // Used when progress is past the tick.
			private const string OFF_CLASS_NAME = "timeTickOff"; // Used when progress is not past the tick.

			private VisualElement rootElement;
			private VisualElement illuminateElement; // Name of the element to apply the on and off classes to.
			private Label rankElement;
			private Label timeElement;

			private int seconds;

			public TimeBarTickBinding(VisualElement rootElement)
			{
				this.rootElement = rootElement;

				illuminateElement = rootElement.Q(ILLUMINATE_ELEMENT_NAME);
				
				rankElement = rootElement.Q<Label>(RANK_ELEMENT_NAME);
				timeElement = rootElement.Q<Label>(TIME_ELEMENT_NAME);
			}

			public void SetRank(int rankId)
			{
				rankElement.text = PtUUISettings.GetOrCreateSettings().RankSettings[rankId].RichTextIcon;
			}

			/// <param name="seconds">Time (in seconds) that must be beaten or matched to achieve this rank.</param>
			public void SetTime(int seconds)
			{
				this.seconds = seconds;
				timeElement.text = UIUtils.SecondsToTime(seconds);
			}

			public int GetSeconds() => seconds;
			
			/// <param name="position">0-1 position along the time bar.</param>
			public void SetPosition(float position)
			{
				rootElement.style.left = new Length(position * 100, LengthUnit.Percent);
			}


			public void Hide()
			{
				rootElement.style.display = DisplayStyle.None;
			}

			public bool GetIsShowing() => rootElement.resolvedStyle.display == DisplayStyle.Flex;

			public void SetPassed(bool isPassed)
			{
				illuminateElement.RemoveFromClassList(ON_CLASS_NAME);
				illuminateElement.RemoveFromClassList(OFF_CLASS_NAME);
				
				if (isPassed)
				{
					illuminateElement.AddToClassList(ON_CLASS_NAME);
				}
				else
				{
					illuminateElement.AddToClassList(OFF_CLASS_NAME);
				}
			}
		}

		private const string PROGRESS_FILL_ELEMENT_NAME = "Fill";
		private const string TICKS_CONTAINER_ELEMENT_NAME = "Ticks";

		private VisualElement progressFillElement;
		private TimeBarTickBinding[] ticks;

		private int minTime;
		private int maxTime;

		public int Min => minTime;
		public int Max => maxTime;

		/// <param name="rootElement">The element that contains all the elements that make up the time bar.</param>
		/// <param name="minTime">Lower end of the bar in seconds.</param>
		/// <param name="maxTime">Higher end of the bar in seconds.</param>
		public TimeBarBinding(VisualElement rootElement, int minTime, int maxTime)
		{
			progressFillElement = rootElement.Q(PROGRESS_FILL_ELEMENT_NAME);
			VisualElement tickContainer = rootElement.Q(TICKS_CONTAINER_ELEMENT_NAME);

			int tickCount = tickContainer.childCount;
			int rankCount = PtUUISettings.GetOrCreateSettings().RankSettings.Length;

			Debug.Assert(tickCount == rankCount - 1, $"Mismatch between the number of ticks in a time bar and the number of achievable ranks. Expected # of ticks {tickCount} + 1 to equal # of ranks {rankCount}.");

			ticks = new TimeBarTickBinding[tickCount];

			int i = 0;
			foreach(VisualElement tick in tickContainer.Children())
			{
				ticks[i++] = new TimeBarTickBinding(tick);
			}

			this.minTime = minTime;
			this.maxTime = maxTime;
		}

		public void SetTick(int rankId, int seconds)
		{
			float position;
			if (seconds < 0)
				position = 0;
			else
				position = 1 - Mathf.InverseLerp(minTime, maxTime, seconds);

			TimeBarTickBinding tick = ticks[rankId - 1];
			tick.SetRank(rankId);
			tick.SetTime(seconds);
			tick.SetPosition(position);
		}

		public void HideTick(int rankId)
		{
			TimeBarTickBinding tick = ticks[rankId - 1];
			tick.Hide();
		}

		/// <summary>
		/// Sets how filled out the bar should be. Illuminates ticks as the bar fills.
		/// </summary>
		/// <param name="seconds">How many seconds the bar should be filled to.</param>
		/// <returns>How many ticks are not illuminated / passed.</returns>
		public int SetProgress(int seconds)
		{
			float position;
			
			if (seconds < 0)
				position = 0;
			else
				position = 1 - Mathf.InverseLerp(minTime, maxTime, seconds);

			progressFillElement.style.width = new Length(position * 100, LengthUnit.Percent);

			// Illuminate ticks as we pass them.
			int unpassedTickCount = 0;
			foreach(TimeBarTickBinding tick in ticks)
			{
				// Ignore hidden ticks.
				if (!tick.GetIsShowing())
					continue;

				bool passed = false;
				
				int tickSeconds = tick.GetSeconds();

				// Handle the negative cases in such a way that positive progress causes negative ticks to illuminate
				// and we can have more negative progress that does not illuminate the negative ticks (for X rank).
				if (tickSeconds < 0)
				{
					passed = tickSeconds <= seconds;
				}
				else if (seconds > 0)
				{
					passed = tickSeconds >= seconds;
				}

				tick.SetPassed(passed);
				if (! passed)
					unpassedTickCount++;
			}

			return unpassedTickCount;
		}
    }
}
