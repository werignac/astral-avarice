using UnityEngine;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Scriptable object that just houses an event.
	/// Useful for when we want many objects to listen to a single
	/// in-game event (e.g. game controller wants to compute planet velocities).
	/// </summary>
    public class ScriptableEvent<T> : ScriptableObject
    {
		private UnityEvent<T> _event = new UnityEvent<T>();

		public void AddListener(UnityAction<T> call)
		{
			_event.AddListener(call);
		}

		public void RemoveListener(UnityAction<T> call)
		{
			_event.RemoveListener(call);
		}

		public void Invoke(T arg)
		{
			_event.Invoke(arg);
		}
    }

	public class ScriptableEvent : ScriptableObject
	{
		private UnityEvent _event = new UnityEvent();

		public void AddListener(UnityAction call)
		{
			_event.AddListener(call);
		}

		public void RemoveListener(UnityAction call)
		{
			_event.RemoveListener(call);
		}

		public void Invoke()
		{
			_event.Invoke();
		}
	}
}
