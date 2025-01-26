using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManagerComponent : MonoBehaviour
{
    public static InputManagerComponent Instance { get; private set; }

	[SerializeField] private InputActionAsset inputActionAsset;
	private InputAction acceptInputAction;

	private void Awake()
	{
		// Singleton enforcement.
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		
		Instance = this;

		acceptInputAction = inputActionAsset.FindActionMap("Player")["Accept"];
	}

	private void Update()
	{
		// If we decide to support a gamepad or touchscreen, get the mouse in some other way.

		// Convert the mouse position to a position in world space.
		Vector2 mousePositionWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (BuildManagerComponent.Instance.IsInBuildState())
			BuildManagerComponent.Instance.Hover(mousePositionWorldSpace);

		
		if (acceptInputAction.WasPerformedThisFrame())
		{
			// If the player was clicking on UI, ignore the input.
			if (EventSystem.current.IsPointerOverGameObject())
			{
				Debug.Log("Ignore Input");
				return;
			}

			Debug.Log("Input Not Ignored");

			// If we're currently building, try to place something.
			if (BuildManagerComponent.Instance.IsInBuildState())
				BuildManagerComponent.Instance.SetPlace();
		}
	}
}
