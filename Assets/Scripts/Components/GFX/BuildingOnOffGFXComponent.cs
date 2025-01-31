using UnityEngine;

public class BuildingOnOffGFXComponent : MonoBehaviour
{
	[SerializeField] private Sprite onSprite;
	[SerializeField] private Sprite offSprite;

	private BuildingComponent buildingComponent;

	[SerializeField] private new SpriteRenderer renderer;

	private void Awake()
	{
		buildingComponent = GetComponentInParent<BuildingComponent>();
	}

	private void LateUpdate()
	{
		if (buildingComponent.BackendBuilding.IsPowered)
		{
			if (renderer.sprite != onSprite)
				renderer.sprite = onSprite;
		}
		else
		{
			if (renderer.sprite != offSprite)
				renderer.sprite = offSprite;
		}
		
	}
}
