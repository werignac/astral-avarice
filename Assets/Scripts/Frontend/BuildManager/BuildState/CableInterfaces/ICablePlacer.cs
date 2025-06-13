using UnityEngine;

namespace AstralAvarice.Frontend
{

	/// <summary>
	/// Interface for actions / states in the BuildManager that place cables.
	/// </summary>
    public interface ICablePlacer
    {
		/// <summary>
		/// Returns the first object the cable is attached to.
		/// </summary>
		ICableAttachment GetFromAttachment();

		/// <summary>
		/// Returns the second object the cable is attached to.
		/// </summary>
		ICableAttachment GetToAttachment();

		/// <summary>
		/// The current length of the cable in world-space.
		/// </summary>
		float Length { get; }

		/// <summary>
		/// Returns the cable cursor used for placing the cable.
		/// </summary>
		/// <returns>The cable cursor that will be used for placing the cable.</returns>
		CableCursorComponent GetCableCursor();

		/// <summary>
		/// Returns whether this cable placer is moving an existing cable.
		/// If so, sets movingCable to be the cable being moved.
		/// </summary>
		/// <param name="movingCable">The cable being moved by this ICablePlacer.</param>
		/// <returns>True if we are moving a cable. False otherwise.</returns>
		bool TryGetMovingCable(out CableComponent movingCable);
    }
}
