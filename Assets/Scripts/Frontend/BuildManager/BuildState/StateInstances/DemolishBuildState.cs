using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using werignac.Utils;

namespace AstralAvarice.Frontend
{
	public class DemolishBuildState : IBuildState
	{
		/// <summary>
		/// The demolishable object we're hovering over. If null, we're not having over a demolishable object.
		/// </summary>
		private IDemolishable _hoveringDemolishable;

		private PlanetComponent _affectedPlanet;

		private SelectionCursorComponent _selectionCursor;

		private GravityFieldCursorComponent _gravityCursor;

		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();

		public BuildStateType GetStateType() => BuildStateType.DEMOLISH;

		public DemolishBuildState(SelectionCursorComponent selectionCursor, GravityFieldCursorComponent gravityCursor)
		{
			_selectionCursor = selectionCursor;
			_gravityCursor = gravityCursor;
		}

		public void Start() { }

		public void Update(BuildStateInput input)
		{
			// NOTE: Only objects that return true for Demolishable() can be demolished.
			Collider2D demolishableCollider = _selectionCursor.FindFirstByPredicate((Collider2D collider) =>
			{
				return collider.TryGetComponentInParent(out IDemolishable demoComponent) && demoComponent.Demolishable();
			});

			IDemolishable demolishable = demolishableCollider == null ? null : demolishableCollider.GetComponentInParent<IDemolishable>();

			SetHoveringDemolishable(demolishable);
			UpdateGravityCursor();

			// TODO: Update Inspector for hovering demolishables?

			// TODO: Check constraints?

			if (input.primaryFire)
			{
				if (_hoveringDemolishable == null)
				{
					Cancel();
					return;
				}

				Apply();
			}
			else if (input.secondaryFire)
			{
				Cancel();
				return;
			}
		}

		private void Apply()
		{
			_hoveringDemolishable.Demolish();

			DemolishBuildStateApplyResult result = new DemolishBuildStateApplyResult
			{
				demolished = _hoveringDemolishable
			};
			OnApplied.Invoke(result);
		}

		private void SetHoveringDemolishable(IDemolishable newDemolishable)
		{
			// Don't do anything if nothing has changed.
			if (newDemolishable == _hoveringDemolishable)
				return;

			if (_hoveringDemolishable != null)
			{
				_hoveringDemolishable.HoverDemolishEnd();
			}

			_hoveringDemolishable = newDemolishable;

			if (newDemolishable != null)
			{
				_hoveringDemolishable.HoverDemolishStart();

				if (newDemolishable is BuildingComponent building)
				{
					SetAffectedPlanet(building.ParentPlanet);
				}
				else
				{
					SetAffectedPlanet(null);
				}
			}
			else
			{
				SetAffectedPlanet(null);
			}
		}

		private void SetAffectedPlanet(PlanetComponent newPlanet)
		{
			bool isDifferent = _affectedPlanet != newPlanet;

			if (!isDifferent)
				return;

			if (_affectedPlanet != null)
				_affectedPlanet.StopProspectingMassChange();

			_affectedPlanet = newPlanet;

			if (newPlanet != null)
				newPlanet.StartProspectingMassChange();
		}

		private void UpdateGravityCursor()
		{
			if (_affectedPlanet == null)
			{
				_gravityCursor.Hide();
				return;
			}

			BuildingComponent building = _hoveringDemolishable as BuildingComponent;
			_gravityCursor.Show();
			_gravityCursor.SetGravityCursor(_affectedPlanet, -building.Data.mass);;
		}

		private void Cancel()
		{
			CancelTransitionSignal signal = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(signal);
		}

		public void CleanUp()
		{
			SetHoveringDemolishable(null);
			_gravityCursor.Hide();
		}
	}
}
