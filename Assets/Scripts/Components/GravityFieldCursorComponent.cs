using UnityEngine;

public class GravityFieldCursorComponent : MonoBehaviour
{
	private SpriteRenderer gravityFieldRenderer;
	private float initialSize;


	private void Awake()
	{
		gravityFieldRenderer = GetComponent<SpriteRenderer>();
		initialSize = transform.localScale.x;
	}

    public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void SetRadii(float outerRadius, float innerRadius)
	{
		transform.localScale = Vector3.one * outerRadius * initialSize * 2;
		gravityFieldRenderer.material.SetFloat(Shader.PropertyToID("_InnerRadius"), innerRadius / outerRadius);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public bool GetIsShowing()
	{
		return gameObject.activeSelf;
	}
}
