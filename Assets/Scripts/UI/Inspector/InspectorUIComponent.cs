using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using werignac.Utils;

[RequireComponent(typeof(UIDocument))]
public class InspectorUIComponent : MonoBehaviour
{
	private enum InspectableSource { DEFAULT, HOVER, SELECT, BUILD_MODE }

	private UIDocument uiDocument;
	
	private Button collapseButton;
	private VisualElement collapsableInspectorContent;
	private VisualElement inspectorUIContainer;

	[SerializeField] private VisualTreeAsset defaultInspectorUI;
	[SerializeField] private GameController gameController;
	[SerializeField] private SelectionCursorComponent selectionCursor;

	private IInspectable currentInspectable;
	private InspectableSource currentInspectableSource;
	private IInspectorController currentController;
	
	private void Awake()
	{
		uiDocument = GetComponent<UIDocument>();
		gameController.OnLevelLoad.AddListener(GameManager_OnLevelLoad);
	}

	private void GameManager_OnLevelLoad()
	{
		collapseButton = uiDocument.rootVisualElement.Q<Button>("CollapseButton");
		collapsableInspectorContent = uiDocument.rootVisualElement.Q("CollapsableContainer");
		inspectorUIContainer = uiDocument.rootVisualElement.Q("ScrolledContent");

		SetDefaultInspectorUI();
	}

	private void Start()
	{
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

	private void SetDefaultInspectorUI()
	{
		var defaultInspectorUIInstance = defaultInspectorUI.Instantiate();
		inspectorUIContainer.Add(defaultInspectorUIInstance);
		
		Label missionName = defaultInspectorUIInstance.Q<Label>("MissionName");
		missionName.text += Data.selectedMission.missionName;

		Label missionDescription = defaultInspectorUIInstance.Q<Label>("MissionDescription");
		missionDescription.text = Regex.Replace(missionDescription.text, @"\$\d*", "$" + Data.selectedMission.cashGoal.ToString("0.00"));
		missionDescription.text = Regex.Replace(missionDescription.text, @"SECONDS", Data.selectedMission.timeLimit.ToString());
	}

	private void Update()
	{
		switch (currentInspectableSource)
		{
			case InspectableSource.DEFAULT:
			case InspectableSource.HOVER:
				IInspectable hovering = GetHoveringInspectable();
				InspectableSource source =  hovering != null ? InspectableSource.HOVER : InspectableSource.DEFAULT;
				SetInspecting(hovering, source);
				break;
		}

		if (currentController != null)
		{
			currentController.UpdateUI();
		}
	}

	private IInspectable GetHoveringInspectable()
	{
		List<Collider2D> allHoveringColliders = new List<Collider2D>(selectionCursor.GetHovering());
		Collider2D foundInspectableCollider = allHoveringColliders.Find((Collider2D collider) => { return collider.TryGetComponentInParent<IInspectable>(out IInspectable _); });

		IInspectable foundInspectable = foundInspectableCollider == null ? null : foundInspectableCollider.GetComponentInParent<IInspectable>();
		return foundInspectable;
	}

	private void SetInspecting(IInspectable inspectable, InspectableSource source)
	{
		switch(currentInspectableSource)
		{
			case InspectableSource.DEFAULT:
			case InspectableSource.BUILD_MODE:
				break;
			case InspectableSource.HOVER:
				currentInspectable.OnHoverExit();
				break;
			case InspectableSource.SELECT:
				currentInspectable.OnSelectEnd();
				break;
		}

		// Instantiate a new inspector if the new inspectable is different from the old one.
		if (currentInspectable != inspectable)
		{
			// Remove old inspectable.
			if (currentController != null)
				currentController.DisconnectInspectorUI();
			inspectorUIContainer.RemoveAt(0);

			IInspectorController newInspectorController = null;
			if (inspectable != null)
			{
				// Create new inspector UI and controller.
				var inspectorAsset = inspectable.GetInspectorElement(out newInspectorController);
				var inspectorInstance = inspectorAsset.Instantiate();
				newInspectorController.ConnectInspectorUI(inspectorInstance);

				inspectorUIContainer.Add(inspectorInstance);
			}
			else
			{
				SetDefaultInspectorUI();
			}
			currentInspectable = inspectable;
			currentController = newInspectorController;
		}

		currentInspectableSource = source;

		switch(source)
		{
			case InspectableSource.DEFAULT:
			case InspectableSource.BUILD_MODE:
				break;
			case InspectableSource.HOVER:
				inspectable.OnHoverEnter();
				break;
			case InspectableSource.SELECT:
				inspectable.OnSelectStart();
				break;
		}

	}
}
