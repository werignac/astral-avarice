using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// A UI element that stays on top of a planet as it moves.
	/// </summary>
	public class CelestialMovementDetectionPlanetTrackerUI : IWorldToScreenUIElement
	{
		private const string TRACKER_ELEMENT_NAME = "TrackerElement";

		private PlanetComponent trackedPlanet;

		private VisualElement uiElement;

		private float padding;

		public Vector3 WorldPosition => trackedPlanet.transform.position;
		public Vector2 Pivot { get => new Vector2(0.5f, 0.5f); }
		public Vector2 Offset { get; set; }
		public VisualElement UIElement {
			get => uiElement;
			set
			{
				uiElement = value;
				if (value != null)
				{
					VisualElement trackerElement = value.Q(TRACKER_ELEMENT_NAME);	
					trackerElement.style.marginTop = trackerElement.style.marginBottom = trackerElement.style.marginRight = trackerElement.style.marginLeft = -padding;
				}
			}
		}
		public ICollection<WorldToScreenComponent> Components { get; } = new List<WorldToScreenComponent>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="trackedPlanet">The planet to track.</param>
		/// <param name="padding">An offset to give some space between the edges of the tracker and the edges of the planet</param>
		public CelestialMovementDetectionPlanetTrackerUI(PlanetComponent trackedPlanet, float padding = 0)
		{
			this.trackedPlanet = trackedPlanet;
			this.AddComponent<ScaleWithWorldComponent>().WorldSize = Vector2.one * trackedPlanet.Radius * 2;

			this.padding = padding;
		}
	}
}
