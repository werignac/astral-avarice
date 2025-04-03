using System;
using UnityEngine;
using UnityEngine.UIElements;
using AstralAvarice.Utils;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Stops selection when the mouse hovers over any element
	/// in this uiDocument's panel. Allows for some exceptions.
	/// </summary>
    public class PanelSelectionBlocker : SelectionBlocker
    {
		/// <summary>
		/// Names of elements in the panel that don't block selection.
		/// Assumes overriden elements are not getting added / removed from the panel.
		/// </summary>
		[SerializeField] private string[] overrideElementNames;

		/// <summary>
		/// Elements in the panel that don't block selection.
		/// </summary>
		private HashSet<VisualElement> overrideElements;

		/// <summary>
		/// Reference to a UIDocument we use to get a panel.
		/// </summary>
		[SerializeField] UIDocument uiDocument;

		/// <summary>
		/// Panel that we're checking
		/// </summary>
		private IRuntimePanel panel;

		private bool blockSelection = false;

		private void Start()
		{
			panel = uiDocument.runtimePanel;

			FindOverrideElements();
		}

		private void FindOverrideElements()
		{
			overrideElements = new HashSet<VisualElement>();

			for (int i = 0; i < overrideElementNames.Length; i++)
			{
				string elementName = overrideElementNames[i];
				VisualElement element = panel.visualTree.Q(elementName);
				overrideElements.Add(element);
			}
		}

		private void Update()
		{
			bool checkMouseOnPanel = EventSystem.current.IsPointerOverGameObject();

			if (!checkMouseOnPanel)
			{
				blockSelection = false;
				return;
			}

			List<VisualElement> pickedElements = new List<VisualElement>();
			bool pickedAnyElement = panel.PickAll(Mouse.current.GetUIDocumentPosition(), pickedElements) != null;

			if (! pickedAnyElement)
			{
				blockSelection = false;
				return;
			}

			blockSelection = !GetVisualElementListHasOverridingElement(pickedElements);
		}

		private bool GetVisualElementListHasOverridingElement(List<VisualElement> pickedElements)
		{
			HashSet<VisualElement> overrideElementsCopy = new HashSet<VisualElement>(overrideElements);
			overrideElementsCopy.ExceptWith(pickedElements); // Difference.

			return overrideElementsCopy.Count != overrideElements.Count;
		}

		public override bool GetBlockSelection()
		{
			return blockSelection;
		}
	}
}
