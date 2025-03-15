using UnityEngine;

public class GridGroupViewComponent : MonoBehaviour
{
	private const string INTENSITY_PARAMETER = "_Intensity";
	[SerializeField] private Material gridGroupViewPostProcessingMaterial;
	[SerializeField] private bool clearOnAwake = true;

	public bool IsShowing
	{
		get { return (GetIntensity() != 0); }
	}

	private void Awake()
	{
		if (clearOnAwake)
			Hide();
	}

	public void Toggle()
	{
		if (GetIntensity() == 0)
			Show();
		else
			Hide();
	}

	public void Show()
	{
		SetIntensity(1);
	}

	public void Hide()
	{
		SetIntensity(0);
	}

	private void SetIntensity(float value)
	{
		gridGroupViewPostProcessingMaterial.SetFloat(Shader.PropertyToID(INTENSITY_PARAMETER), value);
	}

	private float GetIntensity()
	{
		return gridGroupViewPostProcessingMaterial.GetFloat(Shader.PropertyToID(INTENSITY_PARAMETER));
	}
}
