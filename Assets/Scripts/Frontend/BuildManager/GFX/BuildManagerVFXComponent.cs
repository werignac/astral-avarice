using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using AstralAvarice.Frontend;

[RequireComponent(typeof(Volume))]
public class BuildManagerVFXComponent : MonoBehaviour
{
	[SerializeField] private BuildManagerComponent buildManager;
	private Volume postProcessingVolume;

	[SerializeField] private Color buildColor;
	[SerializeField] private Color demolishColor;
	[SerializeField] private Color moveColor;

	private void Awake()
	{
		postProcessingVolume = GetComponent<Volume>();
		buildManager.OnStateChanged.AddListener(BuildManager_OnStateChanged);
	}

	private void BuildManager_OnStateChanged(IBuildState oldState, IBuildState newState)
	{
		
		if (postProcessingVolume.profile.TryGet(out Vignette vignette))
		{
			switch (newState.GetStateType())
			{
				case BuildStateType.NONE:
					vignette.active = false;
					break;
				case BuildStateType.BUILDING:
				case BuildStateType.CABLE:
				case BuildStateType.BUILDING_CHAINED:
					vignette.active = true;
					vignette.color.value = buildColor;
					break;
				case BuildStateType.DEMOLISH:
					vignette.active = true;
					vignette.color.value = demolishColor;
					break;
				case BuildStateType.MOVE:
					vignette.active = true;
					vignette.color.value = moveColor;
					break;
			}
		}

	}
}
