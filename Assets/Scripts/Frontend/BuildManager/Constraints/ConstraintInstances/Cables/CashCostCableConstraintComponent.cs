using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class CashCostCableConstraintComponent : CableConstraintComponent
    {
		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(CableConstraintData state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			// If the player hasn't clicked on the first attach point, don't show a warning.
			if (!state.cableState.GetIsFromAttachmentSetAndNonVolatile())
				return result;

			float cableLength = state.cableState.Length;
			int cableCost = Mathf.CeilToInt(cableLength * Data.cableCostMultiplier);

			// TODO: Get other expenses somehow (e.g. resolution.totalCost).
			int otherExpenses = 0;
			int remainingCash = gameController.Cash - (otherExpenses + cableCost);
			bool canAffordCable = remainingCash >= 0;

			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;

			if (!canAffordCable)
			{
				postfix = $" (Missing ${Mathf.Abs(remainingCash)})";
				warningType = BuildWarning.WarningType.FATAL;
			}

			BuildWarning warning = new BuildWarning($"Cable Costs ${cableCost}{postfix}.", warningType);
			result.AddWarning(warning);

			return result;
		}
	}
}
