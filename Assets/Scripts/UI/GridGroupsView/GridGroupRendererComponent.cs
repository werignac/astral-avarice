using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GridGroupRendererComponent : MonoBehaviour
{

	[SerializeField] protected Renderer[] renderers;

	private void LateUpdate()
	{
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
			return;
		
		SetRendererMaterials();
	}

	protected virtual void SetRendererMaterials()
	{
		int gridGroup = GetGridGroup();

		foreach(Renderer renderer in renderers)
		{
			// Note .material and note .sharedMaterial for separate parameters.
			renderer.material = GridGroupGFX.SetMaterialGridGroupColor(renderer.material, gridGroup);
		}
	}

	protected abstract int GetGridGroup();

}
