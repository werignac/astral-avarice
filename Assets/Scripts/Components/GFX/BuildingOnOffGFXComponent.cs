using UnityEngine;
using UnityEngine.Events;

public class BuildingOnOffGFXComponent : MonoBehaviour
{
	[SerializeField] private Sprite onSprite;
	[SerializeField] private Sprite offSprite;

	private BuildingComponent buildingComponent;

	private bool lastPoweredState = false;

	[SerializeField] private new SpriteRenderer renderer;

	// Invoked when the power state of the building has changed.
	// In theory, this should be on the BuildingComponent itself instead of in here.
	[HideInInspector] public UnityEvent<bool> OnPoweredStateChanged = new UnityEvent<bool>();

	private void Awake()
	{
		buildingComponent = GetComponentInParent<BuildingComponent>();

		OnPoweredStateChanged.AddListener(Self_OnPoweredStateChanged);
	}

	public void Start()
	{
		// Initialize GFX to match starting state.
		if (buildingComponent.BackendBuilding != null)
			OnPoweredStateChanged?.Invoke(buildingComponent.BackendBuilding.IsPowered);
	}

	private void Self_OnPoweredStateChanged(bool isPowered)
	{
		if (isPowered)
		{
			renderer.sprite = onSprite;
		}
		else
		{
			renderer.sprite = offSprite;
		}
	}

	private void LateUpdate()
	{
		if (buildingComponent.BackendBuilding != null)
		{
			if (buildingComponent.BackendBuilding.IsPowered != lastPoweredState)
			{
				OnPoweredStateChanged?.Invoke(buildingComponent.BackendBuilding.IsPowered);
				lastPoweredState = buildingComponent.BackendBuilding.IsPowered;
			}
		}
		
	}
}
