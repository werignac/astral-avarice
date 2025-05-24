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

		private VisualElement warningContainerElement;

		private BuildWarningsUIData BuildWarningsUIData => PtUUISettings.GetOrCreateSettings().BuildWarningsUIData;

		public void Bind(VisualElement ui)
		{
			warningContainerElement = ui.Q(WARNINGS_CONTAINER_ELEMENT_NAME);
		}

		public void UnBind()
		{
			warningContainerElement = null;
		}

		public void SetBuildWarnings(BuildWarningContainer container)
		{
			// TODO: Support setting before binding.
			if (warningContainerElement == null)
				return;

			// TODO: Make less resource intensive. Creating and destroying elements every frame is costly.
			Clear();
			AddBuildWarnings(container);
		}

		private void Clear()
		{
			warningContainerElement.Clear();
		}

		private void AddBuildWarnings(BuildWarningContainer container)
		{
			VisualTreeAsset warningUITemplate = BuildWarningsUIData.buildWarningUIAsset;
			VisualTreeAsset warningSectionUITemplate = BuildWarningsUIData.buildWarningSectionUIAsset;

			IEnumerable<BuildWarning> buildingWarnings = container.GetBuildingWarnings();

			AddBuildWarningsToSection(buildingWarnings, warningContainerElement, warningUITemplate);

			IEnumerable<BuildWarning> cableWarnings = container.GetCableWarnings();

			VisualElement cableSectionUI = warningSectionUITemplate.Instantiate();

			VisualElement cableSectionContainer = cableSectionUI.Q(SECTION_CONTAINER_NAME);

			AddBuildWarningsToSection(cableWarnings, cableSectionContainer, warningUITemplate);

			if (cableSectionContainer.childCount > 0)
				warningContainerElement.Add(cableSectionUI);
		}

		private static void AddBuildWarningsToSection(IEnumerable<BuildWarning> warnings, VisualElement sectionElement, VisualTreeAsset warningUITemplate)
		{
			foreach (BuildWarning warning in warnings)
			{
				VisualElement warningUITemplateInstance = warningUITemplate.Instantiate();
				sectionElement.Add(warningUITemplateInstance);
				SetBuildWarning(warning, warningUITemplateInstance);
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
