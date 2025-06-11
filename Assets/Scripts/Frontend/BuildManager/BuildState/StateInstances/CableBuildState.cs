using UnityEngine;
using System;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	public class CableBuildState : IBuildState, ICablePlacer, IOverrideExternalSignal
	{
		/// <summary>
		/// The first object the cable is attached to.
		/// </summary>
		private ICableAttachment _fromAttachment = null;

		/// <summary>
		/// The second object the calbe is attached to.
		/// WARNING: Be careful when changing attachment points. Sometimes this "to" attachment
		/// can be to a building that cannot receive new cables!
		/// </summary>
		private ICableAttachment _toAttachment = null;

		/// <summary>
		/// The cursor that is used to show the cable that will be placed.
		/// </summary>
		private CableCursorComponent _cableCursor;

		/// <summary>
		/// The component that tells us what the player is hovering over.
		/// </summary>
		private readonly SelectionCursorComponent _selectionCursor;
		/// <summary>
		/// The callback to invoke when this state want to query its constraints.
		/// Returns whether this build state passed all its constraints.
		/// </summary>
		private readonly Func<BuildWarning.WarningType> _queryConstraintCallback;

		/// <summary>
		/// Invoked when we want to switch to a different build state.
		/// </summary>
		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		/// <summary>
		/// Invoked when we place a cable.
		/// </summary>
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();

		public CableBuildState(
				CableCursorComponent cableCursor,
				SelectionCursorComponent selectionCursor,
				Func<BuildWarning.WarningType> queryConstraintCallback,
				BuildingComponent startingBuilding = null
			)
		{
			// TODO: May need a way to override attachments (e.g. Chain attaches to building ghost / building cursor).
			// TODO: May need a way to override cable color (e.g. Chain cable cannot place when building cannot place).

			if (startingBuilding != null)
			{
				if (!startingBuilding.BackendBuilding.CanAcceptNewConnections())
					throw new ArgumentException($"Cannot start cable build state with a building that cannot accept new connections {startingBuilding.gameObject.name}. Has {startingBuilding.BackendBuilding.NumConnected} / {startingBuilding.BackendBuilding.Data.maxPowerLines} connections.");
				SetFromAttachment(startingBuilding, false);
			}

			if (cableCursor == null)
				throw new ArgumentNullException("cableCursor");

			_cableCursor = cableCursor;

			if (selectionCursor == null)
				throw new ArgumentNullException("selectionCursor");

			_selectionCursor = selectionCursor;

			if (queryConstraintCallback == null)
				throw new ArgumentNullException("queryConstraintCallback");

			_queryConstraintCallback = queryConstraintCallback;
		}

		public void Start() { }

		public BuildStateType GetStateType()
		{
			return BuildStateType.CABLE;
		}

		public ICableAttachment GetFromAttachment()
		{
			return _fromAttachment;
		}

		public ICableAttachment GetToAttachment()
		{
			return _toAttachment;
		}

		public float Length
		{
			get
			{
				// If _fromAttachment is not set, nothing is set. Out cable has a length of zero.
				if (_fromAttachment == null)
					return 0;

				// If _toAttachment is not set, we can't get a length.
				if (_toAttachment == null)
					return 0;

				Vector2 fromPosition = _fromAttachment.GetPosition();
				Vector2 toPosition = _toAttachment.GetPosition();

				return Vector2.Distance(fromPosition, toPosition);
			}
		}
		
		public Cost Cost
		{
			get
			{
				int cashCost = Mathf.CeilToInt(Length * Data.cableCostMultiplier);

				return new Cost
				{
					cashCost = cashCost,
					scienceCost = 0
				};
			}
		}

		/// <summary>
		/// Returns whether a cable attachment is an attachment to a particular building.
		/// </summary>
		/// <param name="cableAttachment">The cable attachment to test. Can be null.</param>
		/// <param name="buildingComponent">The building.</param>
		/// <returns>True if the cable attachment is both to a building instance and that building instance is the passed building component.</returns>
		private static bool AttachmentIsToBuilding(ICableAttachment cableAttachment, BuildingComponent buildingComponent)
		{
			if (cableAttachment == null)
				return false;

			if (cableAttachment is BuildingInstanceCableAttachment buildingAttachment)
				return buildingAttachment.BuildingInstance == buildingComponent;

			return false;
		}

		private void FromAttachment_OnDemolish(BuildingComponent buildingComponent)
		{
			// If this function is triggered from the appropriate event, then
			// the current "from" must be the building being passed.

			bool isFromAttachment = AttachmentIsToBuilding(_fromAttachment, buildingComponent);
			if (!isFromAttachment)
				throw new ArgumentException($"Invoked \"from\" attachment callback for CableBuildState with a building that is not the \"from\" building. From attachment: ${_fromAttachment}, Callback building: ${buildingComponent}.");

			// Clear both the "from" and "to" attachments. If hovering over a different building
			// for "to", it will automatically become the "from" on the next update.
			SetFromAttachment(null);
			SetToAttachment(null);
		}

		private void ToAttachment_OnDemolish(BuildingComponent buildingComponent)
		{
			// If this function is triggered from the appropriate event, then
			// the current "to" must be the building being passed.

			bool isToAttachment = AttachmentIsToBuilding(_toAttachment, buildingComponent);
			if (!isToAttachment)
				throw new ArgumentException($"Invoked \"to\" attachment callback for CableBuildState with a building that is not the \"to\" building. To attachment: ${_toAttachment}, Callback building: ${buildingComponent}.");

			// Clear the "to" attachment.
			SetToAttachment(null);
		}

		/// <summary>
		/// Tries to register or deregister a listener to the demolish event of an attachment if it is
		/// an attachment to a building.
		/// </summary>
		/// <param name="attachment">The attachement with the building to listen to (potentially). Can be null.</param>
		/// <param name="listener">The listener to the event.</param>
		/// <param name="registerOrUnregister">True for registering false for deregistering.</param>
		/// <returns>Whether we were able to register or unregister the listener to the event.</returns>
		private static bool TryRegisterOrUnregisterDemolishListener(ICableAttachment attachment, UnityAction<BuildingComponent> listener, bool registerOrUnregister)
		{
			if (attachment != null && attachment is BuildingInstanceCableAttachment buildingAttachment)
			{
				UnityEvent<BuildingComponent> demolishEvent = buildingAttachment.BuildingInstance.OnBuildingDemolished;
				if (registerOrUnregister)
					demolishEvent.AddListener(listener);
				else
					demolishEvent.RemoveListener(listener);
				return true;
			}
			return false;
		}

		
		/// <summary>
		/// Used internally to set the "from" end of the cable attachment.
		/// </summary>
		/// <param name="attachment">The attachment to use for the "from". Can be null.</param>
		private void SetFromAttachment(ICableAttachment attachment)
		{
			TryRegisterOrUnregisterDemolishListener(_fromAttachment, FromAttachment_OnDemolish, false);

			_fromAttachment = attachment;

			TryRegisterOrUnregisterDemolishListener(attachment, FromAttachment_OnDemolish, true);
		}

		private void SetFromAttachment(BuildingComponent buildingComponent, bool isVolatile)
		{
			SetFromAttachment(new BuildingInstanceCableAttachment(buildingComponent, isVolatile));
		}

		/// <summary>
		/// Used internally to set the "to" end of the cable attachment.
		/// </summary>
		/// <param name="attachment">The attachment to use for the "to". Can be null.</param>
		private void SetToAttachment(ICableAttachment attachment)
		{
			TryRegisterOrUnregisterDemolishListener(_toAttachment, ToAttachment_OnDemolish, false);

			_toAttachment = attachment;

			TryRegisterOrUnregisterDemolishListener(attachment, ToAttachment_OnDemolish, true);
		}

		private void SetToAttachment(BuildingComponent buildingComponent, bool isVolatile)
		{
			SetToAttachment(new BuildingInstanceCableAttachment(buildingComponent, isVolatile));
		}

		/// <summary>
		/// Gets whether the player has clicked on the first building
		/// for the connection / this state started with a building to connect from.
		/// </summary>
		public bool GetIsFromAttachmentSetAndNonVolatile()
		{
			if (_fromAttachment == null)
				return false;

			if (_fromAttachment is BuildingInstanceCableAttachment buildingAttachment)
				return !buildingAttachment.IsVolatile;

			return true;
		}

		public void Update(BuildStateInput input)
		{
			// TODO: When a building button is clicked while in this state, transition to the chained state.

			BuildingComponent hoveringBuilding = _selectionCursor.FindFirstBuilding();

			// Set from / to based on the hovering building.
			bool isFromSetNonVolatilePreHover = GetIsFromAttachmentSetAndNonVolatile();
			if (hoveringBuilding != null)
			{
				if (!isFromSetNonVolatilePreHover)
					SetFromAttachment(hoveringBuilding, true);
				else
					SetToAttachment(hoveringBuilding, true);
			}
			else
			{
				if (isFromSetNonVolatilePreHover)
					SetToAttachment(new CursorCableAttachment(_selectionCursor));
				else
					SetFromAttachment(null);
			}

			UpdateCableCursorPosition();

			BuildWarning.WarningType constraintsResult = QueryConstraints();
			bool passesConstraints = constraintsResult < BuildWarning.WarningType.FATAL;

			UpdateCableCursorColor(constraintsResult);

			if (input.primaryFire)
			{
				// The player clicked while not hovering over anything.
				if (hoveringBuilding == null)
				{
					Cancel();
					return;
				}
				
				// Set this according to whether the player has set the from for this cable yet. 
				bool isFromSetAndNonVolatile = GetIsFromAttachmentSetAndNonVolatile();
				// Is the from not set or do we not meet the constraints.
				if (! (isFromSetAndNonVolatile && passesConstraints))
				{
					if (hoveringBuilding.BackendBuilding.CanAcceptNewConnections())
					{
						// Set "from" to the player's selection (chaining in the case of constraintsResult == false).
						SetToAttachment(null); // Will be set to non-null on next update.
						SetFromAttachment(hoveringBuilding, false);
					}
					else
					{
						// Clicked on a building that cannot accept new connections.
						// TODO: Play a fail sound.
					}
					return;
				}

				// Both "from" and "to" are set (since the player is hovering over a
				// building and we're not in the previous if statement).
				// Additionally, we passed our constraints.
				Apply();
				return;
			}
			else if (input.secondaryFire)
			{
				bool isFromSetAndNonVolatile = GetIsFromAttachmentSetAndNonVolatile();

				if (isFromSetAndNonVolatile)
				{
					SetFromAttachment(null);
					SetToAttachment(null);
				}
				else
					Cancel();
				return;
			}
		}

		private BuildWarning.WarningType QueryConstraints()
		{
			return _queryConstraintCallback();
		}

		private void UpdateCableCursorPosition()
		{
			bool show = false;

			if (_fromAttachment != null)
			{
				BuildingInstanceCableAttachment fromBuildingAttachment = _fromAttachment as BuildingInstanceCableAttachment;
				_cableCursor.SetStart(fromBuildingAttachment.BuildingInstance);

				if (_toAttachment != null)
				{
					show = true;
					if (_toAttachment is BuildingInstanceCableAttachment toBuildingAttachment)
						_cableCursor.SetEndBuilding(toBuildingAttachment.BuildingInstance);
					else
						_cableCursor.SetEndPoint(_toAttachment.GetPosition());
				}
				else
				{
					_cableCursor.SetEndPoint(_selectionCursor.GetPosition());
				}
			}
			else
			{
				_cableCursor.SetStart(null);
				_cableCursor.SetEndPoint(_selectionCursor.GetPosition());
			}

			if (show)
				_cableCursor.Show();
			else
				_cableCursor.Hide();
		}

		private void UpdateCableCursorColor(BuildWarning.WarningType constraintsResult)
		{
			_cableCursor.SetCablePlaceability(constraintsResult < BuildWarning.WarningType.FATAL);
		}

		private void Apply()
		{
			GameObject cableObjectInstance = _cableCursor.PlaceCableAtLocation();
			CableComponent cableInstance = cableObjectInstance.GetComponent<CableComponent>();

			CableBuildStateApplyResult result = new CableBuildStateApplyResult
			{
				cost = Cost,
				cableInstance = cableInstance
			};
			OnApplied.Invoke(result);

			// TODO: Use a function to set the attachment to listen to when the building gets destroyed.
			ICableAttachment newFromAttachment = new BuildingInstanceCableAttachment(
				(_toAttachment as BuildingInstanceCableAttachment).BuildingInstance,
				false
			);
			SetFromAttachment(newFromAttachment);
			SetToAttachment(null); // Will be set to non-null on next update.
		}

		/// <summary>
		/// Called internally when the player wants to get out of placing
		/// a cable.
		/// </summary>
		private void Cancel()
		{
			BuildStateTransitionSignal transition = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(transition);
		}

		public void CleanUp()
		{
			// Remove event listeners for the demolishment of buildings.
			TryRegisterOrUnregisterDemolishListener(_fromAttachment, FromAttachment_OnDemolish, false);
			TryRegisterOrUnregisterDemolishListener(_toAttachment, FromAttachment_OnDemolish, false);
			
			_cableCursor.Hide();
		}

		public bool TryOverrideExternalSignal(BuildStateTransitionSignal signal)
		{
			if (!signal.IsExternal)
				throw new ArgumentException("Cannot call TryOverrideExternalSignal with an internal signal.");

			// When the user presses on a building button whilst in the cable state, start chaining
			// from the current "from" building (if there is one).
			if (signal.GetSignalType() == BuildStateTransitionSignalType.BUILDING && GetIsFromAttachmentSetAndNonVolatile())
			{
				BuildingSettingEntry toBuild = (signal as BuildingTransitionSignal).NewBuildingType;
				BuildingComponent chainFrom = (_fromAttachment as BuildingInstanceCableAttachment).BuildingInstance;
				BuildStateTransitionSignal chainSignal = new ChainTransitionSignal(chainFrom, toBuild, false);
				OnRequestTransition.Invoke(chainSignal);
				return true;
			}

			return false;
		}
	}
}
