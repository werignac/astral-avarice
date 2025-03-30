using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.Frontend;
using System.Collections.Generic;

public class PlanetHoverResourcesUIElement : IWorldToScreenUIElement
{
	private PlanetComponent hoveredPlanet;
	private VisualElement uiElement;
	private SpecialResourcesContainerUIBinding resourcesBinding;

	public Vector3 WorldPosition => hoveredPlanet ? hoveredPlanet.transform.position : Vector3.zero;
	public Vector2 Pivot { get; set; }
	public Vector2 Offset { get; set; }
	public VisualElement UIElement
	{
		get => uiElement;

		set
		{
			uiElement = value;

			if (value != null)
			{
				value.style.display = hoveredPlanet != null ? DisplayStyle.Flex : DisplayStyle.None;
				SetAllToIgnoreMouse(value);
				resourcesBinding = new SpecialResourcesContainerUIBinding(value);
			}
			else
				resourcesBinding = null;
		}
	}

	public ICollection<WorldToScreenComponent> Components { get; } = new List<WorldToScreenComponent>();

	private static void SetAllToIgnoreMouse(VisualElement uiElement)
	{
		uiElement.pickingMode = PickingMode.Ignore;

		foreach (VisualElement child in uiElement.Children())
			SetAllToIgnoreMouse(child);
	}

	public PlanetHoverResourcesUIElement(PlanetComponent hoveredPlanet)
	{
		SetHoveredPlanet(hoveredPlanet);
	}

	public PlanetHoverResourcesUIElement() : this(null) { }

	public void SetHoveredPlanet(PlanetComponent hoveredPlanet)
	{
		if (this.hoveredPlanet != null)
			this.hoveredPlanet.OnPlanetDemolished.RemoveListener(HoveredPlanet_OnDemolish);

		this.hoveredPlanet = hoveredPlanet;

		if (hoveredPlanet != null)
			hoveredPlanet.OnPlanetDemolished.AddListener(HoveredPlanet_OnDemolish);

		if (uiElement != null)
		{
			uiElement.style.display = hoveredPlanet != null ? DisplayStyle.Flex : DisplayStyle.None;
			UpdatePlanetResources();
		}
	}

	private void HoveredPlanet_OnDemolish(PlanetComponent demolished)
	{
		if (demolished == hoveredPlanet)
			ClearHoveredPlanet();
	}

	public void ClearHoveredPlanet()
	{
		SetHoveredPlanet(null);
	}

	public void UpdatePlanetResources()
	{
		if (hoveredPlanet != null)
			resourcesBinding?.ShowPlanetResources(hoveredPlanet);
	}
}
