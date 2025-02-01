using UnityEngine;

public class BuildingOutlineComponent : MonoBehaviour
{
	private BuildingComponent buildingComponent;

	[SerializeField] private SpriteRenderer spriteRenderer;
	private Material defaultMaterial;

	private void Awake()
	{
		defaultMaterial = spriteRenderer.sharedMaterial;
		buildingComponent = GetComponentInParent<BuildingComponent>();
	}

	private void Start()
	{
		buildingComponent.OnBuildingSelected.AddListener(SetSelectOutline);
		buildingComponent.OnBuildingHoverStartForSelection.AddListener(SetSelectOutline);
		buildingComponent.OnBuildingHoverStartForDemolish.AddListener(SetDemolishOutline);
		buildingComponent.OnBuildingDeselected.AddListener(ClearOutline);
		buildingComponent.OnBuildingHoverEndForSelection.AddListener(ClearOutline);
		buildingComponent.OnBuildingHoverEndForDemolish.AddListener(ClearOutline);
	}

	public void SetSelectOutline()
	{
		Color color = PtUUISettings.GetOrCreateSettings().SelectColor;
		SetOutlineAndColor(color);
	}

	public void SetDemolishOutline()
	{
		Color color = PtUUISettings.GetOrCreateSettings().DemolishColor;
		SetOutlineAndColor(color);
	}

	private void SetOutlineAndColor(Color color)
	{
		Material outlineMaterial = PtUUISettings.GetOrCreateSettings().BuildingSelectionMaterial;
		outlineMaterial.SetColor(Shader.PropertyToID("_OutlineColor"), color);
		spriteRenderer.material = outlineMaterial;
	}

	public void ClearOutline()
	{
		spriteRenderer.material = defaultMaterial;
	}
}
