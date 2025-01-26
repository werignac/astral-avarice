using UnityEngine;

/// <summary>
/// Handles showing a ghost of where a building will be placed,
/// and checks whether the building has enough room to be placed,
/// and whether there is enough distance between 
/// </summary>
public class BuildingCursorComponent : MonoBehaviour
{
	// The size of the 2D box to check for objects that would collide with the incoming building.
	private Vector2 collisionCheckSize = new Vector2(0.7f, 1.6f);
	// The offset from the origin of the building of the 2D box to check for objects that would collide
	// with the incoming building.
	private Vector2 collisionCheckOffset = new Vector2(0, 0.808422f);
	// the offset from the origin of the building where cables get connected to.
	private Vector2 cableConnectionOffset;

	// Reference to the renderer to manipulate for the ghost sprite.
	[SerializeField] private SpriteRenderer ghostSpriteRenderer;
	// Color to use when something can be placed.
	[SerializeField] private Color placeableColor;
	// Color to use when something cannot be placed.
	[SerializeField] private Color notPlaceableColor;

	// The planet we're planning to spawn the building on.
	private PlanetComponent parentPlanet;

	// Getters
	public PlanetComponent ParentPlanet { get { return parentPlanet; } }
	public bool ShowingCanPlaceBuilding { get; private set; }
	public Vector2 CableConnectionPosition { get => transform.TransformPoint(cableConnectionOffset); }

	/// <summary>
	/// Shows the entire cursor.
	/// </summary>
    public void Show()
	{
		gameObject.SetActive(true);
	}

	/// <summary>
	/// Hides the entire cursor.
	/// </summary>
	public void Hide()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Gets whether the cursor is visible.
	/// </summary>
	public bool GetIsShowing()
	{
		return gameObject.activeSelf;
	}

	/// <summary>
	/// Sets the location and rotation of the cursor to be at a particular
	/// point and rotate in a particular direction.
	/// </summary>
	/// <param name="position">The position to place the cursor (surface of a planet).</param>
	/// <param name="upNormal">The up normal for the cursor (where to rotate to).</param>
	public void SetPositionAndUpNormal(Vector2 position, Vector2 upNormal, PlanetComponent parentPlanet)
	{
		transform.position = position;
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(upNormal.y, upNormal.x) * Mathf.Rad2Deg - 90);
		this.parentPlanet = parentPlanet;
	}

	/// <summary>
	/// Sets the ghost of the building to have a particular sprite, offset, and scale.
	/// </summary>
	public void SetGhost(Sprite ghostSprite, Vector2 ghostOffset, float ghostScale)
	{
		ghostSpriteRenderer.sprite = ghostSprite;
		ghostSpriteRenderer.transform.localPosition = ghostOffset;
		ghostSpriteRenderer.transform.localScale = Vector3.one * ghostScale;
	}

	/// <summary>
	/// Sets the scale and offset of the box for doing collision checks.
	/// </summary>
	public void SetBuildingCollision(Vector2 collisionCheckSize, Vector2 collisionCheckOffset)
	{
		this.collisionCheckSize = collisionCheckSize;
		this.collisionCheckOffset = collisionCheckOffset;
	}

	public void SetBuildingCableConnectionOffset(Vector2 cableConnectionOffset)
	{
		this.cableConnectionOffset = cableConnectionOffset;
	}

	/// <summary>
	/// Tells the cursor to show whether the current building can be placed.
	/// </summary>
	public void SetBuildingPlaceability(bool canPlace)
	{
		ghostSpriteRenderer.color = canPlace ? placeableColor : notPlaceableColor;
		ShowingCanPlaceBuilding = canPlace;
	}

	/// <summary>
	/// Returns which colliders are overlapping with the cursor.
	/// </summary>
	public Collider2D[] QueryOverlappingColliders()
	{
		Vector2 center = transform.position + transform.up * collisionCheckOffset.y + transform.right * collisionCheckOffset.x;
		Vector2 size = collisionCheckSize;
		float angle = transform.rotation.eulerAngles.z;

		return Physics2D.OverlapBoxAll(center, size, angle);
	}

	public GameObject PlaceBuildingAtLocation(GameObject buildingPrefab)
	{
		return parentPlanet.InstantiateBuildingOnPlanet(buildingPrefab, transform.position, transform.rotation);
	}

	// TODO: Check cable raycast.
}
