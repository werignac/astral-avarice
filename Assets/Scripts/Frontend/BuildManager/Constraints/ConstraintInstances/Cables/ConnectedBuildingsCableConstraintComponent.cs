using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Constraint that forces the cables that a building will connect to fit certain
	/// requirements.
	/// </summary>
	public class ConnectedBuildingsCableConstraintComponent : CableConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(CableConstraintData state)
		{
			// TODO: Get the building the player is hovering over. We may need a way to put multiple
			// warnings in the ConstraintQueryResult because we need to do this for each building.
			throw new System.NotImplementedException();
		}
	}
}
