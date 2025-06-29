using UnityEngine.Events;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	public interface IDemolishInspectorEventBus
	{
		public UnityEvent<IDemolishable> OnHoveringDemolishableChanged { get; }
	}

    public class DemolishInspectorController : IInspectorController
	{
		private static readonly string INSTRUCTIONS_ELEMENT_NAME = "Instructions";
		private static readonly string HOVERING_BUILDING_ELEMENT_NAME = "HoveringBuildingInspector";

		/// <summary>
		/// Whether this inspector is connected to UI elements yet.
		/// </summary>
		private bool _isConnected = false;

		/// <summary>
		/// The building we are hovering over.
		/// </summary>
		private BuildingComponent _hoveringBuilding = null;

		/// <summary>
		/// Controller that updates information about the hovering building.
		/// Null if no building is being hovered over.
		/// </summary>
		private BuildingInstanceInspectorController _subController = null;

		/// <summary>
		/// Visual element that simply contains instructions on how to demolish.
		/// </summary>
		private VisualElement _demolishInstructionsElement;
		/// <summary>
		/// Visual element that constains information about the building we're hovering over.
		/// </summary>
		private VisualElement _hoveringBuildingElement;

		/// <summary>
		/// Object that contains events that broadcast changes in highlighted / hovering buildings.
		/// </summary>
		private IDemolishInspectorEventBus _eventBus;

		public void ConnectInspectorUI(VisualElement inspectorUI)
		{
			_demolishInstructionsElement = inspectorUI.Q(INSTRUCTIONS_ELEMENT_NAME);
			_hoveringBuildingElement = inspectorUI.Q(HOVERING_BUILDING_ELEMENT_NAME);

			if (_hoveringBuilding != null)
			{
				ShowHoveringBuilding();
			}
			else
			{
				HideHoveringBuilding();
			}

			_isConnected = true;
		}

		public void SetEventBus(IDemolishInspectorEventBus eventBus)
		{
			if (_eventBus != null)
				_eventBus.OnHoveringDemolishableChanged.RemoveListener(EventBus_OnHoveringDemolishableChanged);

			_eventBus = eventBus;

			if (eventBus != null)
				_eventBus.OnHoveringDemolishableChanged.AddListener(EventBus_OnHoveringDemolishableChanged);
		}

		/// <summary>
		/// Public because it's directly called in the demolish build state after initialization.
		/// </summary>
		/// <param name="demolishable"></param>
		public void EventBus_OnHoveringDemolishableChanged(IDemolishable demolishable)
		{
			SetHoveringBuilding(demolishable as BuildingComponent);
		}

		/// <summary>
		/// Called externally by DemolishBuildState.
		/// </summary>
		/// <param name="hoveringBuilding">The building we are now hovering over. Can be null.</param>
		public void SetHoveringBuilding(BuildingComponent hoveringBuilding)
		{
			_hoveringBuilding = hoveringBuilding;

			if (_isConnected)
			{
				if (hoveringBuilding != null)
				{
					ShowHoveringBuilding();
				}
				else if (hoveringBuilding == null)
				{
					HideHoveringBuilding();
				}
			}
		}

		private void ShowHoveringBuilding()
		{
			_demolishInstructionsElement.style.display = DisplayStyle.None;
			_hoveringBuildingElement.style.display = DisplayStyle.Flex;

			if (_subController == null)
			{
				_subController = new BuildingInstanceInspectorController(_hoveringBuilding);
				_subController.ConnectInspectorUI(_hoveringBuildingElement);
			}
		}

		private void HideHoveringBuilding()
		{
			if (_subController != null)
			{
				_subController.DisconnectInspectorUI();
				_subController = null;
			}

			_demolishInstructionsElement.style.display = DisplayStyle.Flex;
			_hoveringBuildingElement.style.display = DisplayStyle.None;
		}

		public void DisconnectInspectorUI()
		{
			_demolishInstructionsElement = null;
			_hoveringBuildingElement = null;

			if (_subController != null)
			{
				_subController.DisconnectInspectorUI();
				_subController = null;
			}

			_isConnected = false;

			SetEventBus(null); // Get rid of all lingering references.
		}

		public void UpdateUI()
		{
			_subController?.UpdateUI();
		}
	}
}
