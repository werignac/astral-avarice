using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AstralAvarice.Frontend
{
	[RequireComponent(typeof(Collider2D))]
	/// <summary>
	/// Assumes a cable never starts with an invalid overlap.
	/// </summary>
    public class CableCollisionComponent : MonoBehaviour
    {
		/// <summary>
		/// Colliders to ignore when checking for overlaps.
		/// e.g. buildings attached to the cable.
		/// </summary>
		private HashSet<Collider2D> ignoreColliders = new HashSet<Collider2D>();

		/// <summary>
		/// Functors that filter for whether a collider should be ignored.
		/// e.g. cables attached to the same building.
		/// </summary>
		private HashSet<Func<Collider2D, bool>> ignoreFilters = new HashSet<Func<Collider2D, bool>>();

		private int currentOverlapCount = 0;

		/// <summary>
		/// Invoked when a cable has been overlapping with another object for
		/// over GlobalBuildSettings.MaxCableOverlapTime. Used to destroy
		/// the cable.
		/// </summary>
		[HideInInspector] public UnityEvent OnOverlapTimerExpired = new UnityEvent();

		private Coroutine overlapTimer = null;

		/// <summary>
		/// Gets whether the cable is currently overlapping with something it shouldn't.
		/// The cable component waits a little before destroying itself when an overlap is
		/// detected.
		/// </summary>
		private bool IsOverlapping => currentOverlapCount > 0;

		public void AddIgnoreCollider(Collider2D colliderToIgnore)
		{
			ignoreColliders.Add(colliderToIgnore);
		}

		public void AddIngoreFilter(Func<Collider2D, bool> filter)
		{
			ignoreFilters.Add(filter);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			// If a non-valid overlap entered the cable.
			if(! IsValidOverlap(collision.collider))
			{
				// If this will be the first overlap, start the overlap timer.
				bool startTimer = currentOverlapCount == 0;

				// Keep track of how many overlaps the cable is experiencing.
				currentOverlapCount++;

				// Start the timer if it hasn't started already.
				if (startTimer)
				{
					Debug.Assert(overlapTimer == null, $"Cable overlap timer was not null prior to first overlap: {overlapTimer}.");
					overlapTimer = StartCoroutine(OverlapTimer());
				}
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			// If a non-valid overlap left the cable.
			if (!IsValidOverlap(collision.collider))
			{
				// Decrement the number of current overlaps.
				if (currentOverlapCount > 0)
					currentOverlapCount--;
				// Stop the overlap timer if there are no longer any overlaps.
				if (currentOverlapCount == 0 && overlapTimer != null)
				{
					StopCoroutine(overlapTimer);
					overlapTimer = null;
				}
			}
		}

		private bool IsValidOverlap(Collider2D collider)
		{
			bool ignoreCollider = ignoreColliders.Contains(collider);
			
			if (ignoreCollider)
				return true;

			// See if any of the filters allow the collider to be ignoed.
			foreach (Func<Collider2D, bool> filter in ignoreFilters)
			{
				if (filter.Invoke(collider))
					return true;
			}

			return false;
		}

		private IEnumerator OverlapTimer()
		{
			float overlapTimerDuration = GlobalBuildingSettings.GetOrCreateSettings().MaxCableOverlapTime;
			yield return new WaitForSeconds(overlapTimerDuration);

			if (IsOverlapping)
				OnOverlapTimerExpired.Invoke();
		}

		private void OnDestroy()
		{
			if (overlapTimer != null)
				StopCoroutine(overlapTimer);
		}
	}
}
