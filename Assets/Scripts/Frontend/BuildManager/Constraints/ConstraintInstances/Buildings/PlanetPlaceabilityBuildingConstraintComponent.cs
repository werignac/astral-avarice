using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class PlanetPlaceabilityBuildingConstraintComponent : BuildingConstraintComponent
    {
		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (!state.buildState.HasProspectivePlacement())
				return result;

			if (!state.buildingCursor.ParentPlanet.CanPlaceBuildings)
			{
				BuildWarning warning = new BuildWarning("Cannot place buildings on this celestial body.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}
    }
}
