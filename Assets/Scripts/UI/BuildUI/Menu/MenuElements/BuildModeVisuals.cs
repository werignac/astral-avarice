using UnityEngine;

namespace AstralAvarice.VisualData
{
	[CreateAssetMenu(fileName = "BuildMode", menuName = "Visual Info/Build Mode Visuals", order = 3)]
    public class BuildModeVisuals : ScriptableObject, IBuildUIMenuElement
	{

		[SerializeField, InspectorName("Name")] private string modeName;
		[SerializeField, InspectorName("Icon")] private Sprite modeIcon;
		[SerializeField, InspectorName("Priority")] private int modePriority;
		[SerializeField, InspectorName("Description"), Multiline(lines: 10)] private string modeDescription;

		// Getters

		public string Name { get => modeName; }
		public Sprite Icon { get => modeIcon; }
		public int Priority { get => modePriority; }
		public string Description { get => modeDescription; }

    }
}
