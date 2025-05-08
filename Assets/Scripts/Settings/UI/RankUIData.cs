using UnityEngine;

[System.Serializable]
public class RankUIData
{
	public string name;
	public Sprite icon;
	// Sprite that shows up in the status UI instead of the normal icon.
	public Sprite statusOverrideIcon;
	// Index in the name of where the symbol would go.
	public int iconReplacementIndex;

	public Sprite StatusIcon
	{
		get => statusOverrideIcon == null ? icon : statusOverrideIcon;
	}

	// String to use to put the rank in rich text.
	public string RichTextIcon
	{
		get => $"<sprite=\"text-icons\" name=\"{icon.name}\">";
	}

	// Uses rich text to combine the name of the rank with the rank icon for the victory screen.
	public string GetRichTextName(float scaleMultiplier)
	{
		int scalePercent = Mathf.FloorToInt(100 * scaleMultiplier);
		return name.Substring(0, iconReplacementIndex) + 
			$"<size={scalePercent}%>{RichTextIcon}<size=100%>" +
			name.Substring(iconReplacementIndex + 1);
	}
}
