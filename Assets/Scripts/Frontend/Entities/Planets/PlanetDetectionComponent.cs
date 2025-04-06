using UnityEngine;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
	[RequireComponent(typeof(Collider2D))]
    public class PlanetDetectionComponent : MonoBehaviour
    {
		protected HashSet<PlanetComponent> overlappingPlanets = new HashSet<PlanetComponent>();

		// TODO: Make collider-agnostic?
		/// <summary>
		/// Queries all the overlaps in the trigger volume and resets the
		/// overlappingPlanets set.
		/// </summary>
		protected void HardQueryOverlaps()
		{
			overlappingPlanets.Clear();

			// Assumes circle collider.
			CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();

			// TODO: Don't hard-code. Derive from physics settings.
			int layerMask = LayerMask.GetMask("Planets", "Stars");

			Collider2D[] colliders = Physics2D.OverlapCircleAll(
				circleCollider.transform.position,
				circleCollider.radius * circleCollider.transform.lossyScale.x,
				layerMask
			);

			foreach (Collider2D otherCollider in colliders)
			{
				if (otherCollider.gameObject == transform.parent.gameObject)
					continue;

				overlappingPlanets.Add(otherCollider.GetComponent<PlanetComponent>());
			}

			OnOverlappingPlanetsChanged(overlappingPlanets.Count);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			PlanetComponent planet = collision.GetComponent<PlanetComponent>();
			bool found = overlappingPlanets.Add(planet);
			if (found)
				OnOverlappingPlanetsChanged(1);
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			PlanetComponent planet = collision.GetComponent<PlanetComponent>();
			bool found = overlappingPlanets.Remove(planet);
			if (found)
				OnOverlappingPlanetsChanged(-1);
		}

		protected virtual void OnOverlappingPlanetsChanged(int lengthChange) { }
	}
}
