using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Collections.Generic;
using werignac.Utils;
using System;

public class GameController : MonoBehaviour
{
    private GameObject levelObject;
    private GameManager gameManager;
    [SerializeField] private UIDocument buildDocument;

    private Label cashLabel;
    private Label incomeLabel;

	[HideInInspector] public UnityEvent OnLevelLoad = new UnityEvent();

	// Refercnes to in-game objects.
	public List<PlanetComponent> Planets { get; private set; } = new List<PlanetComponent>();
	public List<BuildingComponent> Buildings { get; private set; } = new List<BuildingComponent>();
	public List<CableComponent> Cables { get; private set; } = new List<CableComponent>();

    public int Cash
    {
        get { return (gameManager.Cash); }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = new GameManager(this);
        cashLabel = buildDocument.rootVisualElement.Q("Cash") as Label;
        incomeLabel = buildDocument.rootVisualElement.Q("Income") as Label;
        if (Data.selectedMission != null)
        {
            levelObject = Instantiate<GameObject>(Resources.Load<GameObject>("Levels/" + Data.selectedMission.name));
            gameManager.StartMission(Data.selectedMission);

			CollectInitialGameObjects();

			OnLevelLoad?.Invoke();
        }
    }

	private void CollectInitialGameObjects()
	{
		Planets = WerignacUtils.GetComponentsInActiveScene<PlanetComponent>();
		
		foreach (BuildingComponent buildingComponent in WerignacUtils.GetComponentsInActiveScene<BuildingComponent>())
		{
			RegisterBuilding(buildingComponent);
		}

		foreach(CableComponent cableComponent in WerignacUtils.GetComponentsInActiveScene<CableComponent>())
		{
			RegisterCable(cableComponent);
		}
	}

	// Update is called once per frame
	void Update()
    {
        gameManager.Update(Time.deltaTime);
    }

    public void UpdateCashAndIncome(int newCash, int newIncome)
    {
        cashLabel.text = "" + newCash;
        incomeLabel.text = "(";
        if(newIncome > 0)
        {
            incomeLabel.text += "+";
            incomeLabel.style.color = new StyleColor(new Color(0, 0.9f, 0));
        }
        else if(newIncome == 0)
        {
            incomeLabel.style.color = new StyleColor(new Color(1, 1, 1));
        }
        else
        {
            incomeLabel.style.color = new StyleColor(new Color(0.9f, 0, 0));
        }
        incomeLabel.text += newIncome + ")";
        
    }

    public void EndGame()
    {
        Debug.Log("Game has ended");
        SceneManager.LoadScene("MainMenu");
    }

	public void BuildManager_OnBuildResovle(BuildResolve resolution)
	{
		if (resolution.successfullyPlacedBuilding)
		{
			RegisterBuilding(resolution.builtBuilding);
			gameManager.SpendMoney(resolution.builtBuilding.Data.cost);
		}

		if (resolution.successfullyPlacedCable)
		{
			RegisterCable(resolution.builtCable);
			gameManager.SpendMoney(Mathf.CeilToInt(resolution.builtCable.Length));
		}
	}

	private void RegisterBuilding(BuildingComponent buildingComponent)
	{
		Buildings.Add(buildingComponent);
		
		Building building = new Building(buildingComponent.Data);
		buildingComponent.SetGameBuilding(building);
		gameManager.AddBuilding(building);

		buildingComponent.OnBuildingDestroyed.AddListener(Building_OnDestroy);
	}

	private void Building_OnDestroy(BuildingComponent buildingComponent)
	{
		Buildings.Remove(buildingComponent);
		gameManager.RemoveBuilding(buildingComponent.BackendBuilding);
	}

	private void RegisterCable(CableComponent cableComponent)
	{
		Cables.Add(cableComponent);

		gameManager.AddConnection(cableComponent.Start.BackendBuilding, cableComponent.End.BackendBuilding);

		cableComponent.OnCableDestroyed.AddListener(Cable_OnDestroyed);
	}

	private void Cable_OnDestroyed(CableComponent cableComponent)
	{
		Cables.Remove(cableComponent);
		gameManager.RemoveConnection(cableComponent.Start.BackendBuilding, cableComponent.End.BackendBuilding);
	}
}
