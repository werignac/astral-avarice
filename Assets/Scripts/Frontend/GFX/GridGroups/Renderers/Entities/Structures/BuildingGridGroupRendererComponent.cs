using UnityEngine;

public class BuildingGridGroupRendererComponent : GridGroupRendererComponent
{
	protected BuildingComponent building;

	private void Awake()
	{
		building = GetComponentInParent<BuildingComponent>();
	}

	protected override int GetGridGroup() => building.GridGroup;
}
