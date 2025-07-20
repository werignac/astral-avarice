using UnityEngine;

namespace AstralAvarice.Frontend
{
    [CreateAssetMenu(fileName = "InputEventBus", menuName = "Input/InputEventBus")]
    public class InputEventBus_SO : ScriptableObject
    {
		/// <summary>
		/// World-space position of the cursor.
		/// </summary>
		public Vector2 CursorPosition { get; set; }
    }
}
