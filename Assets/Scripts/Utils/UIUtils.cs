using UnityEngine;

namespace AstralAvarice.Utils
{
    public static class UIUtils
    {
		public static float GetScreenWidth()
		{
			return Screen.width * 1080 / Screen.height;
		}

		public static float GetScreenHeight()
		{
			return 1080;
		}

		/// <summary>
		/// Converts a float number of seconds to a time with teh format
		/// 00:00. If the number of seconds is negative, --:-- is returned.
		/// The number of seconds is floored prior to conversion.
		/// </summary>
		/// <param name="seconds"># of seconds to convert to a string.</param>
		/// <returns>Time-formatted string.</returns>
		public static string SecondsToTime(float seconds)
		{
			return SecondsToTime(Mathf.FloorToInt(seconds));
		}

		/// <summary>
		/// Converts an integer number of seconds to a time with the format
		/// 00:00. If the number of seconds is negative, --:-- is returned.
		/// </summary>
		/// <param name="seconds"># of seconds to convert to a string.</param>
		/// <returns>Time-formatted string</returns>
		public static string SecondsToTime(int seconds)
		{
			if (seconds < 0)
				return "--:--";

			int minutes = seconds / 60; // Integer division.
			int remainingSeconds = seconds % 60;
			
			return $"{minutes.ToString("00")}:{remainingSeconds.ToString("00")}";
		}
    }
}
