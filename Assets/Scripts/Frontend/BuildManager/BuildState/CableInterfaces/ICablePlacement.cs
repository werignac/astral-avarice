using UnityEngine;

namespace AstralAvarice.Frontend
{

	/// <summary>
	/// Interface for actions / states in the BuildManager that place cables.
	/// </summary>
    public interface ICablePlacement
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
    }
}
