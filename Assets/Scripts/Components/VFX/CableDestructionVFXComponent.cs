using UnityEngine;

public class CableDestructionVFXComponent : MonoBehaviour
{
	private const int GFX_CHILD_INDEX = 1;

	// How long the cable retract animation takes.
	private const float CABLE_RETRACT_DURATION = 0.5f;

	private BuildingComponent buildingStart;
	private BuildingComponent buildingEnd;

	private bool IsStartConnected { get { return buildingStart != null; } }
	private bool IsEndConnected { get { return buildingEnd != null; } }

	[SerializeField] private LineRenderer startCableRenderer;
	[SerializeField] private LineRenderer endCableRenderer;

	private bool markedForDestroy = false;

	private float animationProgress = 0;

	private float initialLength;
	private bool startWithbothEnds;

	public void RunVFX(CableComponent cable)
	{
		buildingStart = cable.Start;
		buildingEnd = cable.End;

		// If the cable is not attached to anything, immediately destroy.
		if ((!IsStartConnected) && (!IsEndConnected) || cable.transform.childCount == 0)
		{
			MarkForDestroy();
			startCableRenderer.gameObject.SetActive(false);
			endCableRenderer.gameObject.SetActive(false);
			return;
		}

		LineRenderer cableRenderer = cable.transform.GetChild(GFX_CHILD_INDEX).GetComponent<LineRenderer>();

		// Add alpha points on the gradient to use for animation.
		InitializePoints(cableRenderer);

	}

	private void InitializePoints(LineRenderer cableRenderer)
	{
		Vector3 start = cableRenderer.GetPosition(0);
		Vector3 end = cableRenderer.GetPosition(1);
		initialLength = Vector3.Distance(start, end);

		if (IsStartConnected && IsEndConnected)
		{
			startWithbothEnds = true;
			Vector3 mean = (start + end) / 2;
			startCableRenderer.SetPosition(0, start);
			startCableRenderer.SetPosition(1, mean);
			endCableRenderer.SetPosition(0, mean);
			endCableRenderer.SetPosition(1, end);
		}
		else 
		{
			startWithbothEnds = false;
			if (IsStartConnected)
			{
				startCableRenderer.SetPosition(0, start);
				startCableRenderer.SetPosition(1, end);
				endCableRenderer.gameObject.SetActive(false);
			} else
			{
				endCableRenderer.SetPosition(0, start);
				endCableRenderer.SetPosition(1, end);
				startCableRenderer.gameObject.SetActive(false);
			}
		}
		
	}
	

	private void MarkForDestroy()
	{
		markedForDestroy = true;
	}

	private void LateUpdate()
	{
		UpdateCableEnds();
		UpdateAnimation();

		if (markedForDestroy)
			Destroy(gameObject);
	}

	private void UpdateCableEnds()
	{
		if (IsStartConnected)
		{
			startCableRenderer.SetPosition(0, buildingStart.CableConnectionTransform.position);
		}

		if (IsEndConnected)
		{
			endCableRenderer.SetPosition(1, buildingEnd.CableConnectionTransform.position);
		}
	}

	private void UpdateAnimation()
	{
		// Handle cleanup on buildings destroyed whilst animated.
		if ((!IsStartConnected) && startCableRenderer.gameObject.activeSelf)
		{
			startCableRenderer.gameObject.SetActive(false);
		}

		if ((!IsEndConnected) && endCableRenderer.gameObject.activeSelf)
		{
			endCableRenderer.gameObject.SetActive(false);
		}

		if ((!IsStartConnected) && (!IsEndConnected))
		{
			MarkForDestroy();
			return;
		}

		animationProgress += Time.deltaTime;

		// If we started off with just the cable being destroyed...
		float distance = initialLength * (1 - animationProgress / CABLE_RETRACT_DURATION);

		if (startWithbothEnds)
			distance /= 2;

		if (startCableRenderer.gameObject.activeSelf)
		{
			Vector3 goal = startCableRenderer.GetPosition(0);
			Vector3 move = startCableRenderer.GetPosition(1);
			startCableRenderer.SetPosition(1, UpdatePoint(goal, move, distance));
		}

		if (endCableRenderer.gameObject.activeSelf)
		{
			Vector3 goal = endCableRenderer.GetPosition(1);
			Vector3 move = endCableRenderer.GetPosition(0);
			endCableRenderer.SetPosition(0, UpdatePoint(goal, move, distance));
		}


		if (animationProgress >= CABLE_RETRACT_DURATION)
		{
			MarkForDestroy();
		}
	}

	private Vector3 UpdatePoint(Vector3 goal, Vector3 move, float distance)
	{
		if (Vector3.Distance(goal, move) > distance)
		{
			move = (move - goal).normalized * distance + goal;
		}
		return move;
	}


}
