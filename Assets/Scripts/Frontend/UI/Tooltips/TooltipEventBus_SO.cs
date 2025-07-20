using UnityEngine;
using UnityEngine.Events;

namespace AstralAvarice.UI.Tooltips
{
    [CreateAssetMenu(fileName = "TooltipEventBus", menuName = "Tooltips/EventBus")]
    public class TooltipEventBus_SO : ScriptableObject
    {
		[HideInInspector] public UnityEvent<TooltipLayer> OnRequestAddTooltipLayer = new UnityEvent<TooltipLayer>();
		[HideInInspector] public UnityEvent<TooltipLayer> OnRequestRemoveTooltipLayer = new UnityEvent<TooltipLayer>();

		public void Add(TooltipLayer layer)
		{
			OnRequestAddTooltipLayer.Invoke(layer);
		}

		public void Remove(TooltipLayer layer)
		{
			OnRequestRemoveTooltipLayer.Invoke(layer);
		}
	}
}
