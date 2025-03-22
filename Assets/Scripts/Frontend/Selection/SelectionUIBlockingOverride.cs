using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// By default, when the player hovers over UI, any hovered selections are
	/// stopped. However, sometimes we need continue hovering over objects.
	/// This override determines when we need to continue hovering.
	/// 
	/// This is mainly used for the inspector to prevent undesireable flickering.
	/// </summary>
    public abstract class SelectionUIBlockingOverride : MonoBehaviour
    {
		/// <summary>
		/// Returns whether we should override the default blocking behaviour.
		/// </summary>
		/// <returns>True if we should continue doing selection hovering. False otherwise.</returns>
		public abstract bool GetIgnoreUI();
    }
}
