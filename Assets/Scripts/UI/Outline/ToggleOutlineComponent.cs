using UnityEngine;

/// <summary>
/// Component that makes a list of GFX elements toggle their IsOutlined property.
/// </summary>
public class ToggleOutlineComponent : MonoBehaviour
{
	private const string IS_OUTLINED_PROPERTY = "_IsOutlined"; 

	[Tooltip("GameObjects that should have their layer toggled on / off the Outline layer. When toggled off, the layer is set to the default on Start.")]
	[SerializeField] private Renderer[] toToggleRenderers;

	private bool isOn = false;

	public bool IsOn { get => isOn; }

	public void TurnOnOutline()
	{
		foreach (Renderer toToggle in toToggleRenderers)
			toToggle.material.SetFloat(Shader.PropertyToID(IS_OUTLINED_PROPERTY), 1);

		isOn = true;
	}

	public void TurnOffOutline()
	{
		foreach (Renderer toToggle in toToggleRenderers)
			toToggle.material.SetFloat(Shader.PropertyToID(IS_OUTLINED_PROPERTY), 0);

		isOn = false;
	}

	public void ToggleOutline()
	{
		if (isOn)
			TurnOffOutline();
		else
			TurnOnOutline();
	}
}
