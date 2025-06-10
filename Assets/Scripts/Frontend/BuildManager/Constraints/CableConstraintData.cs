using UnityEngine;

namespace AstralAvarice.Frontend
{
    public struct CableConstraintData
    {
		public readonly CableBuildState cableState;
		public readonly CableCursorComponent cableCursor;
		public readonly int priorCashCosts;

		public CableConstraintData(
			CableBuildState cableState,
			CableCursorComponent cableCursor,
			int priorCashCosts
			)
		{
			this.cableState = cableState;
			this.cableCursor = cableCursor;
			this.priorCashCosts = priorCashCosts;
		}
    }
}
