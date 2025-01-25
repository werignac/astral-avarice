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
		return Instantiate(buildingPrefab, position, rotation, buildingContainerTransform);
	}

}
