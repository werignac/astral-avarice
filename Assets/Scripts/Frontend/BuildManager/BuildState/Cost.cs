using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Represents the required monetary resources the player needs to expend to place a structure.
	/// </summary>
	public struct Cost
	{
		public int cash;
		public int science;

		/// <summary>
		/// Returns a new cost with the sum of the two costs.
		/// </summary>
		/// <param name="otherCost">Another cost to sum to this cost.</param>
		/// <returns>The new cost.</returns>
		public Cost Add(Cost otherCost)
		{
			return new Cost
			{
				cash = cash + otherCost.cash,
				science = science + otherCost.science
			};
		}
	}
}
