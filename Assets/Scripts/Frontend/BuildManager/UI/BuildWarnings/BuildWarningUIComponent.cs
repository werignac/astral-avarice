using System;
using UnityEngine;
using AstralAvarice.UI.Tooltips;

public class BuildWarningUIComponent : MonoBehaviour
{
	[SerializeField] private TooltipComponent tooltipComponent;
	[SerializeField] private TooltipLayerFactory_SO tooltipLayerFactory;

	private TooltipLayer currentTooltipLayer = null;
	private BuildWarningsTooltipController tooltipController = null;

	private void Start()
	{
		BuildManagerComponent.Instance.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void BuildManager_OnStateChanged(BuildState oldState, BuildState newState)
	{
		switch(newState.GetStateType())
		{
			case BuildStateType.BUILDING:
			case BuildStateType.BUILDING_CHAINED:
			case BuildStateType.CABLE:
			case BuildStateType.MOVE:
				InstantiateTooltipLayer();
				BuildManagerComponent.Instance.OnBuildWarningUpdate.AddListener(BuildManager_OnBuildWarningUpdate);
				break;
			default:
				BuildManagerComponent.Instance.OnBuildWarningUpdate.RemoveListener(BuildManager_OnBuildWarningUpdate);
				DestroyTooltipLayer();
				break;
		}
	}

	private void InstantiateTooltipLayer()
	{
		if (currentTooltipLayer == null)
		{
			tooltipController = new BuildWarningsTooltipController();
			currentTooltipLayer = tooltipLayerFactory.MakeLayer(tooltipController);
			tooltipComponent.Add(currentTooltipLayer);
		}
	}

	private void DestroyTooltipLayer()
	{
		if (currentTooltipLayer != null)
		{
			tooltipComponent.Remove(currentTooltipLayer);
			tooltipController = null;
			currentTooltipLayer = null;
		}
	}

	private void BuildManager_OnBuildWarningUpdate(BuildWarningContainer warnings)
	{
		tooltipController.SetBuildWarnings(warnings);
	}
}
