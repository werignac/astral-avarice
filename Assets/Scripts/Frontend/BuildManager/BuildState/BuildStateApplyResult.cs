using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Object that tells us about the result of applying a build state.
	/// Applying a build state = destroying a building, placing a building, placing a cable.
	/// </summary>
	public interface BuildStateApplyResult
	{

	}

	/// <summary>
	/// Object that tells us about the result of placing a building.
	/// </summary>
	public struct BuildingBuildStateApplyResult : BuildStateApplyResult
	{
		public Cost cost;
		public BuildingComponent buildingInstance;
	}

	/// <summary>
	/// Object that tells us about the result of placing a cable.
	/// </summary>
	public struct CableBuildStateApplyResult : BuildStateApplyResult
	{
		public Cost cost;
		public CableComponent cableInstance;
	}

	/// <summary>
	/// Object that tells us about the result of demolishing an object.
	/// </summary>
	public struct DemolishBuildStateApplyResult : BuildStateApplyResult
	{
		public IDemolishable demolished;
	}
}
