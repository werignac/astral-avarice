using UnityEngine;
using System.Collections.Generic;
using werignac.Utils;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Restricts the cable to only overlap the buildings it will connect to
	/// and the other cables that connect to those buildings.
	/// </summary>
	public class OverlappingStructuresCableConstraintComponent : CablePlacerConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(ICablePlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			CableCursorComponent cableCursor = state.GetCableCursor();

			// If a start and an end have not been made yet, this constraint is not applicable yet.
			if (!cableCursor.GetHasValidStartAndEnd())
				return result;

			List<Collider2D> cableOverlaps = new List<Collider2D>(cableCursor.QueryOverlappingColliders());
			
			BuildingComponent fromBuilding;
			BuildingComponent toBuilding;

			if (state.TryGetMovingCable(out CableComponent movingCable))
			{
				// It is assumed that moving cables do not change
				// start / end. Cables are only moving when a building moves.
				fromBuilding = movingCable.Start;
				toBuilding = movingCable.End;
			}
			else
			{
				fromBuilding = GetBuildingComponentFromAttachment(state.GetFromAttachment());
				toBuilding = GetBuildingComponentFromAttachment(state.GetToAttachment());
			}

			int badOverlapIndex = cableOverlaps.FindIndex((Collider2D collider) =>
			{
				return !IsValidCableOverlap(collider, fromBuilding, toBuilding, movingCable);
			});
			bool overlapsAlongCable = badOverlapIndex != -1;

			if (overlapsAlongCable)
			{
				BuildWarning warning = new BuildWarning("Cable overlaps with other structures.", BuildWarning.WarningType.FATAL);
				result.AddWarning(warning);
			}

			return result;
		}

		private static BuildingComponent GetBuildingComponentFromAttachment(ICableAttachment attachment)
		{
			if (attachment != null && attachment is BuildingInstanceCableAttachment buildingAttachment)
				return buildingAttachment.BuildingInstance;

			return null;
		}

		/// <summary>
		/// Determines whether a cable can overlap over the given collider.
		/// Cables can only overlap over buildings that they connect to or other
		/// Cables that share the same builing connections.
		/// </summary>
		private static bool IsValidCableOverlap(
				Collider2D overlapping,
				BuildingComponent startBuilding,
				BuildingComponent endBuildling,
				CableComponent movingCable
			)
		{
			if (overlapping.TryGetComponentInParent(out BuildingComponent overlapBuilding))
			{
				return (overlapBuilding == startBuilding) || (overlapBuilding == endBuildling);
			}

			if (overlapping.TryGetComponentInParent(out CableComponent overlapCable))
			{
				bool startIsConnectingBuilding = (overlapCable.Start == startBuilding) || (overlapCable.Start == endBuildling);
				bool endIsConnectingBuilding = (overlapCable.End == startBuilding) || (overlapCable.End == endBuildling);
				// NOTE: Currently, the next check does nothing because the last two checks are always true when the
				// bottom check is true. If we have multi-segment cables, this check will become relevant.
				bool isMovingCable = movingCable != null && overlapCable == movingCable;

				return startIsConnectingBuilding || endIsConnectingBuilding || isMovingCable;
			}

			return false;
		}
	}
}
