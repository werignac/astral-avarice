using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UIElements;

/// <summary>
/// Stores references to all the buildings that can be built by the player.
/// Set in the project settings.
/// </summary>
public class PtUUISettings: ScriptableObject
{
	// Place to save settings.
	public const string uiSettingsResourcesPath = "Settings/GlobalUISettings";
	public const string uiSettingsPath = "Assets/Resources/Settings/GlobalUISettings.asset";

	[SerializeField] private VisualTreeAsset defaultInspectorUI;
	// The buildings that can be placed by the player.
	[SerializeField] private VisualTreeAsset buildingInspectorUI;

	// Getters
	public VisualTreeAsset DefaultInspectorUI
	{
		get => defaultInspectorUI;
	}
	public VisualTreeAsset BuildingInspectorUI
	{
		get => buildingInspectorUI;
	}



	/// <summary>
	/// Gets a singleton GlobalBuildingSettings reference. If there is already a
	/// GlobalBuildingSettings asset, use that. Otherwise, create a new GlobalBuildingSettings object.
	/// Creates an asset for the GlobalBuildingSettings if it doesn't exist, and we're in editor.
	/// </summary>
	/// <returns>The singleton GlobalBuildingSettings.</returns>
	public static PtUUISettings GetOrCreateSettings()
	{
		PtUUISettings settings;
#if UNITY_EDITOR
		settings = AssetDatabase.LoadAssetAtPath<PtUUISettings>(uiSettingsPath);
#else
		settings = Resources.Load<PtUUISettings>(uiSettingsResourcesPath);
#endif
		if (settings == null)
		{
			settings = ScriptableObject.CreateInstance<PtUUISettings>();
				
			// Initialize fields here.

#if UNITY_EDITOR
			AssetDatabase.CreateAsset(settings, uiSettingsPath);
			AssetDatabase.SaveAssets();
#endif
		}
		return settings;
	}

}

