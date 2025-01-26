using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Stores references to all the buildings that can be built by the player.
/// Set in the project settings.
/// </summary>
public class PtUInputSettings: ScriptableObject
{
	// Place to save settings.
	public const string inputSettingsResourcesPath = "Settings/PtUInputSettings";
	public const string inputSettingsPath = "Assets/Resources/Settings/PtUInputSettings.asset";

	[SerializeField] private GameObject playerInputPrefab;

	// TODO: Include buildings that can be placed in the level builder?

	// Getters
	public GameObject PlayerInputPrefab
	{
		get => playerInputPrefab;
	}

	/// <summary>
	/// Gets a singleton GlobalBuildingSettings reference. If there is already a
	/// GlobalBuildingSettings asset, use that. Otherwise, create a new GlobalBuildingSettings object.
	/// Creates an asset for the GlobalBuildingSettings if it doesn't exist, and we're in editor.
	/// </summary>
	/// <returns>The singleton GlobalBuildingSettings.</returns>
	public static PtUInputSettings GetOrCreateSettings()
	{
		PtUInputSettings settings;
#if UNITY_EDITOR
		settings = AssetDatabase.LoadAssetAtPath<PtUInputSettings>(inputSettingsPath);
#else
		settings = Resources.Load<PtUInputSettings>(inputSettingsResourcesPath);
#endif
		if (settings == null)
		{
			settings = ScriptableObject.CreateInstance<PtUInputSettings>();
				
			// Initialize fields here.

#if UNITY_EDITOR
			AssetDatabase.CreateAsset(settings, inputSettingsPath);
			AssetDatabase.SaveAssets();
#endif
		}
		return settings;
	}

}

