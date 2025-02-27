using UnityEngine;

/// <summary>
/// Handles showing a ghost of where a building will be placed,
/// and checks whether the building has enough room to be placed,
/// and whether there is enough distance between 
/// </summary>
public class BuildingCursorComponent : MonoBehaviour
{
	public enum Placeability { YES, YES_WARNING, NO}

	// The size of the 2D box to check for objects that would collide with the incoming building.
	private Vector2 collisionCheckSize = new Vector2(0.7f, 1.6f);
	// The offset from the origin of the building of the 2D box to check for objects that would collide
	// with the incoming building.
	private Vector2 collisionCheckOffset = new Vector2(0, 0.808422f);
	// the offset from the origin of the building where cables get connected to.
	private Vector2 cableConnectionOffset;

	// Reference to the renderer to manipulate for the ghost sprite.
	[SerializeField] private SpriteRenderer ghostSpriteRenderer;
	// The sprite that shows where cables connect to on the building being built.
	[SerializeField] private ConnectionPointCursorComponent connectionPoint;
	// Color to use when something can be placed.
	[SerializeField] private Color placeableColor;
	// Color to use when something can be placed, but it may not be a good idea (missing resources).
	[SerializeField] private Color placeableWithWarningColor;
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

	public void SetParentPlanet(PlanetComponent parentPlanet)
    {
		this.parentPlanet = parentPlanet;
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

		// If we're not displaying a building, don't display the connection point.
		// Otherwise, show the connection point.
		if (ghostSprite == null)
			connectionPoint.Hide();
		else
			connectionPoint.Show();
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

		connectionPoint.SetPosition(CableConnectionPosition);
	}

	/// <summary>
	/// Tells the cursor to show whether the current building can be placed.
	/// </summary>
	public void SetBuildingPlaceability(Placeability canPlace)
	{
		switch (canPlace)
		{
			case Placeability.YES:
				ghostSpriteRenderer.color =  placeableColor;
				connectionPoint.SetColor(placeableColor);
				ShowingCanPlaceBuilding = true;
				break;
			case Placeability.YES_WARNING:
				ghostSpriteRenderer.color = placeableWithWarningColor;
				connectionPoint.SetColor(placeableWithWarningColor);
				ShowingCanPlaceBuilding = true; // Technically can place building.
				break;
			case Placeability.NO:
				ghostSpriteRenderer.color = notPlaceableColor;
				connectionPoint.SetColor(notPlaceableColor);
				ShowingCanPlaceBuilding = false;
				break;
		}
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

	public GameObject PlaceBuildingAtLocation(GameObject buildingPrefab, bool isPlayerDemolishable = true)
	{
		return parentPlanet.InstantiateBuildingOnPlanet(buildingPrefab, transform.position, transform.rotation, isPlayerDemolishable);
	}

	public void MoveBuildingToLocation(BuildingComponent buildingToMove)
    {
		buildingToMove.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }
}
