using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class PlanetPlaceabilityBuildingConstraintComponent : BuildingConstraintComponent
    {
		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			if (!state.buildingCursor.ParentPlanet.CanPlaceBuildings)
			{
				BuildWarning warning = new BuildWarning("Cannot place buildings on this celestial body.", BuildWarning.WarningType.FATAL);
				return new ConstraintQueryResult(true, warning);
			}

			return new ConstraintQueryResult();
		}
    }
}
