using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using werignac.Utils;

namespace AstralAvarice.Frontend
{
	// TODO: Rename. This name is too close to ConstraintQueryResult?
	public struct MoveConstraintsQueryResult
	{
		private readonly BuildWarning.WarningType _buildingResult;
		private readonly BuildWarning.WarningType[] _cableResults;

		public MoveConstraintsQueryResult(
				BuildWarning.WarningType buildingResult,
				params BuildWarning.WarningType[] cableResults
			)
		{
			_buildingResult = buildingResult;
			_cableResults = new BuildWarning.WarningType[cableResults.Length];
			
			// NOTE: ChainConstraintsQueryResult does this comparison on get instead of construction.
			// might be a good idea to standardize this.
			for (int i = 0; i < cableResults.Length; i++)
			{
				_cableResults[i] = (BuildWarning.WarningType) Mathf.Max((int) cableResults[i], (int) buildingResult);
			}
		}

		public BuildWarning.WarningType GetBuildingResult()
		{
			return _buildingResult;
		}

		public BuildWarning.WarningType[] GetCableResults()
		{
			return _cableResults;
		}
	}

	// TODO: Implement IOverride etc. So what we can chain from moving?
	public class MoveBuildState : IPostConstraintsBuildState<MoveConstraintsQueryResult>, IBuildingPlacer, IMultiCablePlacer
	{
		/// <summary>
		/// Sub class for implementing the cable placer interface for
		/// the cables that are moved by the MoveBuildState.
		/// </summary>
		public class CableMover : ICablePlacer, IDisposable
		{
			private CableComponent _movingCable;

			private BuildingInstanceCableAttachment _fromAttachment;
			private BuildingCursorCableAttachment _toAttachment;

			private CableCursorComponent _cableCursor;

			/// <summary>
			/// Invoked when the moving cable gets demolished.
			/// </summary>
			public UnityEvent<CableMover> OnInvalidate { get; } = new UnityEvent<CableMover>();

			public CableMover(
					CableComponent movingCable,
					BuildingComponent movingBuilding,
					CableCursorComponent cableCursorTemplate,
					BuildingCursorComponent buildingCursor
				)
			{
				_cableCursor = UnityEngine.Object.Instantiate(cableCursorTemplate);
				_toAttachment = new BuildingCursorCableAttachment(buildingCursor);
				SetMovingCableAndBuilding(movingCable, movingBuilding);
			}

			public void SetMovingCableAndBuilding(CableComponent movingCable, BuildingComponent movingBuilding)
			{
				if (movingCable == null)
					throw new ArgumentNullException("movingCable");

				if (movingBuilding == null)
					throw new ArgumentNullException("movingBuilding");

				if (_movingCable != null)
				{
					_movingCable.OnCableDemolished.RemoveListener(Cable_OnDemolish);
				}

				_movingCable = movingCable;

				BuildingComponent nonMovingbuilding;
				if (movingBuilding == movingCable.Start)
					nonMovingbuilding = movingCable.End;
				else if (movingBuilding == movingCable.End)
					nonMovingbuilding = movingCable.Start;
				else
					throw new ArgumentException($"The passed building is not connected to the passed cable.");

				_fromAttachment = new BuildingInstanceCableAttachment(nonMovingbuilding, false);
				_cableCursor.SetStart(nonMovingbuilding);

				movingCable.OnCableDemolished.AddListener(Cable_OnDemolish);
			}

			private void Cable_OnDemolish(CableComponent demolishedCable)
			{
				Debug.Assert(demolishedCable == _movingCable);
				OnInvalidate.Invoke(this);
			}

			public float Length => Vector2.Distance(GetFromAttachment().GetPosition(), GetToAttachment().GetPosition());
			public CableCursorComponent GetCableCursor() => _cableCursor;
			public ICableAttachment GetFromAttachment() => _fromAttachment;
			public ICableAttachment GetToAttachment() => _toAttachment;

			/// <summary>
			/// CableMover is always moving an existing cable.
			/// </summary>
			public bool TryGetMovingCable(out CableComponent movingCable)
			{
				movingCable = _movingCable;
				return true;
			}

			public void UpdateCableCursorPosition()
			{
				_cableCursor.SetEndPoint(_toAttachment.GetPosition());
			}

			public void UpdateCableCursorColor(BuildWarning.WarningType cableConstraintsResult)
			{
				_cableCursor.SetCablePlaceability(cableConstraintsResult < BuildWarning.WarningType.FATAL);
			}

			public void HideCursor()
			{
				_cableCursor.Hide();
			}

			public void ShowCursor()
			{
				_cableCursor.Show();
			}

			public void Dispose()
			{
				_movingCable.OnCableDemolished.RemoveListener(Cable_OnDemolish);
				UnityEngine.Object.Destroy(_cableCursor);
			}
		}

		/// <summary>
		/// Null when no building has been selected yet. Otherwise contains the instance of a building being moved.
		/// </summary>
		private ExistingPlacingBuilding _movingBuilding;
		private PlanetComponent _prospectivePlanet;

		private SelectionCursorComponent _selectionCursor;
		private BuildingCursorComponent _buildingCursor;
		private CableCursorComponent _cableCursorTemplate;
		private List<CableMover> _cableMovers = new List<CableMover>();

		/// <summary>
		/// Needed to get the cables that a building is connected to.
		/// </summary>
		private GameController _gameController;

		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();
		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged { get; } = new UnityEvent<PlanetComponent>();

		public BuildStateType GetStateType() => BuildStateType.MOVE;

		public MoveBuildState(
				SelectionCursorComponent selectionCursor,
				BuildingCursorComponent buildingCursor,
				CableCursorComponent cableCursorTemplate,
				GameController gameController,
				BuildingComponent toMove = null
			)
		{
			if (selectionCursor == null)
				throw new ArgumentNullException("selectionCursor");

			_selectionCursor = selectionCursor;

			if (buildingCursor == null)
				throw new ArgumentNullException("buildingCursor");

			_buildingCursor = buildingCursor;

			if (cableCursorTemplate == null)
				throw new ArgumentNullException("cableCursorTemplate");

			_cableCursorTemplate = cableCursorTemplate;

			if (gameController == null)
				throw new ArgumentNullException("gameController");

			_gameController = gameController;

			if (toMove != null)
			{
				bool canMove = TrySetMovingBuilding(toMove, false);

				if (!canMove)
					throw new ArgumentException($"Cannot move building {toMove} that the player has no authority over.");
			}
		}

		private bool TrySetMovingBuilding(BuildingComponent toMove, bool updateBuildingCursor)
		{
			if (toMove == null)
			{
				SetMovingBuilding(null, updateBuildingCursor);
				return true;
			}
			
			if (toMove != null && !toMove.GetClientHasAuthority())
				return false;

			SetMovingBuilding(new ExistingPlacingBuilding(toMove), updateBuildingCursor);
			return true;
		}

		private void SetMovingBuilding(ExistingPlacingBuilding toPlace, bool updateCursors)
		{
			if (_movingBuilding != null)
			{
				_movingBuilding.BuildingInstance.OnBuildingDemolished.RemoveListener(MovingBuilding_OnDemolish);
			}

			_movingBuilding = toPlace;

			if (toPlace != null)
			{
				toPlace.BuildingInstance.OnBuildingDemolished.AddListener(MovingBuilding_OnDemolish);
			}
			else
			{
				SetProspectingPlanet(null);
			}

			// Update the building cursor.
			if (updateCursors)
			{
				if (toPlace == null)
				{
					HideAllCursors();
				}
				else
				{
					SetCursorsForBuilding(toPlace.BuildingInstance);
				}
			}
		}

		/// <summary>
		/// Sets up all the cursors to project the effects of moving the passed building.
		/// Does not include the colors of the cursors (which is handled in update).
		/// </summary>
		/// <param name="building">The building to match.</param>
		private void SetCursorsForBuilding(BuildingComponent building)
		{
			SetBuildingCursorForBuilding(building);
			SetCableCursorsForBuilding(building);
		}

		/// <summary>
		/// Sets up the building cursor to project the effects of moving the passed building.
		/// Sets the ghost and connection cursor properties.
		/// Does not include the color of the cursor.
		/// </summary>
		/// <param name="building">The building to match.</param>
		private void SetBuildingCursorForBuilding(BuildingComponent building)
		{
			BuildingVisuals visualAsset = building.BuildingVisuals;
			_buildingCursor.SetGhost(visualAsset);

			_buildingCursor.SetBuildingCollisionAndCableConnectionOffsetFromBuilding(building);

			bool canHaveConnections = building.Data.maxPowerLines > 0;
			_buildingCursor.SetShowCableConnectionCursor(canHaveConnections);
		}

		/// <summary>
		/// Sets up cable cursors to project the effects of moving the passed building.
		/// Instantiates new cable cursors if needed and sets them to connect to the
		/// corresponding buildings.
		/// Does not include the color of the cursors.
		/// </summary>
		/// <param name="building">The building with cables to match.</param>
		private void SetCableCursorsForBuilding(BuildingComponent building)
		{
			CableComponent[] cables = _gameController.GetConnectedCables(building).ToArray();

			// Set up the pool of cable movers for each cable connected to the building.
			for (int i = 0; i < Mathf.Max(cables.Length, _cableMovers.Count); i++)
			{
				if (i < cables.Length && i < _cableMovers.Count)
				{
					_cableMovers[i].SetMovingCableAndBuilding(cables[i], building);
				}
				else if (i >= cables.Length)
				{
					_cableMovers[i].Dispose();
				}
				else
				{
					CableMover newMover = new CableMover(
						cables[i],
						building,
						_cableCursorTemplate,
						_buildingCursor
					);

					newMover.OnInvalidate.AddListener(CableMover_OnInvalidate);

					_cableMovers.Add(newMover);
				}
			}

			// Remove any extra cable movers (dispose will already be called on these).
			if (_cableMovers.Count > cables.Length)
			{
				_cableMovers.RemoveRange(cables.Length, _cableMovers.Count - cables.Length);
			}
		}

		private void CableMover_OnInvalidate(CableMover invalidatedCableMover)
		{
			invalidatedCableMover.Dispose();
			_cableMovers.Remove(invalidatedCableMover);
		}

		private void SetProspectingPlanet(PlanetComponent prospectingPlanet)
		{
			if (prospectingPlanet == _prospectivePlanet)
				return;

			_prospectivePlanet = prospectingPlanet;

			OnProspectivePlanetChanged.Invoke(prospectingPlanet);
		}

		private void MovingBuilding_OnDemolish(BuildingComponent demolishedBuilding)
		{
			Debug.Assert(demolishedBuilding == _movingBuilding.BuildingInstance);
			SetMovingBuilding(null, true);
		}

		public void Start()
		{
			// Deferred setup of cursors from construction to allow CleanUp of last state to be called first.
			if (_movingBuilding != null)
				SetCursorsForBuilding(_movingBuilding.BuildingInstance);
		}

		public IPlacingBuilding GetPlacingBuilding()
		{
			return _movingBuilding;
		}

		public PlanetComponent GetProspectivePlanet()
		{
			return _prospectivePlanet;
		}

		public BuildingCursorComponent GetBuildingCursor()
		{
			return _buildingCursor;
		}

		public ICablePlacer[] GetCablePlacers()
		{
			// If we're not moving a building, we're not moving cables.
			// This line allows us to keep the _cableMovers pool populated
			// while surpressing warnings when we're not moving buildings.
			if (_movingBuilding == null)
				return new ICablePlacer[0];

			return _cableMovers.ToArray();
		}

		public void Update(BuildStateInput input)
		{
			// TODO: Find a placement on the nearest planet / on the only planet for the building.
			if (_movingBuilding == null)
			{
				UpdatePriorToBuildingSelected();
			}
			else
			{
				UpdateAfterBuildingSelected();
			}
		}

		private void UpdatePriorToBuildingSelected()
		{
			// TODO: Update which building we're hovering over.
		}

		private void UpdateAfterBuildingSelected()
		{
			PlanetComponent buildingPlanet = _movingBuilding.BuildingInstance.ParentPlanet;

			Vector2 mousePosition = _selectionCursor.GetPosition();
			Vector2 pointOnPlanet = buildingPlanet.GetClosestSurfacePointToPosition(mousePosition);

			float distanceFromPlanet = Vector2.Distance(pointOnPlanet, mousePosition);

			if (distanceFromPlanet > GlobalBuildingSettings.GetOrCreateSettings().MaxDistanceToPlanetToShowBuildingCursor)
			{
				SetProspectingPlanet(null);
				HideAllCursors();
			}
			else
			{
				SetProspectingPlanet(buildingPlanet);
				Vector2 upNormal = buildingPlanet.GetNormalForPosition(pointOnPlanet);
				_buildingCursor.Show();
				foreach (CableMover cableMover in _cableMovers)
				{
					cableMover.ShowCursor();
					cableMover.UpdateCableCursorPosition();
				}
				_buildingCursor.SetPositionAndUpNormal(pointOnPlanet, upNormal, buildingPlanet);
			}
		}

		public void UpdatePostConstraints(BuildStateInput input, MoveConstraintsQueryResult constraintsResult)
		{
			// TODO: Highlight hovering buildings?

			if (_movingBuilding == null)
				UpdatePostConstraintsPriorToBuildingSelected(input);
			else
				UpdatePostConstraintsAfterBuildingSelected(input, constraintsResult);
		}

		private void UpdatePostConstraintsPriorToBuildingSelected(BuildStateInput input)
		{
			if (input.primaryFire)
			{
				// If clicking on a building & we have authority over the building, set it to be the moving
				// building.
				Collider2D hoveringBuildingCollider = _selectionCursor.FindFirstByPredicate((Collider2D collider) =>
				{
					return collider.TryGetComponentInParent(out BuildingComponent building) && building.GetClientHasAuthority();
				});

				if (hoveringBuildingCollider == null)
				{
					BuildingComponent hoveringAnyBuilding = _selectionCursor.FindFirstBuilding();
					if (hoveringAnyBuilding)
					{
						// TODO: Play failed sound.
					}
					else
					{
						// If clicking on nothing, exit this state.
						Cancel();
					}
					return;
				}

				BuildingComponent hoveringBuilding = hoveringBuildingCollider.GetComponentInParent<BuildingComponent>();
				TrySetMovingBuilding(hoveringBuilding, true);
			}
			else if (input.secondaryFire)
			{
				Cancel();
				return;
			}
		}

		private void UpdatePostConstraintsAfterBuildingSelected(BuildStateInput input, MoveConstraintsQueryResult constraintsResult)
		{
			UpdateCursorColors(constraintsResult);

			if (input.primaryFire)
			{
				// If the cursor is near a planet try to place the building.
				if (_prospectivePlanet != null)
				{
					if (constraintsResult.GetBuildingResult() < BuildWarning.WarningType.FATAL)
					{
						Apply();
					}
					else
					{
						// TODO: Play a failed sound.
					}
					return;
				}

				// TODO: Decide whether we want to check if the player is clicking on a building &
				// have separate logic for that. Niche case here since the mouse will usually be near a planet
				// when clicking on a building.

				// If clicking on nothing, exit this state.
				Cancel();
				return;
			}
			else if (input.secondaryFire)
			{
				// Stop moving the current building.
				TrySetMovingBuilding(null, true);
				return;
			}
		}

		private void UpdateCursorColors(MoveConstraintsQueryResult constraintsResult)
		{
			_buildingCursor.SetBuildingPlaceability(constraintsResult.GetBuildingResult());

			for (int i = 0; i < _cableMovers.Count; i++)
			{
				_cableMovers[i].UpdateCableCursorColor(constraintsResult.GetCableResults()[i]);
			}
		}

		private void HideAllCursors()
		{
			_buildingCursor.Hide();
			foreach (CableMover cableMover in _cableMovers)
				cableMover.HideCursor();
		}

		private void Cancel()
		{
			BuildStateTransitionSignal signal = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(signal);
		}

		private void Apply()
		{
			_buildingCursor.MoveBuildingToLocation(_movingBuilding.BuildingInstance);
			
			BuildStateApplyResult result = new MoveBuildStateApplyResult {
				moved = _movingBuilding.BuildingInstance
			};
			OnApplied.Invoke(result);
			
			TrySetMovingBuilding(null, true);
		}

		public void CleanUp()
		{
			_buildingCursor.Hide();

			foreach (CableMover cableMover in _cableMovers)
				cableMover.Dispose();

			_cableMovers.Clear();
		}
	}
}
