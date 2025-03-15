using UnityEngine;

public class CableGridGroupRendererComponent : GridGroupRendererComponent
{
	private CableComponent cable;

	private void Awake() 
	{
		cable = GetComponentInParent<CableComponent>();
	}

	protected override int GetGridGroup() => cable.GridGroup;
}
