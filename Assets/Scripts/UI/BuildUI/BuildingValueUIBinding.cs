using UnityEngine;
using UnityEngine.UIElements;

public class BuildingValueUIBinding
{
	// Enum that determines whether a value ui element shows a divider or not. Used as an argument.
	public enum DividerStyle { WITHOUT_DIVIDER, WITH_DIVIDER };

	// The container that contains everything for the value ui.
	protected VisualElement Container { get; set; } = null;
	// The label to use to show the value of the ui.
	protected Label Value { get; set; } = null;
	// An optional divider that separates the value from a total denominator.
	protected VisualElement Divider { get; set; } = null;
	// An optional total that represents a maximum quanitity.
	protected Label Total { get; set; } = null;

	// Getters
	public bool HasDivider { get => Divider != null; }


	/// <summary>
	/// Constructs a binding for a ui value container. A ui value is a number, sometimes with a maximum or divider.
	/// Examples include:
	/// - Income
	/// - Power Consumed
	/// - Power Produced
	/// 
	/// Value containers are always named <VALUE_NAME>Container.
	/// In this container there is always a label <VALUE_NAME>Amount.
	/// Sometimes, there's also a <VALUE_NAME>Divider and <VALUE_NAME>Total.
	/// </summary>
	/// <param name="container">Either a container of just the value or a larger container that includes a container for a specific value.</param>
	/// <param name="valueContainerPassed">Specifies whether "container" is a container for just a value, or a broader container for which we must find a value container.</param>
	/// <param name="valueName">The name of the value (without "Container", "Amount", etc.). Used to find children of the value container. Optional when valueContainerPassed is false.</param>
	public BuildingValueUIBinding(
		VisualElement container,
		bool valueContainerPassed = true,
		string valueName = null
		)
	{
		
		if (valueContainerPassed)
			// Passed a reference to a container of just a value.
			Container = container;
		else
		{
			// Passed a larger container that contains the value we're looking for.
			if (valueName == null)
				throw new System.ArgumentException($"Passed non-value container {container.name} to a BuildingValueUIBinding without providing a valueName to find a value container.");

			Container = container.Q($"{valueName}Container");
		}
		
		if (valueName == null)
		{
			// If there's no valueName by this point, extract it from our Container.
			int truncatePoint = Container.name.IndexOf("Container");
			valueName = Container.name.Substring(0, truncatePoint);
		}

		Value = Container.Q<Label>($"{valueName}Amount");
		// Optional - though if one exists, so must the other.
		Divider = Container.Q($"{valueName}Divider");
		Total = Container.Q<Label>($"{valueName}Total");

		if (Divider != null)
			Debug.Assert(Total != null, $"Value divider mismatch. Found divider {Divider}, but missing {valueName}Total.");
		else
			Debug.Assert(Total == null, $"Value divider mismatch. Found total {Total} but missing {valueName}Divider.");
	}

	// Set the value in the UI.
	public void SetValue(string value)
	{
		Value.text = value;
	}

	// Set whether to show a divider + total.
	public void SetDividerMode(DividerStyle style)
	{
		DisplayStyle displayStyle;

		switch(style)
		{
			case DividerStyle.WITH_DIVIDER:
				if (!HasDivider)
					throw new System.Exception("Tried to show the divider for a value without a divider.");
				displayStyle = DisplayStyle.Flex;
				break;

			default:
				if (!HasDivider)
					return;
				displayStyle = DisplayStyle.None;
				break;
		}

		Divider.style.display = displayStyle;
		Total.style.display = displayStyle;
	}

	// Set the total (if there is one).
	public void SetTotal(string total)
	{
		if (!HasDivider)
			throw new System.Exception("Tried to set the total for a value without a divider.");
		Total.text = total;
	}

	// Show the whole value ui.
	public void Show()
	{
		Container.style.display = DisplayStyle.Flex;
	}

	// Hide the whole value ui.
	public void Hide()
	{
		Container.style.display = DisplayStyle.None;
	}
}
