using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	// TODO: Rename. This name is too close to ConstraintQueryResult?
	public struct ChainConstraintsQueryResult
	{
		private readonly BuildWarning.WarningType _buildingResult;
		private readonly BuildWarning.WarningType _cableResult;

		public ChainConstraintsQueryResult(
			BuildWarning.WarningType buildingResult,
			BuildWarning.WarningType cableResult
		)
		{
			_buildingResult = buildingResult;
			_cableResult = cableResult;
		}

		public BuildWarning.WarningType GetBuildingResult() => _buildingResult;

		public BuildWarning.WarningType GetCableResult()
		{
			if (_buildingResult == BuildWarning.WarningType.FATAL)
				return BuildWarning.WarningType.FATAL;
			return _cableResult;
		}
	}

	public class ChainBuildState : IPostConstraintsBuildState<ChainConstraintsQueryResult>, IBuildingPlacer, ICablePlacer, IInspectable
    {
		private BuildingBuildState _subBuildingBuildState;

		private SelectionCursorComponent _selectionCursor;
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
			_subBuildingBuildState = new BuildingBuildState(
					toBuild,
					buildingCursor,
					selectionCursor,
					gravityCursor,
					gameController
				);

			// Chain invokations of OnProspectivePlanetChanged out of this state.
			_subBuildingBuildState.OnProspectivePlanetChanged.AddListener(OnProspectivePlanetChanged.Invoke);

			// TODO: Listen to transition and apply events from the sub state.

			_selectionCursor = selectionCursor; // Null check occurs in BuildingBuildState.

			if (cableCursor == null)
				throw new ArgumentNullException("cableCursor");

			_cableCursor = cableCursor;

			_fromAttachment = new BuildingInstanceCableAttachment(fromBuilding, false);
			_toAttachment = new BuildingCursorCableAttachment(buildingCursor);
		}

		public void Start()
		{
			_subBuildingBuildState.Start();
			InitializeCableCursor();
		}

		private void InitializeCableCursor()
		{
			_cableCursor.Show();
			_cableCursor.SetStart(_fromAttachment.BuildingInstance);
		}

		public BuildStateType GetStateType() => BuildStateType.BUILDING_CHAINED;

		public CableCursorComponent GetCableCursor() => _cableCursor;

		public BuildingCursorComponent GetBuildingCursor()
		{
			return _subBuildingBuildState.GetBuildingCursor();
		}

		public Cost GetBuildingCost()
		{
			return _subBuildingBuildState.GetCost();
		}

		public Cost GetCableCost()
		{
			return CableBuildState.GetCableCostFromLength(Length);
		}

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
			return _subBuildingBuildState.GetPlacingBuilding();
		}

		public PlanetComponent GetProspectivePlanet()
		{
			return _subBuildingBuildState.GetProspectivePlanet();
		}

		public void Update(BuildStateInput input)
		{
			_subBuildingBuildState.Update(input);
			UpdateCableCursorPosition();
		}

		public void UpdatePostConstraints(BuildStateInput input, ChainConstraintsQueryResult constraintsResult)
		{
			UpdateCableCursorColor(constraintsResult.GetCableResult());

			_subBuildingBuildState.UpdatePostConstraints(input, constraintsResult.GetBuildingResult());

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
			if (_subBuildingBuildState.GetHasProspectivePlacement())
				_toAttachment = new BuildingCursorCableAttachment(_subBuildingBuildState.GetBuildingCursor());
			else
				_toAttachment = new CursorCableAttachment(_selectionCursor);

			_cableCursor.SetEndPoint(_toAttachment.GetPosition());
		}

		/// <summary>
		/// Copied from CableBuildState.
		/// </summary>
		/// <param name="constraintsResult"></param>
		private void UpdateCableCursorColor(BuildWarning.WarningType constraintsResult)
		{
			_cableCursor.SetCablePlaceability(constraintsResult < BuildWarning.WarningType.FATAL);
		}

		private void DechainToBuilding()
		{
			BuildStateTransitionSignal signal = new BuildingTransitionSignal(_subBuildingBuildState.ToBuild.BuildingSettings, false);
			OnRequestTransition.Invoke(signal);
		}

		private void DechainToCable()
		{
			BuildStateTransitionSignal signal = new CableTransitionSignal(_fromAttachment.BuildingInstance, false);
			OnRequestTransition.Invoke(signal);
		}

		public void CleanUp()
		{
			_subBuildingBuildState.CleanUp();
			_cableCursor.Hide();
		}

		public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
		{
			return _subBuildingBuildState.GetInspectorElement(out inspectorController);
		}
	}
}
