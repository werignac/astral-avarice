using UnityEngine.Events;

/// <summary>
/// Interface for elements that are part of a grid group (e.g. buildings and
/// cables).
/// </summary>
public interface IGridGroupElement
{
	public int GridGroup { get; }
	public UnityEvent<int> OnGridGroupChanged { get; }
}
