using System.Collections.Generic;
using UnityEngine;

public class SelectionCursorComponent : MonoBehaviour
{
	private static string ON_CURSOR_ENTER_FUNCTION_NAME = "OnCursorEnter";
	private static string ON_CURSOR_EXIT_FUNCTION_NAME = "OnCursorExit";

	/// <summary>
	/// Class that prioritizes colliders that are directly under
	/// the cursor.
	/// </summary>
	private class CursorDistanceColliderComparer : IComparer<Collider2D>
	{
		/// <summary>
		/// World Space cursor location.
		/// </summary>
		private Vector2 cursorLocation;

		public CursorDistanceColliderComparer(Vector2 cursorLocation)
		{
			this.cursorLocation = cursorLocation;
		}

		public int Compare(Collider2D x, Collider2D y)
		{
			bool xOverlapsWithCursor = x.OverlapPoint(cursorLocation);
			bool yOverlapsWithCursor = y.OverlapPoint(cursorLocation);

			// Both overlap.
			if (xOverlapsWithCursor && yOverlapsWithCursor)
				return 0;

			// Either one is directly under the cursor or none.
			if (xOverlapsWithCursor)
				return -1;

			if (yOverlapsWithCursor)
				return 1;

			// Neither are under the cursor. Compare by closest point.
			float xDistance = Vector2.Distance(cursorLocation, x.ClosestPoint(cursorLocation));
			float yDistance = Vector2.Distance(cursorLocation, y.ClosestPoint(cursorLocation));

			if (xDistance == yDistance)
				return 0;

			if (xDistance < yDistance)
				return -1;

			return 1;
		}
	}

	/// <summary>
	/// How close an object needs to be to the cursor to be considered selected.
	/// The units of the radius are in pixels.
	/// </summary>
	[SerializeField, Min(0f)] float cursorRadius = 20f;

	/// <summary>
	/// Pixel radius * pixel to world scale ratio.
	/// </summary>
	private float WorldSpaceCursorRadius
	{
		get
		{
			return 2 * Camera.main.orthographicSize / Camera.main.scaledPixelHeight * cursorRadius;
		}
	}

	/// <summary>
	/// A list of objects the cursor is hovering over.
	/// Sorted by objects that are exactly under the cursor first and then objects that are near the cursor.
	/// </summary>
	private List<Collider2D> currentlyHovering = new List<Collider2D>();


	/// <summary>
	/// Moves the cursor to the desired location in world space.
	/// Usually the position of the user's mouse.
	/// </summary>
    public void SetPosition(Vector2 position)
	{
		transform.position = position;
	}

	public Vector2 GetPosition()
	{
		return transform.position;
	}

	/// <summary>
	/// Updates an internal list of the objects that this cursor is hovering over.
	/// </summary>
	public void QueryHovering()
	{
		// TODO: Parameterize.
		int layerMask = ~LayerMask.GetMask();

		List<Collider2D> previouslyHovering = currentlyHovering;

		currentlyHovering = new List<Collider2D>(Physics2D.OverlapCircleAll(
			transform.position,
			WorldSpaceCursorRadius,
			layerMask
		));

		foreach (Collider2D pastHover in previouslyHovering)
			if (!currentlyHovering.Contains(pastHover))
				pastHover.SendMessage(ON_CURSOR_EXIT_FUNCTION_NAME, SendMessageOptions.DontRequireReceiver);

		foreach (Collider2D nextHover in currentlyHovering)
			if (!previouslyHovering.Contains(nextHover))
				nextHover.SendMessage(ON_CURSOR_ENTER_FUNCTION_NAME, SendMessageOptions.DontRequireReceiver);

		currentlyHovering.Sort(new CursorDistanceColliderComparer(transform.position));
	}

	public Collider2D[] GetHovering()
	{
		return currentlyHovering.ToArray();
	}

	/// <summary>
	/// Finds a collider that fullfills the passed predicate.
	/// </summary>
	/// <returns>A collider that matches the predicate's conditions. Null otherwise.</returns>
	public Collider2D FindFirstByPredicate(System.Predicate<Collider2D> predicate)
	{
		return currentlyHovering.Find(predicate);
	}

	public Collider2D FindFirstBuildingCollider()
	{
		return FindFirstByPredicate((Collider2D collider) => { return collider.GetComponentInParent<BuildingComponent>() != null; });
	}

	/// <summary>
	/// Gets the building that is currently being hovered over (if there is one).
	/// </summary>
	/// <returns>The building that is being hovered over. Null if no building is being hovered over.</returns>
	public BuildingComponent FindFirstBuilding()
	{
		// Find a building.
		Collider2D buildingCollider = FindFirstBuildingCollider();

		BuildingComponent buildingComponent = buildingCollider == null ? null : buildingCollider.GetComponentInParent<BuildingComponent>();
		return buildingComponent;
	}


}
