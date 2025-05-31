using UnityEngine;

public class CableCursorComponent : MonoBehaviour
{
	private interface LineEnd
	{
		public Vector2 GetEndPosition();

		public bool IsValid();
	}

	private struct LineEndPoint : LineEnd
	{
		public Vector2 point;

		public bool IsValid() => true;
		public Vector2 GetEndPosition() => point;
	}

	private struct LineEndBuilding : LineEnd
	{
		public BuildingComponent building;

		public bool IsValid() => building != null;
		public Vector2 GetEndPosition() => building.CableConnectionTransform.position;
	} 
	// TODO: Add an end that's the building cursor.

	private LineRenderer lineRenderer;
	private BuildingComponent lineStart = null;
	private LineEnd lineEnd = null;
	
	/// <summary>
	/// The current length of the cable cursor.
	/// </summary>
	public float Length { get { return Vector2.Distance(lineStart.CableConnectionTransform.position, lineEnd.GetEndPosition()); } }

	[SerializeField] private Color placeableColor;
	[SerializeField] private Color notPlaceableColor;
	public bool ShowingCanPlaceCable { get; private set; }

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	private void Start()
	{
		// Set width to equal width in settings.
		float cableWidth = GlobalBuildingSettings.GetOrCreateSettings().CableWidth;
		lineRenderer.widthCurve = AnimationCurve.Constant(0, 1, cableWidth);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public bool GetIsShowing()
	{
		return gameObject.activeSelf;
	}

	public void SetStart(BuildingComponent building)
	{
		lineStart = building;
	}

	// Used when the mouse is the end.
	public void SetEndPoint(Vector2 point)
	{
		lineEnd = new LineEndPoint { point = point };
	}

	public void SetEndBuilding(BuildingComponent building)
	{
		lineEnd = new LineEndBuilding { building = building };
	}
	
	public void SetCablePlaceability(bool isPlaceable)
	{
		Color color = isPlaceable ? placeableColor : notPlaceableColor;

		Gradient gradient = new Gradient();
		gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 0) };
		gradient.colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0) };
		lineRenderer.colorGradient = gradient;

		ShowingCanPlaceCable = isPlaceable;
	}

	private void LateUpdate()
	{
		if (lineStart == null || lineEnd == null || ! lineEnd.IsValid())
			return;

		Vector3[] positions = new Vector3[] {
			lineStart.CableConnectionTransform.position,
			lineEnd.GetEndPosition()
		};

		lineRenderer.SetPositions(positions);
	}

	/// <summary>
	/// Returns which colliders are overlapping with the cursor.
	/// </summary>
	public Collider2D[] QueryOverlappingColliders()
	{
		CableComponent.GetBoxFromPoints(
			lineEnd.GetEndPosition(),
			lineStart.CableConnectionTransform.position,
			out Vector2 center,
			out Vector2 size,
			out float angle
			);

		// TODO: Parameterize.
		int layerMask = ~LayerMask.GetMask("PlanetDetection");

		return Physics2D.OverlapBoxAll(center, size, angle, layerMask);
	}

	public GameObject PlaceCableAtLocation(bool isPlayerDemolishable = true)
	{
		// Cannot create a cable floating in space.
		if (lineEnd is LineEndPoint)
			throw new System.Exception("The end of the cable pointer is not set to a building. Cannot construct a cable.");

		// Get the cable prefab.
		GameObject cablePrefab = GlobalBuildingSettings.GetOrCreateSettings().CablePrefab;

		// Get the middle of the two points.
		Vector2 meanPosition = ((Vector2) lineStart.CableConnectionTransform.position + lineEnd.GetEndPosition()) / 2;

		// Create the cable instance.
		GameObject cableInstance = Instantiate(cablePrefab, meanPosition, Quaternion.identity);

		// Set the cable to connect the two buildings of this cursor.
		BuildingComponent endBuilding = ((LineEndBuilding) lineEnd).building;
		CableComponent cableComponent = cableInstance.GetComponent<CableComponent>();
		cableComponent.SetAttachedBuildings(lineStart, endBuilding);
		cableComponent.SetDemolishable(isPlayerDemolishable);

		return cableInstance;
	}
}
