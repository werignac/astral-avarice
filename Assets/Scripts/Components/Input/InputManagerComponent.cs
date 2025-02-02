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
	private InputAction incrementGameSpeedAction;
	private InputAction decrementGameSpeedAction;
	private InputAction redistributeElectricityAction;
	private InputAction pauseAction;

	[SerializeField] private CameraMovementComponent cameraMovementComponent;
	[SerializeField] private SelectionCursorComponent selectionCursor;
	[SerializeField] private InspectorUIComponent inspector;
	[SerializeField] private GameController gameController;
	[SerializeField] private PauseManager pauseManager;

	private void Awake()
	{
		// Singleton enforcement.
		if (Instance != null)
		{
			Destroy(gameObject);
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
		incrementGameSpeedAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["IncrementGameSpeed"];
		decrementGameSpeedAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["DecrementGameSpeed"];
		redistributeElectricityAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["RedistributeElectricity"];
		pauseAction = PlayerInputSingletonComponent.Instance.Input.currentActionMap["Pause"];
		

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
		UpdateInspector();
		UpdateCameraMovement();
		UpdateGameController();
		UpdatePauseManager();
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
		// If the player was clicking on UI, ignore the input.
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}

		if (acceptAction.WasPerformedThisFrame())
		{
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

	private void UpdateInspector()
	{
		// If the player was clicking on UI, ignore the input.
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		// Objects are not selectable whilst in a build mode.
		if (BuildManagerComponent.Instance.IsInBuildState())
			return;

		// Select the hovered object.
		if (acceptAction.WasPerformedThisFrame())
		{
			inspector.TrySelect();
		}

		// Get rid of the selected object (if there is one).
		if (cancelAction.WasPerformedThisFrame())
		{
			inspector.FreeSelect();
		}
	}

	private void UpdateCameraMovement()
	{
		if (cameraMovementComponent != null)
		{
			cameraMovementComponent.SetHoverInput(Input.mousePosition);
			cameraMovementComponent.SetPanningInput(panAction.IsPressed());
			// User may scroll to read build menu, don't zoom when this is the case.
			if (! EventSystem.current.IsPointerOverGameObject())
				cameraMovementComponent.SetZoomInput(zoomAction.ReadValue<float>());
		}
	}

	private void UpdateGameController()
	{
		if (incrementGameSpeedAction.WasPerformedThisFrame())
		{
			gameController.IncrementGameSpeed();
		}

		if (decrementGameSpeedAction.WasPerformedThisFrame())
		{
			gameController.DecrementGameSpeed();
		}

		if (redistributeElectricityAction.WasPerformedThisFrame())
		{
			gameController.RecomputeIncome();
		}
	}

	private void UpdatePauseManager()
	{
		if (pauseAction.WasPerformedThisFrame())
		{
			if (gameController.GamePaused)
			{
				pauseManager.UnpauseGame();
			}
			else
			{
				pauseManager.PauseGame();
			}
			
		}
	}
}
