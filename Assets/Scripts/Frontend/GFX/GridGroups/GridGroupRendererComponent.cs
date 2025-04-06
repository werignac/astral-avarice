using UnityEngine;
using UnityEngine.SceneManagement;

public class GridGroupRendererComponent : MonoBehaviour
{
	private IGridGroupElement gridGroupElement;

	[SerializeField] protected Renderer[] renderers;

	private void Awake()
	{
		gridGroupElement = GetComponentInParent<IGridGroupElement>();
		gridGroupElement.OnGridGroupChanged.AddListener(GridGroupElement_OnGridGroupChanged);
	}

	private void Start()
	{
		SetRendererMaterials(GetGridGroup());
	}

	private void GridGroupElement_OnGridGroupChanged(int newGridGroup)
	{
		SetRendererMaterials(newGridGroup);
	}

	protected virtual void SetRendererMaterials(int gridGroup)
	{
		foreach(Renderer renderer in renderers)
		{
			// Note .material and note .sharedMaterial for separate parameters.
			renderer.material = GridGroupGFX.SetMaterialGridGroupColor(renderer.material, gridGroup);
		}
	}

	protected int GetGridGroup()
	{
		return gridGroupElement.GridGroup;
	}
}
