using System;
using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class PlanetMinimapRendererComponent : MonoBehaviour
    {
		private PlanetComponent planetComponent;

		[SerializeField] private SpriteRenderer minimapRenderer;

		/// <summary>
		/// Color of the planet in the minimap whilst it isn't moving.
		/// </summary>
		private Color defaultColor;

		private void Awake()
		{
			planetComponent = GetComponentInParent<PlanetComponent>();
		}

		private void Start()
		{
			defaultColor = minimapRenderer.color;

			planetComponent.OnStartMoving.AddListener(Planet_StartMoving);
			planetComponent.OnStopMoving.AddListener(Planet_StopMoving);
		}

		private void Planet_StopMoving()
		{
			minimapRenderer.color = defaultColor;
		}

		private void Planet_StartMoving()
		{
			PtUUISettings uiSettings = PtUUISettings.GetOrCreateSettings();
			minimapRenderer.color = uiSettings.MovingPlanetMinimapColor;
		}
	}
}
