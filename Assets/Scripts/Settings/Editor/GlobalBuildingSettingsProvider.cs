using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a menu in the Player Settings window
/// for defining the global building settings.
/// </summary>
public class GlobalBuildingSettingsProvider
{
	internal static SerializedObject GetSerializedSettings()
	{
		return new SerializedObject(GlobalBuildingSettings.GetOrCreateSettings());
	}

	[SettingsProvider]
	public static SettingsProvider CreateRLSettingsProvider()
	{
		// First parameter is the path in the Settings window.
		// Second parameter is the scope of this setting: it only appears in the Project Settings window.
		var provider = new SettingsProvider("Project/Placeable Buildings", SettingsScope.Project)
		{
			// By default the last token of the path is used as display name if no label is provided.
			label = "Placeable Buildings",
			// Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
			guiHandler = (searchContext) =>
			{
				var settings = GetSerializedSettings();
				EditorGUILayout.PropertyField(settings.FindProperty("buildings"), new GUIContent("Player Plaeable Buildings"));
				EditorGUILayout.PropertyField(settings.FindProperty("cablePrefab"), new GUIContent("Cable Prefab"));
				EditorGUILayout.PropertyField(settings.FindProperty("cableWidth"), new GUIContent("Cable Width"));
				EditorGUILayout.PropertyField(settings.FindProperty("maxCableLength"), new GUIContent("Cable Length"));
				EditorGUILayout.PropertyField(settings.FindProperty("maxCableOverlapTime"), new GUIContent("Cable Overlap Time"));
				EditorGUILayout.PropertyField(settings.FindProperty("cableDestructionVFXPrefab"), new GUIContent("Cable Destruction VFX Prefab"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildingDestructionVFXPrefab"), new GUIContent("Building Destruction VFX Prefab"));
				EditorGUILayout.PropertyField(settings.FindProperty("planetDestructionVFXPrefab"), new GUIContent("Planet Destruction VFX Prefab"));
				
				settings.ApplyModifiedPropertiesWithoutUndo();
				settings.Dispose();
			},

			// Populate the search keywords to enable smart search filtering and label highlighting:
			keywords = new HashSet<string>(new[] { "Build", "Building"})
		};

		return provider;
	}
}
