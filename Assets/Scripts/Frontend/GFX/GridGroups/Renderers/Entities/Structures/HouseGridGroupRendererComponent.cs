using UnityEngine;

/// <summary>
/// Only displays homes that have connections as being in a grid group.
/// Reduces GridGroupView noise.
/// </summary>
public class HouseGridGroupRendererComponent : BuildingGridGroupRendererComponent
{
	protected override void SetRendererMaterials()
	{
		if (building.BackendBuilding.NumConnected == 0)
		{
			foreach (Renderer renderer in renderers)
				renderer.material = GridGroupGFX.SetMaterialIsGridElement(renderer.material, false);
			return;
		}

		int gridGroup = GetGridGroup();

		foreach (Renderer renderer in renderers)
		{
			// Note .material and note .sharedMaterial for separate parameters.
			renderer.material = GridGroupGFX.SetMaterialIsGridElement(renderer.material, true);
			renderer.material = GridGroupGFX.SetMaterialGridGroupColor(renderer.material, gridGroup);
		}
	}
}
