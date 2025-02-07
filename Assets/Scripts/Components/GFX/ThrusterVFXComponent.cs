using System;
using UnityEngine;

public class ThrusterVFXComponent : MonoBehaviour
{
	[SerializeField] private ParticleSystem[] particleSystems;
	[SerializeField] private BuildingOnOffGFXComponent onOffGFXComponent;


	private void Awake()
	{
		onOffGFXComponent.OnPoweredStateChanged.AddListener(OnOff_OnPoweredStateChanged);
	}

	private void OnOff_OnPoweredStateChanged(bool isPowered)
	{
		foreach(ParticleSystem particleSystem in particleSystems)
		{
			if (isPowered)
				particleSystem.Play();
			else
				particleSystem.Stop();
		}
	}
}
