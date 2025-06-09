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
public class GlobalBuildingSettings: ScriptableObject
{
	// Place to save settings.
	public const string buildingSettingsResourcesPath = "Settings/GlobalBuildingSettings";
	public const string buildingSettingsPath = "Assets/Resources/Settings/GlobalBuildingSettings.asset";

	[Header("Buildings")]
	// The buildings that can be placed by the player.
	[SerializeField] private BuildingSettingEntry[] buildings;

	[Header("Cables")]
	// The prefab to use to place buildings.
	[SerializeField] private GameObject cablePrefab;
	// How wide the cabels are.
	[SerializeField, Min(0.01f)] private float cableWidth;
	// How long the cables can stretch up to.
	[SerializeField, Min(1f)] private float maxCableLength;
	// For how long cables can overlap with other structures before snapping.
	[SerializeField, Min(0)] private float maxCableOverlapTime;

	[Header("Planets")]
	// Minimum distance the player's cursor needs to be from a planet in the building build state to show the building cursor.
	[SerializeField] private float minDistanceToPlanetToShowBuildingCursor = 5;

	[Header("Destruction VFX")]
	[SerializeField] private GameObject cableDestructionVFXPrefab;
	[SerializeField] private GameObject buildingDestructionVFXPrefab;
	[SerializeField] private GameObject planetDestructionVFXPrefab;

	// Getters
	public BuildingSettingEntry[] Buildings
	{
		get { return buildings; }
	}

	public GameObject CablePrefab
	{
		get { return cablePrefab; }
	}

	public float CableWidth
	{
		get { return cableWidth; }
	}

	public float MaxCableLength
	{
		get { return maxCableLength; }
	}

	public float MaxCableOverlapTime => maxCableOverlapTime;

	public float MinDistanceToPlanetToShowBuildingCursor => minDistanceToPlanetToShowBuildingCursor;

	public GameObject CableDestructionVFXPrefab
	{
		get => cableDestructionVFXPrefab;
	}

	public GameObject BuildingDestructionVFXPrefab
	{
		get => buildingDestructionVFXPrefab;
	}

	public GameObject PlanetDestructionVFXPrefab
	{
		get => planetDestructionVFXPrefab;
	}

	/// <summary>
	/// Gets a singleton GlobalBuildingSettings reference. If there is already a
	/// GlobalBuildingSettings asset, use that. Otherwise, create a new GlobalBuildingSettings object.
	/// Creates an asset for the GlobalBuildingSettings if it doesn't exist, and we're in editor.
	/// </summary>
	/// <returns>The singleton GlobalBuildingSettings.</returns>
	public static GlobalBuildingSettings GetOrCreateSettings()
	{
		GlobalBuildingSettings settings;
#if UNITY_EDITOR
		settings = AssetDatabase.LoadAssetAtPath<GlobalBuildingSettings>(buildingSettingsPath);
#else
		settings = Resources.Load<GlobalBuildingSettings>(buildingSettingsResourcesPath);
#endif
		if (settings == null)
		{
			settings = ScriptableObject.CreateInstance<GlobalBuildingSettings>();
				
			// Initialize fields here.

#if UNITY_EDITOR
			AssetDatabase.CreateAsset(settings, buildingSettingsPath);
			AssetDatabase.SaveAssets();
#endif
		}
		return settings;
	}

}

