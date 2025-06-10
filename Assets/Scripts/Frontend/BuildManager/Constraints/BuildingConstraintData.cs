using UnityEngine;

namespace AstralAvarice.Frontend
{
    public struct BuildingConstraintData
	{
		public readonly BuildingBuildState buildState;
		public readonly BuildingCursorComponent buildingCursor;
		public readonly int priorCashCosts;

		public BuildingConstraintData(
			BuildingBuildState buildState,
			BuildingCursorComponent buildingCursor,
			int priorCashCosts
			)
		{
			this.buildState = buildState;
			this.buildingCursor = buildingCursor;
			this.priorCashCosts = priorCashCosts;
		}
	}
}
