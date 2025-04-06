using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class ThrusterComponent : MonoBehaviour
    {
		[SerializeField] private BuildingComponent building;
		[SerializeField] private ComputePlanetVelocitiesEvent computePlanetVelocitiesEvent;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
			computePlanetVelocitiesEvent.AddListener(OnComputePlanetVelocities);
        }

		private void OnComputePlanetVelocities(float deltaTime)
		{
			if (building.BackendBuilding.IsPowered)
				AddThrusterForce(deltaTime);
		}

		private void AddThrusterForce(float deltaTime)
		{
			Vector2 force = transform.up.normalized * building.Data.thrust * -1;
			building.ParentPlanet.AddForce(force, deltaTime);
		}

		private void OnDestroy()
		{
			computePlanetVelocitiesEvent.RemoveListener(OnComputePlanetVelocities);
		}
	}
}
