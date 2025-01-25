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
class DemolishBuildState : BuildState { public BuildStateType GetStateType() => BuildStateType.DEMOLISH; }

/// <summary>
/// Singleton component that manages placing buildings.
/// </summary>
public class BuildManagerComponent : MonoBehaviour
{
	public static BuildManagerComponent Instance { get; private set;}

	private List<PlanetComponent> planets = new List<PlanetComponent>();

	// Reference to the cursor used for showing where buildings will go.
	// Also checks whether a building can be placed (collides with other buildings).
	[SerializeField] private BuildingCursorComponent cursor;
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
	public UnityEvent OnBuildResolve = new UnityEvent();

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
		if (state.GetStateType() == BuildStateType.BUILDING_CHAINED)
		{
			BuildingChainedBuildState buildingChainedBuildState = state as BuildingChainedBuildState;
			SetState(new BuildingChainedBuildState(toBuild, buildingChainedBuildState.fromChained));
		}
		else
		{
			SetState(new BuildingBuildState(toBuild));
		}
	}

	private void SetState(BuildState newState)
	{
		BuildState oldState = state;

		// Stop listening for if the chain becomes invalidated.
		if (oldState != null && oldState.GetStateType() == BuildStateType.BUILDING_CHAINED)
			(oldState as BuildingChainedBuildState).OnInvalidate.RemoveListener(BuildingChainedBuildState_OnInvalidate);

		state = newState;

		// Listen for if the chain becomes invalidated.
		if (newState.GetStateType() == BuildStateType.BUILDING_CHAINED)
			(oldState as BuildingChainedBuildState).OnInvalidate.AddListener(BuildingChainedBuildState_OnInvalidate);


		SetUpCursorForBuildState(newState);

		OnStateChanged?.Invoke(oldState, newState);
	}

	private void SetUpCursorForBuildState(BuildState newBuildState)
	{
		switch(newBuildState.GetStateType())
		{
			case BuildStateType.NONE:
				return;
			case BuildStateType.BUILDING_CHAINED:
				// TODO: Preview the cable.
			case BuildStateType.BUILDING:
				{
					BuildingBuildState buildingBuildState = newBuildState as BuildingBuildState;
					BuildingVisuals visualAsset = buildingBuildState.toBuild.VisualAsset;

					Sprite ghostSprite = visualAsset.buildingGhost;
					Vector2 ghostOffset = visualAsset.ghostOffset;
					float ghostScale = visualAsset.ghostScale;

					cursor.SetGhost(ghostSprite, ghostOffset, ghostScale);
					
					BuildingComponent buildingComponent = buildingBuildState.toBuild.Prefab.GetComponent<BuildingComponent>();

					Vector2 colliderSize = buildingComponent.ColliderSize;
					Vector2 colliderOffset = buildingComponent.ColliderOffset;

					cursor.SetBuildingCollision(colliderSize, colliderOffset);
				}
				
				break;
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
		// If we aren't in a build state, hide the cursor.
		bool notInBuildMode = ! IsInBuildState();
		// If there are no planets, there's nowhere to build, so hide the cursor.
		bool noPlanets = planets.Count == 0;

		if (notInBuildMode || noPlanets)
		{
			if (cursor.GetIsShowing())
				cursor.Hide();
			return;
		}

		// TODO: Highlight over hovered buildings in demolish mode.
		if (state.GetStateType() == BuildStateType.DEMOLISH)
		{
			if (cursor.GetIsShowing())
				cursor.Hide();
			return;
		}

		// Check if we're hovering over a building. If so, and we're in chained mode, consider
		// adding a connection.

		// There is at least one planet.

		// If we're hovering over a building, replace it rather than build a new building.

		// Find the planet that has the point on its surface that is the closest to the mouse.
		float closestPlanetDistance = -1f;
		PlanetComponent closestPlanet = null;

		foreach(PlanetComponent planet in planets)
		{
			float distanceToHover = Vector2.Distance(planet.GetClosestSurfacePointToPosition(hoverPosition), hoverPosition);

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
			if (cursor.GetIsShowing())
				cursor.Hide();
			return;
		}

		// Show the build cursor.
		Vector2 buildingPlacePosition = closestPlanet.GetClosestSurfacePointToPosition(hoverPosition);
		Vector2 buildingPlaceNormal = closestPlanet.GetNormalForPosition(hoverPosition);

		cursor.SetPositionAndUpNormal(buildingPlacePosition, buildingPlaceNormal, closestPlanet);

		if (! cursor.GetIsShowing())
		{
			cursor.Show();
		}

#if UNITY_EDITOR
		Debug.DrawLine(closestPlanet.transform.position, buildingPlacePosition, Color.red);
		Debug.DrawLine(buildingPlacePosition, buildingPlacePosition + buildingPlaceNormal, Color.blue);
#endif
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
			return;
		}
		
		// TODO: Handle demolish state.

		if (cursor.GetIsShowing())
		{
			// The player is trying to build something.
			BuildingBuildState buildingBuildState = state as BuildingBuildState;

			// TODO: Check if we're hovering over a building (saved in the buildingBuildState). If so,
			// replace that one instead of building a new building.

			// Determine whether the building can be placed.
			Collider2D[] overlappingColliders = cursor.QueryOverlappingColliders();

			// The only thing that the building should be colliding with is the parent planet.
			bool roomToPlace = overlappingColliders.Length == 1 && cursor.ParentPlanet.OwnsCollider(overlappingColliders[0]);

			// TODO: Implement.
			bool sufficientFunds = true;
			// TODO: Implement.
			bool sufficientResources = true;

			bool canPlace = roomToPlace && sufficientFunds && sufficientResources;

			// Update the cursor graphic.
			cursor.SetBuildingPlaceability(canPlace);

			// TODO: Update the reason as to why the building cannot be placed.

			// If input was received to place the building, place it.
			if (placeThisUpdate)
			{
				if (canPlace)
				{
					//Building building = new Building { Data = buildingBuildState.toBuild.BuildingDataAsset };
					// TODO: Add to game manager.

					cursor.PlaceBuildingAtLocation(buildingBuildState.toBuild.Prefab);

					// Check for a chained state. If so, chain to last placed building.
				}
				else
				{
					// Note in build resolve that the building failed to be placed.
				}
			}

			// Check for chained state.
			// If not in chained state, enter chained state.

			// If input was received to place the building and it was successful
			// and we're in chained state, place the cable.
		}

		// Check whether we're hovering over any buildings.

		// If input was received to place the building and we are hovering over a builing,
		// connect the cables.

		// If nothing was placed, but the place signal was sent, exit the build mode.

		placeThisUpdate = false;
	}



	private bool TryPlace(Vector2 hoverPosition, int buildingType)
	{
		// Get the position on the planet.

		// Get the dimensions of the building.

		// See if the building collides with anything. Maybe handeled by a separate cursor object.

		// If so, don't place the building. Return False.

		// If not, place the building. Return True.

		return false;
	}


}
