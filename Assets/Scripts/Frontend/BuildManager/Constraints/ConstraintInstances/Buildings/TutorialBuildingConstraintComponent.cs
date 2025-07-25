using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// NOTE: May need to be renamed if we make use of multiple constraints
	/// in the tutorial.
	/// </summary>
	public class TutorialBuildingConstraintComponent : BuildingPlacerConstraintComponent
	{
		/// <summary>
		/// The world-space position of where the building is supposed to be placed in the tutorial.
		/// </summary>
		private Vector2 _buildingPlacementTarget;

		/// <summary>
		/// The maximum distance the placed building can be from the target position.
		/// </summary>
		private float _maximumTargetDistance;


		/// <summary>
		/// Sets the fields that determine wher the building can be placed and
		/// how far. Called by the TutorialGameController.
		/// </summary>
		/// <param name="placementTarget">The place the building should be placed at.</param>
		/// <param name="maxDistance">The max distance the building can have from this location.</param>
		public void SetPlacementTargetAndMaxDistance(Vector2 placementTarget, float maxDistance)
		{
			_buildingPlacementTarget = placementTarget;
			_maximumTargetDistance = maxDistance;
		}

		protected override ConstraintQueryResult QueryConstraint_Internal(IBuildingPlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			if (!state.GetHasProspectivePlanet())
				return result;

			float distanceFromTarget = Vector2.Distance(state.GetBuildingCursor().transform.position, _buildingPlacementTarget);

			if (distanceFromTarget > _maximumTargetDistance)
			{
				BuildWarning warning = new BuildWarning("Building not in the position indicated by the tutorial.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}
	}
}
