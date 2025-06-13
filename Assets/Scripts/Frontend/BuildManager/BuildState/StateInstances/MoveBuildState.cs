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
		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged { get; } = new UnityEvent<PlanetComponent>();

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

			// TODO: Update the building cursor.
			if (updateCursors)
			{
				if (toPlace == null)
				{
					HideAllCursors();
				}
				else
				{
					BuildingVisuals visualAsset = toPlace.BuildingInstance.BuildingVisuals;
					_buildingCursor.SetGhost(visualAsset);

					_buildingCursor.SetBuildingCollisionAndCableConnectionOffsetFromBuilding(toPlace.BuildingInstance);

					bool canHaveConnections = toPlace.BuildingInstance.Data.maxPowerLines > 0;
					_buildingCursor.SetShowCableConnectionCursor(canHaveConnections);

					// TODO: Get cable cursors from pool.
				}
			}
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
				foreach (CableCursorComponent cableCursor in _cableCursors)
				{
					cableCursor.Show();
					cableCursor.SetEndPoint(_buildingCursor.CableConnectionPosition);
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

				// TODO: Decide whether we want to check if the player is clicking on a building.

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

			for (int i = 0; i < _cableCursors.Length; i++)
			{
				_cableCursors[i].SetCablePlaceability(constraintsResult.GetCableResults()[i] < BuildWarning.WarningType.FATAL);
			}
		}

		private void HideAllCursors()
		{
			_buildingCursor.Hide();
			foreach (CableCursorComponent cableCursor in _cableCursors)
				cableCursor.Hide();
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
			HideAllCursors();
		}
	}
}
