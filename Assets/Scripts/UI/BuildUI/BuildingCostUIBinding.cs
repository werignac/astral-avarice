using UnityEngine;
using UnityEngine.UIElements;

public class BuildingCostUIBinding : BuildingValueUIBinding
{
	private const string AFFORDABLE_CLASS = "affordableContainer";
	private const string UNAFFORDABLE_CLASS = "unaffordableContainer";
	
	public BuildingCostUIBinding(
		VisualElement container,
		bool valueContainerPassed = true,
		string valueName = null
		) : base(container, valueContainerPassed, valueName)
	{
		
	}

	public void SetAffordability(bool affordable)
	{
		if (affordable)
			SetAffordable();
		else
			SetUnaffordable();
	}

	public void SetAffordable()
	{
		if (Container.ClassListContains(UNAFFORDABLE_CLASS))
			Container.RemoveFromClassList(UNAFFORDABLE_CLASS);
		if (!Container.ClassListContains(AFFORDABLE_CLASS))
			Container.AddToClassList(AFFORDABLE_CLASS);
	}

	public void SetUnaffordable()
	{
		if (Container.ClassListContains(AFFORDABLE_CLASS))
			Container.RemoveFromClassList(AFFORDABLE_CLASS);
		if (!Container.ClassListContains(UNAFFORDABLE_CLASS))
			Container.AddToClassList(UNAFFORDABLE_CLASS);
	}


}
