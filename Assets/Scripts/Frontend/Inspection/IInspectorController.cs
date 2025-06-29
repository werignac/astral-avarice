using UnityEngine;
using UnityEngine.UIElements;

public interface IInspectorController
{
	/// <summary>
	/// Sets up events and references with the inspectorUI to receive inputs
	/// and update information.
	/// </summary>
	/// <param name="inspectorUI">Inspector UI element sent along with this controller to the Inspector.</param>
	public void ConnectInspectorUI(VisualElement inspectorUI);

	/// <summary>
	/// Cleans up events and references with the inspectorUI to stop receiving inputs
	/// and updating information.
	/// 
	/// Equivalent of Dispose() object should remove all references and cease to exist.
	/// </summary>
	public void DisconnectInspectorUI();

	/// <summary>
	/// Called every frame. Allows the ui controller to update values in the inspector.
	/// </summary>
	public void UpdateUI();
}
