using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a menu in the Player Settings window
/// for defining the global building settings.
/// </summary>
public class PtUInputsettingsProvider
{
	internal static SerializedObject GetSerializedSettings()
	{
		return new SerializedObject(PtUInputSettings.GetOrCreateSettings());
	}

	[SettingsProvider]
	public static SettingsProvider CreateRLSettingsProvider()
	{
		// First parameter is the path in the Settings window.
		// Second parameter is the scope of this setting: it only appears in the Project Settings window.
		var provider = new SettingsProvider("Project/PtU Input", SettingsScope.Project)
		{
			// By default the last token of the path is used as display name if no label is provided.
			label = "Input",
			// Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
			guiHandler = (searchContext) =>
			{
				var settings = GetSerializedSettings();
				EditorGUILayout.PropertyField(settings.FindProperty("playerInputPrefab"), new GUIContent("Player Input Prefab"));
				settings.ApplyModifiedPropertiesWithoutUndo();
				settings.Dispose();
			},

			// Populate the search keywords to enable smart search filtering and label highlighting:
			keywords = new HashSet<string>(new[] { "Input"})
		};

		return provider;
	}
}
