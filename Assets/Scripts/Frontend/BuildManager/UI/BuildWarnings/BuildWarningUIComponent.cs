using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BuildWarningUIComponent : MonoBehaviour
{
	private UIDocument uiDocument;
	private VisualElement buildingWarningContainer;
	private VisualElement cableWarningContainer;
	private VisualElement divider;

	[SerializeField] VisualTreeAsset nonFatalWarningTemplate;
	[SerializeField] VisualTreeAsset fatalWarningTemplate;

	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
	}

	private void Start()
	{
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);

		buildingWarningContainer = uiDocument.rootVisualElement.Q("BuildingWarningContainer");
		cableWarningContainer = uiDocument.rootVisualElement.Q("CableWarningContainer");
		divider = uiDocument.rootVisualElement.Q("WarningDivider");
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		switch(newState.GetStateType())
		{
			case BuildStateType.BUILDING:
			case BuildStateType.BUILDING_CHAINED:
			case BuildStateType.CABLE:
				uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
				BuildManagerComponent.Instance.OnBuildWarningUpdate.AddListener(BuildManager_OnBuildWarningUpdate);
				break;
			default:
				uiDocument.rootVisualElement.style.display = DisplayStyle.None;
				BuildManagerComponent.Instance.OnBuildWarningUpdate.RemoveListener(BuildManager_OnBuildWarningUpdate);
				break;
		}
	}

	private void BuildManager_OnBuildWarningUpdate(BuildWarningContainer warnings)
	{
		// Clear previous warnings.
		while (buildingWarningContainer.childCount > 0)
			buildingWarningContainer.RemoveAt(0);
		while (cableWarningContainer.childCount > 0)
			cableWarningContainer.RemoveAt(0);

		bool hadBuildingWarning = false;

		foreach (BuildWarning warning in warnings.GetBuildingWarnings())
		{
			hadBuildingWarning = true;
			buildingWarningContainer.Add(WarningToUI(warning));
		}

		bool hadCableWarning = false;
		foreach (BuildWarning warning in warnings.GetCableWarnings())
		{
			hadCableWarning = true;
			cableWarningContainer.Add(WarningToUI(warning));
		}

		divider.style.display = hadBuildingWarning && hadCableWarning ? DisplayStyle.Flex : DisplayStyle.None;
	}

	private TemplateContainer WarningToUI(BuildWarning warning)
	{
		TemplateContainer uiElement;

		if (warning.GetIsFatal())
			uiElement = fatalWarningTemplate.Instantiate();
		else
			uiElement = nonFatalWarningTemplate.Instantiate();

		uiElement.Q<Label>("Message").text = warning.GetMessage();

		return uiElement;
	}
}
