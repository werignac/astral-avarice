using UnityEngine;

namespace AstralAvarice.Frontend
{
	public struct BuildStateInput
	{
		/// <summary>
		/// Whether the primary fire button was pressed (M1). This is commonly
		/// used for applying states (demolishing, placing a building, placing a cable, etc.)
		/// </summary>
		public bool primaryFire;
		/// <summary>
		/// Whether the secondary fire button was pressed (M2). This is commonly
		/// used for cancelling states.
		/// </summary>
		public bool secondaryFire;
	}
}
