using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Visualization
{
	[CreateAssetMenu(fileName = "VisualizationUISettings", menuName = "Visualization/UI/Settings")]
	public class VisualizationUISettings_SO : ScriptableObject
	{
		[Header("Visualization")]

		[SerializeField, InspectorName("Menu State Element"), Tooltip("UI element to use for visual states (e.g. whether we're showing gravity fields, solar fields, etc.).")]
		private VisualTreeAsset visualizationMenuStateElement;
		[SerializeField, InspectorName("MenuModeElement"), Tooltip("UI element to use for visual modes (e.g. grid group view).")]
		private VisualTreeAsset visualizationMenuModeElement;
		[SerializeField, Tooltip("The states to display in menus for visualizing certain things (e.g. gravity fields, solar fields).")]
		private VisualizationToggleState_SO[] statesToDisplay;

		// Getters

		public VisualTreeAsset MenuStateElement { get => visualizationMenuStateElement; }
		public VisualTreeAsset MenuModeElement { get => visualizationMenuModeElement; }
		public VisualizationToggleState_SO[] StatesToDisplay { get => statesToDisplay; }

	}
}
