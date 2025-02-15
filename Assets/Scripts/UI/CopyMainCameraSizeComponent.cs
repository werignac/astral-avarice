using UnityEngine;

public class CopyMainCameraSizeComponent : MonoBehaviour
{
	protected Camera mainCamera;
	protected Camera selfCamera;

	protected virtual void Awake()
	{
		selfCamera = GetComponent<Camera>();
		mainCamera = Camera.main;
	}

	protected virtual void LateUpdate()
	{
		selfCamera.orthographicSize = mainCamera.orthographicSize;
		selfCamera.aspect = mainCamera.aspect;
	}
}
