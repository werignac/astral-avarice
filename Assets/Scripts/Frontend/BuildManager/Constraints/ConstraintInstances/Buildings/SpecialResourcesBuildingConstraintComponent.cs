using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class SpecialResourcesBuildingConstraintComponent : BuildingPlacerConstraintComponent
	{
		protected override ConstraintQueryResult QueryConstraint_Internal(IBuildingPlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			PlanetComponent placingPlanet = state.GetProspectivePlanet();

			if (placingPlanet == null)
				return result;

			IPlacingBuilding placingBuilding = state.GetPlacingBuilding();

			ResourceType requiredResourceType = ResourceType.Resource_Count;
			int requiredResourceCount = 0;

			if (placingBuilding is NewPlacingBuilding newPlacingBuilding)
			{
				BuildingData buildingData = newPlacingBuilding.BuildingSettings.BuildingDataAsset;

				requiredResourceType = buildingData.requiredResource;
				requiredResourceCount = buildingData.resourceAmountRequired;
			}
			else if (placingBuilding is ExistingPlacingBuilding existingPlacingBuilding)
			{
				requiredResourceType = existingPlacingBuilding.BuildingInstance.Data.requiredResource;
				requiredResourceCount = existingPlacingBuilding.BuildingInstance.Data.resourceAmountRequired;

				if (placingPlanet == existingPlacingBuilding.BuildingInstance.ParentPlanet)
				{
					// If we're moving on the same planet, don't include the resources already provided in the calc
					// for missing resources.
					requiredResourceCount -= existingPlacingBuilding.BuildingInstance.BackendBuilding.ResourcesProvided;
				}
			}

			if (requiredResourceType != ResourceType.Resource_Count && GetPlanetMissingResources(placingPlanet, requiredResourceType, requiredResourceCount))
			{
				result.AddWarning(new BuildWarning("Missing special resources.", BuildWarning.WarningType.ALERT));
			}

			return result;
		}

		private static bool GetPlanetMissingResources(PlanetComponent planet, ResourceType resourceType, int minimumAmount)
		{
			Debug.Assert(resourceType != ResourceType.Resource_Count);
			return planet.GetResourceCount(resourceType) <= 0 || planet.GetAvailableResourceCount(resourceType) < minimumAmount;
		}
	}
}
