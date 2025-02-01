using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a menu in the Player Settings window
/// for defining the global building settings.
/// </summary>
public class PtUUISettingsProvider
{
	internal static SerializedObject GetSerializedSettings()
	{
		return new SerializedObject(PtUUISettings.GetOrCreateSettings());
	}

	[SettingsProvider]
	public static SettingsProvider CreateRLSettingsProvider()
	{
		// First parameter is the path in the Settings window.
		// Second parameter is the scope of this setting: it only appears in the Project Settings window.
		var provider = new SettingsProvider("Project/PtU UI", SettingsScope.Project)
		{
			// By default the last token of the path is used as display name if no label is provided.
			label = "UI",
			// Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
			guiHandler = (searchContext) =>
			{
				var settings = GetSerializedSettings();
				EditorGUILayout.PropertyField(settings.FindProperty("defaultInspectorUI"), new GUIContent("Default Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildingInspectorUI"), new GUIContent("Building Inspector"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildingSelectionMaterial"), new GUIContent("Building Selection Material"));
				EditorGUILayout.PropertyField(settings.FindProperty("selectColor"), new GUIContent("Building Selection Color"));
				EditorGUILayout.PropertyField(settings.FindProperty("demolishColor"), new GUIContent("Building Demolish Color"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildUISelectedButtonSprite"), new GUIContent("Build UI Button Select Sprite"));
				EditorGUILayout.PropertyField(settings.FindProperty("buildUIDeselectedButtonSprite"), new GUIContent("Build UI Button Deselect Sprite"));
				
				settings.ApplyModifiedPropertiesWithoutUndo();
				settings.Dispose();
			},

			// Populate the search keywords to enable smart search filtering and label highlighting:
			keywords = new HashSet<string>(new[] { "UI"})
		};

		return provider;
	}
}
