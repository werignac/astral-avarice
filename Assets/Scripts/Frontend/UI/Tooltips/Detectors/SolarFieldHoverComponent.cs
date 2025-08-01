using UnityEngine;
using AstralAvarice.Frontend;
using UnityEngine.Events;

namespace AstralAvarice.UI.Tooltips
{
    public class SolarFieldHoverComponent : MonoBehaviour
    {
		[SerializeField] private TooltipEventBus_SO _tooltipEventBus;
		[SerializeField] private InputEventBus_SO _inputEventBus;
		[SerializeField] private SolarFieldComponent _solarFieldComponent;
		[SerializeField] private TooltipLayerFactory_SO _tooltipLayerFactory;

		private TooltipLayer _currentTooltipLayer;
		private SolarFieldTooltipController _tooltipController;

		/// <summary>
		/// Invoked on every frame a radius is hovered.
		/// Invoked with -1 when hovering stops.
		/// Used for highlighting a specific radius.
		/// </summary>
		[HideInInspector] public UnityEvent<int> OnHoverRadius = new UnityEvent<int>();

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

		private void OnCursorEnter()
		{
			// Called by selection cursor.
			// Sometimes before Start, so need to make tooltip JIT.
			
			if (_tooltipController == null)
				_tooltipController = new SolarFieldTooltipController();

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

			// Highlight the radius that's being hovered.
			OnHoverRadius.Invoke(solarEnergy);
		}

		private void OnCursorExit()
		{
			// Called by selection cursor.
			if (isActiveAndEnabled && _currentTooltipLayer != null)
				_tooltipEventBus.Remove(_currentTooltipLayer);
			
			_currentTooltipLayer = null;

			// Stop highlighting a radius.
			OnHoverRadius.Invoke(-1);
		}
	}
}
