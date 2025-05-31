using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class CashCostCableConstraintComponent : CableConstraintComponent
    {
		[SerializeField] private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(CableConstraintData state)
		{
			float cableLength = state.cableCursor.Length;
			int cableCost = Mathf.CeilToInt(cableLength * Data.cableCostMultiplier);

			// TODO: Get other expenses somehow (e.g. resolution.totalCost).
			int otherExpenses = 0;
			int remainingCash = gameController.Cash - (otherExpenses + cableCost);
			bool canAffordCable = remainingCash >= 0;

			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;
			bool constraintTriggered = false;

			if (!canAffordCable)
			{
				postfix = $" (Missing ${Mathf.Abs(remainingCash)})";
				warningType = BuildWarning.WarningType.FATAL;
				constraintTriggered = true;
			}

			BuildWarning warning = new BuildWarning($"Cable Costs ${cableCost}{postfix}.", warningType);

			return new ConstraintQueryResult(constraintTriggered, warning);
		}
	}
}
