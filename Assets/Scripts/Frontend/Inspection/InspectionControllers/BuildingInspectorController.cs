using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BuildingInspectorController : BuildingUIBinding, IInspectorController
{
	public virtual void ConnectInspectorUI(VisualElement inspectorUI)
	{
		Bind(inspectorUI);
	}

	public abstract void DisconnectInspectorUI();
	public abstract void UpdateUI();
}
