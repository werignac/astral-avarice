using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

/// <summary>
/// Component on any sphereical planet.
/// </summary>
public class PlanetComponent : MonoBehaviour, IInspectableComponent
{
#if UNITY_EDITOR
	private static readonly int circleSegments = 51;
#endif

	public static float MassToGravityRadius(int mass) => mass / 25f;

	/// <summary>
	/// Event used by BuildManagerComponent to know when a planet
	/// should not longer be buildable because it has been destroyed.
	/// </summary>
	[HideInInspector] public UnityEvent<PlanetComponent> OnPlanetDestroyed = new UnityEvent<PlanetComponent>();
	private bool muteOnMassChangedEvent = false;
	[HideInInspector] public UnityEvent OnMassChanged = new UnityEvent();

	private CircleCollider2D planetCollider;
	[SerializeField] private Transform buildingContainerTransform;
	[SerializeField] private int mass;
	[SerializeField] private int[] resourceCounts = new int[(int)ResourceType.Resource_Count];
	[SerializeField] private int solarOutput;
	[SerializeField] private bool canPlaceBuildings;

#if UNITY_EDITOR
	private LineRenderer gravityRenderer;
#endif

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
		get { return planetCollider.radius * transform.localScale.x; }
	}

	public int SolarOutput
	{
		get { return (solarOutput); }
	}
	public bool CanPlaceBuildings
	{
		get { return (canPlaceBuildings); }
	}
	public Vector2 PlanetVelocity { get; set; }
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
#if UNITY_EDITOR
		AdjustGravityRing();
#endif
	}

	private void Start()
	{
		// Register starting buildings w/ callback for updating
		// planet mass for when they are destroyed.
		foreach (Transform buildingTransform in buildingContainerTransform)
		{
			BuildingComponent building = buildingTransform.GetComponent<BuildingComponent>();
			building.OnBuildingDestroyed.AddListener((BuildingComponent _) => { InvokeMassChangedEvent(); });
		}
	}

	public float DistanceToPosition(Vector2 position)
	{
		return Vector2.Distance(transform.position, position);
	}

	public Vector2 GetClosestSurfacePointToPosition(Vector2 position)
	{
		// Force the point to be on the surface of the planets instead of allowing points inside
		// the volume.
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

	/// <summary>
	/// When the planet is destroyed, notify that it has been destroyed.
	/// </summary>
	private void OnDestroy()
	{
		OnPlanetDestroyed?.Invoke(this);
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

		buildingComponent.OnBuildingDestroyed.AddListener((BuildingComponent _) => { InvokeMassChangedEvent(); });
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
#if UNITY_EDITOR
		AdjustGravityRing();
#endif

		InvokeMassChangedEvent();
    }

	public int GetTotalMass()
	{
		int totalMass = mass;
		for (int i = 0; i < buildingContainerTransform.childCount; ++i)
		{
			BuildingComponent building = buildingContainerTransform.GetChild(i).gameObject.GetComponent<BuildingComponent>();
			if (building != null)
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
#if UNITY_EDITOR
					AdjustGravityRing();
#endif
					// Automatically invoke redraw of gravity field via callbacks.
				}
				else
				{
					BuildingComponent otherBuilding = collision.collider.gameObject.GetComponentInParent<BuildingComponent>();
					if (otherBuilding != null)
					{
						building.Demolish();
						building.transform.parent = null;
#if UNITY_EDITOR
						AdjustGravityRing();
#endif
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
						hitPlanet.Demolish();
						DestroyAllBuildings();
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

#if UNITY_EDITOR
	public void AdjustGravityRing()

	{
		if (gravityRenderer == null)
		{
			gravityRenderer = gameObject.GetComponent<LineRenderer>();
			if (gravityRenderer == null)
			{
				gravityRenderer = gameObject.AddComponent<LineRenderer>();
				gravityRenderer.positionCount = circleSegments;
				gravityRenderer.useWorldSpace = false;
				gravityRenderer.widthMultiplier = 0.1f;
				gravityRenderer.loop = true;
			}
		}

		float circleRadius = GetTotalMass() / 25f;
		circleRadius /= transform.localScale.x; //Adjusting for the scale of the planet.
		DrawGravityCircle(circleRadius);
	}

	private void DrawGravityCircle(float radius)
	{ 

		float x;
		float y;
		float angle = 0;

		for (int i = 0; i<(circleSegments); i++)
		{
			x = Mathf.Sin(Mathf.Deg2Rad* angle)* radius;
			y = Mathf.Cos(Mathf.Deg2Rad* angle)* radius;

			gravityRenderer.SetPosition(i, new Vector3(x, y, 0));

			angle += (360f / (circleSegments - 1));
		}
	}
#endif

public void InvokeMassChangedEvent()
	{
		if (!muteOnMassChangedEvent)
			OnMassChanged.Invoke();
	}

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }

    public void OnSelectStart()
    {
    }

    public void OnSelectEnd()
    {
    }

    public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
    {
        inspectorController = new PlanetInspectorController(this);
        return PtUUISettings.GetOrCreateSettings().PlanetInspectorUI;
    }

	public void Demolish()
	{
		Destroy(gameObject);
	}
}
