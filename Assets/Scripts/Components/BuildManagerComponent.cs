using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
public class BuildingBuildState : BuildState
{
	public readonly BuildingSettingEntry toBuild;

	public BuildingBuildState(BuildingSettingEntry toBuild)
	{
		this.toBuild = toBuild;
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
class DemolishBuildState : BuildState { public BuildStateType GetStateType() => BuildStateType.DEMOLISH; }

public struct BuildResolve
{
	public bool triedPlacingBuilding;
	public bool successfullyPlacedBuilding;
	public BuildingComponent builtBuilding;
	public bool triedPlacingCable;
	public bool successfullyPlacedCable;
	public bool successfullyChoseCableStart;
	public CableComponent builtCable;

	public bool TriedAnything()
	{
		return triedPlacingBuilding || triedPlacingCable;
	}
}

/// <summary>
/// Singleton component that manages placing buildings.
/// </summary>
public class BuildManagerComponent : MonoBehaviour
{
	public static BuildManagerComponent Instance { get; private set;}

	private List<PlanetComponent> planets = new List<PlanetComponent>();

	// Reference to the cursor used for showing where buildings will go.
	// Also checks whether a building can be placed (collides with other buildings).
	[SerializeField] private BuildingCursorComponent buildingCursor;
	[SerializeField] private CableCursorComponent cableCursor;
	// How close the mouse has to be to a planet's surface for the build
	// cursor to show up.
	[SerializeField, Min(0.001f)] private float minDistanceToPlanetToShowCursor;

	// The current state of building manager.
	private BuildState state;

	// Events
	// Invoked when the build state changes. Passes the old state and the new state.
	public UnityEvent<BuildState, BuildState> OnStateChanged = new UnityEvent<BuildState, BuildState>();
	// TODO: Pass information about how the build resolved. e.g. built building, but failed to connect cable
	// build cable but didn't create a new building, build both a building and cable, didn't ask to build a cable, etc.
	public UnityEvent<BuildResolve> OnBuildResolve = new UnityEvent<BuildResolve>();

	// Location the mouse is hovering this update.
	private Vector2 hoverThisUpdate;

	// True when the player asked to place something this update.
	private bool placeThisUpdate = false;

	private void Awake()
	{
		// Singleton enforecement.
		if (Instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		// Initally, we aren't building anything.
		SetState(new NoneBuildState());

		planets = WerignacUtils.GetComponentsInActiveScene<PlanetComponent>();
	}

	public void SetDemolishState()
	{
		SetState(new DemolishBuildState());
	}

	public void SetBuildState(BuildingSettingEntry toBuild)
	{
		switch(state.GetStateType())
		{
			case BuildStateType.CABLE: // Try chaining from the last building with a cable.
				CableBuildState cableBuildState = state as CableBuildState;
				if (cableBuildState != null)
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

	private void SetState(BuildState newState)
	{
		BuildState oldState = state;

		// Stop listening for if the chain becomes invalidated.
		if (oldState != null)
		{
			if (oldState.GetStateType() == BuildStateType.BUILDING_CHAINED)
				(oldState as BuildingChainedBuildState).OnInvalidate.RemoveListener(BuildingChainedBuildState_OnInvalidate);
			if (oldState.GetStateType() == BuildStateType.CABLE)
				(oldState as CableBuildState).OnInvalidateFrom.RemoveListener(CableBuildState_OnInvalidateFrom);
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

	private void CableBuildState_OnInvalidateFrom(CableBuildState state)
	{
		SetState(new CableBuildState());
	}

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
	/// Get whether the player is currently considering
	/// building something.
	/// </summary>
	public bool IsInBuildState()
	{
		return state.GetStateType() != BuildStateType.NONE;
	}

	/// <summary>
	/// When the buildingChainedBuildState becomes invalidated,
	/// switch to a normal chained state.
	/// </summary>
	private void BuildingChainedBuildState_OnInvalidate(BuildingChainedBuildState state)
	{
		SetState(state.ToBuildingBuildState());
	}

	// Called on every update before this component's update and fed the mouse position.
	public void Hover(Vector2 hoverPosition)
	{
		hoverThisUpdate = hoverPosition;
	}

	/// <summary>
	/// Tells the BuildManagerComponent to try placing whatever it has
	/// </summary>
	public void SetPlace()
	{
		placeThisUpdate = true;
	}

	private void Update()
	{
		// Check state.
		if (!IsInBuildState())
		{
			placeThisUpdate = false;
			
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

		// TODO: Handle demolish state.

		// Resolve status. Fill this in as attempts to make buildings are
		// completed.
		BuildResolve resolution = new BuildResolve
		{
			triedPlacingBuilding = false,
			successfullyPlacedBuilding = false,
			builtBuilding = null,
			triedPlacingCable = false,
			successfullyPlacedCable = false,
			successfullyChoseCableStart = false,
			builtCable = null
		};

		// TODO: Keep track of cumulative costs.
		// May need to pay for both cable and building.

		if ((state.GetStateType() & BuildStateType.BUILDING) != 0)
			resolution = UpdateBuildingCursor(resolution);
		else
		{
			if (buildingCursor.GetIsShowing())
			{
				buildingCursor.Hide();
			}
		}

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


		// Handle state changes after placing and notify of object placement.
		if (placeThisUpdate)
		{
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
				
				// If we made just a cable, and were not | BuildStateType.BUILDING, set the state to chain a new cable.
				if (state.GetStateType() == BuildStateType.CABLE && !resolution.successfullyChoseCableStart)
				{
					CableBuildState cableBuildState = state as CableBuildState;
					if (cableBuildState.toBuilding != null)
						SetState(cableBuildState.ChainBuildState());
					else
						SetState(new CableBuildState());
				}

				OnBuildResolve?.Invoke(resolution);
			}
		}

		// If nothing was placed, but the place signal was sent, exit the build mode.

		placeThisUpdate = false;
	}

	private Collider2D[] HoverPointOverlap()
	{
		return Physics2D.OverlapPointAll(hoverThisUpdate);
	}

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

			// TODO: Implement.
			bool sufficientFunds = true;
			// TODO: Implement.
			bool sufficientResources = true;

			bool canPlace = roomToPlace && sufficientFunds && sufficientResources;

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

					GameObject buildingGameObject = buildingCursor.PlaceBuildingAtLocation(buildingBuildState.toBuild.Prefab);

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
	/// Shows and places the cursor to snap onto a planet.
	/// If the cursor cannot snap to a planet, or if we're hover over a building, hide the cursor.
	/// </summary>
	private void UpdateBuildingCursorLocation()
	{
		// TODO: Check if we're hovering over a building. If so, hide the cursor?

		bool noPlanets = planets.Count == 0;
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

		foreach (PlanetComponent planet in planets)
		{
			float distanceToHover = Vector2.Distance(planet.GetClosestSurfacePointToPosition(hoverThisUpdate), hoverThisUpdate);

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
		Vector2 buildingPlacePosition = closestPlanet.GetClosestSurfacePointToPosition(hoverThisUpdate);
		Vector2 buildingPlaceNormal = closestPlanet.GetNormalForPosition(hoverThisUpdate);

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

	private BuildingComponent GetHoveringBuilding()
	{
		// Find a building.
		List<Collider2D> colliderList = new List<Collider2D>(HoverPointOverlap());
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
					cableCursor.SetEndPoint(hoverThisUpdate);

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
					cableCursor.SetEndPoint(hoverThisUpdate);
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
				bool canAffordCable = true;
				// Cable connected to building
				bool connectedToBuilding = hoveringBuilding != null;
				// Building has sufficient connection slots
				bool buildingHasSlots = true;
				// Connection is not redundant
				bool cableIsNotRedundant = true;
				// Cable is only colliding with two buildings
				bool noOverlapsAlongCable = true;

				bool canPlaceCable = cableIsNotTooLong &&
					canAffordCable &&
					connectedToBuilding &&
					buildingHasSlots &&
					cableIsNotRedundant &&
					noOverlapsAlongCable;

				if (placeThisUpdate && hoveringBuilding)
				{
					// The user clicked on a building.
					resolution.triedPlacingCable = true;

					// Try placing the cable and update the resolution.
					if (canPlaceCable)
					{
						GameObject cable = cableCursor.PlaceCableAtLocation();

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
			// If we're showing the building cursor, show the cable there.
			if (! buildingCursor.GetIsShowing())
			{
				cableCursor.SetEndPoint(hoverThisUpdate);
			}
			else
			{ // Otherwise, show the cable connecting to the mouse.
				cableCursor.SetEndPoint(buildingCursor.CableConnectionPosition);
			}

			// Check conditions for cable placement.
			// Cable length
			bool cableIsNotTooLong = cableCursor.Length <= GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength;
			// Cable Cost
			bool canAffordCable = true;
			// Cable connected to building
			bool connectedToBuilding = (buildingCursor.GetIsShowing() && buildingCursor.ShowingCanPlaceBuilding) || 
				(resolution.successfullyPlacedBuilding); // Does the building cursor say it could place a building, or a building was just placed?
			// Building has sufficient connection slots
			bool buildingHasSlots = true;
			// Connection is not redundant
			bool cableIsNotRedundant = true;
			// Cable is only colliding with two buildings
			bool noOverlapsAlongCable = true;

			bool canPlaceCable = cableIsNotTooLong &&
				canAffordCable &&
				connectedToBuilding &&
				buildingHasSlots &&
				cableIsNotRedundant &&
				noOverlapsAlongCable;


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


}
