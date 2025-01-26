using UnityEngine;

public class CableComponent : MonoBehaviour
{
	private BuildingComponent startBuilding;
	private BuildingComponent endBuilding;

	private LineRenderer lineRenderer;

	// Getters
	public float Length
	{
		get
		{
			return Vector2.Distance(
				startBuilding.CableConnectionTransform.position,
				endBuilding.CableConnectionTransform.position
				);
		}
	}

	public BuildingComponent Start
	{
		get => startBuilding;
	}

	public BuildingComponent End
	{
		get => endBuilding;
	}



	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	public void SetAttachedBuildings(BuildingComponent start, BuildingComponent end)
	{
		startBuilding = start;
		endBuilding = end;
	}

	private void LateUpdate()
	{
		Vector3[] positions = new Vector3[]
		{
			startBuilding.CableConnectionTransform.position,
			endBuilding.CableConnectionTransform.position
		};

		// TODO: Handle cable breaks.

		lineRenderer.SetPositions(positions);
	}
}
