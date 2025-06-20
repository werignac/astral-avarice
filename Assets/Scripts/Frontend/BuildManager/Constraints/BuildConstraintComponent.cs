using UnityEngine;

namespace AstralAvarice.Frontend
{
	public abstract class BuildConstraintComponent<StateType> : MonoBehaviour, IBuildConstraint<StateType>
	{
		public ConstraintQueryResult QueryConstraint(StateType state)
		{
			if (!isActiveAndEnabled)
				return new ConstraintQueryResult();

			return QueryConstraint_Internal(state);
		}

		protected abstract ConstraintQueryResult QueryConstraint_Internal(StateType state);
	}
}
