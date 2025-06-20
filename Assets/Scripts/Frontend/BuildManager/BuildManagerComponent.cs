using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using werignac.Utils;
using AstralAvarice.Frontend;

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
	[HideInInspector] public UnityEvent<BuildWarningContext> OnBuildWarningUpdate = new UnityEvent<BuildWarningContext>();
	// Invoked when the user tried to apply in the current build state, but failed to do so.
	[HideInInspector] public UnityEvent OnBuildApplyFailed = new UnityEvent();

	// Collects input from the user (primary & secondary fire) until the end of update
	// where teh input is reset.
	private BuildStateInput input;

	[Tooltip("Whether to override the buildings list in GlobalBuildingSettings.")]
	[SerializeField] private bool overrideBuildingsList = false;
	[Tooltip("The list of buildings that overrides the normal list in GlobalBuildingSettings.")]
	[SerializeField] private BuildingSettingEntry[] buildingsList = new BuildingSettingEntry[0];

	[Header("Constraints")]
	[SerializeField] private BuildingPlacerConstraintComponent[] buildingConstraints;
	[SerializeField] private CablePlacerConstraintComponent[] cableConstraints;
	[SerializeField] private CostConstraintComponent[] costConstraints;

	public IBuildState State { get => state; }

	/// <summary>
	/// When true, we skip quering constraints and calling UpdatePostConstraints this Update.
	/// Useful for when we switch states prior to querying constraints this Update
	/// (otherwise input would be used twice, state might be ready, etc.).
	/// </summary>
	private bool _skipConstraintsQueryThisUpdate = false;

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
		gravityCursor.Hide();
	}

	/// <summary>
	/// Internally changes the state of the build manager.
	/// Handles cleanup for the previous state and initialization for the new state.
	/// </summary>
	/// <param name="newState"></param>
	private void SetState(IBuildState newState)
	{
		// Prevent UpdatePostConstraints being called on a state before Update has been called.
		// Won't affect calls outside of the update loop because _skipConstrainsQueryThisUpdate is set to false at the start of an Update.
		_skipConstraintsQueryThisUpdate = true;

		IBuildState oldState = state;

		if (oldState != null)
		{
			oldState.CleanUp();

			oldState.OnApplied.RemoveListener(State_OnApplied);
			oldState.OnRequestTransition.RemoveListener(State_OnRequestTransition);
			oldState.OnApplyFailed.RemoveListener(State_OnApplyFailed);
		}

		state = newState;

		if (newState != null)
		{
			newState.OnApplied.AddListener(State_OnApplied);
			newState.OnRequestTransition.AddListener(State_OnRequestTransition);
			newState.OnApplyFailed.AddListener(State_OnApplyFailed);

			newState.Start();
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

	public void SendExternalMoveSignal()
	{
		ProcessTransitionSignal(new MoveTransitionSignal(true));
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
			case BuildStateTransitionSignalType.CHAIN:
				DefaultProcessChainTransitionSignal(signal);
				break;
			case BuildStateTransitionSignalType.MOVE:
				DefaultProcessMoveTransitionSignal(signal);
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
			gameController
		));
	}

	private void DefaultProcessCableTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is CableTransitionSignal cableSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a CableTransitionSignal, but wasn't.");

		SetState(new CableBuildState(
			cableCursor,
			selectionCursor,
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

	private void DefaultProcessChainTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is ChainTransitionSignal chainSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a ChainTransitionSignal, but wasn't.");

		SetState(new ChainBuildState(
			chainSignal.NewBuildingType,
			chainSignal.ChainFrom,
			selectionCursor,
			buildingCursor,
			cableCursor,
			gravityCursor,
			gameController
		));
	}

	private void DefaultProcessMoveTransitionSignal(BuildStateTransitionSignal signal)
	{
		if (!(signal is MoveTransitionSignal moveSignal))
			throw new ArgumentException($"Signal {signal} was expected to be a MoveTransitionSignal, but wasn't.");

		SetState(new MoveBuildState(
			selectionCursor,
			buildingCursor,
			cableCursor,
			gameController,
			moveSignal.ToMove
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

	private BuildWarning.WarningType DefaultQueryConstraints(IBuildState state, BuildWarningContext context)
	{
		if (state == null)
			throw new ArgumentNullException("state");

		BuildWarning.WarningType highestWarning = BuildWarning.WarningType.GOOD;

		if (state is IHasCost hasCost)
		{
			Cost cost = hasCost.GetCost();

			CostConstraintData data = new CostConstraintData
			{
				cost = cost,
				preceedingCosts = Cost.ZERO
			};

			BuildWarning.WarningType highestCostWarning = DefaultQueryConstraintType(data, costConstraints, context);

			if (highestCostWarning > highestWarning)
				highestWarning = highestCostWarning;
		}

		if (state is IBuildingPlacer buildingPlacer)
		{
			BuildWarning.WarningType highestBuildingPlacerWarning = DefaultQueryConstraintType(buildingPlacer, buildingConstraints, context);

			if (highestBuildingPlacerWarning > highestWarning)
				highestWarning = highestBuildingPlacerWarning;
		}

		if (state is ICablePlacer cablePlacer)
		{
			BuildWarning.WarningType highestCablePlacerWarning = DefaultQueryConstraintType(cablePlacer, cableConstraints, context);

			if (highestCablePlacerWarning > highestWarning)
				highestWarning = highestCablePlacerWarning;
		}

		return highestWarning;
	}

	private static BuildWarning.WarningType DefaultQueryConstraintType<TInput, TConstraint>(
			TInput constraintInput,
			TConstraint[] constraints,
			BuildWarningContext context
		) where TConstraint : IBuildConstraint<TInput>
	{
		BuildWarning.WarningType highestWarning = BuildWarning.WarningType.GOOD;

		foreach (var constraint in constraints)
		{
			ConstraintQueryResult result = constraint.QueryConstraint(constraintInput);

			context.AddBuildingWarnings(result.Warnings);

			if (result.HighestWarning > highestWarning)
				highestWarning = result.HighestWarning;
		}

		return highestWarning;
	}

	public ChainConstraintsQueryResult QueryChainConstraints(ChainBuildState chainState, BuildWarningContext context)
	{
		if (chainState == null)
			throw new ArgumentNullException("chainState");

		Cost buildingCost = chainState.GetBuildingCost();
		Cost cableCost = chainState.GetCableCost();

		CostConstraintData buildingCostData = new CostConstraintData
		{
			cost = buildingCost,
			preceedingCosts = Cost.ZERO
		};

		CostConstraintData cableCostData = new CostConstraintData
		{
			cost = cableCost,
			preceedingCosts = buildingCost
		};

		// Start by getting the warning for the building.
		BuildWarning.WarningType buildingCostHighestWarning = DefaultQueryConstraintType(buildingCostData, costConstraints, context);
		BuildWarning.WarningType buildingHighestWarning = DefaultQueryConstraintType(chainState as IBuildingPlacer, buildingConstraints, context);

		if (buildingCostHighestWarning > buildingHighestWarning)
			buildingHighestWarning = buildingCostHighestWarning;

		// Then get the warning for the cable.
		BuildWarning.WarningType cableCostHighestWarning = DefaultQueryConstraintType(cableCostData, costConstraints, context);
		BuildWarning.WarningType cableHighestWarning = DefaultQueryConstraintType(chainState as ICablePlacer, cableConstraints, context);

		if (cableCostHighestWarning > cableHighestWarning)
			cableHighestWarning = cableCostHighestWarning;

		return new ChainConstraintsQueryResult(buildingHighestWarning, cableHighestWarning);
	}

	public MoveConstraintsQueryResult QueryMoveConstraints(MoveBuildState moveState, BuildWarningContext context)
	{
		if (moveState == null)
			throw new ArgumentNullException("moveState");

		BuildWarning.WarningType buildingResult = DefaultQueryConstraintType(moveState as IBuildingPlacer, buildingConstraints, context);

		// Query each cable.
		ICablePlacer[] cablePlacers = moveState.GetCablePlacers();
		BuildWarning.WarningType[] cableResults = new BuildWarning.WarningType[cablePlacers.Length];

		for (int i = 0; i < cablePlacers.Length; i++)
		{
			cableResults[i] = DefaultQueryConstraintType(cablePlacers[i], cableConstraints, context);
		}

		return new MoveConstraintsQueryResult(buildingResult, cableResults);
	}

	/// <summary>
	/// Invoked when the current state has failed to be applied (& application was requested by the player).
	/// </summary>
	private void State_OnApplyFailed()
	{
		OnBuildApplyFailed.Invoke();
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
	[Obsolete("Use !IsInactive() instead.")]
	public bool IsInBuildState()
	{
		return (state.GetStateType() != BuildStateType.NONE);
	}

	/// <summary>
	/// Get whether we are currently in a build state that is not
	/// the none state. 
	/// 
	/// TODO: Create IsActive function instead?
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
		_skipConstraintsQueryThisUpdate = false;
		UpdateCurrentState();
		if (!_skipConstraintsQueryThisUpdate)
			QueryConstraintsAndPostUpdate();
		ResetInput();
	}

	/// <summary>
	/// Query the constraints for the current BuildState and call
	/// UpdatePostConstraints if relevant.
	/// </summary>
	private void QueryConstraintsAndPostUpdate()
	{
		BuildWarningContext context = new BuildWarningContext();

		switch (state.GetStateType())
		{
			case BuildStateType.BUILDING_CHAINED:
				{
					ChainBuildState chainState = state as ChainBuildState;
					ChainConstraintsQueryResult queryResult = QueryChainConstraints(chainState, context);
					chainState.UpdatePostConstraints(input, queryResult);
				}
				break;
			case BuildStateType.MOVE:
				{
					MoveBuildState moveState = state as MoveBuildState;
					MoveConstraintsQueryResult queryResult = QueryMoveConstraints(moveState, context);
					moveState.UpdatePostConstraints(input, queryResult);
				}
				break;
			case BuildStateType.NONE:
			case BuildStateType.DEMOLISH:
				break;
			default:
				if (state is IPostConstraintsBuildState<BuildWarning.WarningType> constrainableState)
				{
					// Building and Cable states are handled here.
					BuildWarning.WarningType queryResult = DefaultQueryConstraints(constrainableState, context);
					constrainableState.UpdatePostConstraints(input, queryResult);
				}
				else
					Debug.LogWarning($"Unrecognized build state type {state.GetStateType()} may be missing logic for querying constraints.");
				break;
		}

		OnBuildWarningUpdate.Invoke(context);
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
