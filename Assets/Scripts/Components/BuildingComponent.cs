using UnityEngine;
using UnityEngine.Events;

public class BuildingComponent : MonoBehaviour
{
	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private Transform cableConnectionPoint;

	[SerializeField] private PlanetComponent parentPlanet;

	// Events
	[HideInInspector] public UnityEvent<BuildingComponent> OnBuildingDestroyed = new UnityEvent<BuildingComponent>();

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
			return cableConnectionPoint.position;
		}
	}

	private void OnDestroy()
	{
		OnBuildingDestroyed?.Invoke(this);
	}

    /// <summary>
    /// Sets the location and rotation of the building to be at a particular
    /// point and rotate in a particular direction.
    /// </summary>
    public void SetPositionAndUpNormal()
    {
		if (parentPlanet != null)
		{
			Vector2 position = parentPlanet.GetClosestSurfacePointToPosition(transform.position);
			Vector2 upNormal = parentPlanet.GetNormalForPosition(position);
			transform.position = position;
			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(upNormal.y, upNormal.x) * Mathf.Rad2Deg - 90);
		}
    }

}
