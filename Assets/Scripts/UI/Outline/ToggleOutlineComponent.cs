using UnityEngine;

/// <summary>
/// Component that makes a list of GFX elements reside on or off the outline layer
/// so that the outline PostProcess draws around these elements.
/// </summary>
public class ToggleOutlineComponent : MonoBehaviour
{
	private const string OUTLINE_LAYER_NAME = "Outline"; 

	[Tooltip("GameObjects that should have their layer toggled on / off the Outline layer. When toggled off, the layer is set to the default on Start.")]
	[SerializeField] private GameObject[] toToggleList;
	private int[] defaultLayerList;

	private bool isOn = false;

	public bool IsOn { get => isOn; }

	private void Start()
	{
		defaultLayerList = new int[toToggleList.Length];

		for (int i = 0; i < toToggleList.Length; i++)
		{
			GameObject toToggle = toToggleList[i];
			defaultLayerList[i] = toToggle.layer;
		}
	}

	public void TurnOnOutline()
	{
		foreach (GameObject toToggle in toToggleList)
			toToggle.layer = LayerMask.NameToLayer(OUTLINE_LAYER_NAME);

		isOn = true;
	}

	public void TurnOffOutline()
	{
		for (int i = 0; i < toToggleList.Length; i++)
		{
			GameObject toToggle = toToggleList[i];
			int defaultLayer = defaultLayerList[i];
			toToggle.layer = defaultLayer;
		}

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
