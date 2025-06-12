using UnityEngine;

namespace AstralAvarice.Frontend
{
    public interface ICablePlacerConstraint: IBuildConstraint<ICablePlacer> { }

	public abstract class CablePlacerConstraintComponent : MonoBehaviour, ICablePlacerConstraint
	{
		public abstract ConstraintQueryResult QueryConstraint(ICablePlacer state);
	}
}
