using UnityEngine;

public class GridGroupMinimapRendererComponent : MonoBehaviour
{
	[SerializeField] protected SpriteRenderer minimapRenderer;
	
	protected void SetGridGroupColor(int gridGroup)
	{
		Color color = GridGroupGFX.GridGroupToColor(gridGroup);
		color.a = 1f;
		minimapRenderer.color = color;
	}
}
