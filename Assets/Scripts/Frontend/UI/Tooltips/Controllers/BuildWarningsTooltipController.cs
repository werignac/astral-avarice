using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

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

		private const string WARNING_GOOD_CLASS_NAME = "warningTypeGood";
		private const string WARNING_FATAL_CLASS_NAME = "warningTypeFatal";
		private const string WARNING_ALERT_CLASS_NAME = "warningTypeAlter";

		private VisualElement warningContainerElement;
		
		private List<VisualElement> warningTemplateElements = new List<VisualElement>();

		public void Bind(VisualElement ui)
		{
			warningContainerElement = ui.Q(WARNINGS_CONTAINER_ELEMENT_NAME);

			foreach(VisualElement child in warningContainerElement.Children())
			{
				warningTemplateElements.Add(child);
			}
		}

		public void UnBind()
		{
			warningContainerElement = null;
			warningTemplateElements.Clear();
		}

		public void SetBuildWarnings(BuildWarningContainer container)
		{
			int i = 0;
			IEnumerator<BuildWarning> iterator = container.GetAllWarnings();
			BuildWarning warning = default;

			for (; iterator.MoveNext();)
			{
				warning = iterator.Current;

				if (i >= warningTemplateElements.Count)
				{
					Debug.LogWarning($"Detected {warningTemplateElements.Count} elements for displaying warnings, but more were needed.");
					break;
				}

				VisualElement warningTemplateElement = warningTemplateElements[i];
				warningTemplateElement.style.display = DisplayStyle.Flex;
				SetBuildWarning(warning, warningTemplateElement);
				i++;
			}

			// TODO: Remove
			Debug.Log($"Found {i} warnings.");

			for (; i < warningTemplateElements.Count; i++)
			{
				warningTemplateElements[i].style.display = DisplayStyle.None;
			}
		}

		public void SetBuildWarning(BuildWarning warning, VisualElement warningTemplateElement)
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
