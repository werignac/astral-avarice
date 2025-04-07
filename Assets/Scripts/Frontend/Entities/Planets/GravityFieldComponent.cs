using UnityEngine;
using System.Collections.Generic;
using System;

namespace AstralAvarice.Frontend
{
    public class GravityFieldComponent : PlanetDetectionComponent
    {
		private struct GravityResult
		{
			public PlanetComponent planet;
			public Vector3 velocityChange;
		}

		private PlanetComponent planet;

		[SerializeField] private CircleCollider2D gravityFieldCollider;
		[SerializeField] private ComputePlanetVelocitiesEvent computePlanetVelocitiesEvent;

		private float Radius
		{
			get => gravityFieldCollider.radius * transform.lossyScale.x;
			set { gravityFieldCollider.radius = value / transform.lossyScale.x; }
		}

		private void Awake()
		{
			planet = GetComponentInParent<PlanetComponent>();
		}

		private void Start()
		{
			Planet_OnMassChanged();
			planet.OnMassChanged.AddListener(Planet_OnMassChanged);

			// Collect initial overlaps.
			HardQueryOverlaps();
		}

		private void Planet_OnMassChanged()
		{
			Radius = planet.GravityRadius;
		}

		/// <summary>
		/// Called when computePlanetVelocitiesEvent is invoked
		/// and we have some planets in our gravity field.
		/// </summary>
		/// <param name="deltaTime">Time between frames including game speed.</param>
		private void OnComputePlanetVelocities(float deltaTime)
		{
			GravityResult[] results = ComputeGravityForces(deltaTime);
			ApplyGravityForces(results);
		}

		private GravityResult[] ComputeGravityForces(float deltaTime)
		{
			GravityResult[] results = new GravityResult[overlappingPlanets.Count];

			int i = 0;

			foreach(PlanetComponent otherPlanet in overlappingPlanets)
			{
				GravityResult result = new GravityResult
				{
					planet = otherPlanet,
					velocityChange = ComputeGravityForceForPlanet(otherPlanet, deltaTime)
				};

				results[i] = result;
				i++;
			}

			return results;
		}

		public Vector3 ComputeGravityForceForPlanet(PlanetComponent otherPlanet, float deltaTime)
		{
			Vector3 difference = transform.position - otherPlanet.transform.position;
			
			float magnitude;

			// NOTE: Carried over formula from GameController.
			if (difference.magnitude > Radius)
				magnitude = 0;
			else
				magnitude = Radius / difference.magnitude / otherPlanet.GetTotalMass() * deltaTime;


			return difference.normalized * magnitude;
		}

		private void ApplyGravityForces(GravityResult[] results)
		{
			foreach(GravityResult result in results)
			{
				result.planet.AddVelocity(result.velocityChange);
			}
		}

		/// <summary>
		/// Called when the overlapping planets set changes.
		/// </summary>
		/// <param name="lengthChange">Number of elements added / removed. Never 0.</param>
		protected override void OnOverlappingPlanetsChanged(int lengthChange)
		{
			// If we just emptied our overlap set, stop listening to compute planet velocities event.
			if (lengthChange < 0 && overlappingPlanets.Count == 0)
				computePlanetVelocitiesEvent.RemoveListener(OnComputePlanetVelocities);
			// If we just added to our overlap set from empty, start listening to compute planet velocities event.
			else if (lengthChange == overlappingPlanets.Count)
				computePlanetVelocitiesEvent.AddListener(OnComputePlanetVelocities);
		}
	}
}
