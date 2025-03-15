using UnityEngine;

/// <summary>
/// Toggles the outline postprocessing effect for a building.
/// Toggled VFX must be set manually in the inspector.
/// </summary>
public class BuildingOutlineComponent : ToggleOutlineComponent
{
	private BuildingComponent buildingComponent;

	private void Awake()
	{
		buildingComponent = GetComponentInParent<BuildingComponent>();

		buildingComponent.OnBuildingHoverStartForSelection.AddListener(TurnOnOutline);
		buildingComponent.OnBuildingSelected.AddListener(TurnOnOutline);
		buildingComponent.OnBuildingHoverStartForDemolish.AddListener(TurnOnOutline);

		buildingComponent.OnBuildingHoverEndForSelection.AddListener(TurnOffOutline);
		buildingComponent.OnBuildingDeselected.AddListener(TurnOffOutline);
		buildingComponent.OnBuildingHoverEndForDemolish.AddListener(TurnOffOutline);
	}
}
