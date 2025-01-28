using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManagerComponent : MonoBehaviour
{
    public static InputManagerComponent Instance { get; private set; }
	private InputAction acceptAction;
	private InputAction panAction;
	private InputAction cancelAction;
	private InputAction zoomAction;

	[SerializeField] private CameraMovementComponent cameraMovementComponent;
	[SerializeField] private SelectionCursorComponent selectionCursor;

	private void Awake()
	{
		// Singleton enforcement.
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		
		Instance = this;
	}

	private void Start()
	{
		acceptAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Accept"];
		panAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Pan"];
		cancelAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Cancel"];
		zoomAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Zoom"];

		StartCoroutine(RefreshInputComponent());
	}

	// From https://discussions.unity.com/t/no-ui-input-actions-after-scene-load/814381/2
	private IEnumerator RefreshInputComponent()
	{
		yield return new WaitForEndOfFrame();
		PlayerInputSingletonComponent.Instance.Input.enabled = false;
		yield return new WaitForEndOfFrame();
		PlayerInputSingletonComponent.Instance.Input.enabled = true;
	}

	private void Update()
	{
		UpdateSelectionCursor();
		UpdateBuildManager();
		UpdateCameraMovement();
	}

	private void UpdateSelectionCursor()
	{
		// Convert the mouse position to a position in world space.
		Vector2 mousePositionWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		selectionCursor.SetPosition(mousePositionWorldSpace);
		selectionCursor.QueryHovering();
	}

	private void UpdateBuildManager()
	{
		if (acceptAction.WasPerformedThisFrame())
		{
			// If the player was clicking on UI, ignore the input.
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			// If we're currently building, try to place something.
			if (BuildManagerComponent.Instance.IsInBuildState())
				BuildManagerComponent.Instance.SetPlace();
		}

		if (cancelAction.WasPerformedThisFrame())
		{
			if (BuildManagerComponent.Instance.IsInBuildState())
				BuildManagerComponent.Instance.SetNoneState();
		}
	}

	private void UpdateCameraMovement()
	{
		cameraMovementComponent.SetHoverInput(Input.mousePosition);
		cameraMovementComponent.SetPanningInput(panAction.IsPressed());
		cameraMovementComponent.SetZoomInput(zoomAction.ReadValue<float>());
	}
}
