using UnityEngine;

/// <summary>
/// Object that can be removed by the player when the BuildManagerComponent is in the demolish mode.
/// </summary>
public interface IDemolishable
{
	/// <summary>
	/// The player selected this object for demolition.
	/// </summary>
	public void Demolish();

	/// <summary>
	/// The player is hovering over this object whilst in demolition mode.
	/// </summary>
	public void HoverDemolishStart();

	/// <summary>
	/// The player stopped hovering over this object whilst in demolition mode.
	/// </summary>
	public void HoverDemolishEnd();
}
