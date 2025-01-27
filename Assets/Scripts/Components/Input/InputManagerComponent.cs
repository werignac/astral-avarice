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

	[SerializeField] private CameraMovementComponent cameraMovementComponent;

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
		// If we decide to support a gamepad or touchscreen, get the mouse in some other way.

		// Convert the mouse position to a position in world space.
		Vector2 mousePositionWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (BuildManagerComponent.Instance.IsInBuildState())
			BuildManagerComponent.Instance.Hover(mousePositionWorldSpace);

		cameraMovementComponent.HoverInput(Input.mousePosition);

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

		cameraMovementComponent.SetPanningInput(panAction.IsPressed());

		
	}
}
