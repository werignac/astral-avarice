using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class BuildingComponent : MonoBehaviour, IDemolishable, IInspectableComponent, IGridGroupElement
{
	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private Transform cableConnectionPoint;
	[SerializeField] private BuildingData buildingData;
	[SerializeField] private BuildingVisuals buildingVisuals;

	[SerializeField] private PlanetComponent parentPlanet;
	[SerializeField] private bool isPlayerDemolishable = false;
	private Building gameBuilding;

	// True right before the building gets demolished.
	public bool isDemolishing { get; private set; } = false;

	// Events
	[HideInInspector] public UnityEvent<BuildingComponent> OnBuildingDemolished = new UnityEvent<BuildingComponent>();
	[HideInInspector] public UnityEvent OnBuildingHoverStartForSelection = new UnityEvent();
	[HideInInspector] public UnityEvent OnBuildingHoverEndForSelection = new UnityEvent();
	[HideInInspector] public UnityEvent OnBuildingSelected = new UnityEvent();
	[HideInInspector] public UnityEvent OnBuildingDeselected = new UnityEvent();
	[HideInInspector] public UnityEvent OnBuildingHoverStartForDemolish = new UnityEvent();
	[HideInInspector] public UnityEvent OnBuildingHoverEndForDemolish = new UnityEvent();
	[HideInInspector] public UnityEvent<int> OnGridGroupChanged { get; private set; } = new UnityEvent<int>();

	// Getters
	public BuildingData Data
	{
		get { return (buildingData); }
	}

	public Building BackendBuilding
	{
		get => gameBuilding;
	}

	public BuildingVisuals BuildingVisuals
	{
		get => buildingVisuals;
	}

	/// <summary>
	/// Get how wide (x) and tall (y) the building is.
	/// </summary>
	public Vector2 ColliderSize
	{
		get
		{
			return boxCollider.size;
		}
	}

	/// <summary>
	/// Get the location of the center of the collider relative to the building's origin.
	/// </summary>
	public Vector2 ColliderOffset
	{
		get
		{
			return boxCollider.offset;
		}
	}

	public Vector2 CableConnectionOffset
	{
		get
		{
			return cableConnectionPoint.localPosition;
		}
	}

	public Transform CableConnectionTransform
	{
		get
		{
			return cableConnectionPoint;
		}
	}

	public PlanetComponent ParentPlanet
    {
        get
        {
			return (parentPlanet);
        }
    }

	/// <summary>
	/// Which independent power grid does this building belong to?
	/// </summary>
	public int GridGroup
	{
		get => BackendBuilding != null ? BackendBuilding.BuildingGroup : -1;
	}

    /// <summary>
    /// Sets the location and rotation of the building to be at a particular
    /// point and rotate in a particular direction.
    /// </summary>
    public bool SetPositionAndUpNormal()
    {
		if (parentPlanet != null)
		{
			Vector2 position = parentPlanet.GetClosestSurfacePointToPosition(transform.position);
			Vector2 upNormal = parentPlanet.GetNormalForPosition(position);
			transform.position = position;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(upNormal.y, upNormal.x) * Mathf.Rad2Deg - 90);
			transform.SetParent(parentPlanet.BuildingContainer);
			return (true);
		}
		return (false);
    }

	// TODO: Use for when objects move.
	private bool IsValidOverlap(Collider2D overlapping)
	{
		if (overlapping.TryGetComponent(out PlanetComponent overlappingPlanet))
		{
			// May not actually ever detect parent planet due to parent-child relationship depending on overlap detection method.
			return overlappingPlanet == parentPlanet;
		}

		return false;
	}

	/// <summary>
	/// Sets the gameBuilding member to the passed building for future use.
	/// </summary>
	public void SetGameBuilding(Building building)
	{
		gameBuilding = building;
		// Chain the grid group changed events.
		building.onGridGroupChanged.AddListener(OnGridGroupChanged.Invoke);
	}

	public void Demolish()
	{
		isDemolishing = true;

		OnBuildingDemolished?.Invoke(this);

		if(parentPlanet != null)
		{
			transform.parent = null;
		}

		Vector3 spawnLocation = transform.TransformPoint(boxCollider.offset);
		Instantiate(GlobalBuildingSettings.GetOrCreateSettings().BuildingDestructionVFXPrefab, spawnLocation, Quaternion.identity);


		Destroy(gameObject);
	}

	public void HoverDemolishStart()
	{
		OnBuildingHoverStartForDemolish?.Invoke();
	}

	public void HoverDemolishEnd()
	{
		OnBuildingHoverEndForDemolish?.Invoke();
	}

	public void SetDemolishable(bool isDemolishable)
	{
		isPlayerDemolishable = isDemolishable;
	}

	public bool Demolishable()
	{
		return isPlayerDemolishable;
	}

	/// <summary>
	/// Returns whether the client has authority over this building.
	/// Could be used in multiplayer applications.
	/// 
	/// If a client has authority over this building, they are permitted
	/// to move and demolish the building.
	/// </summary>
	/// <returns>True if this client has authority over the building. False otherwise.</returns>
	public bool GetClientHasAuthority()
	{
		return Demolishable();
	}

	public void OnHoverEnter()
	{
		OnBuildingHoverStartForSelection?.Invoke();
	}

	public void OnHoverExit()
	{
		OnBuildingHoverEndForSelection?.Invoke();
	}

	public void OnSelectStart()
	{
		OnBuildingSelected?.Invoke();
	}

	public void OnSelectEnd()
	{
		OnBuildingDeselected?.Invoke();
	}

	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = new BuildingInstanceInspectorController(this);
		return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
	}

	public void SetParentPlanet(PlanetComponent planet)
	{
		parentPlanet = planet;
	}

	public bool OwnsCollider(Collider2D collider)
    {
		return (collider == boxCollider);
    }
}
