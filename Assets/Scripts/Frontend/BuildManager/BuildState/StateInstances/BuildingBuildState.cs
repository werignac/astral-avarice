using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Build state of the BuildManager when placing a new building.
	/// </summary>
	public class BuildingBuildState : IBuildState, IBuildingPlacement, IInspectable
	{
		#region Fields
		/// <summary>
		/// The object describing the new building we wish to place.
		/// </summary>
		private readonly NewPlacingBuilding _toBuild;
		/// <summary>
		/// The planet that we want to build on. Can be null if the planet is not decided.
		/// </summary>
		private PlanetComponent _prospectivePlanet = null;
		/// <summary>
		/// The cursor we use to show where we are placing the building.
		/// </summary>
		private BuildingCursorComponent _buildingCursor;
		/// <summary>
		/// The cursor we use to show changes in planet mass.
		/// </summary>
		private GravityFieldCursorComponent _gravityCursor;
		/// <summary>
		/// Reference to the game controller for access to planets.
		/// 
		/// TODO: Replace w/ reference to scriptable object that stores the current game state?
		/// </summary>
		private readonly GameController _gameController;
		/// <summary>
		/// The cursor that we use to see what we're hovering over and whether we are clicking on
		/// a building.
		/// </summary>
		private readonly SelectionCursorComponent _selectionCursor;
		/// <summary>
		/// The callback to invoke when this state wants to query its constraints.
		/// Returns whether this build state passed all its constraints.
		/// 
		/// TODO: Have a different return type that allows for alerts (color the building cursor orange).
		/// </summary>
		private readonly Func<bool> _queryConstraintCallback;
		#endregion Fields

		#region Events
		/// <summary>
		/// Event fired when the player changes what planet they're prospecting building on.
		/// </summary>
		public UnityEvent<PlanetComponent> OnProspectivePlanetChanged { get; } = new UnityEvent<PlanetComponent>();
		/// <summary>
		/// Event fired when this state want to transition to a different state.
		/// </summary>
		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		/// <summary>
		/// Event fired when a building gets placed. Cost subtraction occurs via this event.
		/// </summary>
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();
		#endregion Events

		public NewPlacingBuilding ToBuild => _toBuild;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="toBuild">The setting entry for the new building we wish to place.</param>
		public BuildingBuildState(
				BuildingSettingEntry toBuild,
				BuildingCursorComponent buildingCursor,
				SelectionCursorComponent selectionCursor,
				GravityFieldCursorComponent gravityCursor,
				GameController gameController,
				Func<bool> queryConstraintCallback
			)
		{
			if (toBuild == null)
				throw new ArgumentNullException("toBuild");

			_toBuild = new NewPlacingBuilding(toBuild);

			if (buildingCursor == null)
				throw new ArgumentNullException("buildCursor");

			_buildingCursor = buildingCursor;

			if (selectionCursor == null)
				throw new ArgumentNullException("selectionCursor");

			_selectionCursor = selectionCursor;

			if (gravityCursor == null)
				throw new ArgumentNullException("gravityCursor");

			_gravityCursor = gravityCursor;

			if (gameController == null)
				throw new ArgumentNullException("gameController");

			_gameController = gameController;

			if (queryConstraintCallback == null)
				throw new ArgumentNullException("queryConstraintCallback");

			_queryConstraintCallback = queryConstraintCallback;

			// Set up the building cursor for the type of building we're placing.
			InitializeBuildingCursor();
		}

		public BuildStateType GetStateType()
		{
			return BuildStateType.BUILDING;
		}

		public IPlacingBuilding GetPlacingBuilding()
		{
			return _toBuild;
		}

		/// <summary>
		/// Returns the planet we are prospecting building on.
		/// </summary>
		/// <returns></returns>
		public PlanetComponent GetProspectivePlanet()
		{
			return _prospectivePlanet;
		}

		/// <summary>
		/// Assigns a new planet to be prospected for placing a planet.
		/// Notifies the new planet, the old planet, and the world about the change in planets.
		/// 
		/// If the new planet is the same as the old planet, nothing happens.
		/// </summary>
		/// <param name="newProspectivePlanet">The new planet to start prospecting.</param>
		private void SetProspectivePlanet(PlanetComponent newProspectivePlanet)
		{
			bool isDifferent = _prospectivePlanet != newProspectivePlanet;

			if (!isDifferent)
				return;

			// Tell the old planet we're no longer prospecting it.
			if (_prospectivePlanet != null)
				_prospectivePlanet.StopProspectingMassChange();

			_prospectivePlanet = newProspectivePlanet;

			// Tell the world and the new planet about the change in prospecting planets.
			OnProspectivePlanetChanged?.Invoke(newProspectivePlanet);
			if (newProspectivePlanet != null)
				newProspectivePlanet.StartProspectingMassChange();
		}

		/// <summary>
		/// Gets the cost of placing this build state's building.
		/// Called externally by cost constraint scripts.
		/// </summary>
		/// <returns>Returns the cash and science cost of placing this building.</returns>
		public Cost GetCost()
		{
			return new Cost
			{
				cashCost = _toBuild.BuildingSettings.BuildingDataAsset.cost,
				scienceCost = _toBuild.BuildingSettings.BuildingDataAsset.scienceCost
			};
		}

		/// <summary>
		/// Initializes the building cursor to display the building we plan on placing.
		/// Includes information such as the ghost sprite, the collider size, and the
		/// cable connection point.
		/// </summary>
		private void InitializeBuildingCursor()
		{
			BuildingVisuals visualAsset = _toBuild.BuildingSettings.VisualAsset;

			Sprite ghostSprite = visualAsset.buildingGhost;
			Vector2 ghostOffset = visualAsset.ghostOffset;
			float ghostScale = visualAsset.ghostScale;

			_buildingCursor.SetGhost(ghostSprite, ghostOffset, ghostScale);

			BuildingComponent buildingComponent = _toBuild.BuildingSettings.Prefab.GetComponent<BuildingComponent>();

			Vector2 colliderSize = buildingComponent.ColliderSize;
			Vector2 colliderOffset = buildingComponent.ColliderOffset;

			_buildingCursor.SetBuildingCollision(colliderSize, colliderOffset);

			Vector2 cableConnectionOffset = buildingComponent.CableConnectionOffset;

			_buildingCursor.SetBuildingCableConnectionOffset(cableConnectionOffset);
		}

		/// <summary>
		/// Called externally by owner every frame.
		/// </summary>
		/// <param name="input">Input from the player.</param>
		public void Update(BuildStateInput input)
		{
			// TODO: Check if we're hovering over a building. If so, hide the cursor?

			// TODO: Use selection cursor for world position?
			UpdateBuildingPosition(_selectionCursor.GetPosition());
			
			bool constraintsResult = QueryConstraints();
			
			if (input.primaryFire)
			{
				// Check if we are hovering over another building. If so, transition to chained.
				BuildingComponent hoveringBuilding = _selectionCursor.FindFirstBuilding();
				if (hoveringBuilding != null)
				{
					if (!hoveringBuilding.BackendBuilding.CanAcceptNewConnections())
					{
						Chain(hoveringBuilding);
						return;
					}
				}

				// If the player is clicking on empty space, cancel.
				if (_prospectivePlanet == null)
				{
					Cancel();
					return;
				}

				// Otherwise, check if we can apply after query constraints.
				if (constraintsResult)
				{
					Apply();
					return;
				}
				else
				{
					// TODO: If not, play a failed sound.
					return;
				}
			}
			else if (input.secondaryFire)
			{
				Cancel();
				return;
			}
		}

		/// <summary>
		/// Updates where we want to place the building including which
		/// planet and where on the planet we want to place it.
		/// </summary>
		/// <param name="mousePosition">The position of the mouse in world space.</param>
		private void UpdateBuildingPosition(Vector2 mousePosition)
		{
			bool noPlanets = _gameController.Planets.Count == 0;
			
			if (noPlanets)
			{ // If there are no planets, there's nothing to build on.
				if (_buildingCursor.GetIsShowing())
					_buildingCursor.Hide();

				if (_gravityCursor.GetIsShowing())
					_gravityCursor.Hide();

				// If there are no planets, set the prospective planet to be null.
				// We don't anticipate to change any planets.
				SetProspectivePlanet(null);
				return;
			}

			// Find the planet that has the point on its surface that is the closest to the mouse.
			float closestPlanetDistance = -1f;
			PlanetComponent closestPlanet = null;

			foreach (PlanetComponent planet in _gameController.Planets)
			{
				float distanceToHover = Vector2.Distance(planet.GetClosestSurfacePointToPosition(mousePosition), mousePosition);

				// If this is the first planet, assume it's the closest.
				// Otherwise, store the new closest.
				if (closestPlanetDistance < 0 || distanceToHover < closestPlanetDistance)
				{
					closestPlanetDistance = distanceToHover;
					closestPlanet = planet;
				}
			}

			// Check that the closestPlanetDistance is  below a certain threshold. If not, hide.
			float minCursorDistance = GlobalBuildingSettings.GetOrCreateSettings().MinDistanceToPlanetToShowBuildingCursor;
			if (closestPlanetDistance > minCursorDistance)
			{
				if (_buildingCursor.GetIsShowing())
					_buildingCursor.Hide();
				if (_gravityCursor.GetIsShowing())
					_gravityCursor.Hide();

				// If there are no nearby planets, set the prospective planet to be null.
				// We don't anticipate to change any planets.
				SetProspectivePlanet(null);
				return;
			}

			// Show the build cursor.
			Vector2 buildingPlacePosition = closestPlanet.GetClosestSurfacePointToPosition(mousePosition);
			Vector2 buildingPlaceNormal = closestPlanet.GetNormalForPosition(mousePosition);

			_buildingCursor.SetPositionAndUpNormal(buildingPlacePosition, buildingPlaceNormal, closestPlanet);

			if (!_buildingCursor.GetIsShowing())
			{
				_buildingCursor.Show();
			}

			if (!_gravityCursor.GetIsShowing())
			{
				_gravityCursor.Show();
			}

			_gravityCursor.SetGravityCursor(closestPlanet, _toBuild.BuildingSettings.BuildingDataAsset.mass);

			// We anticipate that a building will be placed on a planet.
			// Notify others that we anticipate the mass of this planet to change.
			SetProspectivePlanet(closestPlanet);
		}

		/// <summary>
		/// Checks all the constrains for this state and determines whether we can
		/// currently place a building.
		/// </summary>
		private bool QueryConstraints()
		{
			// Check all the constraints for this state.
			return _queryConstraintCallback();
			// Set somewhere whether we are able to place the building in the current state.
			// NOTE: In order to avoid circular references, the actual check may need to happen externally.
			// TODO: Update cursor color to reflect constraint results.
		}

		/// <summary>
		/// Called internally when the player wants to and can place the selected buidling.
		/// </summary>
		private void Apply()
		{
			// Place the building (currently done in the building cursor.)
			GameObject buildingGameObject = _buildingCursor.PlaceBuildingAtLocation(_toBuild.BuildingSettings.Prefab);
			BuildingComponent building = buildingGameObject.GetComponent<BuildingComponent>();
			// Signal to communicate that we built the building (subtracing funds is handeled here).
			BuildingBuildStateApplyResult result = new BuildingBuildStateApplyResult
			{
				cost = GetCost(),
				buildingInstance = building
			};
			OnApplied.Invoke(result);
			// Transition to chaining from the new building.
			Chain(building);
		}

		/// <summary>
		/// Called internally when the player wants to get out of this state without
		/// placing a building.
		/// </summary>
		private void Cancel()
		{
			BuildStateTransitionSignal transition = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(transition);
		}

		private void Chain(BuildingComponent chainFrom)
		{
			BuildStateTransitionSignal transition = new ChainTransitionSignal(chainFrom, _toBuild.BuildingSettings, false);
			OnRequestTransition.Invoke(transition);
		}

		public void CleanUp()
		{
			if (_prospectivePlanet != null)
			{
				// Signal to others that we're no longer prospecting building on the planet
				// we were previously prospecting.
				OnProspectivePlanetChanged.Invoke(null);
				// Signal to the planet that we're no longer prospecting building on it.
				_prospectivePlanet.StopProspectingMassChange();
			}

			_buildingCursor.Hide();
		}

		/// <summary>
		/// Returns the UI element + controller that is shown in the inspector while we're in this build state.
		/// The UI element is the same as what's shown when hovering over the building buttons in the build menu.
		/// </summary>
		/// <param name="inspectorController">The controller that determines how to set up the UI element to show the correct building properties.</param>
		/// <returns>The UI element to show in the inspector.</returns>
		public VisualTreeAsset GetInspectorElement(out IInspectorController inspectorController)
		{
			inspectorController = new BuildingButtonInspectorController(_toBuild.BuildingSettings);
			return PtUUISettings.GetOrCreateSettings().BuildingInspectorUI;
		}
	}
}
