using UnityEngine;

public class CableColorComponent : MonoBehaviour
{
	private CableComponent cableComponent;

	private LineRenderer lineRenderer;
	private Gradient defaultGradient;


	private void Awake()
	{
		cableComponent = GetComponent<CableComponent>();
		lineRenderer = GetComponent<LineRenderer>();
		defaultGradient = lineRenderer.colorGradient;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		cableComponent.OnCableHoverStartForDemolish.AddListener(SetColorDemolish);
		cableComponent.OnCableHoverEndForDemolish.AddListener(ClearColor);
    }

	public void SetColorDemolish()
	{
		Color color = PtUUISettings.GetOrCreateSettings().DemolishColor;
		SetColor(color);
	}

	private void SetColor(Color color)
	{
		Gradient gradient = new Gradient();
		gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f) };
		gradient.colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0f) };

		lineRenderer.colorGradient = gradient;
	}

	private void ClearColor()
	{
		lineRenderer.colorGradient = defaultGradient;
	}
}
