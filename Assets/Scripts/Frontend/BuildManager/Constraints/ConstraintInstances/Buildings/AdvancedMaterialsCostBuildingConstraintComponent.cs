using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class AdvancedMaterialsCostBuildingConstraintComponent : BuildingConstraintComponent
	{
		private const string ADVANCED_MATERIALS_RICH_TEXT_ICON = "<sprite =\"text-icons\" name=\"science\">";

		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			int buildingScienceCost = state.buildState.toBuild.BuildingDataAsset.scienceCost;
			if (buildingScienceCost > 0)
			{
				int scienceAfterPurchase = gameController.HeldScience - buildingScienceCost;
				bool sufficientScience = scienceAfterPurchase >= 0;

				string postfix = "";
				BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;
				bool constraintTriggered = false;
				
				// TODO: Use advanced materials symbol.
				if (!sufficientScience)
				{
					postfix = $"(Missing {ADVANCED_MATERIALS_RICH_TEXT_ICON}{Mathf.Abs(scienceAfterPurchase)})";
					warningType = BuildWarning.WarningType.FATAL;
					constraintTriggered = true;
				}

				BuildWarning warning = new BuildWarning($"Costs {ADVANCED_MATERIALS_RICH_TEXT_ICON}{buildingScienceCost}{postfix}.", warningType);

				return new ConstraintQueryResult(constraintTriggered, warning);
			}

			return new ConstraintQueryResult();
		}
	}
}
