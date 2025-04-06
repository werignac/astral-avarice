using UnityEngine;
using UnityEngine.Events;


namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Component that checks for whether the transform of the cable component connection
	/// point has changed this frame. If so, fires the position changed event.
	/// 
	/// Used by cables to reduce the number of update calls.
	/// 
	/// Doing the update on buildings as opposed to cables reduces the number of
	/// LateUpdate calls.
	/// </summary>
    public class CableConnectionPointMonitorComponent : MonoBehaviour
    {
		[HideInInspector] public UnityEvent onConnectionPositionHasChanged = new UnityEvent();

		[SerializeField] private Transform cableConnectionTransform;

        // Update is called once per frame
        void Update()
        {
			if (cableConnectionTransform.hasChanged)
				onConnectionPositionHasChanged.Invoke();
			cableConnectionTransform.hasChanged = false;
        }
    }
}
