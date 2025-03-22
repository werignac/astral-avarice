using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Overrides the default UI blocking behaviour when
	/// the user happens to be hovering over the inspector and
	/// an inspectable game object.
	/// </summary>
    public class InspectorSelectionUIBlockingOverride : SelectionUIBlockingOverride
    {
		private const string INSPECTOR_ELEMENT_NAME = "InspectorTemplate";

		[SerializeField] UIDocument uiDocument;

		private VisualElement inspectorElement;


		private bool overrideBlocking = false;

		private void Start()
		{
			inspectorElement = uiDocument.rootVisualElement.Q(INSPECTOR_ELEMENT_NAME);
			inspectorElement.RegisterCallback<MouseEnterEvent>(Inspector_OnMouseEnter);
			inspectorElement.RegisterCallback<MouseLeaveEvent>(Inspector_OnMouseLeave);
		}

		private void Inspector_OnMouseEnter(MouseEnterEvent evt)
		{
			overrideBlocking = true;
		}

		private void Inspector_OnMouseLeave(MouseLeaveEvent evt)
		{
			overrideBlocking = false;
		}

		public override bool GetIgnoreUI()
		{
			return overrideBlocking;
		}
	}
}
