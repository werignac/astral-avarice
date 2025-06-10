using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class CashCostBuildingConstraintComponent : BuildingConstraintComponent
	{
		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(BuildingConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (!state.buildState.HasProspectivePlacement())
				return result;

			int buildingCashCost = state.buildState.ToBuild.BuildingSettings.BuildingDataAsset.cost;
			int cashAfterPurchase = gameController.Cash - (state.priorCashCosts + buildingCashCost);
			bool sufficientCash = cashAfterPurchase >= 0;

			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;

			if (!sufficientCash)
			{
				postfix = $" (Missing ${Mathf.Abs(cashAfterPurchase)})";
				warningType = BuildWarning.WarningType.FATAL;
			}

			BuildWarning warning = new BuildWarning($"Cost ${buildingCashCost}{postfix}.", warningType);

			result.AddWarning(warning);

			return result;
		}
	}
}
