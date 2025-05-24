using UnityEngine;

namespace AstralAvarice.Frontend
{
    public interface IBuildingConstraint : IBuildConstraint<BuildingConstraintData> { }

	public abstract class BuildingConstraintComponent : MonoBehaviour, IBuildingConstraint
	{
		public abstract ConstraintQueryResult QueryConstraint(BuildingConstraintData state);
	}
}
