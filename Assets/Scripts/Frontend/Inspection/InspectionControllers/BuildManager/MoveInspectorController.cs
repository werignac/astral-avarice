using UnityEngine.Events;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	public interface IMoveInspectorEventBus
	{
		public UnityEvent<BuildingComponent> OnHoveringBuildingChanged { get; }
	}

	/// <summary>
	/// A lot of this is copied from the DemolishInspectorController class. There
	/// are many opportunities for getting rid of cloned code.
	/// </summary>
	public class MoveInspectorController : IInspectorController
	{
		private static readonly string INSTRUCTIONS_ELEMENT_NAME = "Container";
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
		/// Visual element that simply contains instructions on how to move.
		/// </summary>
		private VisualElement _moveInstructionsElement;
		/// <summary>
		/// Visual element that constains information about the building we're hovering over.
		/// </summary>
		private VisualElement _hoveringBuildingElement;

		/// <summary>
		/// Object that contains events that broadcast changes in highlighted / hovering buildings.
		/// </summary>
		private IMoveInspectorEventBus _eventBus;

		public void ConnectInspectorUI(VisualElement inspectorUI)
		{
			_moveInstructionsElement = inspectorUI.Q(INSTRUCTIONS_ELEMENT_NAME);
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

		public void SetEventBus(IMoveInspectorEventBus eventBus)
		{
			if (_eventBus != null)
				_eventBus.OnHoveringBuildingChanged.RemoveListener(SetHoveringBuilding);

			_eventBus = eventBus;

			if (eventBus != null)
				_eventBus.OnHoveringBuildingChanged.AddListener(SetHoveringBuilding);
		}

		/// <summary>
		/// Called externally by MoveBuildState.
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
			_moveInstructionsElement.style.display = DisplayStyle.None;
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

			_moveInstructionsElement.style.display = DisplayStyle.Flex;
			_hoveringBuildingElement.style.display = DisplayStyle.None;
		}

		public void DisconnectInspectorUI()
		{
			_moveInstructionsElement = null;
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
