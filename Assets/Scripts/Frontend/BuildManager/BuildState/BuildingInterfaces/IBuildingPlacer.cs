using System;
using UnityEngine;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Interface for abstracting over the types of buildings we could be placing (new building, moving an old building).
	/// </summary>
	public interface IPlacingBuilding
	{

	}

	/// <summary>
	/// IPlacingBuilding implementation for new buildings.
	/// Used for when placing new buildings.
	/// </summary>
	public class NewPlacingBuilding : IPlacingBuilding
	{
		public BuildingSettingEntry BuildingSettings { get; private set; }

		public NewPlacingBuilding(BuildingSettingEntry buildingSettings)
		{
			if (buildingSettings == null)
				throw new ArgumentNullException("buildingSettings");

			BuildingSettings = buildingSettings;
		}
	}

	/// <summary>
	/// IPlacingBuilding implementation for existing buildings.
	/// Used for when moving buildings.
	/// </summary>
	public class ExistingPlacingBuilding : IPlacingBuilding
	{
		public BuildingComponent BuildingInstance { get; private set; }

		public ExistingPlacingBuilding(BuildingComponent buildingInstance)
		{
			if (buildingInstance == null)
				throw new ArgumentNullException("buildingInstance");

			BuildingInstance = buildingInstance;
		}
	}

	/// <summary>
	/// Interface for actions / state in the BuildManager that will place buildings.
	/// </summary>
    public interface IBuildingPlacer
    {
		/// <summary>
		/// Returns the building we are trying to place.
		/// </summary>
		public IPlacingBuilding GetPlacingBuilding();

		/// <summary>
		/// Returns the planet we are trying to place the building on.
		/// </summary>
		public PlanetComponent GetProspectivePlanet();

		/// <summary>
		/// Invoked when the planet we will place the building on changes.
		/// </summary>
		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged { get; }

		/// <summary>
		/// Returns the building cursor that is being used to place the building.
		/// </summary>
		/// <returns></returns>
		public BuildingCursorComponent GetBuildingCursor();
    }
}
