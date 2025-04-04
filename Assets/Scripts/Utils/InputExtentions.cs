using UnityEngine;
using UnityEngine.InputSystem;

namespace AstralAvarice.Utils
{
    public static class InputExtentions
    {
		public static Vector2 GetUIDocumentPosition(this Mouse mouse)
		{
			Vector2 mousePosition = mouse.position.ReadValue();
			mousePosition = new Vector2(mousePosition.x * 1080 / Screen.height, mousePosition.y * 1080 / Screen.height);
			return new Vector2(mousePosition.x, 1080 - mousePosition.y);
		}
    }
}
