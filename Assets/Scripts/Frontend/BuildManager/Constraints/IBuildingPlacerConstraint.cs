using UnityEngine;

namespace AstralAvarice.Frontend
{
    public interface IBuildingPlacerConstraint : IBuildConstraint<IBuildingPlacer> { }

	public abstract class BuildingPlacerConstraintComponent : MonoBehaviour, IBuildingPlacerConstraint
	{
		public abstract ConstraintQueryResult QueryConstraint(IBuildingPlacer state);
	}
}
