using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class InspectorUIComponent : MonoBehaviour
{
	private UIDocument uiDocument;
	
	private Button collapseButton;
	private VisualElement collapsableInspectorContent;



	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
	}

	private void Start()
	{
		collapseButton = uiDocument.rootVisualElement.Q<Button>("CollapseButton");
		collapsableInspectorContent = uiDocument.rootVisualElement.Q("CollapsableContainer");


		collapseButton.RegisterCallback<ClickEvent>(CollapseButton_OnClick);
	}

	private void CollapseButton_OnClick(ClickEvent evt)
	{
		// TODO: Change collapse button graphic.

		switch (collapsableInspectorContent.style.display.value)
		{
			case DisplayStyle.None:
				collapsableInspectorContent.style.display = DisplayStyle.Flex;
				break;
			case DisplayStyle.Flex:
				collapsableInspectorContent.style.display = DisplayStyle.None;
				break;
		}
	}
}
