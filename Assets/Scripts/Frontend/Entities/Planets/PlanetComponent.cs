using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

/// <summary>
/// Component on any sphereical planet.
/// </summary>
public class PlanetComponent : MonoBehaviour, IInspectableComponent
{

	public static float MassToGravityRadius(int mass) => mass / 25f;

	/// <summary>
	/// Event used by BuildManagerComponent to know when a planet
	/// should not longer be buildable because it has been destroyed.
	/// </summary>
	[HideInInspector] public UnityEvent<PlanetComponent> OnPlanetDemolished = new UnityEvent<PlanetComponent>();
	private bool muteOnMassChangedEvent = false;
	[HideInInspector] public UnityEvent OnMassChanged = new UnityEvent();
	[HideInInspector] public UnityEvent OnHoverStart = new UnityEvent();
	[HideInInspector] public UnityEvent OnSelectedStart = new UnityEvent();
	[HideInInspector] public UnityEvent OnHoverEnd = new UnityEvent();
	[HideInInspector] public UnityEvent OnSelectedEnd = new UnityEvent();
	// Called when the player is hovering near or on buildings in the planet in the build or demolish modes.
	[HideInInspector] public UnityEvent OnStartProspectingMassChange = new UnityEvent();
	[HideInInspector] public UnityEvent OnStopProspectingMassChange = new UnityEvent();
	[HideInInspector] public UnityEvent OnStartMoving = new UnityEvent();
	[HideInInspector] public UnityEvent OnStopMoving = new UnityEvent();

	private CircleCollider2D planetCollider;
	[SerializeField] private Transform buildingContainerTransform;
	[SerializeField] private int mass;
	[SerializeField] private int[] resourceCounts = new int[(int)ResourceType.Resource_Count];
	[SerializeField] private int solarOutput;
	// Solar energy accumulated from adjacent starts this frame.
	private int accumulatedSolarEnergy = 0;
	[SerializeField] private bool canPlaceBuildings;
	[SerializeField] private string planetName = "Planet";

	private Vector2 planetVelocity;
	/// <summary>
	/// Velocity accumulated this FixedUpdate.
	/// Set to Vector2.zero at the end of FixedUpdate.
	/// </summary>
	private Vector2 accumulatedVelocity;
	private new Rigidbody2D rigidbody;

	public Transform BuildingContainer
	{
		get { return (buildingContainerTransform); }
	}
	public int Mass
	{
		get { return (mass); }
	}
	public float GravityRadius
	{
		get { return MassToGravityRadius(GetTotalMass()); }
	}
	public float Radius
	{
		get { return (planetCollider != null ?  planetCollider : GetComponent<CircleCollider2D>()).radius * transform.localScale.x; }
	}

	public int SolarOutput
	{
		get { return (solarOutput); }
	}
	public bool CanPlaceBuildings
	{
		get { return (canPlaceBuildings); }
	}
	public string PlanetName
	{
		get { return (planetName); }
	}
	public Vector2 PlanetVelocity
	{
		get
		{
			return planetVelocity;
		}

		set
		{
			bool isStartingToMove = (planetVelocity.magnitude == 0) && (value.magnitude > 0);
			bool isStopping = (planetVelocity.magnitude > 0) && value.magnitude == 0;

			planetVelocity = value;

			if (isStartingToMove)
				OnStartMoving?.Invoke();
			if (isStopping)
				OnStopMoving?.Invoke();
		}
	}
	public bool Destroyed { get; set; } = false;

	public int GetResourceCount(ResourceType resource)
	{
		if (resource == ResourceType.Resource_Count)
		{
			return (0);
		}
		return (resourceCounts[(int)resource]);
	}
	public void SetResourceCount(ResourceType resource, int amount)
	{
		if (resource != ResourceType.Resource_Count)
		{
			resourceCounts[(int)resource] = amount;
		}
	}

	private void Awake()
	{
		planetCollider = GetComponent<CircleCollider2D>();
	}

	private void Start()
	{
		// Register starting buildings w/ callback for updating
		// planet mass for when they are destroyed.
		foreach (Transform buildingTransform in buildingContainerTransform)
		{
			BuildingComponent building = buildingTransform.GetComponent<BuildingComponent>();
			building.OnBuildingDemolished.AddListener((BuildingComponent _) => { InvokeMassChangedEvent(); });
		}

		rigidbody = GetComponent<Rigidbody2D>();
	}

	public float DistanceToPosition(Vector2 position)
	{
		return Vector2.Distance(transform.position, position);
	}

	public Vector2 GetClosestSurfacePointToPosition(Vector2 position)
	{
		// Force the point to be on the surface of the planets instead of allowing points inside
		// the volume.
		if(planetCollider == null)
        {
            planetCollider = GetComponent<CircleCollider2D>();
        }
		Vector2 radialDirection = (planetCollider.ClosestPoint(position) - (Vector2)transform.position).normalized;
		Vector2 radialOffset = radialDirection * planetCollider.radius * transform.localScale.x;
		return radialOffset + (Vector2)transform.position;
	}

	/// <summary>
	/// Gets the normal of the planet's surface based on the nearest
	/// point on the planet's surface relative to the position.
	/// </summary>
	/// <param name="position"></param>
	/// <returns>Normlized vector representing the direction of the surface of the planet.</returns>
	public Vector2 GetNormalForPosition(Vector2 position)
	{
		return (position - (Vector2)transform.position).normalized;
	}

	public bool OwnsCollider(Collider2D checkOwns)
	{
		return planetCollider == checkOwns;
	}

	public GameObject InstantiateBuildingOnPlanet(
		GameObject buildingPrefab,
		Vector2 position,
		Quaternion rotation,
		bool isPlayerDemolishable = true
		)
	{
		GameObject building = Instantiate(buildingPrefab, position, rotation, buildingContainerTransform);
		// Counteract upscaling from parent transform (odd that I started needed to do this without changing much).
		// Assumes scaling on planet is uniform (x == y == z).
		building.transform.localScale = Vector3.one * 1 / transform.localScale.x;
		BuildingComponent buildingComponent = building.GetComponent<BuildingComponent>();
		buildingComponent.SetDemolishable(isPlayerDemolishable);
		buildingComponent.SetParentPlanet(this);

		buildingComponent.OnBuildingDemolished.AddListener((BuildingComponent _) => { InvokeMassChangedEvent(); });
		InvokeMassChangedEvent();

		return building;
	}

	public void DestroyAllBuildings()
	{
		// Stop repeated firing of OnMassChangedEvent.
		muteOnMassChangedEvent = true;

		for (int i = buildingContainerTransform.childCount - 1; i >=0 ; --i)
		{
			buildingContainerTransform.GetChild(i).GetComponent<BuildingComponent>().Demolish();
        }
		buildingContainerTransform.DetachChildren();

		muteOnMassChangedEvent = false;
		// Fire the OnMassChangedEvent

		InvokeMassChangedEvent();
    }

	public int GetTotalMass()
	{
		int totalMass = mass;
		for (int i = 0; i < buildingContainerTransform.childCount; ++i)
		{
			BuildingComponent building = buildingContainerTransform.GetChild(i).gameObject.GetComponent<BuildingComponent>();
			if (building != null && ! building.isDemolishing)
			{
				totalMass += building.Data.mass;
			}
		}
		return (Mathf.Max(1, totalMass));
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (gameObject != null && collision.collider.gameObject != null && gameObject != collision.collider.gameObject)
		{
			BuildingComponent building;
			if ((building = collision.otherCollider.gameObject.GetComponentInParent<BuildingComponent>()) != null)
			{
				PlanetComponent hitPlanet = collision.collider.gameObject.GetComponent<PlanetComponent>();
				if (hitPlanet != null)
				{
					building.Demolish();
                    building.transform.parent = null;
					// Automatically invoke redraw of gravity field via callbacks.
				}
				else
				{
					BuildingComponent otherBuilding = collision.collider.gameObject.GetComponentInParent<BuildingComponent>();
					if (otherBuilding != null)
					{
						building.Demolish();
						building.transform.parent = null;
						// Automatically invoke redraw of gravity field via callbacks.
					}
				}
			}
			else
			{
				PlanetComponent hitPlanet = collision.collider.gameObject.GetComponent<PlanetComponent>();
				if (hitPlanet != null && !Destroyed && !hitPlanet.Destroyed)
				{
					Debug.Log(GetTotalMass() + " " + hitPlanet.GetTotalMass());
					if (GetTotalMass() > hitPlanet.GetTotalMass())
					{
						hitPlanet.Destroyed = true;
						DestroyAllBuildings();
						hitPlanet.Demolish();
					}
					else if (GetTotalMass() == hitPlanet.GetTotalMass())
					{
						hitPlanet.Destroyed = true;
						Destroyed = true;
						hitPlanet.Demolish();
						Demolish();
					}
				}
			}
		}
	}

	public int GetAvailableResourceCount(ResourceType resource)
	{
		int available = GetResourceCount(resource);
		for (int i = 0; available > 0 && i < buildingContainerTransform.childCount; ++i)
		{
			BuildingComponent building = buildingContainerTransform.GetChild(i).gameObject.GetComponent<BuildingComponent>();
			if (building != null && building.Data.requiredResource == resource)
			{
				available = Mathf.Max(0, available - building.Data.resourceAmountRequired);
			}
		}
		return (available);
	}

	public void InvokeMassChangedEvent()
	{
		if (!muteOnMassChangedEvent)
			OnMassChanged.Invoke();
	}

    public void OnHoverEnter()
    {
		OnHoverStart?.Invoke();
    }

    public void OnHoverExit()
    {
		OnHoverEnd?.Invoke();
    }

    public void OnSelectStart()
    {
		OnSelectedStart?.Invoke();
    }

    public void OnSelectEnd()
    {
		OnSelectedEnd?.Invoke();
    }

    public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
    {
        inspectorController = new PlanetInspectorController(this);
        return PtUUISettings.GetOrCreateSettings().PlanetInspectorUI;
    }

	public void Demolish()
	{
		OnPlanetDemolished?.Invoke(this);

		DestroyAllBuildings();

		GameObject destructionVFXGameObject = Instantiate(GlobalBuildingSettings.GetOrCreateSettings().PlanetDestructionVFXPrefab, transform.position, Quaternion.identity);
		destructionVFXGameObject.transform.localScale = Vector3.one * Radius * 2;

		Destroy(gameObject);
	}


	/// <summary>
	/// Called by the build manager depending on the build mode.
	/// 
	/// Necessary for showing the gravity field of the planet when
	/// we plan on building on it with the general gravity field visualization
	/// disabled.
	/// </summary>
	public void StartProspectingMassChange()
	{
		OnStartProspectingMassChange?.Invoke();
	}

	/// <summary>
	/// Called by the build manager depending on the build mode.
	/// 
	/// Necessary for hiding the gravity field of the planet when
	/// we plan on building on it with the general gravity field visualization
	/// disabled.
	/// </summary>
	public void StopProspectingMassChange()
	{
		OnStopProspectingMassChange?.Invoke();
	}

	/// <summary>
	/// Add to the planet's current velocity.
	/// </summary>
	/// <param name="addedVelocity">The amount of velocity to add.</param>
	public void AddVelocity(Vector2 addedVelocity)
	{
		accumulatedVelocity += addedVelocity;
	}

	public void AddForce(Vector2 force, float deltaTime)
	{
		// NOTE: Calcs in GameController used gravityRadius instead of mass for some reason.
		AddVelocity(force / GravityRadius * deltaTime);
	}

	public void SumVelocity(float deltaTime)
	{
		// Check if accumulated velocity is 0. If so, decelerate.
		if (accumulatedVelocity.magnitude < Mathf.Epsilon && PlanetVelocity.magnitude > Mathf.Epsilon)
			accumulatedVelocity = ComputeDecelerationVelocityChange(deltaTime);

		PlanetVelocity += accumulatedVelocity;

		accumulatedVelocity = Vector2.zero;
	}

	private Vector2 ComputeDecelerationVelocityChange(float deltaTime)
	{
		Vector2 velocityChange = PlanetVelocity * -1;
		if (velocityChange.magnitude > deltaTime)
			velocityChange = velocityChange.normalized * deltaTime;
		return velocityChange;
	}

	/// <summary>
	/// Move the planet based on the current PlanetVelocity.
	/// </summary>
	public void ApplyVelocity(float deltaTime)
	{
		Vector2 movement = PlanetVelocity * deltaTime;
		
		if (movement.magnitude > Mathf.Epsilon)
			rigidbody.MovePosition(rigidbody.position + movement);
	}

	/// <summary>
	/// Adds solar energy to this planet this frame.
	/// </summary>
	public void AddSolarEnergy(int addedSolar)
	{
		accumulatedSolarEnergy += addedSolar;
	}

	/// <summary>
	/// Sets solar enery this planet has to the amount accumulated
	/// this frame.
	/// </summary>
	public void ApplySolar()
	{
		int sum = SolarOutput + accumulatedSolarEnergy;
		SetResourceCount(ResourceType.Solar, sum);
		accumulatedSolarEnergy = 0;
	}
}
