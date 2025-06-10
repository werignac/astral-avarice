using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using werignac.Utils;
using AstralAvarice.Frontend;

[Serializable]
public class BuildConstraintEntry<TBuildConstraint, TBuildConstraintState> where TBuildConstraint : IBuildConstraint<TBuildConstraintState>
{
	[Tooltip("The constraint that restricts how a structure can be placed.")]
	public TBuildConstraint constraint;
	[Tooltip("Whether this constraint should be applied when moving buildings in addition to initially placing them.")]
	public bool appliesToMove = true;
}

/// <summary>
/// Singleton component that manages placing buildings.
/// </summary>
public class BuildManagerComponent : MonoBehaviour
{
	// TODO: Use scriptable objects instead of singletons?
	public static BuildManagerComponent Instance { get; private set;}

	[SerializeField] private GameController gameController;

	// Reference to the cursor used for showing where buildings will go.
	// Also checks whether a building can be placed (collides with other buildings).
	[SerializeField] private BuildingCursorComponent buildingCursor;
	[SerializeField] private CableCursorComponent cableCursor;
	[SerializeField] private SelectionCursorComponent selectionCursor;
	[SerializeField] private GravityFieldCursorComponent gravityCursor;
	private List<CableCursorComponent> moveCableCursors = new List<CableCursorComponent>();

	// The current state of building manager.
	private IBuildState state;

	// Events
	// Invoked when the build state changes. Passes the old state and the new state.
	[HideInInspector] public UnityEvent<IBuildState, IBuildState> OnStateChanged = new UnityEvent<IBuildState, IBuildState>();
	// Pass information about how the build resolved. e.g. built building, but failed to connect cable
	// build cable but didn't create a new building, build both a building and cable, didn't ask to build a cable, etc.
	// Though information about demolitions are sent, using it is not recommended. Instead listen to destroy
	// events for buildings and cables.
	[HideInInspector] public UnityEvent<BuildStateApplyResult> OnBuildApply = new UnityEvent<BuildStateApplyResult>();
	[HideInInspector] public UnityEvent<BuildWarningContainer> OnBuildWarningUpdate = new UnityEvent<BuildWarningContainer>();


	// Collects input from the user (primary & secondary fire) until the end of update
	// where teh input is reset.
	private BuildStateInput input;

	[Tooltip("Whether to override the buildings list in GlobalBuildingSettings.")]
	[SerializeField] private bool overrideBuildingsList = false;
	[Tooltip("The list of buildings that overrides the normal list in GlobalBuildingSettings.")]
	[SerializeField] private BuildingSettingEntry[] buildingsList = new BuildingSettingEntry[0];

	[Header("Constraints")]
	[SerializeField] private BuildConstraintEntry<BuildingConstraintComponent, BuildingConstraintData>[] buildingConstraints;
	[SerializeField] private BuildConstraintEntry<CableConstraintComponent, CableConstraintData>[] cableConstraints;

	public IBuildState State { get => state; }

	public BuildingSettingEntry[] PlaceableBuildings
	{
		get
		{
			if (overrideBuildingsList)
				return buildingsList;
			else
				return GlobalBuildingSettings.GetOrCreateSettings().Buildings;
		}
	}

	private void Awake()
	{
		// Singleton enforecement.
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		
		Instance = this;
	}

	private void Start()
	{
		// Initally, we aren't building anything.
		SetState(new NoneBuildState());

		// TODO: Amend this. It should be the gameController who registers its listeners.
		OnBuildApply.AddListener(gameController.BuildManager_OnBuildResovle);

		buildingCursor.Hide();
		cableCursor.Hide();
	}

	/// <summary>
	/// Internally changes the state of the build manager.
	/// Handles cleanup for the previous state and initialization for the new state.
	/// </summary>
	/// <param name="newState"></param>
	private void SetState(IBuildState newState)
	{
		IBuildState oldState = state;

		if (oldState != null)
		{
			oldState.CleanUp();

			oldState.OnApplied.RemoveListener(State_OnApplied);
			oldState.OnRequestTransition.RemoveListener(State_OnRequestTransition);
		}

		state = newState;

		if (state != null)
		{
			state.OnApplied.AddListener(State_OnApplied);
			state.OnRequestTransition.AddListener(State_OnRequestTransition);
		}

		OnStateChanged?.Invoke(oldState, newState);
	}

	public void SendExternalCancelSignal()
	{
		ProcessTransitionSignal(new CancelTransitionSignal(true));
	}

	public void SendExternalBuildSignal(BuildingSettingEntry buildingSettings)
	{
		ProcessTransitionSignal(new BuildingTransitionSignal(
			buildingSettings,
			true
		));
	}

	public void SendExternalCableSignal(BuildingComponent cableStart = null)
	{
		ProcessTransitionSignal(new CableTransitionSignal(
			cableStart,
			true
		));
	}

	public void SendExternalDemolishSignal()
	{
		ProcessTransitionSignal(new DemolishTransitionSignal(true));
	}

	private void ProcessTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (signal.IsExternal && state != null && state is IOverrideExternalSignal overrideSignal)
		{
			// Override handling.
			bool signalProcessed = overrideSignal.TryOverrideExternalSignal(signal);
			if (signalProcessed)
				return;
		}

		// Default handling.
		switch(signal.GetSignalType())
		{
			case BuildStateTransitionSignalType.CANCEL:
				DefaultProcessCancelTransitionSignal(signal);
				break;
			case BuildStateTransitionSignalType.BUILDING:
				DefaultProcessBuildingTransitionSignal(signal);
				break;
			case BuildStateTransitionSignalType.CABLE:
				DefaultProcessCableTransitionSignal(signal);
				break;
			case BuildStateTransitionSignalType.DEMOLISH:
				DefaultProcessDemolishTransitionSignal(signal);
				break;
		}
	}

	private void DefaultProcessCancelTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is CancelTransitionSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a CancelTransitionSignal, but wasn't.");
		
		SetState(new NoneBuildState());
	}

	private void DefaultProcessBuildingTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is BuildingTransitionSignal buildingSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a BuildingTransitionSignal, but wasn't.");

		SetState(new BuildingBuildState(
			buildingSignal.NewBuildingType,
			buildingCursor,
			selectionCursor,
			gravityCursor,
			gameController,
			QueryBuildingConstraints 
		));
	}

	private void DefaultProcessCableTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is CableTransitionSignal cableSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a CableTransitionSignal, but wasn't.");

		SetState(new CableBuildState(
			cableCursor,
			selectionCursor,
			QueryCableConstraints,
			cableSignal.CableFrom
		));
	}

	private void DefaultProcessDemolishTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is DemolishTransitionSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a DemolishTransitionSignal, but wasn't.");

		SetState(new DemolishBuildState(
			selectionCursor,
			gravityCursor
		));
	}


	/// <summary>
	/// Invoked when the current state applies itself (adds, removes, or destroys a building or cable).
	/// </summary>
	/// <param name="applyResult">The result of the current state being applied.</param>
	private void State_OnApplied(BuildStateApplyResult applyResult)
	{
		OnBuildApply.Invoke(applyResult);
	}

	/// <summary>
	/// TODO: Combine with QueryCableConstraints?
	/// </summary>
	public BuildWarning.WarningType QueryBuildingConstraints()
	{
		if (!(state is BuildingBuildState buildingState))
			throw new Exception($"Cannot query building constraints while not in the building build state. Current state {state}.");

		// TODO: Build data in state?
		BuildingConstraintData data = new BuildingConstraintData
		(
			buildingState,
			buildingCursor,
			0
		);

		BuildWarning.WarningType highestWarning = BuildWarning.WarningType.GOOD;
		BuildWarningContainer warningContainer = new BuildWarningContainer();

		foreach (var constraintEntry in buildingConstraints)
		{
			ConstraintQueryResult result = constraintEntry.constraint.QueryConstraint(data);

			if (result.HasWarning)
			{
				warningContainer.AddBuildingWarning(result.Warning);

				if (result.Warning.GetWarningType() > highestWarning)
					highestWarning = result.Warning.GetWarningType();
			}
		}

		OnBuildWarningUpdate.Invoke(warningContainer);

		return highestWarning;
	}

	/// <summary>
	/// TODO: Combine with QueryBuildingConstraints
	/// </summary>
	public BuildWarning.WarningType QueryCableConstraints()
	{
		if (!(state is CableBuildState cableState))
			throw new Exception($"Cannot query building constraints while not in the cable build state. Current state {state}.");

		// TODO: Build data in state?
		CableConstraintData data = new CableConstraintData
		(
			cableState,
			cableCursor,
			0
		);

		BuildWarning.WarningType highestWarning = BuildWarning.WarningType.GOOD;
		BuildWarningContainer warningContainer = new BuildWarningContainer();

		foreach (var constraintEntry in cableConstraints)
		{
			ConstraintQueryResult result = constraintEntry.constraint.QueryConstraint(data);

			if (result.HasWarning)
			{
				warningContainer.AddCableWarning(result.Warning);
				if (result.Warning.GetWarningType() > highestWarning)
					highestWarning = result.Warning.GetWarningType();
			}
		}

		OnBuildWarningUpdate.Invoke(warningContainer);

		return highestWarning;
	}

	/// <summary>
	/// Invoked when the current state requests to transition to a different state.
	/// </summary>
	/// <param name="signal"></param>
	private void State_OnRequestTransition(BuildStateTransitionSignal signal)
	{
		ProcessTransitionSignal(signal);
	}

	/// <summary>
	/// Get whether the player is currently using the BuildManager.
	/// This can either be for building or demolishing.
	/// 
	/// TODO: Replace with !IsInactive().
	/// </summary>
	public bool IsInBuildState()
	{
		return (state.GetStateType() != BuildStateType.NONE);
	}

	/// <summary>
	/// Get whether we are currently in a build state that is not
	/// the none state. 
	/// </summary>
	/// <returns>True if we are in the none state. False otherwise.</returns>
	public bool IsInactive()
	{
		return (state == null || state.GetStateType() == BuildStateType.NONE);
	}

	/// <summary>
	/// Get whether the BuildManager should block building selecting.
	/// This can either be due to building or demolishing being active, but not move.
	/// </summary>
	public bool BlockSelection()
	{
		return (IsInBuildState() && state.GetStateType() != BuildStateType.MOVE);
	}

	private void Update()
	{
		UpdateCurrentState();
		ResetInput();
	}

	/// <summary>
	/// Updates the current state.
	/// </summary>
	private void UpdateCurrentState()
	{
		state?.Update(input);
	}

	/// <summary>
	/// Clear the input object to have no input.
	/// </summary>
	private void ResetInput()
	{
		input = new BuildStateInput();
	}

	/// <summary>
	/// Marks input for primary fire (typically applying the current state e.g. placing buildings).
	/// </summary>
	public void MarkPrimaryFireInput()
	{
		input.primaryFire = true;
	}

	/// <summary>
	/// Marks input for secondary fire (typically canceling the current state).
	/// </summary>
	public void MarkSecondaryFireInput()
	{
		input.secondaryFire = true;
	}
}
