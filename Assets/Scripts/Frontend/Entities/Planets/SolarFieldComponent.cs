using System;
using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class SolarFieldComponent : PlanetDetectionComponent
    {
        private struct SolarResult
		{
			public PlanetComponent planet;
			public int additionalSolar;
		}

		private PlanetComponent planet;

		[SerializeField] private CircleCollider2D solarFieldCollider;
		[SerializeField] private ComputePlanetSolarEnergyEvent computeSolarEnergyEvent;

		private float Radius
		{
			get => solarFieldCollider.radius * transform.lossyScale.x;
			set { solarFieldCollider.radius = value / transform.lossyScale.x; }
		}

		private void Awake()
		{
			planet = GetComponentInParent<PlanetComponent>();
			Radius = planet.SolarOutput;
			HardQueryOverlaps();
		}

		private void OnComputeSolarEnergy()
		{
			SolarResult[] results = ComputeSolarEnergies();
			ApplySolarEnergies(results);
		}

		private SolarResult[] ComputeSolarEnergies()
		{
			SolarResult[] results = new SolarResult[overlappingPlanets.Count];

			int i = 0;

			foreach (PlanetComponent overlappingPlanet in overlappingPlanets)
			{
				results[i] = new SolarResult
				{
					planet = overlappingPlanet,
					additionalSolar = ComputeSolarEnergyForPlanet(overlappingPlanet)
				};

				i++;
			}

			return results;
		}

		private int ComputeSolarEnergyForPlanet(PlanetComponent otherPlanet)
		{
			Vector3 otherPosition = otherPlanet.transform.position;
			return GetSolarEnergyAtPoint(otherPosition);
		}

		public int GetSolarEnergyAtPoint(Vector3 worldPosition)
		{
			int solarOutput = planet.SolarOutput;
			Vector3 starPosition = transform.position;
			return Mathf.Max(0, Mathf.CeilToInt(solarOutput - Vector2.Distance(starPosition, worldPosition)));
		}

		private void ApplySolarEnergies(SolarResult[] results)
		{
			foreach(SolarResult result in results)
			{
				result.planet.AddSolarEnergy(result.additionalSolar);
			}
		}

		protected override void OnOverlappingPlanetsChanged(int lengthChange)
		{
			// If we just emptied our overlap set, stop listening to compute planet velocities event.
			if (lengthChange < 0 && overlappingPlanets.Count == 0)
				computeSolarEnergyEvent.RemoveListener(OnComputeSolarEnergy);
			// If we just added to our overlap set from empty, start listening to compute planet velocities event.
			else if (lengthChange == overlappingPlanets.Count)
				computeSolarEnergyEvent.AddListener(OnComputeSolarEnergy);
		}
	}
}
