using UnityEngine;
using System;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Gives a signal that determines how we transition to a different state.
	/// e.g. CANCEL on BuildingBuildState goes to NoneBuildState.
	/// CANCEL on BuildingChainedBuildState goes to BuildingBuildState.
	/// </summary>
	public enum BuildStateTransitionSignalType
	{
		CANCEL,
		BUILDING,
		CABLE,
		CHAIN,
		MOVE,
		DEMOLISH
	}

	public abstract class BuildStateTransitionSignal
	{
		public bool IsExternal { get; private set; }

		public BuildStateTransitionSignal(bool isExternal)
		{
			IsExternal = isExternal;
		}

		public abstract BuildStateTransitionSignalType GetSignalType();
	}

	public static class BuildStateTransitionSignalExtentions
	{
		public static void AssertSignalIsExternal(this BuildStateTransitionSignal signal)
		{
			if (!signal.IsExternal)
			{
				throw new ArgumentException($"Passed signal {signal} is not an internal signal, not an external signal.");
			}
		}
	}

	public class CancelTransitionSignal : BuildStateTransitionSignal
	{
		public CancelTransitionSignal(bool isExternal) : base(isExternal) { }
		public override BuildStateTransitionSignalType GetSignalType() => BuildStateTransitionSignalType.CANCEL;
	}

	public class BuildingTransitionSignal : BuildStateTransitionSignal
	{
		public BuildingSettingEntry NewBuildingType { get; private set; }

		public BuildingTransitionSignal(BuildingSettingEntry newBuildingType, bool isExternal) : base(isExternal)
		{
			if (newBuildingType == null)
				throw new ArgumentNullException("newBuildingType");
			
			NewBuildingType = newBuildingType;
		}

		public override BuildStateTransitionSignalType GetSignalType() => BuildStateTransitionSignalType.BUILDING;
	}

	public class CableTransitionSignal : BuildStateTransitionSignal
	{
		public BuildingComponent CableFrom { get; private set; }

		public CableTransitionSignal(BuildingComponent cableFrom, bool isExternal) : base(isExternal)
		{
			// NOTE: CableFrom can be null. In this case, we start cabling w/o
			// a start point.
			CableFrom = cableFrom;
		}

		public override BuildStateTransitionSignalType GetSignalType() => BuildStateTransitionSignalType.CABLE;
	}

	public class ChainTransitionSignal : BuildStateTransitionSignal
	{
		public BuildingComponent ChainFrom { get; private set; }

		public BuildingSettingEntry NewBuildingType { get; private set; }

		public ChainTransitionSignal(BuildingComponent chainFrom, BuildingSettingEntry newBuildingType, bool isExternal) : base(isExternal)
		{
			if (chainFrom == null)
				throw new ArgumentNullException("chainFrom");

			ChainFrom = chainFrom;

			if (newBuildingType == null)
				throw new ArgumentNullException("newbuildingType");

			NewBuildingType = newBuildingType;
		}

		public override BuildStateTransitionSignalType GetSignalType() => BuildStateTransitionSignalType.CHAIN;
	}

	public class DemolishTransitionSignal : BuildStateTransitionSignal
	{
		public DemolishTransitionSignal(bool isExternal) : base(isExternal) { }
		public override BuildStateTransitionSignalType GetSignalType() => BuildStateTransitionSignalType.DEMOLISH;
	}

}
