using UnityEngine;
using AstralAvarice.Frontend;

namespace AstralAvarice.UI.Tooltips
{
    public class SolarFieldTooltipDetectorComponent : MonoBehaviour
    {
		[SerializeField] private TooltipEventBus_SO _tooltipEventBus;
		[SerializeField] private InputEventBus_SO _inputEventBus;
		[SerializeField] private SolarFieldComponent _solarFieldComponent;
		[SerializeField] private TooltipLayerFactory_SO _tooltipLayerFactory;

		private TooltipLayer _currentTooltipLayer;
		private SolarFieldTooltipController _tooltipController;

		private void Start()
		{
			_tooltipController = new SolarFieldTooltipController();
		}

		private void OnEnable()
		{
			if (_currentTooltipLayer != null)
				_tooltipEventBus.Add(_currentTooltipLayer);
		}

		private void OnDisable()
		{
			if (_currentTooltipLayer != null)
				_tooltipEventBus.Remove(_currentTooltipLayer);
		}

		private void OnDestroy()
		{
			OnDisable();
		}

		public void OnCursorEnter()
		{
			// Called by selection cursor.
			_currentTooltipLayer = _tooltipLayerFactory.MakeLayer(_tooltipController);

			if (isActiveAndEnabled)
				_tooltipEventBus.Add(_currentTooltipLayer);
		}

		private void Update()
		{
			if (_currentTooltipLayer == null)
				return;
			
			Vector3 cursorPosition = _inputEventBus.CursorPosition;
			int solarEnergy = _solarFieldComponent.GetSolarEnergyAtPoint(cursorPosition);
			_tooltipController.SetSolarEnergy(solarEnergy);
		}

		private void OnCursorExit()
		{
			// Called by selection cursor.
			if (isActiveAndEnabled)
				_tooltipEventBus.Remove(_currentTooltipLayer);
			
			_currentTooltipLayer = null;
		}
	}
}
