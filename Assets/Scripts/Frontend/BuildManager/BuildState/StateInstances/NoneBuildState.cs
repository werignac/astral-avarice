using UnityEngine;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// The build state of the BuildManager when no building actions are occurring.
	/// </summary>
    public class NoneBuildState : IBuildState
    {
		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();

		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();

		public UnityEvent OnApplyFailed { get; } = new UnityEvent();

		public BuildStateType GetStateType() => BuildStateType.NONE;
		public void Start() { }
		public void Update(BuildStateInput input) { }
		public void CleanUp() { }
	}
}
