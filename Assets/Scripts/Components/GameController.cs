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
	[SerializeField] private AudioSource sfxAudio;
	[SerializeField] private AudioClip buildClip;
	[SerializeField] private AudioClip demolishClip;
	[SerializeField] private AudioClip cableConnectClip;
	[SerializeField] private DataSet gameDataSet;
	[SerializeField] private GameObject victoryDocument;
	[SerializeField] private GameObject defeatDocument;


    private Label cashLabel;
    private Label incomeLabel;
	private Label scienceLabel;
	private Label scienceIncomeLabel;
	private Label timeLabel;
	protected int gameSpeed;
	private float endGameTime = 0;
	private bool gameEnded = false;

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
	public int HeldScience
    {
        get { return (gameManager.ScienceHeld); }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		gameSpeed = 1;
        gameManager = new GameManager(this);
        cashLabel = buildDocument.rootVisualElement.Q("Cash") as Label;
        incomeLabel = buildDocument.rootVisualElement.Q("Income") as Label;
		scienceLabel = buildDocument.rootVisualElement.Q("Science") as Label;
		scienceIncomeLabel = buildDocument.rootVisualElement.Q("ScienceIncome") as Label;
		timeLabel = buildDocument.rootVisualElement.Q("Time") as Label;
		if(Data.selectedMission == null)
		{
			Data.selectedMission = gameDataSet.missionDatas[0];
		}
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
	protected virtual void Update()
    {
		if (gameEnded)
		{
			endGameTime += Time.deltaTime;
			if (endGameTime > 4 || Input.GetKeyDown(KeyCode.Return))
			{
				ReturnToMenu();
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Equals) && gameSpeed < 5)
			{
				++gameSpeed;
			}
			if (Input.GetKeyDown(KeyCode.Minus) && gameSpeed > 1)
			{
				--gameSpeed;
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				gameManager.CalculateIncome();
			}
			gameManager.Update(Time.deltaTime * gameSpeed);

			string timeText = "X" + gameSpeed + "     ";
			timeText += Mathf.FloorToInt(gameManager.TimePassed / 60);
			timeText += ":" + (Mathf.FloorToInt((gameManager.TimePassed % 60)));
			timeLabel.text = timeText;
		}
	}

    void FixedUpdate()
    {
		if (!gameEnded)
		{
			MovePlanets();
		}
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
				float planetMass = planet.GetTotalMass() / 25f;
				for (int p = 0; p < Planets.Count; ++p)
				{
					if (p != i)
					{
						PlanetComponent other = Planets[p];
						Vector2 distance = planet.transform.position - other.transform.position;
						if (distance.magnitude < planetMass)
						{
							planetTranslations[p] += distance.normalized * planetMass / distance.magnitude / other.GetTotalMass() * Time.fixedDeltaTime * gameSpeed;
						}
					}
				}
				for(int c = 0; c < planet.BuildingContainer.childCount; ++c)
                {
					BuildingComponent building = planet.BuildingContainer.GetChild(c).gameObject.GetComponent<BuildingComponent>();
					if(building != null && building.BackendBuilding.IsPowered &&  building.Data.thrust != 0)
                    {
						Vector3 movement = building.transform.up.normalized * building.Data.thrust / planetMass * Time.fixedDeltaTime * gameSpeed * -1;
						planetTranslations[i] += new Vector2(movement.x, movement.y);
                    }
                }
			}
			for (int i = 0; i < Planets.Count; ++i)
			{
				if(planetTranslations[i].magnitude < 0.00000001f && Planets[i].PlanetVelocity.magnitude > 0.0001f)
                {
					planetTranslations[i] = Planets[i].PlanetVelocity * -1;
					if(planetTranslations[i].magnitude > Time.fixedDeltaTime)
                    {
						planetTranslations[i] = planetTranslations[i].normalized * Time.fixedDeltaTime;
                    }
                }
				Rigidbody2D body = Planets[i].gameObject.GetComponent<Rigidbody2D>();
				if (body != null)
                {
					Planets[i].PlanetVelocity += planetTranslations[i];
                    body.MovePosition(body.position + (Planets[i].PlanetVelocity * Time.fixedDeltaTime));
                }
			}
			UpdatePlanetsSolar();
			CheckCableSnap();
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
	public void UpdateScienceLabels(int newScience, int newIncome)
	{
		scienceLabel.text = "" + newScience;
		scienceIncomeLabel.text = "(";
		if (newIncome > 0)
		{
			scienceIncomeLabel.text += "+";
			scienceIncomeLabel.style.color = new StyleColor(new Color(0, 0.9f, 0));
		}
		else if (newIncome == 0)
		{
			scienceIncomeLabel.style.color = new StyleColor(new Color(1, 1, 1));
		}
		else
		{
			scienceIncomeLabel.style.color = new StyleColor(new Color(0.9f, 0, 0));
		}
		scienceIncomeLabel.text += newIncome + ")";
	}

    public void EndGame(bool victory)
    {
        Debug.Log("Game has ended");
		gameEnded = true;
		if(victory)
        {
			if (victoryDocument != null)
			{
				victoryDocument.SetActive(true);
			}
        }
		else
        {
			if (defeatDocument != null)
			{
				defeatDocument.SetActive(true);
			}
        }
    }

	public void ReturnToMenu()
    {
		SceneManager.LoadScene("MainMenu");
    }

	public virtual void BuildManager_OnBuildResovle(BuildResolve resolution)
	{
		if (resolution.successfullyPlacedBuilding)
		{
			RegisterBuilding(resolution.builtBuilding);
			gameManager.SpendMoney(resolution.builtBuilding.Data.cost);
			gameManager.SpendScience(resolution.builtBuilding.Data.scienceCost);
			sfxAudio.clip = buildClip;
			sfxAudio.Play();
		}

		if (resolution.successfullyPlacedCable)
		{
			RegisterCable(resolution.builtCable);
			gameManager.SpendMoney(Mathf.CeilToInt(resolution.builtCable.Length));
			if(!sfxAudio.isPlaying)
			{
                sfxAudio.clip = cableConnectClip;
                sfxAudio.Play();
            }
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
		if (sfxAudio != null && sfxAudio.gameObject != null)
		{
			sfxAudio.clip = demolishClip;
			sfxAudio.Play();
		}
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
        if (sfxAudio != null && sfxAudio.gameObject != null)
        {
            sfxAudio.clip = demolishClip;
            sfxAudio.Play();
        }
    }

	private void RegisterPlanet(PlanetComponent planetComponent)
	{
		Planets.Add(planetComponent);

		planetComponent.OnPlanetDestroyed.AddListener(Planet_OnDestroyed);
	}

    protected virtual void Planet_OnDestroyed(PlanetComponent planetComponent)
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
		UpdateBuildingResources();
    }

	public void UpdateBuildingResources()
	{
		for (int i = 0; i < Planets.Count; ++i)
		{
			PlanetComponent planet = Planets[i];
			int[] totalResources = new int[(int)ResourceType.Resource_Count];
			for(int r = 0; r < totalResources.Length; ++r)
            {
				totalResources[r] = planet.GetResourceCount((ResourceType)r);
            }				
			for(int b = 0; b < planet.BuildingContainer.childCount; ++b)
            {
				BuildingComponent building = planet.BuildingContainer.GetChild(b).gameObject.GetComponent<BuildingComponent>();
				if(building != null)
                {
					if(building.Data.requiredResource != ResourceType.Resource_Count)
                    {
						building.BackendBuilding.ResourcesProvided = Mathf.Min(totalResources[(int)building.Data.requiredResource], building.BackendBuilding.Data.resourceAmountRequired);
						totalResources[(int)building.Data.requiredResource] = Mathf.Max(0, totalResources[(int)building.Data.requiredResource] - building.BackendBuilding.Data.resourceAmountRequired);
                    }
                }
            }
		}
	}

	public void CheckCableSnap()
    {
		for(int i = 0; i < Cables.Count; ++i)
        {
			if(Cables[i].Length > GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength)
            {
                Destroy(Cables[i].gameObject);
            }
			else
			{
                CableComponent.GetBoxFromPoints(
                    Cables[i].End.CableConnectionTransform.position,
                    Cables[i].Start.CableConnectionTransform.position,
                    out Vector2 center,
                    out Vector2 size,
                    out float angle
                    );

                List<Collider2D> cableOverlaps = new List<Collider2D>(Physics2D.OverlapBoxAll(center, size, angle));
                int badOverlapIndex = cableOverlaps.FindIndex((Collider2D collider) =>
                {
                    return !IsValidCableOverlap(collider, Cables[i].Start, Cables[i].End);
                });
                bool noOverlapsAlongCable = badOverlapIndex == -1;
				if(noOverlapsAlongCable)
                {
					Cables[i].CableOverlapTime = 0;
                }
				else
				{
					Cables[i].CableOverlapTime += Time.fixedDeltaTime;
					if (Cables[i].CableOverlapTime > 0.5f)
					{
						Destroy(Cables[i].gameObject);
					}
				}
            }
        }
    }

    /// <summary>
    /// Determines whether a cable can overlap over the given collider.
    /// Cables can only overlap over buildings that they connect to or other
    /// Cables that share the same builing connections.
    /// </summary>
    private bool IsValidCableOverlap(Collider2D overlapping, BuildingComponent startBuilding, BuildingComponent endBuildling)
    {
        if (overlapping.TryGetComponentInParent(out BuildingComponent overlapBuilding))
        {
            return (overlapBuilding == startBuilding) || (overlapBuilding == endBuildling);
        }

        if (overlapping.TryGetComponentInParent(out CableComponent overlapCable))
        {
            bool startIsConnectingBuilding = (overlapCable.Start == startBuilding) || (overlapCable.Start == endBuildling);
            bool endIsConnectingBuilding = (overlapCable.End == startBuilding) || (overlapCable.End == endBuildling);

            return startIsConnectingBuilding || endIsConnectingBuilding;
        }

        // TODO: Detect other cables?
        return false;
    }
}
