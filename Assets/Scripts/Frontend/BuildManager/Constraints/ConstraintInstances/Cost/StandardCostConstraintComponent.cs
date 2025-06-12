using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class StandardCostConstraintComponent : CostConstraintComponent
	{
		private const string ADVANCED_MATERIALS_RICH_TEXT_ICON = "<sprite =\"text-icons\" name=\"science\">";
		
		[SerializeField]
		private GameController gameController;

		public override ConstraintQueryResult QueryConstraint(CostConstraintData data)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (TryGetAffordabilityWarning(data.cost.cash, gameController.Cash - data.preceedingCosts.science, "$", out BuildWarning cashWarning))
			{
				result.AddWarning(cashWarning);
			}

			if (TryGetAffordabilityWarning(data.cost.science, gameController.HeldScience - data.preceedingCosts.science, ADVANCED_MATERIALS_RICH_TEXT_ICON, out BuildWarning scienceWarning))
			{
				result.AddWarning(scienceWarning);
			}

			return result;
		}

		private static bool TryGetAffordabilityWarning(int cost, int currentAmount, string resourceSymbol, out BuildWarning warning)
		{
			if (cost > 0)
			{
				int remainderPostPurchase = currentAmount - cost;
				bool sufficientResource = remainderPostPurchase >= 0;

				string postfix = "";
				BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;

				// TODO: Use advanced materials symbol.
				if (!sufficientResource)
				{
					postfix = $"(Missing {resourceSymbol}{Mathf.Abs(remainderPostPurchase)})";
					warningType = BuildWarning.WarningType.FATAL;
				}

				warning = new BuildWarning($"Costs {resourceSymbol}{cost}{postfix}.", warningType);
				return true;
			}

			warning = new BuildWarning();
			return false;
		}


	}
}
