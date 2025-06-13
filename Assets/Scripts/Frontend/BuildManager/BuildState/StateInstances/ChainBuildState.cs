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

	public class ChainBuildState : IPostConstraintsBuildState<ChainConstraintsQueryResult>, IBuildingPlacer, ICablePlacer, IOverrideExternalSignal, IInspectable
    {
		private BuildingBuildState _subBuildingBuildState;

		private SelectionCursorComponent _selectionCursor;
		private BuildingCursorComponent _buildingCursor;
		private CableCursorComponent _cableCursor;
		private GravityFieldCursorComponent _gravityCursor;

		private GameController _gameController;

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
			if (toBuild.BuildingDataAsset.maxPowerLines <= 0)
				throw new ArgumentException($"Cannot enter chain mode with a building {toBuild.BuildingDataAsset.buildingName} that cannot have connections.");


			_selectionCursor = selectionCursor; // Null check occurs in BuildingBuildState.
			_buildingCursor = buildingCursor; // Null check occurs in BuildingBuildState.
			_gravityCursor = gravityCursor; // Null check occurs in BuildingBuildState.
			_gameController = gameController; // Null check occurs in BuildingBuildState.

			SetSubBuildingBuildState(toBuild, false);

			if (cableCursor == null)
				throw new ArgumentNullException("cableCursor");

			_cableCursor = cableCursor;

			// Cable cursor is set in start.
			SetFromAttachment(new BuildingInstanceCableAttachment(fromBuilding, false), false);
			_toAttachment = new BuildingCursorCableAttachment(buildingCursor);
		}

		private void SetSubBuildingBuildState(BuildingSettingEntry toBuild, bool callStart)
		{
			if (_subBuildingBuildState != null)
			{
				_subBuildingBuildState.OnProspectivePlanetChanged.RemoveListener(OnProspectivePlanetChanged.Invoke);
				_subBuildingBuildState.CleanUp();
			}

			BuildingBuildState newSubState = new BuildingBuildState(
					toBuild,
					_buildingCursor,
					_selectionCursor,
					_gravityCursor,
					_gameController
				);

			_subBuildingBuildState = newSubState;

			// Chain invokations of OnProspectivePlanetChanged out of this state.
			newSubState.OnProspectivePlanetChanged.AddListener(OnProspectivePlanetChanged.Invoke);
			// TODO: Listen to transition events from the sub state.

			if (callStart)
				newSubState.Start();
		}

		private bool TrySetFromAttachment(BuildingComponent building, bool updateCableCursor)
		{
			if (building == null)
				throw new ArgumentNullException("building");

			if (!building.BackendBuilding.CanAcceptNewConnections())
				return false;

			SetFromAttachment(new BuildingInstanceCableAttachment(building, false), updateCableCursor);
			return true;
		}

		private void SetFromAttachment(BuildingInstanceCableAttachment attachment, bool updateCableCursor)
		{
			if (attachment == null)
				throw new ArgumentNullException("attachment");

			if (attachment.IsVolatile)
				Debug.LogWarning("Chain build state should not have volatile from attachments, but received a volatile attachment.");

			if (_fromAttachment != null)
			{
				_fromAttachment.BuildingInstance.OnBuildingDemolished.RemoveListener(FromAttachment_OnDemolish);
			}

			_fromAttachment = attachment;

			attachment.BuildingInstance.OnBuildingDemolished.AddListener(FromAttachment_OnDemolish);

			if (updateCableCursor)
				_cableCursor.SetStart(attachment.BuildingInstance);
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

		/// <summary>
		/// ChainBuildState is always making a new cable and is never moving an existing cable.
		/// </summary>
		public bool TryGetMovingCable(out CableComponent movingCable)
		{
			movingCable = null;
			return false;
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

			bool applied = false; // Keep track of whether we applied.
			BuildStateTransitionSignal capturedSignal = null; // Keep track of whether a signal was sent.

			UnityAction<BuildStateTransitionSignal> subTransitionListener = (BuildStateTransitionSignal signal) => {
				capturedSignal = signal;
			};

			UnityAction<BuildStateApplyResult> subApplyListener = (BuildStateApplyResult applyResult) => {
				applied = true;
				SubBuildingBuildState_OnApplied(applyResult, constraintsResult.GetCableResult());
			};

			_subBuildingBuildState.OnApplied.AddListener(subApplyListener);
			_subBuildingBuildState.OnRequestTransition.AddListener(subTransitionListener);
			
			_subBuildingBuildState.UpdatePostConstraints(input, constraintsResult.GetBuildingResult());
			
			_subBuildingBuildState.OnApplied.RemoveListener(subApplyListener);
			_subBuildingBuildState.OnRequestTransition.RemoveListener(subTransitionListener);

			if (applied) // If we applied, no need to do anything else.
				return;

			if (capturedSignal != null)
			{
				// TODO: Process captured signal.
				bool signalProcessed = TryProcessSubStateSignal(capturedSignal);
				if (signalProcessed)
					return;
			}

			if (input.primaryFire)
			{
				// The player is either hovering over nothing,
				// trying to place building in an illegal spot,
				// or hovering over another building.

				BuildingComponent hoveringbuilding = _selectionCursor.FindFirstBuilding();

				if (hoveringbuilding != null)
				{
					// The player is hovering / clicking on a building.
					// Might never be called due to chain signals being processed via TryProcessSubSignal().
					TrySetFromAttachment(hoveringbuilding, true);
					return;
				}

				if (_subBuildingBuildState.GetProspectivePlanet() == null)
				{
					// The player is hovering over nothing.
					Cancel();
					return;
				}

				// The player is trying to place a building in an illegal spot.
				// No need to play failed build sound, this is handeled by sub controller.
				return;
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
			if (_subBuildingBuildState.GetHasProspectivePlanet())
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

		private void SubBuildingBuildState_OnApplied(BuildStateApplyResult result, BuildWarning.WarningType cableConstraintsQueryResult)
		{
			// Firstly, propegate the application from the sub state upwards to initialize the building object.
			OnApplied.Invoke(result);

			// Next see if we can place a cable.
			BuildingBuildStateApplyResult buildingResult = (BuildingBuildStateApplyResult) result;
			if (cableConstraintsQueryResult < BuildWarning.WarningType.FATAL)
			{
				// Temporarily set the end point on the cable to the new building.
				// Will be reset upon next Update().
				_cableCursor.SetEndBuilding(buildingResult.buildingInstance);
				// Place the cable.
				GameObject cableGameObject = _cableCursor.PlaceCableAtLocation();
				CableComponent cable = cableGameObject.GetComponent<CableComponent>();

				// Send a signal that we placed the cable.
				Cost cableCost = GetCableCost();
				CableBuildStateApplyResult cableResult = new CableBuildStateApplyResult
				{
					cableInstance = cable,
					cost = cableCost
				};
				OnApplied.Invoke(cableResult);
			}
			
			// Finally, figure out which state we're in after applying.
			
			// If the building we placed has slots, chain from them.
			if (buildingResult.buildingInstance.BackendBuilding.CanAcceptNewConnections())
			{
				SetFromAttachment(new BuildingInstanceCableAttachment(buildingResult.buildingInstance, false), true);
				return;
			}

			// If the building we were previously chaining from has slots, continue to chain from them.
			if (_fromAttachment.BuildingInstance.BackendBuilding.CanAcceptNewConnections())
				return;

			// Otherwise, we have nowhere to chain from. Revert to normal build state.
			DechainToBuilding();
		}

		private void FromAttachment_OnDemolish(BuildingComponent demolishedBuilding)
		{
			Debug.Assert(demolishedBuilding == _fromAttachment.BuildingInstance);
			DechainToBuilding();
		}

		private bool TryProcessSubStateSignal(BuildStateTransitionSignal signal)
		{
			switch (signal.GetSignalType())
			{
				case BuildStateTransitionSignalType.CHAIN:
					ChainTransitionSignal chainSignal = signal as ChainTransitionSignal;
					return TrySetFromAttachment(chainSignal.ChainFrom, true);
			}

			return false;
		}

		public bool TryOverrideExternalSignal(BuildStateTransitionSignal signal)
		{
			if (!signal.IsExternal)
				throw new ArgumentException("Cannot call TryOverrideExternalSignal with an internal signal.");

			switch (signal.GetSignalType())
			{
				case BuildStateTransitionSignalType.BUILDING:
					{
						// If we receive a build signal, set the building we're constructing to the new building type.
						BuildingTransitionSignal buildingSignal = signal as BuildingTransitionSignal;
						if (buildingSignal.NewBuildingType.BuildingDataAsset.maxPowerLines > 0)
						{
							SetSubBuildingBuildState(buildingSignal.NewBuildingType, true);
							return true;
						}
						// The new building type does not support cables. Switch to the normal build state.
					}
					break;
			}

			return false;
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

		private void Cancel()
		{
			BuildStateTransitionSignal signal = new CancelTransitionSignal(false);
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
