using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

namespace AstralAvarice.Frontend
{
	public class PlanetTrackerComponent : WorldToScreenComponent
	{
		private const string ON_SCREEN_TRACKER_ELEMENT_NAME = "OnScreenTrackerElement";
		private const string OFF_SCREEN_TRACKER_ELEMENT_NAME = "OffScreenTrackerElement";

		private VisualElement onScreenElement;
		private VisualElement offScreenElement;

		private PlanetComponent TrackedPlanet { get; set; }

		private float OnScreenPadding { get; set; }

		public Vector3 OutWorldPosition { get; private set; }

		public void TrackPlanet(PlanetComponent trackedPlanet, float onScreenPadding)
		{
			TrackedPlanet = trackedPlanet;
			OnScreenPadding = onScreenPadding;
		}

		public override void Update()
		{
			if (onScreenElement == null)
				InitializeOnOffScreenElements();

			UpdateOnOffScreen();
		}

		private void UpdateOnOffScreen()
		{
			bool isOnScreen = GetIsOnScreen();

			if (isOnScreen)
				UpdateOnScreen();
			else
				UpdateOffScreen();
		}

		private bool GetIsOnScreen()
		{
			// TODO: include planet radius.

			Camera mainCamera = Camera.main;
			Vector2 viewportPoint = mainCamera.WorldToViewportPoint(TrackedPlanet.transform.position);
			return 0 <= viewportPoint.x && viewportPoint.x <= 1 && 0 <= viewportPoint.y && viewportPoint.y <= 1;
		}

		private void UpdateOnScreen()
		{
			onScreenElement.style.display = DisplayStyle.Flex;
			offScreenElement.style.display = DisplayStyle.None;

			OutWorldPosition = TrackedPlanet.transform.position;
		}

		private void UpdateOffScreen()
		{
			onScreenElement.style.display = DisplayStyle.None;
			offScreenElement.style.display = DisplayStyle.Flex;

			OutWorldPosition = GetClosestOnScreenPosition(TrackedPlanet.transform.position, Camera.main);

			// Rotate the element to point towards the tracked planet.
			Vector2 directionFromCamera = TrackedPlanet.transform.position - Camera.main.transform.position;
			float angle = -Mathf.Atan2(directionFromCamera.y, directionFromCamera.x) - Mathf.PI / 2;
			offScreenElement.style.rotate = new Rotate(new Angle(angle, AngleUnit.Radian));
		}

		private static Vector3 GetClosestOnScreenPosition(Vector3 offScreenPosition, Camera mainCamera)
		{
			Vector2 viewportPoint = mainCamera.WorldToViewportPoint(offScreenPosition);

			// Center the viewport point around origin of vector space.
			viewportPoint -= Vector2.one * 0.5f;

			// Scale the viewport point until both of the axes are <= 0.5f.
			float horizontalViewportDistance = Mathf.Abs(viewportPoint.x);
			float verticalViewportDistance = Mathf.Abs(viewportPoint.y);
			float largestDistance = Mathf.Max(horizontalViewportDistance, verticalViewportDistance);
			viewportPoint *= 0.5f / largestDistance;

			// De-center the viewport point.
			viewportPoint += Vector2.one * 0.5f;

			return mainCamera.ViewportToWorldPoint(viewportPoint);
		}

		private void InitializeOnOffScreenElements()
		{
			onScreenElement = UIElement.Q(ON_SCREEN_TRACKER_ELEMENT_NAME);
			offScreenElement = UIElement.Q(OFF_SCREEN_TRACKER_ELEMENT_NAME);

			onScreenElement.style.marginTop = onScreenElement.style.marginBottom = onScreenElement.style.marginRight = onScreenElement.style.marginLeft = -OnScreenPadding;
		}
	}

	/// <summary>
	/// A UI element that stays on top of a planet as it moves.
	/// </summary>
	public class CelestialMovementDetectionPlanetTrackerUI : IWorldToScreenUIElement
	{
		private PlanetTrackerComponent planetTrackerComponent;

		public Vector3 WorldPosition => planetTrackerComponent.OutWorldPosition;
		public Vector2 Pivot { get => new Vector2(0.5f, 0.5f); }
		public Vector2 Offset { get; set; }
		public VisualElement UIElement { get; set; }
		public ICollection<WorldToScreenComponent> Components { get; } = new List<WorldToScreenComponent>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="trackedPlanet">The planet to track.</param>
		/// <param name="padding">An offset to give some space between the edges of the tracker and the edges of the planet</param>
		public CelestialMovementDetectionPlanetTrackerUI(PlanetComponent trackedPlanet, float padding = 0)
		{
			this.AddComponent<ScaleWithWorldComponent>().WorldSize = Vector2.one * trackedPlanet.Radius * 2;
			planetTrackerComponent = this.AddComponent<PlanetTrackerComponent>();
			planetTrackerComponent.TrackPlanet(trackedPlanet, padding);
		}
	}
}
