using UnityEngine;

namespace AstralAvarice.UI.Tooltips
{
    public class AttachTooltipToEventBusComponent : MonoBehaviour
    {
		[SerializeField] private TooltipEventBus_SO _tooltipEventBus;
		[SerializeField] private TooltipComponent _tooltipComponent;

		private void Awake()
		{
			Attach();
		}

		private void Attach()
		{
			_tooltipEventBus.OnRequestAddTooltipLayer.AddListener(_tooltipComponent.Add);
			_tooltipEventBus.OnRequestRemoveTooltipLayer.AddListener(_tooltipComponent.Remove);
		}
	}
}
