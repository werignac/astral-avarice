using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SpecialResourcesContainerUIBinding
{
	private VisualElement specialResourcesContainer;

	private Dictionary<ResourceType, SpecialResourceUIBinding> specialResourcesBindings;

	public SpecialResourcesContainerUIBinding(VisualElement specialResourcesContainer)
	{
		this.specialResourcesContainer = specialResourcesContainer;

		specialResourcesBindings = new Dictionary<ResourceType, SpecialResourceUIBinding>
		{
			{ResourceType.Solar, new SpecialResourceUIBinding(specialResourcesContainer.Q("SolarResource")) },
			{ResourceType.Coal, new SpecialResourceUIBinding(specialResourcesContainer.Q("CoalResource")) },
			{ResourceType.Wind, new SpecialResourceUIBinding(specialResourcesContainer.Q("WindResource")) },
			{ResourceType.Thermal, new SpecialResourceUIBinding(specialResourcesContainer.Q("GeothermalResource")) },
		};

		InitializeIcons();
	}

	private void InitializeIcons()
	{
		foreach(ResourceType resourceType in specialResourcesBindings.Keys)
		{
			var binding = specialResourcesBindings[resourceType];
			binding.SetResourceIcon(resourceType);
		}
	}

	public void Show()
	{
		specialResourcesContainer.style.display = DisplayStyle.Flex;
	}

	public void Hide()
	{
		specialResourcesContainer.style.display = DisplayStyle.None;
	}

	/// <summary>
	/// Shows all the passed resources. Hides all others.
	/// </summary>
	/// <param name="resourcesToShow">An array of all the resources to show.</param>
	/// <returns>A resource type that will be shown and the binding associated with that resource. In the order of resourcestoShow. This is used to set quantities and totals.</returns>
	public IEnumerable<System.Tuple<ResourceType, IRestrictedSpecialResourceUIBinding>> ShowResources(ResourceType[] resourcesToShow)
	{
		// Show all the types that should be shown.
		foreach(ResourceType typeToShow in resourcesToShow)
		{
			if (! specialResourcesBindings.ContainsKey(typeToShow))
			{
				Debug.LogWarning($"Tried to show special resource {typeToShow} in container, but this type does not exist in the container. Skipping.");
				continue;
			}

			var toShow = specialResourcesBindings[typeToShow];
			toShow.Show();

			yield return new System.Tuple<ResourceType, IRestrictedSpecialResourceUIBinding>(typeToShow, toShow);
		}

		// Hide all the types that should be hidden.
		foreach(ResourceType potentialTypeToHide in specialResourcesBindings.Keys)
		{
			if (System.Array.IndexOf(resourcesToShow, potentialTypeToHide) >= 0)
				continue;

			specialResourcesBindings[potentialTypeToHide].Hide();
		}
	}
}
