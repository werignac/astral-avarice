using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Component that controls the UI that says "Place a Building to Start"
	/// at the start of every level. Hides itself once the player places
	/// a building, makes a change, etc.
	/// 
	/// The UI is shown / hidden using the enabled state of the monobehaviour.
	/// </summary>
    public class PlaceABuildingToStartUIOverlayComponent: MonoBehaviour
    {
		[SerializeField] private UIDocument uiDocument;
		[SerializeField] private GameController gameController;

		private void Start()
		{
			// Disable the UI once the game has started.
			gameController.OnGameStart.AddListener(() => enabled = false);
		}


		private void OnEnable()
		{
			uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
		}

		private void OnDisable()
		{
			// Check for null due OnDisable being called when destroyed.
			if (uiDocument)
				uiDocument.rootVisualElement.style.display = DisplayStyle.None;
		}
	}
}
