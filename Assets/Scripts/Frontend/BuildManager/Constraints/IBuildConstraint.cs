using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// A build constraint is a rule that determines whether a structure (building or cable)
	/// can be placed. Examples include: affordability, overlapping with other structures, and cable length.
	/// </summary>
	/// <typeparam name="StateType">The state the constraint takes in and checks whether it is valid.</typeparam>
    public interface IBuildConstraint<StateType>
    {
		public ConstraintQueryResult QueryConstraint(StateType state);
    }
}
