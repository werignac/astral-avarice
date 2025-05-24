using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class SpecialResourcesBuildingConstraintComponent : BuildingConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			BuildingBuildState buildingBuildState = state.buildState;
			BuildingCursorComponent buildingCursor = state.buildingCursor;

			if (buildingBuildState.toBuild.BuildingDataAsset.requiredResource != ResourceType.Resource_Count)
			{
				if (buildingCursor.ParentPlanet.GetResourceCount(buildingBuildState.toBuild.BuildingDataAsset.requiredResource) <= 0
					|| buildingCursor.ParentPlanet.GetAvailableResourceCount(buildingBuildState.toBuild.BuildingDataAsset.requiredResource) < buildingBuildState.toBuild.BuildingDataAsset.resourceAmountRequired)
				{
					BuildWarning warning = new BuildWarning("Missing Special Resources.", BuildWarning.WarningType.ALERT);
					return new ConstraintQueryResult(false, warning); // Send false because you're allowed to place a building on a planet w/o special resources.
				}
			}

			return new ConstraintQueryResult();
		}
	}
}
