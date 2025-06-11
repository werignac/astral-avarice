using UnityEngine;
using System;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Interface for abstracting the different types of things cables can attach to.
	/// </summary>
	public interface ICableAttachment
    {
		Vector2 GetPosition();
    }

	public class BuildingInstanceCableAttachment : ICableAttachment
	{
		/// <summary>
		/// True when this attachment is being used because the player is hovering over
		/// the building. False when the player has clicked on this building.
		/// </summary>
		public bool IsVolatile { get; private set; }

		public BuildingComponent BuildingInstance { get; private set; }
		
		public BuildingInstanceCableAttachment(
				BuildingComponent buildingInstance,
				bool isVolatile
			)
		{
			if (buildingInstance == null)
				throw new ArgumentNullException("buildingInstance");

			BuildingInstance = buildingInstance;

			IsVolatile = isVolatile;
		}

		public Vector2 GetPosition()
		{
			return BuildingInstance.CableConnectionTransform.position;
		}
	}

	/// <summary>
	/// Attaches a cable cursor to the building cursor (building ghost).
	/// </summary>
	public class BuildingCursorCableAttachment : ICableAttachment
	{
		public BuildingCursorComponent BuildingCursor { get; private set; }

		public BuildingCursorCableAttachment(BuildingCursorComponent buildingCursor)
		{
			if (buildingCursor == null)
				throw new ArgumentNullException("buildingCursor");

			BuildingCursor = buildingCursor;
		}

		public Vector2 GetPosition()
		{
			return BuildingCursor.CableConnectionPosition;
		}
	}

	/// <summary>
	/// Attaches a cable cursor to the player's mouse.
	/// </summary>
	public class CursorCableAttachment : ICableAttachment
	{
		public SelectionCursorComponent SelectionCursor { get; private set; }

		public CursorCableAttachment(SelectionCursorComponent selectionCursor)
		{
			SelectionCursor = selectionCursor;
		}

		public Vector2 GetPosition()
		{
			return SelectionCursor.transform.position;
		}
	}
}
