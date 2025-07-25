using UnityEngine;

namespace AstralAvarice.Backend
{
    [CreateAssetMenu(fileName = "HowToPlayCategory", menuName = "HowToPlay/Category")]
    public class HowToPlayCategory_SO : ScriptableObject
    {
		public string categoryName;
		
		[SerializeField, ColorUsage(false)] private Color _color;
		public Color Color {
			get
			{
				return new Color(_color.r, _color.g, _color.b, 1);
			}
		}
		
		public HowToPlayTip[] tips;
    }
}
