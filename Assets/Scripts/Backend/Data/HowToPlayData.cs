using UnityEngine;
using UnityEngine.Video;

namespace AstralAvarice.Backend
{
	[CreateAssetMenu(fileName = "HowToPlayData", menuName = "Scriptable Objects/HowToPlayData")]
	public class HowToPlayData : ScriptableObject
	{
		public HowToPlayCategory_SO[] categories;

		[SerializeField, ColorUsage(false)] private Color _unhoveredTint;
		[SerializeField, ColorUsage(false)] private Color _hoveredTint;
		[SerializeField, ColorUsage(false)] private Color _pressedTint;
		[SerializeField, ColorUsage(false)] private Color _disabledTint;

		public Color UnhoveredTint => new Color(_unhoveredTint.r, _unhoveredTint.g, _unhoveredTint.b, 1);
		public Color HoveredTint => new Color(_hoveredTint.r, _hoveredTint.g, _hoveredTint.b, 1);
		public Color PressedTint => new Color(_pressedTint.r, _pressedTint.g, _pressedTint.b, 1);
		public Color DisabledTint => new Color(_disabledTint.r, _disabledTint.g, _disabledTint.b, 1);

	}
}
