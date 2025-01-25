using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerComponent : MonoBehaviour
{
    public static InputManagerComponent Instance { get; private set; }

	[SerializeField] private InputActionAsset inputActionAsset;

	private void Awake()
	{
		// Singleton enforcement.
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		
		Instance = this;

		RegisterInputActions(inputActionAsset.FindActionMap("Player"));
	}

	private void RegisterInputActions(InputActionMap inputActions)
	{
		inputActions["Accept"].performed += InputAction_AcceptPerformed;
	}

	private void DeregisterInputActions(InputActionMap inputActions)
	{
		inputActions["Accept"].performed -= InputAction_AcceptPerformed;
	}

	private void InputAction_AcceptPerformed(InputAction.CallbackContext obj)
	{
		// If we're currently building, try to place something.
		if (BuildManagerComponent.Instance.IsInBuildState())
			BuildManagerComponent.Instance.SetPlace();
	}

	private void Update()
	{
		// If we decide to support a gamepad or touchscreen, get the mouse in some other way.

		// Convert the mouse position to a position in world space.
		Vector2 mousePositionWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (BuildManagerComponent.Instance.IsInBuildState())
			BuildManagerComponent.Instance.Hover(mousePositionWorldSpace);
	}

	private void OnDestroy()
	{
		DeregisterInputActions(inputActionAsset.FindActionMap("Player"));
	}
}
