using UnityEngine;

namespace AstralAvarice.Frontend
{
    public interface ICableConstraint: IBuildConstraint<CableConstraintData> { }

	public abstract class CableConstraintComponent : MonoBehaviour, ICableConstraint
	{
		public abstract ConstraintQueryResult QueryConstraint(CableConstraintData state);
	}
}
