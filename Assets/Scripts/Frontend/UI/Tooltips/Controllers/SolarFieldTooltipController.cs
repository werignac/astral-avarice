using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.UI.Tooltips
{
	public class SolarFieldTooltipController : ITooltipUIController
	{
		private const string TOOLTIP_LABEL_ELEMENT_NAME = "TooltipLabel";
		private const string SOLAR_RESOURCE_ELEMENT_RICH_TEXT = "<sprite=\"text-icons\" name=\"solar\">";

		private int _solarEnergy;
		private Label _labelElement;

		public void SetSolarEnergy(int solarEnergy)
		{
			_solarEnergy = solarEnergy;

			if (_labelElement != null)
				UpdateTooltipText();
		}

		public void Bind(VisualElement ui)
		{
			_labelElement = ui.Q<Label>(TOOLTIP_LABEL_ELEMENT_NAME);

			Debug.Assert(_labelElement != null, $"Missing child {TOOLTIP_LABEL_ELEMENT_NAME} from tooltip VisualElement {_labelElement}.");

			UpdateTooltipText();
		}

		public void UnBind()
		{
			_labelElement = null;
		}

		private void UpdateTooltipText()
		{
			_labelElement.text = $"Planets at this position will receive {_solarEnergy} {SOLAR_RESOURCE_ELEMENT_RICH_TEXT} solar energy.";
		}
	}
}
