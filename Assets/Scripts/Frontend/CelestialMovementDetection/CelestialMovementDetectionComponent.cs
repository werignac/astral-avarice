using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Fires events based on when and how many planets are moving.
	/// When a planet is destroyed, it is considered as stopping.
	/// </summary>
    public class CelestialMovementDetectionComponent : MonoBehaviour
    {
		[SerializeField] private GameController gameController;

		private HashSet<PlanetComponent> movingPlanets = new HashSet<PlanetComponent>();
		
		/// <summary>
		/// Are any planets in the level currently moving?
		/// </summary>
		public bool IsAnyPlanetMoving => movingPlanets.Count > 0;
		/// <summary>
		/// How many planets in the level are currently moving?
		/// </summary>
		public int MovingPlanetsCount => movingPlanets.Count;

		/// <summary>
		/// Invoked whenever any planet starts moving. Passes the planet.
		/// </summary>
		[HideInInspector] public UnityEvent<PlanetComponent> OnAnyPlanetStartMoving = new UnityEvent<PlanetComponent>();
		/// <summary>
		/// Invoked whenever any planet stops moving. Passes the planet.
		/// </summary>
		[HideInInspector] public UnityEvent<PlanetComponent> OnAnyPlanetStopMoving = new UnityEvent<PlanetComponent>();
		/// <summary>
		/// Invoked when IsAnyPlanetMoving goes from false to true.
		/// </summary>
		[HideInInspector] public UnityEvent OnFirstPlanetStartMoving = new UnityEvent();
		/// <summary>
		/// Invoked when IsAnyPlanetMoving goes from true to false.
		/// </summary>
		[HideInInspector] public UnityEvent OnAllPlanetsStopMoving = new UnityEvent(); 

		private void Awake()
		{
			// NOTE: If we ever added a mechanic where planets can appear, this component
			// would need to listen to an event invoked when planets are added.
			gameController.OnLevelLoad.AddListener(GameController_OnLevelLoad);
		}

		private void GameController_OnLevelLoad()
		{
			ListenForMovement(gameController.Planets);
		}

		private void ListenForMovement(IEnumerable<PlanetComponent> planets)
		{
			foreach(PlanetComponent planet in planets)
			{
				// Need to create a new variable due to C# variable capture in lambdas.
				PlanetComponent localPlanet = planet;

				// These delegates won't get destroyed until the planets get destroyed,
				// but that's fine as the lifetime of CelectialMovementDetectionComponent
				// is longer than that of any planet.
				planet.OnStartMoving.AddListener(
					() => this?.Planet_OnStartMoving(localPlanet)
				);
				planet.OnStopMoving.AddListener(
					() => this?.Planet_OnStopMoving(localPlanet)
				);
				planet.OnPlanetDemolished.AddListener(Planet_OnStopMoving);
			}
		}

		private void Planet_OnStartMoving(PlanetComponent planet)
		{
			bool firstPlanetToMove = !IsAnyPlanetMoving;
			
			movingPlanets.Add(planet);
			
			OnAnyPlanetStartMoving?.Invoke(planet);

			if (firstPlanetToMove)
				OnFirstPlanetStartMoving?.Invoke();
		}

		private void Planet_OnStopMoving(PlanetComponent planet)
		{
			bool removedPlanet = movingPlanets.Remove(planet);

			if (!removedPlanet)
				return;

			OnAnyPlanetStopMoving?.Invoke(planet);

			if (!IsAnyPlanetMoving)
				OnAllPlanetsStopMoving?.Invoke();
		}
	}
}
