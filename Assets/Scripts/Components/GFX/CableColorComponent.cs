using UnityEngine;

public class CableColorComponent : MonoBehaviour
{
	private CableComponent cableComponent;

	[SerializeField] private LineRenderer lineRenderer;
	private Gradient defaultGradient;


	private void Awake()
	{
		cableComponent = GetComponent<CableComponent>();

#if UNITY_EDITOR
		if (lineRenderer == null)
			lineRenderer = GetComponentInChildren<LineRenderer>();
#endif

		defaultGradient = lineRenderer.colorGradient;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		cableComponent.OnCableDemolished.AddListener((CableComponent _) => { ClearColor(); });
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
