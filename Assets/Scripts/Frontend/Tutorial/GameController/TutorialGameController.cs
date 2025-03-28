using UnityEngine;
using werignac.Utils;

public class TutorialGameController : GameController
{
    enum TutorialStateChangeCondition { click = 0, building = 1, cable = 2, planetDestroyed = 3, cableFour = 4, selectedHouse = 5, selectedThruster = 6, clickFour = 7, demolish = 8, move = 9, count };
    enum TutorialAllowedAction { none = 0, cable = 1, pylon = 2, fission = 3, lab = 4, coal = 5, demolish = 6, move = 7, count };

    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private MissionData tutorialMission;
    [SerializeField] private BuildMenuUIComponent buildMenu;
    [SerializeField] private TutorialStateChangeCondition[] stateChangeConditions;
    [SerializeField] private TutorialAllowedAction[] allowedActions;
    [SerializeField] private int[] gameSpeedOverrides;
    [SerializeField] private Vector3[] cameraFocuses;
    [SerializeField] private GameObject[] displayObjects;
    [SerializeField] private bool[] setBuildToNone;
    [SerializeField] private Vector3[] buildLocations; // The first two numbers (x, y) are the center of a circle with radius z where the build is allowed.
    [SerializeField] private int[] buildMenuHighlights = new int[] { -1 };
    [SerializeField] private int[] buildSubMenuHighlights = new int[] { -1 };

    private int currentTutorialState = 0;
    private bool advanceAtEndOfNextUpdate = false;
    private int numCables = 0;

    public bool cablesAreAllowed()
    {
        return (allowedActions[currentTutorialState] == TutorialAllowedAction.cable || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool demolishAllowed()
    {
        return (allowedActions[currentTutorialState] == TutorialAllowedAction.demolish || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool moveAllowed()
    {
        return (allowedActions[currentTutorialState] == TutorialAllowedAction.move || allowedActions[currentTutorialState] == TutorialAllowedAction.count);
    }
    public bool buildingAllowed(BuildingData building)
    {
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

    private void AdvanceToNextState()
    {
        ++currentTutorialState;
        if (currentTutorialState >= stateChangeConditions.Length)
        {
            EndGame(true, 0);
            ReturnToMenu();
        }
        else
        {
            if (setBuildToNone[currentTutorialState])
            {
                BuildManagerComponent.Instance.SetNoneState();
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
            buildMenu.UpdateHighlightIndecies(buildMenuHighlights[currentTutorialState], buildSubMenuHighlights[currentTutorialState]);
            tutorialUI.ShowNextElement();
        }
    }

    protected override void Start()
    {
        if(tutorialMission != null)
        {
            Data.selectedMission = tutorialMission;
        }
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
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
        if(advanceAtEndOfNextUpdate)
        {
            AdvanceToNextState();
            advanceAtEndOfNextUpdate = false;
            numCables = 0;
        }
    }

    public override void BuildManager_OnBuildResovle(BuildResolve resolution)
    {
        bool placedIncorrectly = false;
        if(resolution.successfullyPlacedBuilding)
        {
            if(buildLocations[currentTutorialState].z > 0)
            {
                float distance = (resolution.builtBuilding.transform.position - new Vector3(buildLocations[currentTutorialState].x, buildLocations[currentTutorialState].y)).magnitude;
                if(distance > buildLocations[currentTutorialState].z)
                {
                    resolution.successfullyPlacedBuilding = false;
                    resolution.totalCost -= resolution.builtBuilding.Data.cost;
                    Destroy(resolution.builtBuilding.gameObject);
                    resolution.builtBuilding = null;
                    placedIncorrectly = true;
                }
            }
        }
        if (resolution.successfullyPlacedCable)
        {
            if (buildLocations[currentTutorialState].z > 0)
            {
                float distance = (resolution.builtCable.Start.transform.position - new Vector3(buildLocations[currentTutorialState].x, buildLocations[currentTutorialState].y)).magnitude;
                if (distance > buildLocations[currentTutorialState].z)
                {
                    resolution.successfullyPlacedCable = false;
                    resolution.totalCost -= Mathf.CeilToInt(resolution.builtCable.Length * Data.cableCostMultiplier);
                    Destroy(resolution.builtBuilding.gameObject);
                    resolution.builtBuilding = null;
                    placedIncorrectly = true;
                }
                else
                {
                    distance = (resolution.builtCable.End.transform.position - new Vector3(buildLocations[currentTutorialState].x, buildLocations[currentTutorialState].y)).magnitude;
                    if (distance > buildLocations[currentTutorialState].z)
                    {
                        resolution.successfullyPlacedCable = false;
                        resolution.totalCost -= Mathf.CeilToInt(resolution.builtCable.Length * Data.cableCostMultiplier);
                        Destroy(resolution.builtCable.gameObject);
                        resolution.builtCable = null;
                        placedIncorrectly = true;
                    }
                }
            }
        }

        if(placedIncorrectly)
        {
            BuildManagerComponent.Instance.SetNoneState();
            if (cameraFocuses[currentTutorialState].z > 0)
            {
                FocusCamera(cameraFocuses[currentTutorialState]);
            }
        }

        base.BuildManager_OnBuildResovle(resolution);

        if (resolution.successfullyPlacedBuilding)
        {
            if (stateChangeConditions[currentTutorialState] == TutorialStateChangeCondition.building)
            {
                advanceAtEndOfNextUpdate = true;
            }
            UpdateBuildingResources();
        }

        if (resolution.successfullyPlacedCable)
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
