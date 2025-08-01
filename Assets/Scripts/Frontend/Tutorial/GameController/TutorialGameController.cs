using UnityEngine;
using werignac.Utils;
using AstralAvarice.Frontend;
using System;

public class TutorialGameController : GameController
{
    enum TutorialStateChangeCondition { click = 0, building = 1, cable = 2, planetDestroyed = 3, cableFour = 4, selectedHouse = 5, selectedThruster = 6, clickFour = 7, demolish = 8, move = 9, gridExceedsPower = 10, count };
    enum TutorialAllowedAction { none = 0, cable = 1, pylon = 2, fission = 3, lab = 4, coal = 5, demolish = 6, move = 7, count };

	[SerializeField] private bool endGameWhenOutOfTutorialSteps = false; // Used in the basic tutorial. TODO: Make this generic.
	[SerializeField] private bool restrictChaining = false; // Used in the basic tutorial. TODO: Make this generic.
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private MissionData tutorialMission;
    [SerializeField] private BuildMenuUIComponent buildMenu;
    [SerializeField] private TutorialStateChangeCondition[] stateChangeConditions;
    [SerializeField] private TutorialAllowedAction[] allowedActions;
    [SerializeField] private int[] gameSpeedOverrides;
    [SerializeField] private Vector3[] cameraFocuses;
    [SerializeField] private GameObject[] displayObjects;
    [SerializeField] private bool[] setBuildToNone;
	/// <summary>
	/// The first two numbers (x, y) are the center of a circle with radius z where the build is allowed.
	/// This is used for placing both buildings and cables (Build, Cable, Chain, and Move states).
	/// </summary>
	[SerializeField] private Vector3[] buildLocations;
    [SerializeField] private int[] buildMenuHighlights = new int[] { -1 };
    [SerializeField] private int[] buildSubMenuHighlights = new int[] { -1 };

    private int currentTutorialState = 0;
    private bool advanceAtEndOfNextUpdate = false;
    private int numCables = 0;

	[Header("Constraints")]
	[SerializeField] private TutorialBuildingConstraintComponent _tutorialBuildingConstraint;
	[SerializeField] private CablePlacerConstraintComponent _tutorialCableConstraint;

    public bool cablesAreAllowed()
    {
		if (currentTutorialState >= allowedActions.Length)
			return true;

        return (allowedActions[currentTutorialState] == TutorialAllowedAction.cable || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool demolishAllowed()
    {
		if (currentTutorialState >= allowedActions.Length)
			return true;

        return (allowedActions[currentTutorialState] == TutorialAllowedAction.demolish || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool moveAllowed()
    {
		if (currentTutorialState >= allowedActions.Length)
			return true;

        return (allowedActions[currentTutorialState] == TutorialAllowedAction.move || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool buildingAllowed(BuildingData building)
    {
		if (currentTutorialState >= allowedActions.Length)
			return true;

        if (allowedActions[currentTutorialState] == TutorialAllowedAction.count)
        {
            return (true);
        }
        if(allowedActions[currentTutorialState] == TutorialAllowedAction.pylon)
        {
            return (building.buildingName == "Pylon");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.fission)
        {
            return (building.buildingName == "Fission Plant");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.lab)
        {
            return (building.buildingName == "Small Lab");
        }
        else if (allowedActions[currentTutorialState] == TutorialAllowedAction.coal)
        {
            return (building.buildingName == "Small Coal Plant");
        }
        return (false);
    }

	/// <summary>
	/// Advances to the nest step of the tutorial.
	/// </summary>
    private void AdvanceToNextState()
    {
		if (currentTutorialState == stateChangeConditions.Length)
			return;

		++currentTutorialState;
        // If we have no more steps in the tutorial, end the game.
		if (currentTutorialState >= stateChangeConditions.Length)
        {
			if (endGameWhenOutOfTutorialSteps)
			{
				tutorialUI.ShowNextElement();
				EndGame(true, 0);
			}
			return;
        }

		// Otherwise, set things up for the next step.
        if (setBuildToNone[currentTutorialState])
        {
            BuildManagerComponent.Instance.SendExternalCancelSignal();
        }
            
		if(displayObjects[currentTutorialState - 1] != null)
        {
            displayObjects[currentTutorialState - 1].SetActive(false);
        }
            
		if(displayObjects[currentTutorialState] != null)
        {
            displayObjects[currentTutorialState].SetActive(true);
        }

        if(cameraFocuses[currentTutorialState].z > 0f)
        {
            FocusCamera(cameraFocuses[currentTutorialState]);
        }

		// If there is a building restriction, enable the building constraint.
		UpdatePlaceBuildingBuildConstraint();

		// If there is a cable restriction, enable the cable constraint.
		UpdatePlaceCableBuildConstraint();

		// If there is a demolish restriction, enable the demolish constraint.
		UpdateDemolishBuildConstraint();

		buildMenu.UpdateHighlightIndecies(buildMenuHighlights[currentTutorialState], buildSubMenuHighlights[currentTutorialState]);
            
		tutorialUI.ShowNextElement();
    }

	/// <summary>
	/// Invoked when changing tutorial steps.
	/// 
	/// Updates the constraint for placing buildings to be active or inactive
	/// depending on where we are in the tutorial. Also updates the position
	/// and distance the constraint uses.
	/// </summary>
	private void UpdatePlaceBuildingBuildConstraint()
	{
		Vector3 buildLocation = buildLocations[currentTutorialState];

		if (buildLocation.z <= 0) // z = Distance from target.
		{
			// Disable the building placement constraint.
			if (_tutorialBuildingConstraint != null)
				_tutorialBuildingConstraint.enabled = false;
			return;
		}

		if (_tutorialBuildingConstraint == null)
		{
			Debug.LogError("Missing reference to tutorial building constraint in TutorialGameController when we are in a step that restricts the position of a building.");
			return;
		}

		_tutorialBuildingConstraint.enabled = true;

		// Vector2 removes the z coordinate for the first argument,
		// which is the max distance in the second argument.
		_tutorialBuildingConstraint.SetPlacementTargetAndMaxDistance(buildLocation, buildLocation.z);
	}

	/// <summary>
	/// Invoked when changing tutorial steps.
	/// 
	/// Updates the constraint for placing cables to be active or inactive
	/// depending on where we are in the tutorial. Also updates the position
	/// and distance the constraint uses.
	/// </summary>
	private void UpdatePlaceCableBuildConstraint()
	{
		// TODO: Add a cable constraint.
	}

	/// <summary>
	/// Invoked when changing tutorial steps.
	/// </summary>
	private void UpdateDemolishBuildConstraint()
	{
		// TODO: Add a demolish constraint.
	}

	/// <summary>
	/// A listener to the BuildManager's OnApplyFailed event.
	/// This listener will move the camera to the focus of this step
	/// of the tutorial when the user fails to apply in a build state.
	/// </summary>
	private void BuildManager_OnApplyFailed()
	{
		// Removed resetting the build state.
		// Since the tutorial component is used for about half
		// of the levels, cancelling here makes the behaviour inconsistent
		// for half of the game.

		if (currentTutorialState >= cameraFocuses.Length)
			return;

		if (cameraFocuses[currentTutorialState].z > 0)
			FocusCamera(cameraFocuses[currentTutorialState]);
	}

    protected override void Start()
    {
        if(tutorialMission != null)
        {
            Data.selectedMission = tutorialMission;
        }

        base.Start();

		// Reset the camera + build state on build failed.
		BuildManagerComponent.Instance.OnBuildApplyFailed.AddListener(BuildManager_OnApplyFailed);

		// TODO: Make more generic.
		if (restrictChaining)
		{
			BuildManagerComponent.Instance.SetDefaultTransitionHandlerOverride(
				BuildStateTransitionSignalType.CHAIN,
				RestrictChaining
			);
		}
	}

	private bool RestrictChaining(BuildStateTransitionSignal signal)
	{
		if (!(signal is ChainTransitionSignal chainSignal))
			throw new ArgumentException($"Expected signal to be a chain signal, but got signal {signal}.");

		return chainSignal.NewBuildingType.BuildingDataAsset.buildingName != "Pylon"; 
	}

    protected override void Update()
    {
        base.Update();

		if (currentTutorialState >= stateChangeConditions.Length)
			return;

        if (gameSpeedOverrides[currentTutorialState] >= 0)
        {
            gameSpeed = gameSpeedOverrides[currentTutorialState];
        }
        if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.click && !GamePaused && Input.GetMouseButtonDown(0))
        {
            advanceAtEndOfNextUpdate = true;
        }
        if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.clickFour && !GamePaused && Input.GetMouseButtonDown(0))
        {
            ++numCables;
            if(numCables >= 4)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }

		// IMPORTANT: Advances to the next step of the tutorial.
        if(advanceAtEndOfNextUpdate)
        {
            AdvanceToNextState();
            advanceAtEndOfNextUpdate = false;
            numCables = 0;
        }
    }

    public override void BuildManager_OnBuildResovle(BuildStateApplyResult result)
    {
		if (currentTutorialState >= stateChangeConditions.Length)
		{
			base.BuildManager_OnBuildResovle(result);
			return;
		}

		// If the player demolished the building they were supposed to demolish,
		// move to the next step of the tutorial.
		if (result is DemolishBuildStateApplyResult)
        {
            if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.demolish)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }
		

        base.BuildManager_OnBuildResovle(result);

		// If the player placed a building, and the current step of the tutorial was to place a building,
		// move to the next part of the tutorial.
		// Assumes that the placed building was correct.
        if (result is BuildingBuildStateApplyResult)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.building)
            {
                advanceAtEndOfNextUpdate = true;
            }
            UpdateBuildingResources(); // Might be called manually due to time scale often being 0 in the tutorials.
        }
		
		// If the player demolished any buildings, update the building resources.
		// This might be called manually due to the time scale often being 0 in tutorials.
        if(result is DemolishBuildStateApplyResult)
        {
            UpdateBuildingResources();
        }

		// If the player placed a cable, and the current step of the tutorial was to place a cable,
		// move to the next part of the tutorial.
		// Assumes that the cable was placed in the right spot.
        if (result is CableBuildStateApplyResult cableResult)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.cable)
            {
                advanceAtEndOfNextUpdate = true;
            }
            else if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.cableFour)
            {
                ++numCables;
                if(numCables >= 4)
                {
                    advanceAtEndOfNextUpdate = true;
                }
            }
			else if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.gridExceedsPower)
			{
				int gridGroup = cableResult.cableInstance.Start.GridGroup;
				GridGroupData gridGroupData = GetGridGroupData(gridGroup);

				// Hard-coded value for basic tutorial.
				// TODO: Rewrite the tutorial systems so that it's
				// extensible for new steps.
				int powerThreshold = 20;
				
				// This probably also isn't placed in the best spot.
				// Assumes that the grid is not already at this threshold.
				
				if (gridGroupData.TotalPowerProduced > powerThreshold)
					advanceAtEndOfNextUpdate = true;
			}
        }

		// If the player moved a building, and the current step of the tutorial was to move a buildling,
		// move to the next part of the tutorial.
		// Assumes that the moved building was placing in the right spot.
        if(result is MoveBuildStateApplyResult)
        {
            if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.move)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }
    }

    protected override void Planet_OnDestroyed(PlanetComponent planetComponent)
    {
        base.Planet_OnDestroyed(planetComponent);
        if (currentTutorialState < stateChangeConditions.Length && stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.planetDestroyed)
        {
            advanceAtEndOfNextUpdate = true;
        }
    }

    protected override void RegisterBuilding(BuildingComponent buildingComponent)
    {
        base.RegisterBuilding(buildingComponent);
        buildingComponent.OnBuildingSelected.AddListener(delegate { BuildingSelected(buildingComponent); });
    }

    public void BuildingSelected(BuildingComponent building)
    {
        if (currentTutorialState < stateChangeConditions.Length)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.selectedHouse && building.Data.name.Contains("House"))
            {
                advanceAtEndOfNextUpdate = true;
            }
            else if(stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.selectedThruster && building.Data.name.Contains("Thruster"))
            {
                advanceAtEndOfNextUpdate = true;
            }
        }
        
    }

    public void FocusCamera(Vector2 focusPosition)
    {
        mainCamera.GetComponent<CameraMovementComponent>().MoveTo(focusPosition);
    }
}
