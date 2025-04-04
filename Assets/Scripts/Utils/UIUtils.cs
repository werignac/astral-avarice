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
    }
}
