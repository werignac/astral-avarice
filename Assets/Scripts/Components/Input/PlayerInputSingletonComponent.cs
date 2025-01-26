using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enforces player input to be a singleton.
/// Creates an instance of the player input when any scene is loaded.
/// </summary>
public class PlayerInputSingletonComponent: MonoBehaviour
{
    public static PlayerInputSingletonComponent Instance { get; private set; }

	public PlayerInput Input
	{
		get
		{
			return GetComponent<PlayerInput>();
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void InstantiatePlayerInput()
	{
		Instantiate(PtUInputSettings.GetOrCreateSettings().PlayerInputPrefab);
	}
}
