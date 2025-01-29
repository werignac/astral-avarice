using UnityEngine;

public class TutorialGameController : GameController
{
    enum TutorialStateChangeCondition { click = 0, building = 1, cable = 2, planetDestroyed = 3 };
    enum TutorialAllowedAction { none = 0, cable = 1, pylon = 2, fission = 3, lab = 4, coal = 5 };

    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private TutorialStateChangeCondition[] stateChangeConditions;
    [SerializeField] private TutorialAllowedAction[] allowedActions;
    [SerializeField] private int[] gameSpeedOverrides;

    private int currentTutorialState = 0;
    private bool advanceAtEndOfNextUpdate = false;

    public bool cablesAreAllowed()
    {
        return (allowedActions[currentTutorialState] == TutorialAllowedAction.cable);
    }
    public bool buildingAllowed(BuildingData building)
    {
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

    private void AdvanceToNextState()
    {
        ++currentTutorialState;
        if (currentTutorialState >= stateChangeConditions.Length)
        {
            EndGame();
        }
        else
        {
            BuildManagerComponent.Instance.SetNoneState();
            tutorialUI.ShowNextElement();
        }
    }

    protected override void Update()
    {
        base.Update();
        if (gameSpeedOverrides[currentTutorialState] >= 0)
        {
            gameSpeed = gameSpeedOverrides[currentTutorialState];
        }
        if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.click && Input.GetMouseButtonDown(0))
        {
            advanceAtEndOfNextUpdate = true;
        }
        if(advanceAtEndOfNextUpdate)
        {
            AdvanceToNextState();
            advanceAtEndOfNextUpdate = false;
        }
    }

    public override void BuildManager_OnBuildResovle(BuildResolve resolution)
    {
        base.BuildManager_OnBuildResovle(resolution);

        if (resolution.successfullyPlacedBuilding)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.building)
            {
                advanceAtEndOfNextUpdate = true;
            }
        }

        if (resolution.successfullyPlacedCable)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.cable)
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
}
