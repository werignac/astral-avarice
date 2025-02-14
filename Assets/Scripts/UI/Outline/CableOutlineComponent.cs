using UnityEngine;

public class CableOutlineComponent : ToggleOutlineComponent
{
	private CableComponent cableComponent;


	private void Awake()
	{
		cableComponent = GetComponentInParent<CableComponent>();

		cableComponent.OnCableHoverStartForDemolish.AddListener(TurnOnOutline);

		cableComponent.OnCableHoverEndForDemolish.AddListener(TurnOffOutline);
	}
}
