using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Interface that defines the quantity and total setters for a SpecialResourceUIBinding.
/// Useful in the SpecialResourcesContainerUIBinding which controls the icon of
/// special resource UI elements and doesn't want others to mess with that.
/// </summary>
public interface IRestrictedSpecialResourceUIBinding
{
	void SetQuantity(int quantity);
	void SetTotal(int total);
}

public class SpecialResourceUIBinding : IRestrictedSpecialResourceUIBinding
{
	private VisualElement resourceElement;
	private VisualElement resourceIcon;
	private Label quantityValue;
	private Label quantityTotal;

	public SpecialResourceUIBinding(VisualElement resourceElement)
	{
		this.resourceElement = resourceElement.Q("SpecialResourceItem");

		resourceIcon = resourceElement.Q("ResourceIcon");
		quantityValue = resourceElement.Q<Label>("QuantityValue");
		quantityTotal = resourceElement.Q<Label>("QuantityTotal");
	}

	/// <summary>
	/// Show the entire special resource UI element.
	/// </summary>
	public void Show()
	{
		resourceElement.style.display = DisplayStyle.Flex;
	}

	/// <summary>
	/// Hide the entire special resource UI element.
	/// </summary>
	public void Hide()
	{
		resourceElement.style.display = DisplayStyle.None;
	}

	/// <summary>
	/// Sets the icon of the special resource UI element to match the passed resource type.
	/// </summary>
	public void SetResourceIcon(ResourceType resourceType)
	{
		var resourceData = PtUUISettings.GetOrCreateSettings().GetSpecialResourceUIData(resourceType);
		SetResourceIcon(resourceData.Icon);
	}

	/// <summary>
	/// Sets the icon of the special resource UI element to be the passed sprite.
	/// </summary>
	private void SetResourceIcon(Sprite icon)
	{
		resourceIcon.style.backgroundImage = new StyleBackground(icon);
	}

	/// <summary>
	/// Sets the quantity of the special resouce UI element to match the passed int.
	/// This is the number that appears before the "/" in the UI element.
	/// </summary>
	public void SetQuantity(int quantity)
	{
		quantityValue.text = quantity.ToString();
	}

	/// <summary>
	/// Sets the total of the special resouce UI element to match the passed int.
	/// This is the number that appears after the "/" in the UI element.
	/// </summary>
	public void SetTotal(int total)
	{
		quantityTotal.text = total.ToString();
	}


	public void SetAll(ResourceType resourceType, int quanitity, int total)
	{
		SetResourceIcon(resourceType);
		SetQuantity(quanitity);
		SetTotal(total);
	}
}
