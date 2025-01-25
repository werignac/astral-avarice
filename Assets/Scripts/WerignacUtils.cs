/// Author: William Erignac
/// Version 12-05-2024
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

namespace werignac.Utils
{
	/// <summary>
	/// A small collection of general-purpose utility functions that mostly
	/// wrap existing Unity functionalities.
	/// </summary>
	public static class WerignacUtils
	{
		#region TryGetComponentInParent
		/// <summary>
		/// Wraps GetComponetInParent to return a boolean to see whether the component was found.
		/// Outputs the component via outComponent.
		/// </summary>
		/// <typeparam name="T">The type of component to look for.</typeparam>
		/// <param name="inComponent">The component whose parents will be searched.</param>
		/// <param name="outComponent">The found component.</param>
		/// <returns>Whether a component of type T was found.</returns>
		public static bool TryGetComponentInParent<T>(this Component inComponent, out T outComponent)
		{
			outComponent = inComponent.GetComponentInParent<T>();
			return outComponent != null;
		}

		/// <summary>
		/// Wraps GetComponentInParent to return a boolean to see whether the component was found.
		/// Outputs the component via outComponent.
		/// </summary>
		/// <typeparam name="T">The type of component ot look for.</typeparam>
		/// <param name="gameObject">The gameObject whose parents will be searched.</param>
		/// <param name="component">The found component.</param>
		/// <returns>Whether a component of type T was found.</returns>
		public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component)
		{
			component = gameObject.GetComponentInParent<T>();
			return component != null;
		}
		#endregion

		#region TryGetComponentInAll

		/// <summary>
		/// Try to get a component from all the GameObjects in the active scene.
		/// </summary>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <param name="outComponent">The component that was found. Default of T otherwise.</param>
		/// <returns>Whether the component was found.</returns>
		public static bool TryGetComponentInActiveScene<T>(out T outComponent)
		{
			return TryGetComponentInScene<T>(SceneManager.GetActiveScene(), out outComponent);
		}

		/// <summary>
		/// Get all components of a given type from all the GameObjects in the active scene.
		/// </summary>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <returns>The found components.</returns>
		public static List<T> GetComponentsInActiveScene<T>()
		{
			return GetComponentsInScene<T>(SceneManager.GetActiveScene());
		}

		/// <summary>
		/// Try to get a component from all the GameObjects in a specific scene.
		/// </summary>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <param name="scene">The scene to search for the component in.</param>
		/// <param name="outComponent">The component that was found. Default of T otherwise.</param>
		/// <returns>Whether the component was found.</returns>
		public static bool TryGetComponentInScene<T>(Scene scene, out T outComponent)
		{
			outComponent = default(T);
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				outComponent = root.GetComponentInChildren<T>();
				if (outComponent != null)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Get all components of a given type from all the GameObjects in the passed scene.
		/// </summary>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <returns>The components found.</returns>
		public static List<T> GetComponentsInScene<T>(Scene scene)
		{
			List<T> outComponents = new List<T>();
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				outComponents.AddRange(root.GetComponentsInChildren<T>());
			}
			return outComponents;
		}

		#endregion

		#region BroadcastToAll
		/// <summary>
		/// Calls Broadcast on all the root game objects of this scene with the specified parameters.
		/// Doesn't require a receiver.
		/// </summary>
		/// <param name="methodName">The function to call.</param>
		/// <param name="parameter">The parameters to pass into the function.</param>
		public static void BroadcastToAll(string methodName, object parameter = null)
		{
			foreach (GameObject root in GetActiveSceneRoots())
				root.BroadcastMessage(methodName, parameter, SendMessageOptions.DontRequireReceiver);
		}
		#endregion

		#region PercolateUp

		/// <summary>
		/// An object that is used to pass data upwards from a Percolate() call.
		/// Type T is the type of data contained in this object.
		/// Use GetData() to get the data.
		/// Use Halt() to stop the Percolation from continuing upwards.
		/// </summary>
		/// <typeparam name="T">The type of data stored in this object.</typeparam>
		public class Percolation<T>
		{
			private bool halt = false;
			private T data;

			public Percolation(T _data)
			{
				data = _data;
			}

			public T GetData()
			{
				return data;
			}

			public void Halt()
			{
				halt = true;
			}

			public bool GetHalt()
			{
				return halt;
			}
		}

		/// <summary>
		/// Percolates a message up through this GameObject's parents iteravely.
		/// The message is sent to parents by passing it to a string-specified function (recieverFunction)
		/// using Unity's SendMessage. Does not require a receiver.
		/// Starts by calling receiverFunction on obj.
		/// 
		/// Percolation can be halted at any time.
		/// </summary>
		/// <typeparam name="T">The type of data to store in the percolation object.</typeparam>
		/// <param name="obj">The object to percolate data up from.</param>
		/// <param name="recieverFunction">The string name of the funciton to call.</param>
		/// <param name="data">The data to store in the percolation object.</param>
		public static void Percolate<T>(this GameObject obj, string recieverFunction, T data)
		{
			GameObject current = obj.transform.parent.gameObject;
			Percolation<T> percolation = new Percolation<T>(data);

			while (current && !percolation.GetHalt())
			{
				current.SendMessage(recieverFunction, percolation, SendMessageOptions.DontRequireReceiver);
				current = current.transform.parent.gameObject;
			}
		}

		#endregion

		#region ForEach Gameobject

		/// <summary>
		/// Performs a breadth-first search on a game object and its children invoking the passed
		/// function on each game object.
		/// </summary>
		public static void ForEach(this GameObject toIterateOver, Action<GameObject> onVisit)
		{
			Queue<GameObject> toVisit = new Queue<GameObject>();
			toVisit.Enqueue(toIterateOver);

			while (toVisit.Count > 0)
			{
				GameObject visit = toVisit.Dequeue();
				onVisit(visit);

				for (int i = 0; i < visit.transform.childCount; i++)
				{
					toVisit.Enqueue(visit.transform.GetChild(i).gameObject);
				}
			}
		}

		#endregion

		#region GetSceneRoots
		/// <summary>
		/// Returns a list of the root gameobjects in the active scene.
		/// </summary>
		/// <returns>A list of the root gameobjects in the active scene.</returns>
		public static GameObject[] GetActiveSceneRoots()
		{
			return SceneManager.GetActiveScene().GetRootGameObjects();
		}

		#endregion GetSceneRoots
	}
}
