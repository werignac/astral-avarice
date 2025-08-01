using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using AstralAvarice.Utils;

namespace AstralAvarice.UI.Tooltips
{
	public class UIDocumentTooltipsComponent : MonoBehaviour
	{
		[SerializeField] private TooltipComponent tooltip;

		/// <summary>
		/// Reference to a  UI document with a reference to the panel
		/// to check.
		/// </summary>
		[SerializeField] private UIDocument panelDocument;

		private IRuntimePanel panel;

		private UIDocumentTooltipController uiController = new UIDocumentTooltipController();

		private TooltipLayer _activeLayer = null;

		private TooltipLayer ActiveLayer {
			get
			{
				return _activeLayer;
			}

			set
			{
				_activeLayer = value;
			}
		}

		[SerializeField] private TooltipLayerFactory_SO layerFactory;


		private void Start()
		{
			panel = panelDocument.runtimePanel;
		}

		private void Update()
		{
			Vector2 screenPosition = Mouse.current.GetUIDocumentPosition();

			VisualElement hoveringElement = panel.Pick(screenPosition);

			if (TryGetTooltipText(hoveringElement, out string tooltipText))
			{
				if (ActiveLayer == null)
				{
					ActiveLayer = layerFactory.MakeLayer(uiController);
					tooltip.Add(ActiveLayer);
				}

				uiController.SetText(tooltipText);
			}
			else
			{
				if (ActiveLayer != null)
				{
					tooltip.Remove(ActiveLayer);
					ActiveLayer = null;
				}
			}
		}

		private bool TryGetTooltipText(VisualElement element, out string tooltipText)
		{
			if (element == null)
			{
				tooltipText = null;
				return false;
			}

			if (element.tooltip != null && element.tooltip.Length > 0)
			{
				tooltipText = element.tooltip;
				return true;
			}

			return TryGetTooltipText(element.parent, out tooltipText);
		}
	}
}
