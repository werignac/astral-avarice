using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

// IMPORTANT BUG: USING SETACTIVE(false) ON THE GAMEOBJECT WILL MAKE IT SO THAT YOU CANNOT UPDATE UI DOCUMENT ELEMENTS!!!
// DON'T EVER DO THIS FOR ANY UIDOCUMENT. USE style.display = DisplayStyles.None!!!
public class PlanetHoverResourcesComponent : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private InspectorUIComponent inspector;
	private RawImage screenSpaceRenderer;
	private SpecialResourcesContainerUIBinding specialResourcesContainer;

	private PlanetComponent currentPlanet = null;

	private bool IsShowingPlanetResources { get => currentPlanet != null; }

	private float baseCameraSize;

	private void Awake()
	{
		screenSpaceRenderer = GetComponent<RawImage>();
		UIDocument specialResourcesUIDocument = GetComponent<UIDocument>();
		specialResourcesContainer = new SpecialResourcesContainerUIBinding(specialResourcesUIDocument.rootVisualElement);

		// When selections and hovers are made / done, update the resources container and show / hide.
		inspector.OnHoverEnter.AddListener(Inspector_OnHoverEnter);
		inspector.OnSelectStart.AddListener(Inspector_OnSelectStart);
		inspector.OnHoverExit.AddListener(Inspector_OnHoverExit);
		inspector.OnSelectEnd.AddListener(Inspector_OnSelectEnd);

		baseCameraSize = mainCamera.orthographicSize;
	}

	private void Start()
	{
		Hide();
	}

	private void Inspector_OnHoverEnter(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Inspector_OnSelectStart(IInspectableComponent potentialPlanet)
	{
		CheckForPlanetAndBind(potentialPlanet);
	}

	private void Inspector_OnHoverExit(IInspectableComponent potentialPlanet)
	{
		// If we're exiting a hover, there's no point in checking if the hover is the planet we're currently bound to.
		// Either the last hover was the planet we're bound to, or we're not currently bound to anything.
		if (IsShowingPlanetResources)
			HideAndUnbindFromPlanet();
	}

	private void Inspector_OnSelectEnd(IInspectableComponent potentialPlanet)
	{
		// If a selection has ended, there's no point in checking if the selection is the planet we're currently bound to.
		// Either the last selection was the planet we're bound to, or we're not currently bound to anything.
		if (IsShowingPlanetResources)
			HideAndUnbindFromPlanet();
	}

	private void CheckForPlanetAndBind(IInspectableComponent potentialPlanet)
	{
		if (potentialPlanet is PlanetComponent)
		{
			BindToPlanet(potentialPlanet as PlanetComponent);
		}
	}

	private void BindToPlanet(PlanetComponent toBind)
	{
		currentPlanet = toBind;
		currentPlanet.OnPlanetDemolished.AddListener(BoundPlanet_OnDemolish);
		Show();
		UpdateAll();
	}

	private void BoundPlanet_OnDemolish(PlanetComponent arg0)
	{
		HideAndUnbindFromPlanet();
	}

	private void HideAndUnbindFromPlanet()
	{
		Hide();
		if (currentPlanet != null)
			currentPlanet.OnPlanetDemolished.RemoveListener(BoundPlanet_OnDemolish);
		currentPlanet = null;
	}

	private void Show()
	{
		screenSpaceRenderer.enabled = true;
	}

	private void Hide()
	{
		screenSpaceRenderer.enabled = false;
	}

	private void LateUpdate()
	{
		if (!IsShowingPlanetResources)
			return;

		UpdateAll();
	}

	private void UpdateAll()
	{
		UpdatePosition();
		UpdateResourcesContainer();
		UpdateRendererSize();
	}

	private void UpdatePosition()
	{
		transform.position = currentPlanet.transform.position;
	}

	private void UpdateResourcesContainer()
	{
		foreach (var pair in specialResourcesContainer.ShowResources(GetAvailableResourceTypes()))
		{
			int resourceQuantity = currentPlanet.GetAvailableResourceCount(pair.Item1);
			int resourceTotal = currentPlanet.GetResourceCount(pair.Item1);

			pair.Item2.SetQuantity(resourceQuantity);
			pair.Item2.SetTotal(resourceTotal);
		}
	}

	private ResourceType[] GetAvailableResourceTypes()
	{
		List<ResourceType> resourceTypes = new List<ResourceType>();

		for (int i = 0; i < (int)ResourceType.Resource_Count; ++i)
		{
			if (currentPlanet.GetResourceCount((ResourceType)i) > 0)
				resourceTypes.Add((ResourceType)i);
		}

		return resourceTypes.ToArray();
	}

	private void UpdateRendererSize()
	{
		screenSpaceRenderer.transform.localScale = Vector3.one * mainCamera.orthographicSize / baseCameraSize;
	}
}
