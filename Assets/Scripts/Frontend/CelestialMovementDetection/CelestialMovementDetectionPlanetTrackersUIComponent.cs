using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;

namespace AstralAvarice.Frontend
{

	/// <summary>
	/// Creates and destroys planet tracker UI elements as planets start and stop moving.
	/// </summary>
	public class CelestialMovementDetectionPlanetTrackersUIComponent: MonoBehaviour
    {
		[SerializeField] private CelestialMovementDetectionComponent detectionComponent;

		[SerializeField] private WorldToScreenUIManagerComponent worldToScreen_BelowDefault;
		[SerializeField] private WorldToScreenUIManagerComponent worldToScreen_AboveDefault;

		[SerializeField] private VisualTreeAsset trackerUITemplate;

		private Dictionary<PlanetComponent, CelestialMovementDetectionPlanetTrackerUI> planetMovementTrackers = new Dictionary<PlanetComponent, CelestialMovementDetectionPlanetTrackerUI>();

		[Header("Flash VFX")]

		private const string FLASH_CLASS_NAME = "bright";

		[SerializeField, Min(0.1f)] private float flashPeriod = 1f;

		private Coroutine flashCoroutine;

		private void Awake()
		{
			detectionComponent.OnAnyPlanetStartMoving.AddListener(Detection_OnAnyPlanetStartMoving);
			detectionComponent.OnAnyPlanetStopMoving.AddListener(Detection_OnAnyPlanetStopMoving);

			flashCoroutine = StartCoroutine(FlashLoop());
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

			GetWorldToScreenUIManager(tracker.GetOnOffScreenMode()).Add(tracker, trackerUITemplate);
		}

		private void StopTrackingPlanet(PlanetComponent planet)
		{
			Debug.Assert(planetMovementTrackers.ContainsKey(planet), $"Asked to stop tracking untracked planet {planet}'s movement.");

			CelestialMovementDetectionPlanetTrackerUI tracker = planetMovementTrackers[planet];
			planetMovementTrackers.Remove(planet);
			GetWorldToScreenUIManager(tracker.GetOnOffScreenMode()).Remove(tracker);
		}

		private CelestialMovementDetectionPlanetTrackerUI MakeTracker(PlanetComponent planet)
		{
			CelestialMovementDetectionPlanetTrackerUI tracker = new CelestialMovementDetectionPlanetTrackerUI(planet);
			// Ok to do lambda here. Lifetime of this object always exceeds lifetime of the trackers.
			tracker.OnOffScreenOnScreenModeChanged.AddListener(
				(bool mode) =>
				{
					this?.Tracker_OnOffScreenOnScreenModeChanged(tracker, mode);
				}
			);
			return tracker;
		}

		/// <summary>
		/// Switch layers when a planet moves on / off screen.
		/// </summary>
		/// <param name="tracker">The tracker to move layers.</param>
		/// <param name="isOnScreen">True if the planet's tracker is now on screen. False otherwise.</param>
		private void Tracker_OnOffScreenOnScreenModeChanged(CelestialMovementDetectionPlanetTrackerUI tracker, bool isOnScreen)
		{
			WorldToScreenUIManagerComponent removeFrom = GetOppositeWorldToScreenUIManager(isOnScreen);
			WorldToScreenUIManagerComponent addTo = GetWorldToScreenUIManager(isOnScreen);

			removeFrom.Remove(tracker);
			addTo.Add(tracker);
		}

		/// <summary>
		/// Gets the WorldToScreenUIManagerComponent that is associated
		/// to whether a tracker's planet is on or off screen. If the tracker's
		/// planet is on screen, we want to get the WorldToScreenUIManager that 
		/// is below the default UI. If the tracker's planet
		/// is off screen, we want to get the WorldToScreenUIManager that is above
		/// the default UI.
		/// </summary>
		/// <param name="isOnScreen">Whether the tracker's planet is on screen.</param>
		/// <returns>The WorldToScreenUIManagerComponent that we want to add a tracker element to.</returns>
		private WorldToScreenUIManagerComponent GetWorldToScreenUIManager(bool isOnScreen)
		{
			return isOnScreen ? worldToScreen_BelowDefault : worldToScreen_AboveDefault;
		}

		/// <summary>
		/// Returns the opposite of GetWorldToScreenUIManager.
		/// </summary>
		/// <param name="isOnScreen">Whether the tracker's planet is on screen.</param>
		/// <returns>The opposite of GetOppositeWorldToScreenUIManager.</returns>
		private WorldToScreenUIManagerComponent GetOppositeWorldToScreenUIManager(bool isOnScreen)
		{
			return GetWorldToScreenUIManager(!isOnScreen);
		}

		private IEnumerator FlashLoop()
		{
			while (this != null)
			{
				SetFlash(true);
				yield return new WaitForSecondsRealtime(flashPeriod / 2);

				SetFlash(false);
				yield return new WaitForSecondsRealtime(flashPeriod / 2);
			}
		}

		private void SetFlash(bool onOrOff)
		{
			foreach(IWorldToScreenUIElement tracker in planetMovementTrackers.Values)
			{
				if (onOrOff)
					tracker.UIElement.AddToClassList(FLASH_CLASS_NAME);
				else
					tracker.UIElement.RemoveFromClassList(FLASH_CLASS_NAME);
			}
		}

		private void OnDestroy()
		{
			StopCoroutine(flashCoroutine);
		}
	}
}
