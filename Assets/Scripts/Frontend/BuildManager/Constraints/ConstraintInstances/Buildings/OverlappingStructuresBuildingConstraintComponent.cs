using UnityEngine;
using System;
using werignac.Utils;

namespace AstralAvarice.Frontend
{
	public class OverlappingStructuresBuildingConstraintComponent : BuildingPlacerConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(IBuildingPlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();
			PlanetComponent placingPlanet = state.GetProspectivePlanet();
			if (placingPlanet == null)
				return result;

			BuildingCursorComponent buildingCursor = state.GetBuildingCursor();

			// Determine whether the building can be placed.
			Collider2D[] overlappingColliders = buildingCursor.QueryOverlappingColliders();

			IPlacingBuilding placingBuilding = state.GetPlacingBuilding();

			// Function that returns true when the building cursor is allowed to overlap with the passed collider.
			// Returns false otherwise.
			Func<Collider2D, bool> filter;
			if (placingBuilding is NewPlacingBuilding newPlacingBuilding)
			{
				filter = GetNewBuildingColliderFilter(newPlacingBuilding, placingPlanet);
			}
			else if (placingBuilding is ExistingPlacingBuilding existingPlacingBuilding)
			{
				filter = GetExistingBuildingColliderFilter(existingPlacingBuilding, placingPlanet);
			}
			else
			{
				throw new Exception($"Cannot recognize placing building {placingBuilding} to get an overlap filter.");
			}

			// The only thing that the building should be colliding with is the parent planet,
			// itself if moving, or one of its colliders if moving.
			bool anyUnacceptableOverlap = false;
			foreach (Collider2D collider in overlappingColliders)
			{
				if (! filter(collider))
				{
					anyUnacceptableOverlap = true;
					break;
				}
			}
			
			if (anyUnacceptableOverlap)
			{
				BuildWarning warning = new BuildWarning("Building overlaps with other structures.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}

		/// <summary>
		/// Returns a function that checks whether a collider belongs the the planet
		/// that this building is being placed on. Any other collisions between
		/// the building cursor and colliders are unacceptable for a new building.
		/// </summary>
		/// <param name="newPlacingBuilding">The new building to place.</param>
		/// <param name="placingPlanet">The planet the building is being placed on.</param>
		/// <returns>A filter function.</returns>
		private Func<Collider2D, bool> GetNewBuildingColliderFilter(
				NewPlacingBuilding newPlacingBuilding,
				PlanetComponent placingPlanet
			)
		{
			return (Collider2D collider) => placingPlanet.GetIsPlanetCollider(collider);
		}

		/// <summary>
		/// Returns a function that checks whether a collider belongs to the planet
		/// that this building is being moved on, the building that is being moved,
		/// or a cable of the building that is being moved. Any other
		/// collisions between the moving building and colliders are unacceptable.
		/// </summary>
		/// <param name="existingPlacingbuilding">The building being moved.</param>
		/// <param name="placingPlanet">The planet the building is being moved on.</param>
		/// <returns>A filter function.</returns>
		private Func<Collider2D, bool> GetExistingBuildingColliderFilter(
				ExistingPlacingBuilding existingPlacingbuilding,
				PlanetComponent placingPlanet
			)
		{
			return (Collider2D collider) =>
			{
				if (placingPlanet.GetIsPlanetCollider(collider))
					return true;

				if (existingPlacingbuilding.BuildingInstance.GetIsBuildingCollider(collider))
					return true;

				if (collider.TryGetComponentInParent(out CableComponent cableComponent))
				{
					if (cableComponent.GetConnectsTo(existingPlacingbuilding.BuildingInstance))
						return true;
				}

				return false;
			};
		}
	}
}
