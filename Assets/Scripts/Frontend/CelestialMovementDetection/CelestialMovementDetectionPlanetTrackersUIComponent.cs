using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// A UI element that stays on top of a planet as it moves.
	/// </summary>
	public class CelestialMovementDetectionPlanetTrackerUI : IWorldToScreenUIElement
	{
		private PlanetComponent trackedPlanet;

		public Vector3 WorldPosition => trackedPlanet.transform.position;
		public Vector2 Pivot { get => new Vector2(0.5f, 0.5f); }
		public Vector2 Offset { get; set; }
		public VisualElement UIElement { get; set; }

		public CelestialMovementDetectionPlanetTrackerUI(PlanetComponent trackedPlanet)
		{
			this.trackedPlanet = trackedPlanet;
		}
	}

	/// <summary>
	/// Creates and destroyed planet tracker UI elements as planets start and stop moving.
	/// </summary>
	public class CelestialMovementDetectionPlanetTrackersUIComponent: MonoBehaviour
    {
		[SerializeField] private CelestialMovementDetectionComponent detectionComponent;

		[SerializeField] private WorldToScreenUIManagerComponent worldToScreen;

		[SerializeField] private VisualTreeAsset trackerUITemplate;

		private Dictionary<PlanetComponent, CelestialMovementDetectionPlanetTrackerUI> planetMovementTrackers = new Dictionary<PlanetComponent, CelestialMovementDetectionPlanetTrackerUI>();

		private void Awake()
		{
			detectionComponent.OnAnyPlanetStartMoving.AddListener(Detection_OnAnyPlanetStartMoving);
			detectionComponent.OnAnyPlanetStopMoving.AddListener(Detection_OnAnyPlanetStopMoving);
		}

		private void Detection_OnAnyPlanetStartMoving(PlanetComponent planet)
		{
			TrackPlanet(planet);
		}

		private void Detection_OnAnyPlanetStopMoving(PlanetComponent planet)
		{
			StopTrackingPlanet(planet);
		}

		private void TrackPlanet(PlanetComponent planet)
		{
			Debug.Assert(!planetMovementTrackers.ContainsKey(planet), $"Asked to track planet {planet} movement twice.");

			CelestialMovementDetectionPlanetTrackerUI tracker = MakeTracker(planet);
			planetMovementTrackers[planet] = tracker;
			worldToScreen.Add(tracker, trackerUITemplate);
		}

		private void StopTrackingPlanet(PlanetComponent planet)
		{
			Debug.Assert(planetMovementTrackers.ContainsKey(planet), $"Asked to stop tracking untracked planet {planet}'s movement.");

			CelestialMovementDetectionPlanetTrackerUI tracker = planetMovementTrackers[planet];
			planetMovementTrackers.Remove(planet);
			worldToScreen.Remove(tracker);
		}

		private CelestialMovementDetectionPlanetTrackerUI MakeTracker(PlanetComponent planet)
		{
			CelestialMovementDetectionPlanetTrackerUI tracker = new CelestialMovementDetectionPlanetTrackerUI(planet);
			return tracker;
		}
	}
}
