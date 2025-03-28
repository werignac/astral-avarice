using UnityEngine;
using UnityEngine.Events;


namespace AstralAvarice.Visualization
{
	/// <summary>
	/// State that fires en event when a visual effect is toggled on or off.
	/// Use these when you want static references to UI states with boolean values.
	/// 
	/// For example:
	/// Each solar field in the game needs to know when the button for toggling the solar fields on
	/// or off is pressed. The problem is that these solar fields exist in the prefab for a star / level.
	/// They do not have references to the UI by default.
	/// 
	/// The solar fields could find a reference to the UI via singleton or by searching (or conversely,
	/// the UI could find the solar fields), but all of these options have architecture comprimises
	/// (e.g. dependencies).
	/// 
	/// Instead, each solar field has a reference to a UIToggleState scriptable object. On awake,
	/// the solar fields get the current toggle state (boolean on or off), then they listen to events
	/// for when the state is changed.
	/// 
	/// The UI button also has a reference to this state, and notifies / changes its value when the
	/// user presses the button to toggle the solar fields visualization.
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "VisualizationToggleState", menuName = "Visualization/States/Toggle", order = 0)]
	public class VisualizationToggleState_SO : ScriptableObject
	{
		[SerializeField, InspectorName("Toggle Value")] private bool value;
		[SerializeField] private Sprite icon;
		[SerializeField] private string displayName;
		[Header("PlayerPrefs")]
		[SerializeField, Tooltip("Default value for new players (fresh installs).")]
		private bool newPlayerValue;
		[SerializeField] private string playerPrefName;

		// Invoked when the state changes.
		[HideInInspector] private UnityEvent<bool> OnChanged = new UnityEvent<bool>();

		// Getters

		/// <summary>
		/// Get the state at the current point in time.
		/// </summary>
		public bool Value { get => value; }

		/// <summary>
		/// Gets the icon used to represent this toggleable visualization.
		/// </summary>
		public Sprite Icon => icon;

		/// <summary>
		/// Gets the name used to represent this toggleable visualization.
		/// </summary>
		public string DisplayName => displayName;

		/// <summary>
		/// Set the state of the toggle explicitly.
		/// This calls the OnChanged event if the passed state
		/// is different from the old state.
		/// </summary>
		/// <param name="newValue">The new state value.</param>
		public void SetToggleValue(bool newValue)
		{
			// Minimize unecessary calls.
			if (newValue == value)
				return;

			value = newValue;

			SaveStateToPlayerPrefs();

			OnChanged?.Invoke(value);
		}

		/// <summary>
		/// Set the value of the state to the opposite of the current state's value.
		/// </summary>
		public void Toggle()
		{
			SetToggleValue(!value);
		}

		/// <summary>
		/// Add listeners to the event that is fired when the state changes.
		/// </summary>
		/// <param name="listener">The listener that will be invoked when the state changes. Takes the new toggle state.</param>
		public void AddStateChangeListener(UnityAction<bool> listener)
		{
			OnChanged?.AddListener(listener);
		}

		/// <summary>
		/// Removes listeners to the event that is fired when the state changes.
		/// </summary>
		/// <param name="listener">The listener to remove from the state change event.</param>
		public void RemoveStateChangeListener(UnityAction<bool> listener)
		{
			OnChanged.RemoveListener(listener);
		}


		/// <summary>
		/// Gets the value of the visualization state or read newPlayerValue when the playerprefs
		/// haven't been set yet.
		/// </summary>
		public void FetchStateFromPlayerPrefs()
		{
			bool playerPrefsState = PlayerPrefs.GetInt(playerPrefName, newPlayerValue ? 1 : 0) > 0;
			SetToggleValue(playerPrefsState);
		}

		/// <summary>
		/// Saves the current state of visualization to playerprefs.
		/// </summary>
		public void SaveStateToPlayerPrefs()
		{
			PlayerPrefs.SetInt(playerPrefName, value ? 1 : 0);
		}

	}
}
