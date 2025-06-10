using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class OverlappingStructuresBuildingConstraintComponent : BuildingConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			// Determine whether the building can be placed.
			Collider2D[] overlappingColliders = state.buildingCursor.QueryOverlappingColliders();

			// The only thing that the building should be colliding with is the parent planet.
			bool roomToPlace = overlappingColliders.Length == 1 && state.buildingCursor.ParentPlanet.OwnsCollider(overlappingColliders[0]);
			if (!roomToPlace)
			{
				BuildWarning warning = new BuildWarning("Building overlaps with other structures.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}
	}
}
