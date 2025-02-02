using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

public class PlanetOutlineComponent : MonoBehaviour
{
	private PlanetComponent planetComponent;
	[SerializeField] private SpriteRenderer planetRenderer;
	[SerializeField] private float outlineWidth;

	private void Awake()
	{
		planetComponent = GetComponentInParent<PlanetComponent>();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
			return;
#endif
		planetComponent.OnHoverStart.AddListener(ShowOutline);
		planetComponent.OnSelectedStart.AddListener(ShowOutline);
		planetComponent.OnHoverEnd.AddListener(HideOutline);
		planetComponent.OnSelectedEnd.AddListener(HideOutline);
	}

	private void ShowOutline()
	{
		planetRenderer.material.SetFloat(Shader.PropertyToID("_OutlineWidth"), outlineWidth);
		planetRenderer.material.SetColor(Shader.PropertyToID("_OutlineColor"), PtUUISettings.GetOrCreateSettings().SelectColor);
	}

	private void HideOutline()
	{
		planetRenderer.material.SetFloat(Shader.PropertyToID("_OutlineWidth"), 0f);
	}
}
