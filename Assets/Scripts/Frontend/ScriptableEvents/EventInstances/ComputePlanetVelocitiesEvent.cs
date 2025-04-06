using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Event invoked when we want to update the velocities of each planet
	/// prior to moving the planets.
	/// 
	/// Is invoked by game controller.
	/// 
	/// Passes deltaTime.
	/// </summary>
	[CreateAssetMenu(fileName = "ComputePlanetVelocitiesEvent", menuName = "Game Events/Compute Planet Velocities")]
	public class ComputePlanetVelocitiesEvent : ScriptableEvent<float> { }
}
