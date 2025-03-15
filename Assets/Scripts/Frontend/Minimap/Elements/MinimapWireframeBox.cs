using UnityEngine;

/// <summary>
/// A base class that manipulates a lineRenderer to form a wireframe
/// 2D box.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class MinimapWireframeBox : MonoBehaviour
{
	private LineRenderer boxRenderer;

	protected virtual void Awake()
	{
		boxRenderer = GetComponent<LineRenderer>();
	}

	// Sets the line renderer to have the points given in the bounds.
	public void SetBox(Bounds bounds)
	{
		boxRenderer.positionCount = 4;

		boxRenderer.SetPositions(new Vector3[] {
			new Vector3(bounds.min.x, bounds.min.y, 0),
			new Vector3(bounds.min.x, bounds.max.y, 0),
			new Vector3(bounds.max.x, bounds.max.y, 0),
			new Vector3(bounds.max.x, bounds.min.y, 0),
		});
	}
}
