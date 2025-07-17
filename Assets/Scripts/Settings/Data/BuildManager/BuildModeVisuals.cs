using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.VisualData
{
	[CreateAssetMenu(fileName = "BuildMode", menuName = "Visual Info/Build Mode Visuals", order = 3)]
    public class BuildModeVisuals : ScriptableObject, IBuildUIMenuElement, IInspectable
	{

		[SerializeField, InspectorName("Name")] private string modeName;
		[SerializeField, InspectorName("Icon")] private Sprite modeIcon;
		[SerializeField, InspectorName("Priority")] private int modePriority;
		[SerializeField, InspectorName("Description"), Multiline(lines: 10)] private string modeDescription;
		[SerializeField, InspectorName("Inspector Asset")] private VisualTreeAsset modeInspectorAsset;

		// Getters

		public string Name { get => modeName; }
		public Sprite Icon { get => modeIcon; }
		public int Priority { get => modePriority; }
		public string Description { get => modeDescription; }

		public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
		{
			// TODO: Change per build mode.

			inspectorController = null;
			return modeInspectorAsset;
		}
	}
}
