using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class CostConstraintData
    {
		/// <summary>
		/// The cost of this item / action.
		/// </summary>
		public Cost cost;
		/// <summary>
		/// Other costs that must be accounted for before this cost.
		/// </summary>
		public Cost preceedingCosts;
    }
}
