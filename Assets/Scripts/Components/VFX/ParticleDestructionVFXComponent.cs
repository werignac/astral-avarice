using UnityEngine;
using System.Collections;

public class ParticleDestructionVFXComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		ParticleSystem particleSystem = GetComponent<ParticleSystem>();
		StartCoroutine(DestructionTimerCoroutine(particleSystem.main.duration + particleSystem.main.startLifetime.constantMax));
    }

    private IEnumerator DestructionTimerCoroutine(float particleDuration)
	{
		yield return new WaitForSeconds(particleDuration);
		Destroy(gameObject);
	}
}
