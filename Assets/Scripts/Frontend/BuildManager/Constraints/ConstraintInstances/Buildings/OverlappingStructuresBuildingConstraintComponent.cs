using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class OverlappingStructuresBuildingConstraintComponent : BuildingPlacerConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(IBuildingPlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (state.GetProspectivePlanet() == null)
				return result;

			BuildingCursorComponent buildingCursor = state.GetBuildingCursor();

			// Determine whether the building can be placed.
			Collider2D[] overlappingColliders = buildingCursor.QueryOverlappingColliders();

			// The only thing that the building should be colliding with is the parent planet.
			bool roomToPlace = overlappingColliders.Length == 1 && buildingCursor.ParentPlanet.OwnsCollider(overlappingColliders[0]);
			if (!roomToPlace)
			{
				BuildWarning warning = new BuildWarning("Building overlaps with other structures.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}
	}
}
