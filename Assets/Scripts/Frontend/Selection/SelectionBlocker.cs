using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// By default, the selection component does not check for whether the mouse
	/// is over UI. Selection blockers stop the selection component from hovering
	/// or selecting game objects under specific conditions (like the mouse being
	/// over UI).
	///
	/// NOTE: When a selection blocker says that selections should be blocked,
	/// this only tells the selection component not to make new selections,
	/// remove old selections (via user input), nor perform hover updates.
	/// 
	/// Blocking selection does not clear the current selection.
	/// 
	/// </summary>
    public abstract class SelectionBlocker : MonoBehaviour
    {
		/// <summary>
		/// Returns whether we should block selection.
		/// </summary>
		/// <returns>True if we should block selection. False otherwise.</returns>
		public abstract bool GetBlockSelection();
    }
}
