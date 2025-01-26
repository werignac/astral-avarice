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

	public Transform BuildingContainer
	{
		get { return (buildingContainerTransform); }
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

	public GameObject InstantiateBuildingOnPlanet(GameObject buildingPrefab, Vector2 position, Quaternion rotation)
	{
		GameObject building = Instantiate(buildingPrefab, position, rotation, buildingContainerTransform);
		// Counteract upscaling from parent transform (odd that I started needed to do this without changing much).
		// Assumes scaling on planet is uniform (x == y == z).
		building.transform.localScale = Vector3.one * 1 / transform.localScale.x;
		return building;
	}

}
