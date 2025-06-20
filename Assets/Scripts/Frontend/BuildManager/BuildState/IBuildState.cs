using UnityEngine;
using UnityEngine.Events;
using System;

namespace AstralAvarice.Frontend
{
	// TODO: Put all these lingering classes into their own scripts.
	public interface IOverrideExternalSignal
	{
		/// <summary>
		/// Overrides handling of transition signals from external sources
		/// (e.g. user presses building button) from the default BuildManagerComponent
		/// behaviour.
		/// </summary>
		/// <param name="signal">The signal to process.</param>
		/// <returns>Whether the signal was processed. If false, default handling is used.</returns>
		public bool TryOverrideExternalSignal(BuildStateTransitionSignal signal);
	}

	public enum BuildStateType
	{
		NONE = 0,
		BUILDING = 1,
		CABLE = 2,
		BUILDING_CHAINED = 3,
		DEMOLISH = 4,
		MOVE = 8
	}

	/// <summary>
	/// TODO: Find a better name?
	/// </summary>
	public interface IHasCost
	{
		public Cost GetCost();
	}

	/// <summary>
	/// Interface for all the states of the build manager.
	/// </summary>
    public interface IBuildState
    {
		/// <summary>
		/// Invoked internally when the user wants to switch to a different build state.
		/// </summary>
		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; }

		/// <summary>
		/// Invoked internally when the build state is applied.
		/// </summary>
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } // TODO: This could be implemented by OnRequestTransition.

		/// <summary>
		/// Invoked when the user tried to apply this build state and failed.
		/// </summary>
		public UnityEvent OnApplyFailed { get; }

		/// <summary>
		/// Returns the type of this build state.
		/// </summary>
		public BuildStateType GetStateType();

		/// <summary>
		/// Called by owner every frame (before querying constraints).
		/// </summary>
		/// <param name="input">Input from the player.</param>
		public void Update(BuildStateInput input);

		/// <summary>
		/// Called after construction and after CleanUp for the last state, but before
		/// the first update.
		/// </summary>
		void Start();

		/// <summary>
		/// Called before this build state is destroyed / when we're switching out of this build state.
		/// </summary>
		public void CleanUp();
	}

	/// <summary>
	/// Build state that processes the result of applying constraints.
	/// Almost all BuildStates implement this interface.
	/// </summary>
	public interface IPostConstraintsBuildState<TConstraintsQueryResult> : IBuildState
	{
		/// <summary>
		/// Called by owner every frame after querying constraints.
		/// </summary>
		/// <param name="input">Input from the player.</param>
		/// <param name="constraintsResult">Result of querying constraints.</param>
		public void UpdatePostConstraints(BuildStateInput input, TConstraintsQueryResult constraintsResult);
	}
}
