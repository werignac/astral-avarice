using UnityEngine;
using UnityEngine.InputSystem;

namespace AstralAvarice.Utils
{
    public static class InputExtentions
    {
		public static Vector2 GetUIDocumentPosition(this Mouse mouse)
		{
			Vector2 mousePosition = mouse.position.ReadValue();
			return new Vector2(mousePosition.x, Screen.height - mousePosition.y);
		}
    }
}
