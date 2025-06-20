using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class PlanetPlaceabilityBuildingConstraintComponent : BuildingPlacerConstraintComponent
    {
		protected override ConstraintQueryResult QueryConstraint_Internal(IBuildingPlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			PlanetComponent planet = state.GetProspectivePlanet();

			if (planet == null)
				return result;

			if (!planet.CanPlaceBuildings)
			{
				BuildWarning warning = new BuildWarning("Cannot place buildings on this celestial body.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}
    }
}
