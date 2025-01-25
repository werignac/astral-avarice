using UnityEngine;
using UnityEngine.Events;

public class BuildingComponent : MonoBehaviour
{
	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private Transform cableConnectionPoint;

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

}
