using System;
using UnityEngine;
using UnityEngine.UIElements;

// In the future, this class may not be needed, as the minimap may be part of
// the UI where it cannot be shown and hidden.
public class MinimapComponent : MonoBehaviour
{
	private const string MINIMAP_ELEMENT_NAME = "Minimap";

	[SerializeField] private MinimapCameraComponent minimapCamera;
	[SerializeField] private CameraMovementComponent mainCamera;

	private UIDocument uiDocument;

	private VisualElement minimapElement;

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		minimapElement = uiDocument.rootVisualElement.Q(MINIMAP_ELEMENT_NAME);
	}

	private void Start()
	{
		minimapElement.RegisterCallback<ClickEvent>(Minimap_OnClick);
	}

	private void Minimap_OnClick(ClickEvent evt)
	{
		Vector2 normalizedPosition = new Vector2(
			evt.localPosition.x / minimapElement.resolvedStyle.width,
			evt.localPosition.y / minimapElement.resolvedStyle.height
		);


		MoveMainCameraToMinimapPosition(normalizedPosition);
	}

	/// <summary>
	/// Takes a position on the minimap and teleports the main camera to that position.
	/// </summary>
	/// <param name="minimapPosition">Position in the minimap to teleport to. Normalized
	/// vector, where 0,0 is the top left.</param>
	private void MoveMainCameraToMinimapPosition(Vector2 minimapPosition)
	{
		Vector2 viewPortPosition = new Vector2(minimapPosition.x, 1 - minimapPosition.y);

		Vector2 mainCameraPosition = minimapCamera.MinimapSpaceToWorldSpace(viewPortPosition);

		mainCamera.MoveTo(mainCameraPosition);
	}

	public bool GetIsShowing()
	{
		return uiDocument.rootVisualElement.style.display == DisplayStyle.Flex;
	}

    public void Show()
	{
		uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
	}

	public void Hide()
	{
		uiDocument.rootVisualElement.style.display = DisplayStyle.None;
	}

	public void Toggle()
	{
		if (GetIsShowing())
			Hide();
		else
			Show();
	}
}
