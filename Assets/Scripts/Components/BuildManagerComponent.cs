using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using werignac.Utils;

public enum BuildStateType
{
	NONE = 0,
	BUILDING = 1,
	CABLE = 2,
	BUILDING_CHAINED = 3,
	DEMOLISH = 4
}

public interface BuildState { public BuildStateType GetStateType(); }
public class NoneBuildState : BuildState { public BuildStateType GetStateType() => BuildStateType.NONE; }
public class BuildingBuildState : BuildState, IInspectable
{
	public readonly BuildingSettingEntry toBuild;

	public BuildingBuildState(BuildingSettingEntry toBuild)
	{
		this.toBuild = toBuild;
	}

	public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
	{
		inspectorController = new BuildingButtonInspectorController(this.toBuild);
		return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
	}

	public virtual BuildStateType GetStateType()
	{
		return BuildStateType.BUILDING;
	}
}
public class BuildingChainedBuildState : BuildingBuildState
{
	public readonly BuildingComponent fromChained;

	/// <summary>
	/// Invoked when the chained building is destroyed
	/// while in this state.
	/// </summary>
	public UnityEvent<BuildingChainedBuildState> OnInvalidate = new UnityEvent<BuildingChainedBuildState>();

	public BuildingChainedBuildState(BuildingSettingEntry toBuild, BuildingComponent fromChained) :
		base(toBuild)
	{
		this.fromChained = fromChained;

		// When the chained object is destroyed, invoke the event
		// to change the build state.
		fromChained.OnBuildingDestroyed.AddListener(
			(BuildingComponent _) => { OnInvalidate.Invoke(this); }
		);
	}

	public override BuildStateType GetStateType()
	{
		return BuildStateType.BUILDING_CHAINED;
	}

	/// <summary>
	/// Converts this BuildingChainedBuildState to a BuildingBuildState.
	/// Used for when the build state is invalidated.
	/// </summary>
	/// <returns>A build state without the chaining.</returns>
	public BuildingBuildState ToBuildingBuildState()
	{
		return new BuildingBuildState(toBuild);
	}
}
class CableBuildState : BuildState {

	public readonly BuildingComponent fromBuilding = null;
	public BuildingComponent toBuilding = null;

	// Events
	public UnityEvent<CableBuildState> OnInvalidateFrom = new UnityEvent<CableBuildState>();
	public UnityEvent<CableBuildState> OnInvalidateTo = new UnityEvent<CableBuildState>();

	public CableBuildState() { }

	public CableBuildState(BuildingComponent fromBuilding)
	{
		this.fromBuilding = fromBuilding;

		fromBuilding.OnBuildingDestroyed.AddListener((BuildingComponent _) =>
		{
			OnInvalidateFrom?.Invoke(this);
		});
	}

	public BuildStateType GetStateType() => BuildStateType.CABLE;

	private void ToBuilding_OnDestroy(BuildingComponent _)
	{
		OnInvalidateTo?.Invoke(this);
	}

	public void SetToBuilding(BuildingComponent toBuilding)
	{
		if (this.toBuilding != null)
			this.toBuilding.OnBuildingDestroyed.RemoveListener(ToBuilding_OnDestroy);

		this.toBuilding = toBuilding;

		if (this.toBuilding != null)
			this.toBuilding.OnBuildingDestroyed.AddListener(ToBuilding_OnDestroy);
	}

	/// <summary>
	/// Create a build state used for chaining cables together.
	/// </summary>
	public CableBuildState ChainBuildState()
	{
		return new CableBuildState(toBuilding);
	}
}
class DemolishBuildState : BuildState {

	// Can be null if not hovering over anything.
	private IDemolishable hoveringDemolishable = null;

	public BuildStateType GetStateType() => BuildStateType.DEMOLISH;

	/// <summary>
	/// Stops and starts hovering VFX based on the passed
	/// hovering object.
	/// </summary>
	/// <param name="newHoveringDemolishable">Object that the player is hovering over that can be demolished. Null
	/// if the player isn't hovering over anything.</param>
	public void SetHoveringDemolishable(IDemolishable newHoveringDemolishable)
	{
		// We're hoving over the same object, no need to call methods for changing hover status.
		if (hoveringDemolishable == newHoveringDemolishable)
			return;

		if (hoveringDemolishable != null)
			hoveringDemolishable.HoverDemolishEnd();

		hoveringDemolishable = newHoveringDemolishable;

		if (newHoveringDemolishable != null)
			newHoveringDemolishable.HoverDemolishStart();
	}

	/// <summary>
	/// Call when the state switched from out of a demolish state.
	/// Stops all hovering VFX.
	/// </summary>
	public void StopLingeringHovering()
	{
		if (hoveringDemolishable != null)
			hoveringDemolishable.HoverDemolishEnd();
	}
}

public struct BuildResolve
{
	public bool triedPlacingBuilding;
	public bool successfullyPlacedBuilding;
	public BuildingComponent builtBuilding;
	public bool triedPlacingCable;
	public bool successfullyPlacedCable;
	public bool successfullyChoseCableStart;
	public CableComponent builtCable;
	public bool triedDemolishBuilding;
	// Note: This may not be the only thing that is destroyed in a demolition!
	public IDemolishable demolishTarget;
	public int totalCost;

	public bool TriedAnything()
	{
		return triedPlacingBuilding || triedPlacingCable || triedDemolishBuilding;
	}
}

/// <summary>
/// Singleton component that manages placing buildings.
/// </summary>
public class BuildManagerComponent : MonoBehaviour
{
	public static BuildManagerComponent Instance { get; private set;}

	[SerializeField] private GameController gameController;

	// Reference to the cursor used for showing where buildings will go.
	// Also checks whether a building can be placed (collides with other buildings).
	[SerializeField] private BuildingCursorComponent buildingCursor;
	[SerializeField] private CableCursorComponent cableCursor;
	[SerializeField] private SelectionCursorComponent selectionCursor;
	// How close the mouse has to be to a planet's surface for the build
	// cursor to show up.
	[SerializeField, Min(0.001f)] private float minDistanceToPlanetToShowCursor;

	// The current state of building manager.
	private BuildState state;

	// Events
	// Invoked when the build state changes. Passes the old state and the new state.
	[HideInInspector] public UnityEvent<BuildState, BuildState> OnStateChanged = new UnityEvent<BuildState, BuildState>();
	// Pass information about how the build resolved. e.g. built building, but failed to connect cable
	// build cable but didn't create a new building, build both a building and cable, didn't ask to build a cable, etc.
	// Though information about demolitions are sent, using it is not recommended. Instead listen to destroy
	// events for buildings and cables.
	[HideInInspector] public UnityEvent<BuildResolve> OnBuildResolve = new UnityEvent<BuildResolve>();


	// True when the player asked to place something this update.
	private bool placeThisUpdate = false;

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

		OnBuildResolve.AddListener(gameController.BuildManager_OnBuildResovle);
	}

	public void SetNoneState()
	{
		SetState(new NoneBuildState());
	}

	/// <summary>
	/// Set the BuildManager to allow the player to demolish objects.
	/// </summary>
	public void SetDemolishState()
	{
		SetState(new DemolishBuildState());
	}

	/// <summary>
	/// Set the BuildManager to allow the player to build the specified building.
	/// </summary>
	/// <param name="toBuild">The building to build.</param>
	public void SetBuildState(BuildingSettingEntry toBuild)
	{
		switch(state.GetStateType())
		{
			case BuildStateType.CABLE: // Try chaining from the last building with a cable.
				CableBuildState cableBuildState = state as CableBuildState;
				if (cableBuildState != null && cableBuildState.fromBuilding != null)
					SetState(new BuildingChainedBuildState(toBuild, cableBuildState.fromBuilding));
				else
					SetState(new BuildingBuildState(toBuild));
				break;
			case BuildStateType.BUILDING_CHAINED: // Continue chaining.
				BuildingChainedBuildState buildingChainedBuildState = state as BuildingChainedBuildState;
				SetState(new BuildingChainedBuildState(toBuild, buildingChainedBuildState.fromChained));
				break;
			default:
				SetState(new BuildingBuildState(toBuild));
				break;
		}
	}

	/// <summary>
	/// Set the BuildManager to allow the player to build cables.
	/// </summary>
	public void SetCableState()
	{
		switch(state.GetStateType())
		{
			case BuildStateType.BUILDING_CHAINED:
				BuildingChainedBuildState buildingChainedBuildState = state as BuildingChainedBuildState;
				SetState(new CableBuildState(buildingChainedBuildState.fromChained));
				break;
			default:
				SetState(new CableBuildState());
				break;
		}
	}

	/// <summary>
	/// Internally changes the state of the build manager.
	/// Handles cleanup for the previous state and initialization for the new state.
	/// </summary>
	/// <param name="newState"></param>
	private void SetState(BuildState newState)
	{
		BuildState oldState = state;

		if (oldState != null)
		{
			// Stop listening for if the chain becomes invalidated.
			if (oldState.GetStateType() == BuildStateType.BUILDING_CHAINED)
				(oldState as BuildingChainedBuildState).OnInvalidate.RemoveListener(BuildingChainedBuildState_OnInvalidate);
			if (oldState.GetStateType() == BuildStateType.CABLE)
				(oldState as CableBuildState).OnInvalidateFrom.RemoveListener(CableBuildState_OnInvalidateFrom);
			// Stop lingering VFX from hovering.
			if (oldState.GetStateType() == BuildStateType.DEMOLISH)
				(oldState as DemolishBuildState).StopLingeringHovering();
		}

		state = newState;

		// Listen for if the chain becomes invalidated.
		if (newState.GetStateType() == BuildStateType.BUILDING_CHAINED)
			(newState as BuildingChainedBuildState).OnInvalidate.AddListener(BuildingChainedBuildState_OnInvalidate);
		if (newState.GetStateType() == BuildStateType.CABLE)
			(newState as CableBuildState).OnInvalidateFrom.AddListener(CableBuildState_OnInvalidateFrom);

		SetUpCursorForBuildState(newState);

		OnStateChanged?.Invoke(oldState, newState);
	}

	/// <summary>
	/// Set the building cursor dimensions and graphic.
	/// Set the cable cursor to be attached to a start building.
	/// </summary>
	private void SetUpCursorForBuildState(BuildState newBuildState)
	{
		// Set up the cursor for the building.
		if ((newBuildState.GetStateType() & BuildStateType.BUILDING) != 0)
		{
			BuildingBuildState buildingBuildState = newBuildState as BuildingBuildState;
			BuildingVisuals visualAsset = buildingBuildState.toBuild.VisualAsset;

			Sprite ghostSprite = visualAsset.buildingGhost;
			Vector2 ghostOffset = visualAsset.ghostOffset;
			float ghostScale = visualAsset.ghostScale;

			buildingCursor.SetGhost(ghostSprite, ghostOffset, ghostScale);
					
			BuildingComponent buildingComponent = buildingBuildState.toBuild.Prefab.GetComponent<BuildingComponent>();

			Vector2 colliderSize = buildingComponent.ColliderSize;
			Vector2 colliderOffset = buildingComponent.ColliderOffset;

			buildingCursor.SetBuildingCollision(colliderSize, colliderOffset);

			Vector2 cableConnectionOffset = buildingComponent.CableConnectionOffset;

			buildingCursor.SetBuildingCableConnectionOffset(cableConnectionOffset);
		}

		// Set up the cursor for the cable.
		if ((newBuildState.GetStateType() & BuildStateType.CABLE) != 0)
		{
			switch(newBuildState.GetStateType())
			{
				case BuildStateType.CABLE:
					{
						CableBuildState cableBuildState = state as CableBuildState;
						cableCursor.SetStart(cableBuildState.fromBuilding);
						cableCursor.SetEndBuilding(null);
					}
					break;
				case BuildStateType.BUILDING_CHAINED:
					{
						BuildingChainedBuildState buildingChainedBuildState = state as BuildingChainedBuildState;
						cableCursor.SetStart(buildingChainedBuildState.fromChained);
						cableCursor.SetEndBuilding(null);
					}
					break;
			}
		}
	}

	/// <summary>
	/// Get whether the player is currently using the BuildManager.
	/// This can either be for building or demolishing.
	/// </summary>
	public bool IsInBuildState()
	{
		return state.GetStateType() != BuildStateType.NONE;
	}

	/// <summary>
	/// When the buildingChainedBuildState becomes invalidated,
	/// switch to a normal build state.
	/// </summary>
	private void BuildingChainedBuildState_OnInvalidate(BuildingChainedBuildState state)
	{
		SetState(state.ToBuildingBuildState());
	}

	/// <summary>
	/// When the start building of a cable is destroyed while placing a new cable,
	/// stay in the cable state, but make the player create a new cable from scratch.
	/// </summary>
	private void CableBuildState_OnInvalidateFrom(CableBuildState state)
	{
		SetState(new CableBuildState());
	}

	/// <summary>
	/// Tells the BuildManagerComponent to try placing whatever it has.
	/// Also tells the BuildManager to demolish if it's in demolish mode.
	/// </summary>
	public void SetPlace()
	{
		placeThisUpdate = true;
	}

	private void Update()
	{
		// If we aren't building anything...
		if (!IsInBuildState())
		{
			// Don't process any input.
			placeThisUpdate = false;
			
			// Hide all the cursors if they're not already hidden.
			if (buildingCursor.GetIsShowing())
			{
				buildingCursor.Hide();
			}

			if (cableCursor.GetIsShowing())
			{
				cableCursor.Hide();
			}
			
			return;
		}

		// Resolve status. Fill this in as attempts to make or demolish buildings are
		// completed.
		BuildResolve resolution = new BuildResolve
		{
			triedPlacingBuilding = false,
			successfullyPlacedBuilding = false,
			builtBuilding = null,
			triedPlacingCable = false,
			successfullyPlacedCable = false,
			successfullyChoseCableStart = false,
			builtCable = null,
			triedDemolishBuilding = false,
			demolishTarget = null,
			totalCost = 0
		};

		// TODO: Keep track of cumulative costs.
		// May need to pay for both cable and building.

		// Try to place a building. Either in BUILDING or BUILDING_CHAINED state.
		if ((state.GetStateType() & BuildStateType.BUILDING) != 0)
			resolution = UpdateBuildingCursor(resolution);
		else
		{
			if (buildingCursor.GetIsShowing())
			{
				buildingCursor.Hide();
			}
		}

		// Try to place a cable. Either in CABLE or BUILDING_CHAINED state.
		if ((state.GetStateType() & BuildStateType.CABLE) != 0)
		{
			resolution = UpdateCableCursor(resolution);
		}
		else
		{
			if (cableCursor.GetIsShowing())
			{
				cableCursor.Hide();
			}
		}

		// Try to demolish a building.
		if (state.GetStateType() == BuildStateType.DEMOLISH)
		{
			resolution = UpdateDemolish(resolution);
		}

		// Handle state changes after placing and notify of object placement.
		if (placeThisUpdate)
		{
			// If the player clicked, but it was clear the player wasn't trying to do anything
			// (e.g. clicking on empty space) then, switch to no build state.
			if (!resolution.TriedAnything())
				SetState(new NoneBuildState());
			else
			{
				// If we made a building, or made a cable but also tried making a building, set the state to chain a new building.
				if ((state.GetStateType() & BuildStateType.BUILDING) != 0)
				{
					BuildingBuildState buildingBuildState = state as BuildingBuildState;

					if (resolution.successfullyPlacedBuilding)
					{
						SetState(new BuildingChainedBuildState(buildingBuildState.toBuild, resolution.builtBuilding));
					}
					else if (resolution.successfullyPlacedCable)
					{
						// Get the "To" building of the cable. Use that in the BuildingChainedBuildState.
						SetState(new BuildingChainedBuildState(buildingBuildState.toBuild, resolution.builtCable.End));
					}
				}
				
				// If we are in cable mode and we tried to make a complete cable (even if the cable
				// was invalid) set the state to chain a new cable.
				if (state.GetStateType() == BuildStateType.CABLE && !resolution.successfullyChoseCableStart)
				{
					CableBuildState cableBuildState = state as CableBuildState;
					if (cableBuildState.toBuilding != null)
						SetState(cableBuildState.ChainBuildState());
					else
						SetState(new CableBuildState());
				}

				// Notify others that the player tried to build or demolish something, and specify what they built.
				OnBuildResolve?.Invoke(resolution);
			}
		}

		// Reset place signal.
		placeThisUpdate = false;
	}

	private BuildResolve UpdateDemolish(BuildResolve resolution)
	{
		// Only called when we're in the demolish state.
		DemolishBuildState demolishBuildState = state as DemolishBuildState;

		// See if the mouse is hovering over an IDemolishable.
		List<Collider2D> allHoveringColliders = new List<Collider2D>(selectionCursor.GetHovering());
		Collider2D idemolishableCollider = allHoveringColliders.Find((Collider2D collider) =>
		{
			return collider.TryGetComponentInParent(out IDemolishable demoComponent) && demoComponent.Demolishable();
		});

		// Get the IDemolishable component if there is one.
		IDemolishable hoveringDemolishable = idemolishableCollider == null ? null : idemolishableCollider.GetComponentInParent<IDemolishable>();

		// Update calls to hovering demolishable for VFX.
		demolishBuildState.SetHoveringDemolishable(hoveringDemolishable);

		// If the player clicked, demolish the building
		if (placeThisUpdate && hoveringDemolishable != null)
		{
			resolution.triedDemolishBuilding = true;

			hoveringDemolishable.Demolish();
			
			resolution.demolishTarget = hoveringDemolishable;
		}

		return resolution;
	}

	/// <summary>
	/// Update the status of the cursor to refelct whether a building
	/// can be placed.
	/// Try placing a building if input to do so was sent.
	/// </summary>
	private BuildResolve UpdateBuildingCursor(BuildResolve resolution)
	{
		UpdateBuildingCursorLocation();

		if (buildingCursor.GetIsShowing())
		{
			// The player is trying to build something.
			BuildingBuildState buildingBuildState = state as BuildingBuildState;

			// TODO: Check if we're hovering over a building (saved in the buildingBuildState). If so,
			// replace that one instead of building a new building.

			// Determine whether the building can be placed.
			Collider2D[] overlappingColliders = buildingCursor.QueryOverlappingColliders();

			// The only thing that the building should be colliding with is the parent planet.
			bool roomToPlace = overlappingColliders.Length == 1 && buildingCursor.ParentPlanet.OwnsCollider(overlappingColliders[0]);

			bool sufficientFunds = gameController.Cash >= (resolution.totalCost + buildingBuildState.toBuild.BuildingDataAsset.cost) && gameController.HeldScience >= buildingBuildState.toBuild.BuildingDataAsset.scienceCost;
			
			bool sufficientResources = true;
            /*if(buildingBuildState.toBuild.BuildingDataAsset.requiredResource != ResourceType.Resource_Count)
            {
				if(buildingCursor.ParentPlanet.GetResourceCount(buildingBuildState.toBuild.BuildingDataAsset.requiredResource) <= 0 
					|| buildingCursor.ParentPlanet.GetResourceCount(buildingBuildState.toBuild.BuildingDataAsset.requiredResource) < buildingBuildState.toBuild.BuildingDataAsset.resourceAmountRequired)
                {
					sufficientResources = false;
                }
            }*/

			bool canPlace = roomToPlace && sufficientFunds && sufficientResources && buildingCursor.ParentPlanet.CanPlaceBuildings;

			if(canPlace)
            {
				resolution.totalCost = resolution.totalCost + buildingBuildState.toBuild.BuildingDataAsset.cost;
            }

			// Update the cursor graphic.
			buildingCursor.SetBuildingPlaceability(canPlace);

			// TODO: Update the reason as to why the building cannot be placed.

			// If input was received to place the building, place it.
			if (placeThisUpdate)
			{
				resolution.triedPlacingBuilding = true;

				if (canPlace)
				{
					//Building building = new Building { Data = buildingBuildState.toBuild.BuildingDataAsset };
					// TODO: Add to game manager.

					GameObject buildingGameObject = buildingCursor.PlaceBuildingAtLocation(buildingBuildState.toBuild.Prefab, true);

					resolution.builtBuilding = buildingGameObject.GetComponent<BuildingComponent>();
					resolution.successfullyPlacedBuilding = true;
				}
				else
				{
					// Note in build resolve that the building failed to be placed.
					resolution.successfullyPlacedBuilding = false;
				}
			}
		}

		return resolution;
	}

	/// <summary>
	/// Shows and places the building cursor to snap onto a planet.
	/// If the cursor is out of all planets' reaches, hide the cursor.
	/// </summary>
	private void UpdateBuildingCursorLocation()
	{
		// TODO: Check if we're hovering over a building. If so, hide the cursor?

		bool noPlanets = gameController.Planets.Count == 0;
		if (noPlanets)
		{ // If there are no planets, there's nothing to build on.
			if (! buildingCursor.GetIsShowing())
			{
				buildingCursor.Hide();
				return;
			}
		}

		// Find the planet that has the point on its surface that is the closest to the mouse.
		float closestPlanetDistance = -1f;
		PlanetComponent closestPlanet = null;

		foreach (PlanetComponent planet in gameController.Planets)
		{
			float distanceToHover = Vector2.Distance(planet.GetClosestSurfacePointToPosition(selectionCursor.GetPosition()), selectionCursor.GetPosition());

			// If this is the first planet, assume it's the closest.
			// Otherwise, store the new closest.
			if (closestPlanetDistance < 0 || distanceToHover < closestPlanetDistance)
			{
				closestPlanetDistance = distanceToHover;
				closestPlanet = planet;
				continue;
			}
		}

		// Check that the closestPlanetDistance is  below a certain threshold. If not, hide.
		if (closestPlanetDistance > minDistanceToPlanetToShowCursor)
		{
			if (buildingCursor.GetIsShowing())
				buildingCursor.Hide();
			return;
		}

		// Show the build cursor.
		Vector2 buildingPlacePosition = closestPlanet.GetClosestSurfacePointToPosition(selectionCursor.GetPosition());
		Vector2 buildingPlaceNormal = closestPlanet.GetNormalForPosition(selectionCursor.GetPosition());

		buildingCursor.SetPositionAndUpNormal(buildingPlacePosition, buildingPlaceNormal, closestPlanet);

		if (!buildingCursor.GetIsShowing())
		{
			buildingCursor.Show();
		}

#if UNITY_EDITOR
		Debug.DrawLine(closestPlanet.transform.position, buildingPlacePosition, Color.red);
		Debug.DrawLine(buildingPlacePosition, buildingPlacePosition + buildingPlaceNormal, Color.blue);
#endif
	}

	/// <summary>
	/// Gets the building that is currently being hovered over (if there is one)
	/// </summary>
	/// <returns></returns>
	private BuildingComponent GetHoveringBuilding()
	{
		// Find a building.
		List<Collider2D> colliderList = new List<Collider2D>(selectionCursor.GetHovering());
		Collider2D buildingCollider = colliderList.Find((Collider2D collider) => { return collider.GetComponentInParent<BuildingComponent>() != null; });

		BuildingComponent buildingComponent = buildingCollider == null ? null : buildingCollider.GetComponentInParent<BuildingComponent>();
		return buildingComponent;
	}

	private BuildResolve UpdateCableCursor(BuildResolve resolution)
	{
		// Show cable by default.
		if (!cableCursor.GetIsShowing())
			cableCursor.Show();

		// Check if hovering over a building.
		BuildingComponent hoveringBuilding = GetHoveringBuilding();
			
		if (state.GetStateType() == BuildStateType.CABLE)
		{
			CableBuildState cableBuildState = state as CableBuildState;

			if (cableBuildState.fromBuilding == null)
			{
				if (placeThisUpdate && hoveringBuilding != null)
				{
					// The user clicked on a building. This is now the start of a connection.

					// TODO: Check cable connection number restrictions.
					SetState(new CableBuildState(hoveringBuilding));
					cableCursor.SetStart(hoveringBuilding);
					cableCursor.SetEndPoint(selectionCursor.GetPosition());

					resolution.triedPlacingBuilding = true;
					resolution.successfullyChoseCableStart = true;
					return resolution;
				}
				else
				{
					// The user is hover over space, or is hovering over a building w/o clicking.
					cableCursor.Hide();
					return resolution;
				}
			}
			else
			{
				// The user has already selected a previous building an needs to select the next one.
				
				// Show where the cable would currently end.
				if (hoveringBuilding == null)
				{
					cableCursor.SetEndPoint(selectionCursor.GetPosition());
					cableBuildState.SetToBuilding(null);
				}
				else
				{
					cableCursor.SetEndBuilding(hoveringBuilding);
					cableBuildState.SetToBuilding(hoveringBuilding);
				}

				// Check conditions for cable placement.
				// Cable length
				bool cableIsNotTooLong = cableCursor.Length <= GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength;

				// Cable Cost
				int cableCost = Mathf.CeilToInt(cableCursor.Length);
				bool canAffordCable = gameController.Cash >= (resolution.totalCost + cableCost);

				// Cable connected to building
				bool connectedToBuilding = hoveringBuilding != null;
				// Building has sufficient connection slots
				bool buildingHasSlots = false;

				// Connection is not redundant (see if).
				bool cableIsNotRedundant = true;

				if (cableBuildState.fromBuilding != null && cableBuildState.toBuilding != null)
				{
					buildingHasSlots = (cableBuildState.fromBuilding.BackendBuilding.NumConnected < cableBuildState.fromBuilding.Data.maxPowerLines)
					  && (cableBuildState.toBuilding.BackendBuilding.NumConnected < cableBuildState.toBuilding.Data.maxPowerLines);
					
					cableIsNotRedundant = !cableBuildState.fromBuilding.BackendBuilding.HasConnection(cableBuildState.toBuilding.BackendBuilding);
				}
				// Cable is only colliding with two buildings
				List<Collider2D> cableOverlaps = new List<Collider2D>(cableCursor.QueryOverlappingColliders());
				int badOverlapIndex = cableOverlaps.FindIndex((Collider2D collider) =>
				{
					return !IsValidCableOverlap(collider, cableBuildState.fromBuilding, cableBuildState.toBuilding);
				});
				bool noOverlapsAlongCable = badOverlapIndex == -1;

				bool canPlaceCable = cableIsNotTooLong &&
					canAffordCable &&
					connectedToBuilding &&
					buildingHasSlots &&
					cableIsNotRedundant &&
					noOverlapsAlongCable;

				if(canPlaceCable)
                {
					resolution.totalCost = resolution.totalCost + cableCost;
                }

				if (placeThisUpdate && hoveringBuilding)
				{
					// The user clicked on a building.
					resolution.triedPlacingCable = true;

					// Try placing the cable and update the resolution.
					if (canPlaceCable)
					{
						GameObject cable = cableCursor.PlaceCableAtLocation(true);

						resolution.builtCable = cable.GetComponent<CableComponent>();
						resolution.successfullyPlacedCable = true;
					}
				}

				// Update the cursor to show whether the cable can be placed.
				cableCursor.SetCablePlaceability(canPlaceCable);

				// If the user clicks on empty space, the empty resolution
				// will tell the BuildManagerComponent to exit cable mode.
			}
		}
		else if (state.GetStateType() == BuildStateType.BUILDING_CHAINED)
		{
			BuildingChainedBuildState buildingChainedBuildState = state as BuildingChainedBuildState;

			// If we're showing the building cursor, show the cable there.
			if (! buildingCursor.GetIsShowing())
			{
				cableCursor.SetEndPoint(selectionCursor.GetPosition());
			}
			else
			{ // Otherwise, show the cable connecting to the mouse.
				cableCursor.SetEndPoint(buildingCursor.CableConnectionPosition);
			}

			// Check conditions for cable placement.
			// Cable length
			bool cableIsNotTooLong = cableCursor.Length <= GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength;
			// Cable Cost
			int cableCost = Mathf.CeilToInt(cableCursor.Length);
			bool canAffordCable = gameController.Cash >= (resolution.totalCost + cableCost);
			// Cable connected to building
			bool connectedToBuilding = (buildingCursor.GetIsShowing() && buildingCursor.ShowingCanPlaceBuilding) || 
				(resolution.successfullyPlacedBuilding); // Does the building cursor say it could place a building, or a building was just placed?
			// Building has sufficient connection slots
			bool buildingHasSlots = false;
			if (buildingChainedBuildState.fromChained != null)
			{
				buildingHasSlots = (buildingChainedBuildState.fromChained.BackendBuilding.NumConnected < buildingChainedBuildState.fromChained.Data.maxPowerLines);
			}
			// Connection is not redundant
			bool cableIsNotRedundant = true;
			// Cable is only colliding with two buildings
			List<Collider2D> cableOverlaps = new List<Collider2D>(cableCursor.QueryOverlappingColliders());
			int badOverlapIndex = cableOverlaps.FindIndex((Collider2D collider) =>
			{
				return !IsValidCableOverlap(collider, buildingChainedBuildState.fromChained, resolution.builtBuilding);
			});
			bool noOverlapsAlongCable = badOverlapIndex == -1;

			bool canPlaceCable = cableIsNotTooLong &&
				canAffordCable &&
				connectedToBuilding &&
				buildingHasSlots &&
				cableIsNotRedundant &&
				noOverlapsAlongCable;

			if(canPlaceCable)
            {
				resolution.totalCost = resolution.totalCost + cableCost;
            }

			if (placeThisUpdate && (buildingCursor.GetIsShowing() || resolution.successfullyPlacedBuilding))
			{
				resolution.triedPlacingCable = true;

				// TODO: Cover using pylons as a shorthand for more cables / building replacement (stretch).

				if (resolution.successfullyPlacedBuilding && canPlaceCable)
				{
					cableCursor.SetEndBuilding(resolution.builtBuilding);
					GameObject cable = cableCursor.PlaceCableAtLocation();

					resolution.builtCable = cable.GetComponent<CableComponent>();
					resolution.successfullyPlacedCable = true;
				}
			}

			cableCursor.SetCablePlaceability(canPlaceCable);
		}

		return resolution;
	}

	/// <summary>
	/// Determines whether a cable can overlap over the given collider.
	/// Cables can only overlap over buildings that they connect to or other
	/// Cables that share teh same builing connections.
	/// </summary>
	private static bool IsValidCableOverlap(Collider2D overlapping, BuildingComponent startBuilding, BuildingComponent endBuildling)
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
