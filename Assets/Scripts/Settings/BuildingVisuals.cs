using UnityEngine;

[CreateAssetMenu(fileName = "BuildingVisuals", menuName = "Visual Info/Building")]
public class BuildingVisuals : ScriptableObject
{
	// Name to show in in-game inspector.
	public string buildingName;
	// Icon to show on buttons.
	public Texture2D buildingIcon;
	// Descripton to show in inspector.
	public string buildingDescription;
	// Ghost sprite to use as building cursor.
	public Sprite buildingGhost;
	// Offset of the ghost relative to the building's origin.
	public Vector2 ghostOffset;
	// Scale of the ghost.
	public float ghostScale;
}
