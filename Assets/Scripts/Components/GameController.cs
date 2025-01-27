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

	// Getters
	public Vector2 LevelBounds { get; private set; }

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

			LevelBounds = levelObject.GetComponent<LevelBuilder>().levelDimentions;

			CollectInitialGameObjects();

			OnLevelLoad?.Invoke();
        }
    }

	private void CollectInitialGameObjects()
	{
		foreach(PlanetComponent planet in WerignacUtils.GetComponentsInActiveScene<PlanetComponent>())
		{
			RegisterPlanet(planet);
		}
		
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

    void FixedUpdate()
    {
        MovePlanets();
    }

    private void MovePlanets()
	{
		if (Planets.Count > 1)
		{
			List<Vector2> planetTranslations = new List<Vector2>();
			for (int i = 0; i < Planets.Count; ++i)
			{
				planetTranslations.Add(new Vector2(0, 0));
			}
			for (int i = 0; i < Planets.Count; ++i)
			{
				PlanetComponent planet = Planets[i];
				float planetMass = planet.GetTotalMass() / 100f;
				for (int p = 0; p < Planets.Count; ++p)
				{
					if (p != i)
					{
						PlanetComponent other = Planets[p];
						Vector2 distance = planet.transform.position - other.transform.position;
						if (distance.magnitude < planetMass)
						{
							planetTranslations[p] += distance.normalized * planetMass / distance.magnitude * Time.fixedDeltaTime;
						}
					}
				}
			}
			for (int i = 0; i < Planets.Count; ++i)
			{
				Rigidbody2D body = Planets[i].gameObject.GetComponent<Rigidbody2D>();
				if (body != null)
                {
                    body.MovePosition(body.position + planetTranslations[i]);
                }
			}
		}
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

	private void RegisterPlanet(PlanetComponent planetComponent)
	{
		Planets.Add(planetComponent);

		planetComponent.OnPlanetDestroyed.AddListener(Planet_OnDestroyed);
	}

    private void Planet_OnDestroyed(PlanetComponent planetComponent)
    {
        Planets.Remove(planetComponent);
    }

    public void UpdatePlanetsSolar()
    {
		for(int i = 0; i < Planets.Count; ++i)
        {
			PlanetComponent planet = Planets[i];
			int solarAmount = planet.SolarOutput;
			for(int p = 0; p < Planets.Count; ++p)
            {
				if(p != i)
                {
					PlanetComponent other = Planets[p];
					solarAmount += Mathf.Max(0, Mathf.CeilToInt(other.SolarOutput - Vector2.Distance(planet.transform.position, other.transform.position)));
                }
            }
			planet.SetResourceCount(ResourceType.Solar, solarAmount);
        }
    }
}
