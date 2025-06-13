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
	public class MoveBuildState : IPostConstraintsBuildState<MoveConstraintsQueryResult>, IBuildingPlacer
	{
		/// <summary>
		/// Null when no building has been selected yet. Otherwise contains the instance of a building being moved.
		/// </summary>
		private ExistingPlacingBuilding _movingBuilding;
		private PlanetComponent _prospectivePlanet;

		private SelectionCursorComponent _selectionCursor;
		private BuildingCursorComponent _buildingCursor;
		private CableCursorComponent[] _cableCursors = new CableCursorComponent[0];

		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();

		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged => throw new System.NotImplementedException();

		public BuildStateType GetStateType() => BuildStateType.MOVE;

		public MoveBuildState(
				SelectionCursorComponent selectionCursor,
				BuildingCursorComponent buildingCursor,
				BuildingComponent toMove = null
			)
		{
			if (selectionCursor == null)
				throw new ArgumentNullException("selectionCursor");

			_selectionCursor = selectionCursor;

			if (buildingCursor == null)
				throw new ArgumentNullException("buildingCursor");

			_buildingCursor = buildingCursor;

			// TODO: Get a pool of cable cursors instead of an array.

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
				SetMovingBuilding(null, false);
				return true;
			}
			
			if (toMove != null && !toMove.GetClientHasAuthority())
				return false;

			SetMovingBuilding(new ExistingPlacingBuilding(toMove), updateBuildingCursor);
			return true;
		}

		private void SetMovingBuilding(ExistingPlacingBuilding toPlace, bool updateBuildingCursor)
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

			// TODO: Update the building cursor.
			if (updateBuildingCursor)
			{
				if (toPlace == null)
					_buildingCursor.Hide();
				else
				{
					
				}
			}
		}

		private void MovingBuilding_OnDemolish(BuildingComponent demolishedBuilding)
		{
			Debug.Assert(demolishedBuilding == _movingBuilding.BuildingInstance);
			SetMovingBuilding(null, true);
		}

		public void Start()
		{
			
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

		public void Update(BuildStateInput input)
		{
			// TODO: Find a placement on the nearest planet / on the only planet for the building.
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

		private void UpdatePostConstraintsAfterBuildingSelected(BuildStateInput input, MoveConstraintsQueryResult constraintsresult)
		{
			if (input.primaryFire)
			{
				// TODO: If the cursor is near a planet try to place the building.

				// TODO: If clicking on nothing, exit this state?
			}
			else if (input.secondaryFire)
			{
				// Stop moving the current building.
				TrySetMovingBuilding(null, true);
				return;
			}
		}

		private void Cancel()
		{
			BuildStateTransitionSignal signal = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(signal);
		}

		public void CleanUp()
		{
			_buildingCursor.Hide();
			foreach (CableCursorComponent cableCursor in _cableCursors)
				cableCursor.Hide();
		}
	}
}
