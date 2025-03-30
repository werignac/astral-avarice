using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using AstralAvarice.Frontend;

public class PlanetHoverResourcesComponent : MonoBehaviour
{
	[SerializeField] private SelectionComponent selection;
	[SerializeField] private WorldToScreenUIManagerComponent worldToScreen;
	[SerializeField] private VisualTreeAsset planetResourcesUITemplate;

	private PlanetHoverResourcesUIElement worldUIElement;

	private void Awake()
	{
		// When selections and hovers are made / done, update the resources container and show / hide.
		selection.OnHoverEnter.AddListener(Selection_OnHoverEnter);
		selection.OnSelectStart.AddListener(Selection_OnSelectStart);
		selection.OnHoverExit.AddListener(Selection_OnHoverExit);
		selection.OnSelectEnd.AddListener(Selection_OnSelectEnd);
	}

	private void Start()
	{
		worldUIElement = new PlanetHoverResourcesUIElement();
		worldToScreen.Add(worldUIElement, planetResourcesUITemplate);

		worldUIElement.Pivot = new Vector2(0.5f, 0.5f);
	}

	private void Selection_OnHoverEnter(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Selection_OnSelectStart(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Selection_OnHoverExit(IInspectableComponent potentialPlanet)
	{
		// If we're exiting a hover, there's no point in checking if the hover is the planet we're currently bound to.
		// Either the last hover was the planet we're bound to, or we're not currently bound to anything.
		worldUIElement.ClearHoveredPlanet();
	}

	private void Selection_OnSelectEnd(IInspectableComponent potentialPlanet)
	{
		// If a selection has ended, there's no point in checking if the selection is the planet we're currently bound to.
		// Either the last selection was the planet we're bound to, or we're not currently bound to anything.
		worldUIElement.ClearHoveredPlanet();
	}

	private void CheckForPlanetAndBind(IInspectableComponent potentialPlanet)
	{
		if (potentialPlanet is PlanetComponent)
		{
			worldUIElement.SetHoveredPlanet(potentialPlanet as PlanetComponent);
		}
	}

	private void Update()
	{
		worldUIElement.UpdatePlanetResources();
	}
}
