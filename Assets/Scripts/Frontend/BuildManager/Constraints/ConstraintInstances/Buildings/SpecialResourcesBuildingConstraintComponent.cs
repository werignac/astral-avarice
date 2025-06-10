using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class SpecialResourcesBuildingConstraintComponent : BuildingConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (!state.buildState.HasProspectivePlacement())
				return result;

			BuildingBuildState buildingBuildState = state.buildState;
			BuildingCursorComponent buildingCursor = state.buildingCursor;
			BuildingData buildingData = buildingBuildState.ToBuild.BuildingSettings.BuildingDataAsset;

			if (buildingData.requiredResource != ResourceType.Resource_Count)
			{
				if (buildingCursor.ParentPlanet.GetResourceCount(buildingData.requiredResource) <= 0
					|| buildingCursor.ParentPlanet.GetAvailableResourceCount(buildingData.requiredResource) < buildingData.resourceAmountRequired)
				{
					BuildWarning warning = new BuildWarning("Missing Special Resources.", BuildWarning.WarningType.ALERT);
					result.AddWarning(warning);
				}
			}

			return result;
		}
	}
}
