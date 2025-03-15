using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the position and animation of the connection point of a building cursor.
/// This component is on the object that shows where cables connect to the building.
/// 
/// The object pulses, starting from a small size, growing outwards, and losing opacity.
/// The controls for this animation oare in the component's fields.
/// </summary>
public class ConnectionPointCursorComponent: MonoBehaviour
{
	private Renderer connectionPointRenderer;

	[SerializeField] private AnimationCurve sizeCurve;
	[SerializeField] private float sizeMultiplier;
	[SerializeField] private AnimationCurve opacityCurve;
	[SerializeField] private float period;

	private Material RendererMaterial { get => connectionPointRenderer.material; }

	private Coroutine animationCoroutine;

	private void Awake()
	{
		connectionPointRenderer = GetComponent<Renderer>();
	}

	/// <summary>
	/// Animation loop.
	/// </summary>
	private IEnumerator Animation()
	{
		// Keep animating until destruction.
		while (this != null)
		{
			UpdateAnimationStep();

			yield return new WaitForEndOfFrame();
		}
	}

	private void UpdateAnimationStep()
	{
		// Value between 0 and 1 of progress in animation.
		float animationTimePosition = (Time.unscaledTime % period) / period;

		Vector2 size = Vector2.one * sizeCurve.Evaluate(animationTimePosition) * sizeMultiplier;
		
		float alpha = opacityCurve.Evaluate(animationTimePosition);
		Color newColor = RendererMaterial.color;
		newColor.a = alpha;

		transform.localScale = size;
		RendererMaterial.color = newColor;
	}

	/// <summary>
	/// Sets the color of the cursor, but not the opacity.
	/// </summary>
	public void SetColor(Color color)
	{
		float alpha = RendererMaterial.color.a;
		color.a = alpha;
		RendererMaterial.color = color;
	}

	/// <summary>
	/// Sets the position of the cursor in world space.
	/// </summary>
	public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	/// <summary>
	/// Clean up coroutine on destroy.
	/// </summary>
	private void OnDestroy()
	{
		if (animationCoroutine == null)
			return;

		StopCoroutine(animationCoroutine);
	}

	private void OnEnable()
	{
		if (animationCoroutine != null)
			StopCoroutine(animationCoroutine);
		animationCoroutine = StartCoroutine(Animation());
	}

	public void Show()
	{
		if (connectionPointRenderer == null)
			return;

		connectionPointRenderer.enabled = true;
	}

	public void Hide()
	{
		if (connectionPointRenderer == null)
			return;

		connectionPointRenderer.enabled = false;
	}

}
