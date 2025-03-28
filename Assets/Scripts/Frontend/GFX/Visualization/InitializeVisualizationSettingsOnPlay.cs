using UnityEngine;

namespace AstralAvarice.Visualization
{
    public static class InitializeVisualizationSettingsOnPlay
    {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void FetchAllVisualizationSettingsFromPlayerPrefs()
		{
			PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();
			VisualizationUISettings_SO visualizationSettings = uiSettings.VisualizationSettings;
			
			foreach(var visualization in visualizationSettings.StatesToDisplay)
			{
				visualization.FetchStateFromPlayerPrefs();
			}
		}
    }
}
