using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
    public class ChainBuildState : IBuildState, IBuildingPlacer, ICablePlacer, IInspectable
    {
		private BuildingBuildState subBuildingBuildState;

		private SelectionCursorComponent _selectionCursor;
		private BuildingCursorComponent _buildingCursor;
		private CableCursorComponent _cableCursor;

		private BuildingInstanceCableAttachment _fromAttachment;
		private ICableAttachment _toAttachment;

		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged { get; } = new UnityEvent<PlanetComponent>();

		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();

		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();

		public ChainBuildState(
				BuildingSettingEntry toBuild,
				BuildingComponent fromBuilding,
				SelectionCursorComponent selectionCursor,
				BuildingCursorComponent buildingCursor,
				CableCursorComponent cableCursor,
				GravityFieldCursorComponent gravityCursor,
				GameController gameController
			)
		{
			subBuildingBuildState = new BuildingBuildState(
					toBuild,
					buildingCursor,
					selectionCursor,
					gravityCursor,
					gameController,
					() => { return BuildWarning.WarningType.FATAL; }
				);

			// Chain invokations of OnProspectivePlanetChanged out of this state.
			subBuildingBuildState.OnProspectivePlanetChanged.AddListener(OnProspectivePlanetChanged.Invoke);

			// TODO: Listen to transition and apply events from the sub state.

			_selectionCursor = selectionCursor; // Null check occurs in BuildingBuildState.
			_buildingCursor = buildingCursor;

			if (cableCursor == null)
				throw new ArgumentNullException("cableCursor");

			_cableCursor = cableCursor;

			_fromAttachment = new BuildingInstanceCableAttachment(fromBuilding, false);
			_toAttachment = new BuildingCursorCableAttachment(buildingCursor);
		}

		public void Start()
		{
			subBuildingBuildState.Start();
			InitializeCableCursor();
		}

		private void InitializeCableCursor()
		{
			_cableCursor.Show();
			_cableCursor.SetStart(_fromAttachment.BuildingInstance);
		}

		public BuildStateType GetStateType() => BuildStateType.BUILDING_CHAINED;

		public float Length => Vector2.Distance(_fromAttachment.GetPosition(), _toAttachment.GetPosition());

		public ICableAttachment GetFromAttachment()
		{
			return _fromAttachment;
		}

		public ICableAttachment GetToAttachment()
		{
			return _toAttachment;
		}

		public IPlacingBuilding GetPlacingBuilding()
		{
			return subBuildingBuildState.GetPlacingBuilding();
		}

		public PlanetComponent GetProspectivePlanet()
		{
			return subBuildingBuildState.GetProspectivePlanet();
		}

		public void Update(BuildStateInput input)
		{
			subBuildingBuildState.Update(input);

			UpdateCableCursorPosition();

			if (input.primaryFire)
			{
				
			}
			else if (input.secondaryFire)
			{
				DechainToCable();
				return;
			}
		}

		private void UpdateCableCursorPosition()
		{
			// If we are prospecting a place to put the building, the building cursor is showing.
			if (subBuildingBuildState.GetHasProspectivePlacement())
				_toAttachment = new BuildingCursorCableAttachment(_buildingCursor);
			else
				_toAttachment = new CursorCableAttachment(_selectionCursor);

			_cableCursor.SetEndPoint(_toAttachment.GetPosition());
		}

		private void DechainToBuilding()
		{
			BuildStateTransitionSignal signal = new BuildingTransitionSignal(subBuildingBuildState.ToBuild.BuildingSettings, false);
			OnRequestTransition.Invoke(signal);
		}

		private void DechainToCable()
		{
			BuildStateTransitionSignal signal = new CableTransitionSignal(_fromAttachment.BuildingInstance, false);
			OnRequestTransition.Invoke(signal);
		}

		public void CleanUp()
		{
			subBuildingBuildState.CleanUp();
			_cableCursor.Hide();
		}

		public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
		{
			return subBuildingBuildState.GetInspectorElement(out inspectorController);
		}
	}
}
