using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using AstralAvarice.Utils;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	public class PlanetTrackerComponent : WorldToScreenComponent
	{
		private const string ON_SCREEN_TRACKER_ELEMENT_NAME = "OnScreenTrackerElement";
		private const string OFF_SCREEN_TRACKER_ELEMENT_NAME = "OffScreenTrackerElement";

		private VisualElement onScreenElement;
		private VisualElement offScreenElement;

		/// <summary>
		/// Used to track whether we need to invoke the OnPlanetOnScreenChanged
		/// event.
		/// </summary>
		private bool planetOnScreenLastUpdate = false;

		private PlanetComponent TrackedPlanet { get; set; }

		/// <summary>
		/// TODO: update margins if this changes?
		/// </summary>
		private float OnScreenPadding { get; set; }
		/// <summary>
		/// TODO: update margins if this changes?
		/// </summary>
		private float OnScreenWidth { get; set; }
		private float OnScreenDotAspect { get; set; }
		private int OnScreenDotCount { get; set; }

		public Vector3 OutWorldPosition { get; private set; }

		public bool IsPlanetOnScreen => planetOnScreenLastUpdate;

		/// <summary>
		/// Invoked when the planet moves on / off screen.
		/// True is passed when the planet moves on screen.
		/// False is passed when the planet moves off screen.
		/// </summary>
		public UnityEvent<bool> OnPlanetOnScreenChanged = new UnityEvent<bool>();

		public void TrackPlanet(
			PlanetComponent trackedPlanet,
			float onScreenPadding = 10,
			float onScreenWidth = 20f,
			float onScreenDotAspect = 1.5f,
			int onScreenDotCount = 10
			)
		{
			TrackedPlanet = trackedPlanet;

			OnScreenPadding = onScreenPadding;
			OnScreenWidth = onScreenWidth;
			OnScreenDotAspect = onScreenDotAspect;
			OnScreenDotCount = onScreenDotCount;
		}

		public override void Update()
		{
			if (onScreenElement == null)
				InitializeOnOffScreenElements();

			UpdateOnOffScreen();
		}

		private void UpdateOnOffScreen()
		{
			bool isOnScreen = ComputeIsOnScreen();

			if (isOnScreen)
				UpdateOnScreen();
			else
				UpdateOffScreen();

			// NOTE: If the planet is on screen on the first update, this
			// will be invoked on the first update. If the planet is off screen
			// on the first update, this will not be invoked on the first update.
			if (isOnScreen ^ planetOnScreenLastUpdate)
			{
				OnPlanetOnScreenChanged.Invoke(isOnScreen);
				planetOnScreenLastUpdate = isOnScreen;
			}
		}

		private bool ComputeIsOnScreen()
		{
			Camera mainCamera = Camera.main;
			Vector2 planetPosition = TrackedPlanet.transform.position;
			Vector2 planetDisplacementFromCamera = (Vector2) mainCamera.transform.position - planetPosition;
			planetDisplacementFromCamera = Vector2.ClampMagnitude(planetDisplacementFromCamera, TrackedPlanet.Radius);
			Vector2 closestPointToCamera = planetPosition + planetDisplacementFromCamera;

			Vector2 viewportPoint = mainCamera.WorldToViewportPoint(closestPointToCamera);
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

			OutWorldPosition = GetClosestOnScreenPosition(TrackedPlanet.transform.position, Camera.main, out Vector2 direction);

			// Rotate the element to point towards the tracked planet.
			//Vector2 directionFromCamera = TrackedPlanet.transform.position - OutWorldPosition;
			float angle = -Mathf.Atan2(direction.y, direction.x) - Mathf.PI / 2;
			offScreenElement.style.rotate = new Rotate(new Angle(angle, AngleUnit.Radian));
		}

		private static Vector3 GetClosestOnScreenPosition(Vector3 offScreenPosition, Camera mainCamera, out Vector2 direction)
		{
			Vector2 viewportPoint = mainCamera.WorldToViewportPoint(offScreenPosition);
			
			// Clamp to sides of screen.
			viewportPoint.x = Mathf.Clamp01(viewportPoint.x);
			viewportPoint.y = Mathf.Clamp01(viewportPoint.y);

			// Form a direction based on which sides of the screen the ui element is clamped to.
			direction = new Vector2();

			if (viewportPoint.x == 1)
				direction.x = 1;
			if (viewportPoint.x == 0)
				direction.x = -1;
			if (viewportPoint.y == 1)
				direction.y = 1;
			if (viewportPoint.y == 0)
				direction.y = -1;

			return mainCamera.ViewportToWorldPoint(viewportPoint);
		}

		private void InitializeOnOffScreenElements()
		{
			onScreenElement = UIElement.Q(ON_SCREEN_TRACKER_ELEMENT_NAME);
			offScreenElement = UIElement.Q(OFF_SCREEN_TRACKER_ELEMENT_NAME);

			onScreenElement.style.marginTop = onScreenElement.style.marginBottom = onScreenElement.style.marginRight = onScreenElement.style.marginLeft = -(OnScreenPadding + OnScreenWidth);
			onScreenElement.generateVisualContent += DrawOnScreenTrackerElement;
			onScreenElement.MarkDirtyRepaint();
		}

		private void DrawOnScreenTrackerElement(MeshGenerationContext ctx)
		{
			Painter2D painter2D = ctx.painter2D;

			Color color = onScreenElement.resolvedStyle.unityBackgroundImageTintColor;
			color.a = 1;
			painter2D.strokeColor = color;

			Vector2 center = new Vector2(onScreenElement.resolvedStyle.width / 2, onScreenElement.resolvedStyle.height / 2);
			float outerRadius = Mathf.Min(onScreenElement.resolvedStyle.width, onScreenElement.resolvedStyle.height) / 2;
			float innerRadius = outerRadius - OnScreenWidth;

			float dotAndSpaceAngle = 360 / OnScreenDotCount;
			float dotAngle = dotAndSpaceAngle * (OnScreenDotAspect / (OnScreenDotAspect + 1));
			
			for (int i = 0; i < OnScreenDotCount; i++)
				painter2D.DrawTorusSlice(i * dotAndSpaceAngle, i * dotAndSpaceAngle + dotAngle, center, outerRadius, innerRadius);
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
		/// Invoked when the planet this tracker is tracking goes on / off screen.
		/// True when the planet is on screen and the tracker is a circle.
		/// False when the planet is off screen and the tracker is an arrow.
		/// </summary>
		public UnityEvent<bool> OnOffScreenOnScreenModeChanged = new UnityEvent<bool>();

		/// <param name="trackedPlanet">The planet to track.</param>
		/// <param name="padding">An offset to give some space between the edges of the tracker and the edges of the planet</param>
		public CelestialMovementDetectionPlanetTrackerUI(PlanetComponent trackedPlanet, float padding = 0)
		{
			this.AddComponent<ScaleWithWorldComponent>().WorldSize = Vector2.one * trackedPlanet.Radius * 2;
			planetTrackerComponent = this.AddComponent<PlanetTrackerComponent>();
			planetTrackerComponent.TrackPlanet(trackedPlanet, padding);
			// Chain invokations.
			planetTrackerComponent.OnPlanetOnScreenChanged.AddListener(OnOffScreenOnScreenModeChanged.Invoke);
		}


		/// <returns>
		/// True when the tracked planet is on screen and the tracker is a circle.
		/// False when the tracked planet is off screen and the tracker is an arrow.
		/// </returns>
		public bool GetOnOffScreenMode()
		{
			return planetTrackerComponent.IsPlanetOnScreen;
		}
	}
}
