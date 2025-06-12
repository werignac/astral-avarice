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
    }
}
