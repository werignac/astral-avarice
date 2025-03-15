using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif


public class RandomizeMaterialSeed : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	
	private void Start()
	{
#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name == "LevelBuilder")
			return;
#endif

		spriteRenderer.material.SetFloat(Shader.PropertyToID("_Seed"), Random.Range(1, 1000));
	}
}
