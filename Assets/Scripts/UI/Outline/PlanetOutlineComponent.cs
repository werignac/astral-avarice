using UnityEngine;

/// <summary>
/// Toggles the outline postprocessing effect for when this planet is selected.
/// </summary>
public class PlanetOutlineComponent : ToggleOutlineComponent
{
	private PlanetComponent planetComponent;

	private void Awake()
	{
		planetComponent = GetComponentInParent<PlanetComponent>();

		planetComponent.OnHoverStart.AddListener(TurnOnOutline);
		planetComponent.OnSelectedStart.AddListener(TurnOnOutline);

		planetComponent.OnHoverEnd.AddListener(TurnOffOutline);
		planetComponent.OnSelectedEnd.AddListener(TurnOffOutline);
	}
}
