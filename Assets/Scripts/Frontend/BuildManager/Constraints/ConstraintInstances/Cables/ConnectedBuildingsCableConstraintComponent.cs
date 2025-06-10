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
			ConstraintQueryResult result = new ConstraintQueryResult();

			ICableAttachment fromAttachment = state.cableState.GetFromAttachment();
			ICableAttachment toAttachment = state.cableState.GetToAttachment();

			if (TryGetWarningForAttachment(fromAttachment, "starting", out BuildWarning fromWarning))
				result.AddWarning(fromWarning);

			// Don't show the warnings for the "to" building while we haven't set the "from" building yet.
			if (state.cableState.GetIsFromAttachmentSetAndNonVolatile())
			{
				if (TryGetWarningForAttachment(toAttachment, "ending", out BuildWarning toWarning))
					result.AddWarning(toWarning);

				if (TryGetWarningForRedundancy(fromAttachment, toAttachment, out BuildWarning redundantWarning))
					result.AddWarning(redundantWarning);
			}

			return result;
		}

		public bool TryGetWarningForAttachment(ICableAttachment attachment, string attachmentName, out BuildWarning warning)
		{
			if (attachment == null)
			{
				warning = GetMissingAttachmentWarning(attachmentName);
				return true;
			}

			if (attachment is CursorCableAttachment)
			{
				warning = GetMissingAttachmentWarning(attachmentName);
				return true;
			}

			if (attachment is BuildingInstanceCableAttachment buildingAttachment)
			{
				if (buildingAttachment.BuildingInstance.Data.maxPowerLines <= 0)
				{
					warning = GetBuildingTakesNoConnectionsWarning(buildingAttachment.BuildingInstance);
					return true;
				}

				if (! buildingAttachment.BuildingInstance.BackendBuilding.CanAcceptNewConnections())
				{
					warning = GetMissingConnectionsWarning(buildingAttachment.BuildingInstance);
					return true;
				}
			}

			warning = new BuildWarning();
			return false;
		}

		private static BuildWarning GetMissingAttachmentWarning(string attachmentName)
		{
			return new BuildWarning($"Missing {attachmentName} attachment.", BuildWarning.WarningType.FATAL);
		}

		private static BuildWarning GetBuildingTakesNoConnectionsWarning(BuildingComponent building)
		{
			return new BuildWarning($"{building.Data.buildingName} does not support connections.", BuildWarning.WarningType.FATAL);
		}

		private static BuildWarning GetMissingConnectionsWarning(BuildingComponent building)
		{
			return new BuildWarning($"{building.Data.buildingName} cannot support any more connections. Limit is {building.Data.maxPowerLines}.", BuildWarning.WarningType.FATAL);
		}

		public bool TryGetWarningForRedundancy(ICableAttachment fromAttachment, ICableAttachment toAttachment, out BuildWarning warning)
		{
			if (fromAttachment == null || toAttachment == null)
			{
				warning = new BuildWarning();
				return false;
			}

			bool fromAttachesToBuilding = fromAttachment is BuildingInstanceCableAttachment;
			bool toAttachesToBuilding = toAttachment is BuildingInstanceCableAttachment;

			bool bothAttachToBuildings = fromAttachesToBuilding && toAttachesToBuilding;

			if (! bothAttachToBuildings)
			{
				warning = new BuildWarning();
				return false;
			}

			BuildingInstanceCableAttachment fromBuildingAttachment = fromAttachment as BuildingInstanceCableAttachment;
			BuildingInstanceCableAttachment toBuildingAttachment = toAttachment as BuildingInstanceCableAttachment;

			if (fromBuildingAttachment.BuildingInstance == toBuildingAttachment.BuildingInstance)
			{
				warning = GetStartAndEndBuildingsAreTheSameWarning(fromBuildingAttachment.BuildingInstance);
				return true;
			}

			if (fromBuildingAttachment.BuildingInstance.BackendBuilding.HasConnection(toBuildingAttachment.BuildingInstance.BackendBuilding))
			{
				warning = GetRedundantConnectionWarning(fromBuildingAttachment.BuildingInstance, toBuildingAttachment.BuildingInstance);
				return true;
			}

			if (fromBuildingAttachment.BuildingInstance.GridGroup == toBuildingAttachment.BuildingInstance.GridGroup)
			{
				warning = GetSameGridGroupWarning(fromBuildingAttachment.BuildingInstance, toBuildingAttachment.BuildingInstance);
				return true;
			}

			warning = new BuildWarning();
			return false;
		}

		private static BuildWarning GetStartAndEndBuildingsAreTheSameWarning(BuildingComponent building)
		{
			return new BuildWarning($"Cannot connect a building to itself.", BuildWarning.WarningType.FATAL);
		}

		private static BuildWarning GetRedundantConnectionWarning(BuildingComponent fromBuilding, BuildingComponent toBuilding)
		{
			return new BuildWarning($"A connection between this {fromBuilding.Data.buildingName} and {toBuilding.Data.buildingName} already exists.", BuildWarning.WarningType.FATAL);
		}

		private static BuildWarning GetSameGridGroupWarning(BuildingComponent fromBuilding, BuildingComponent toBuilding)
		{
			return new BuildWarning($"This {fromBuilding.Data.buildingName} and {toBuilding.Data.buildingName} are already part of the same power grid.", BuildWarning.WarningType.ALERT);
		}
	}
}
