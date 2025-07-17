using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using AstralAvarice.Frontend;

namespace AstralAvarice.UI.Tooltips
{
	/// <summary>
	/// Class that controls the tooltips for when we are placing buildings and cables.
	/// </summary>
	public class BuildWarningsTooltipController : ITooltipUIController
	{
		private const string WARNINGS_CONTAINER_ELEMENT_NAME = "WarningsContainer";

		private const string WARNING_ELEMENT_NAME = "Warning";
		private const string WARNING_DESCRIPTION_ELEMENT_NAME = "WarningDescription";

		private const string SECTION_CONTAINER_NAME = "SubSectionContainer";

		private const string WARNING_GOOD_CLASS_NAME = "warningTypeGood";
		private const string WARNING_FATAL_CLASS_NAME = "warningTypeFatal";
		private const string WARNING_ALERT_CLASS_NAME = "warningTypeAlert";

		private VisualElement rootUIElement;
		private VisualElement warningContainerElement;

		private BuildWarningsUIData BuildWarningsUIData => PtUUISettings.GetOrCreateSettings().BuildWarningsUIData;

		public void Bind(VisualElement ui)
		{
			rootUIElement = ui;
			warningContainerElement = ui.Q(WARNINGS_CONTAINER_ELEMENT_NAME);
		}

		public void UnBind()
		{
			rootUIElement = null;
			warningContainerElement = null;
		}

		public void SetBuildWarnings(BuildWarningContext container)
		{
			// TODO: Support setting before binding.
			if (warningContainerElement == null)
				return;

			// TODO: Make less resource intensive. Creating and destroying elements every frame is costly.
			Clear();
			if (container.GetHasChildren())
			{
				rootUIElement.style.display = DisplayStyle.Flex;
				AddBuildWarnings(container);
			}
			else
			{
				rootUIElement.style.display = DisplayStyle.None;
			}
		}

		private void Clear()
		{
			warningContainerElement.Clear();
		}

		private void AddBuildWarnings(BuildWarningContext container)
		{
			VisualTreeAsset warningUITemplate = BuildWarningsUIData.buildWarningUIAsset;
			VisualTreeAsset warningSectionUITemplate = BuildWarningsUIData.buildWarningSectionUIAsset;

			AddBuildWarningElementsToSection(
				container.GetChildren(),
				warningContainerElement,
				warningUITemplate,
				warningSectionUITemplate
			);
		}

		private static void AddBuildWarningElementsToSection(IEnumerable<BuildWarningElement> elements, VisualElement sectionElement, VisualTreeAsset warningUITemplate, VisualTreeAsset sectionUITemplate)
		{
			foreach (BuildWarningElement element in elements)
			{
				if (element is BuildWarning warning)
				{
					VisualElement warningUITemplateInstance = warningUITemplate.Instantiate();
					sectionElement.Add(warningUITemplateInstance);
					SetBuildWarning(warning, warningUITemplateInstance);
				}
				else if (element is BuildWarningSection section)
				{
					VisualElement sectionUITemplateInstance = sectionUITemplate.Instantiate();
					sectionElement.Add(sectionUITemplateInstance);
					AddBuildWarningElementsToSection(
						section.GetChildren(),
						sectionUITemplateInstance.Q(SECTION_CONTAINER_NAME),
						warningUITemplate,
						sectionUITemplate
					);
				}
				else
				{
					Debug.LogWarning($"Unrecognized build warning element {element}. Skipping display in tooltip.");
				}
			}
		}

		private static void SetBuildWarning(BuildWarning warning, VisualElement warningTemplateElement)
		{
			VisualElement warningElement = warningTemplateElement.Q(WARNING_ELEMENT_NAME);
			Label descriptionLabel = warningElement.Q<Label>(WARNING_DESCRIPTION_ELEMENT_NAME);

			ClearWarningTypeClasses(warningElement);
			
			string className = WarningTypeToClassName(warning);

			warningElement.AddToClassList(className);
			descriptionLabel.text = warning.GetMessage();
		}

		private static void ClearWarningTypeClasses(VisualElement container)
		{
			BuildWarning.WarningType[] warningTypes = (BuildWarning.WarningType[])Enum.GetValues(typeof(BuildWarning.WarningType));

			foreach (BuildWarning.WarningType warningType in warningTypes)
			{
				string warningTypeClassName = WarningTypeToClassName(warningType);
				container.RemoveFromClassList(warningTypeClassName);
			}
		}

		private static string WarningTypeToClassName(BuildWarning warning)
		{
			return WarningTypeToClassName(warning.GetWarningType());
		}

		private static string WarningTypeToClassName(BuildWarning.WarningType warningType)
		{
			switch (warningType)
			{
				case BuildWarning.WarningType.GOOD:
					return WARNING_GOOD_CLASS_NAME;
				case BuildWarning.WarningType.ALERT:
					return WARNING_ALERT_CLASS_NAME;
				case BuildWarning.WarningType.FATAL:
					return WARNING_FATAL_CLASS_NAME;
			}

			return null;
		}
	}
}
