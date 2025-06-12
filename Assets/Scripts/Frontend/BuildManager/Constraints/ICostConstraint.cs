using UnityEngine;

namespace AstralAvarice.Frontend
{
    public interface ICostConstraint : IBuildConstraint<CostConstraintData> { }

	public abstract class CostConstraintComponent : MonoBehaviour, ICostConstraint
	{
		public abstract ConstraintQueryResult QueryConstraint(CostConstraintData data);
	}
}
