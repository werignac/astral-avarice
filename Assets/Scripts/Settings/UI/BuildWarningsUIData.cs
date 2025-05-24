using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    [CreateAssetMenu(fileName = "BuildWarningsUIData", menuName = "Tooltips/BuildWarnings/Data")]
    public class BuildWarningsUIData : ScriptableObject
    {
		/// <summary>
		/// UI Document for a single build warning.
		/// </summary>
		public VisualTreeAsset buildWarningUIAsset;
		/// <summary>
		/// UI Document for a section that contains multiple build warnings.
		/// </summary>
		public VisualTreeAsset buildWarningSectionUIAsset;
    }
}
