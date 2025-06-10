using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class AdvancedMaterialsCostBuildingConstraintComponent : BuildingConstraintComponent
	{
		private const string ADVANCED_MATERIALS_RICH_TEXT_ICON = "<sprite =\"text-icons\" name=\"science\">";

		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			int buildingScienceCost = state.buildState.ToBuild.BuildingSettings.BuildingDataAsset.scienceCost;
			
			if (buildingScienceCost > 0)
			{
				int scienceAfterPurchase = gameController.HeldScience - buildingScienceCost;
				bool sufficientScience = scienceAfterPurchase >= 0;

				string postfix = "";
				BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;
				
				// TODO: Use advanced materials symbol.
				if (!sufficientScience)
				{
					postfix = $"(Missing {ADVANCED_MATERIALS_RICH_TEXT_ICON}{Mathf.Abs(scienceAfterPurchase)})";
					warningType = BuildWarning.WarningType.FATAL;
				}

				BuildWarning warning = new BuildWarning($"Costs {ADVANCED_MATERIALS_RICH_TEXT_ICON}{buildingScienceCost}{postfix}.", warningType);
				result.AddWarning(warning);
			}

			return result;
		}
	}
}
