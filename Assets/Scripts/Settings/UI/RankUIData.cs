using UnityEngine;

[System.Serializable]
public class RankUIData
{
	public string name;
	public Sprite icon;
	// Sprite that shows up in the status UI instead of the normal icon.
	public Sprite statusOverrideIcon;
	// Index in the name of where the symbol would go.
	public int symbolReplacementIndex;

	public Sprite StatusIcon
	{
		get => statusOverrideIcon == null ? icon : statusOverrideIcon;
	}

	// Divides the name in halves based on where the symbol would go when we combine the name and the symbol into
	// one UI element.
	public string CombinedPrefix
	{
		get => name.Substring(0, symbolReplacementIndex);
	}

	public string CombinedPostfix
	{
		get => name.Substring(symbolReplacementIndex + 1);
	}
}
