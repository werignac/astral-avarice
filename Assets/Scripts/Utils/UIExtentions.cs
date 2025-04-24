using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AstralAvarice.Utils
{
    public static class UIExtentions
    {
		/// <summary>
		/// Computes the delay + duration of all transition animaitons for the visual element
		/// and then returns the longest of these.
		/// </summary>
		/// <param name="style">The style of the visual element to measure the transition duration of.</param>
		/// <returns>The longest total duration of any of the visual element's transition animations.</returns>
		public static float GetTotalTransitionDuration(this IResolvedStyle style)
		{
			IEnumerator<TimeValue> delays = style.transitionDelay.GetEnumerator();
			IEnumerator<TimeValue> durations = style.transitionDuration.GetEnumerator();

			float longestTotalDuration = 0f;

			while(delays.MoveNext() && durations.MoveNext())
			{
				TimeValue delay = delays.Current;
				TimeValue duration = durations.Current;

				float totalDuration = delay.value + duration.value;
				longestTotalDuration = Mathf.Max(totalDuration, longestTotalDuration);
			}

			return longestTotalDuration;
		}
    }
}
