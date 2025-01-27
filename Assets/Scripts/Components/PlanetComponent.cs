using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component on any sphereical planet.
/// </summary>
public class PlanetComponent : MonoBehaviour
{

	/// <summary>
	/// Event used by BuildManagerComponent to know when a planet
	/// should not longer be buildable because it has been destroyed.
	/// </summary>
	[HideInInspector] public UnityEvent<PlanetComponent> OnPlanetDestroyed = new UnityEvent<PlanetComponent>();

	private CircleCollider2D planetCollider;
	[SerializeField] private Transform buildingContainerTransform;
	[SerializeField] private int mass;
	[SerializeField] private int[] resourceCounts = new int[(int)ResourceType.Resource_Count];
	[SerializeField] private int solarOutput;
	[SerializeField] private bool canPlaceBuildings;

	public Transform BuildingContainer
	{
		get { return (buildingContainerTransform); }
	}
	public int Mass
	{
		get { return (mass); }
	}
	public int SolarOutput
    {
		get { return (solarOutput); }
    }
	public bool CanPlaceBuildings
    {
        get { return (canPlaceBuildings); }
    }

	public int GetResourceCount(ResourceType resource)
    {
		if(resource == ResourceType.Resource_Count)
        {
			return (0);
        }
		return (resourceCounts[(int)resource]);
    }
	public void SetResourceCount(ResourceType resource, int amount)
    {
		if(resource != ResourceType.Resource_Count)
        {
			resourceCounts[(int)resource] = amount;
        }
    }


	private void Awake()
	{
		planetCollider = GetComponent<CircleCollider2D>();
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
		return  radialOffset + (Vector2) transform.position;
	}

	/// <summary>
	/// Gets the normal of the planet's surface based on the nearest
	/// point on the planet's surface relative to the position.
	/// </summary>
	/// <param name="position"></param>
	/// <returns>Normlized vector representing the direction of the surface of the planet.</returns>
	public Vector2 GetNormalForPosition(Vector2 position)
	{
		return (position - (Vector2) transform.position).normalized;
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
		building.GetComponent<BuildingComponent>().SetDemolishable(isPlayerDemolishable);
		return building;
	}

	public void DestroyAllBuildings()
    {
		for(int i = 0; i < buildingContainerTransform.childCount; ++i)
        {
			Destroy(buildingContainerTransform.GetChild(i).gameObject);
        }
    }

	public int GetTotalMass()
    {
		int totalMass = mass;
		for (int i = 0; i < buildingContainerTransform.childCount; ++i)
        {
			BuildingComponent building = buildingContainerTransform.GetChild(i).gameObject.GetComponent<BuildingComponent>();
			if(building != null)
            {
				totalMass += building.Data.mass;
            }
        }
		return (totalMass);
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.otherCollider.gameObject != null)
		{
			PlanetComponent hitPlanet = collision.otherCollider.gameObject.GetComponent<PlanetComponent>();
			if (hitPlanet != null)
			{
				if (GetTotalMass() < hitPlanet.GetTotalMass())
				{
					Destroy(gameObject);
					hitPlanet.DestroyAllBuildings();
				}
				else
				{
					Destroy(hitPlanet.gameObject);
					DestroyAllBuildings();
				}
			}
		}
	}

}
