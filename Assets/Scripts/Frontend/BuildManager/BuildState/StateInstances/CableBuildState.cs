using UnityEngine;
using System;
using UnityEngine.Events;

namespace AstralAvarice.Frontend
{
	public class CableBuildState : IPostConstraintsBuildState<BuildWarning.WarningType>, ICablePlacer, IOverrideExternalSignal, IHasCost
	{
		/// <summary>
		/// Temporary variable usef to store the building to start attaching from.
		/// Needed because setting the building invokes certain visual effects
		/// that must be done in start instead of on construction.
		/// </summary>
		private BuildingComponent _initialFromAttachment;

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
		/// Invoked when we want to switch to a different build state.
		/// </summary>
		public UnityEvent<BuildStateTransitionSignal> OnRequestTransition { get; } = new UnityEvent<BuildStateTransitionSignal>();
		/// <summary>
		/// Invoked when we place a cable.
		/// </summary>
		public UnityEvent<BuildStateApplyResult> OnApplied { get; } = new UnityEvent<BuildStateApplyResult>();
		/// <summary>
		/// Invoked when the user tried to place a cable, but failed to do so.
		/// </summary>
		public UnityEvent OnApplyFailed { get; } = new UnityEvent();

		public CableBuildState(
				CableCursorComponent cableCursor,
				SelectionCursorComponent selectionCursor,
				BuildingComponent startingBuilding = null
			)
		{
			if (startingBuilding != null)
			{
				if (!startingBuilding.BackendBuilding.CanAcceptNewConnections())
					throw new ArgumentException($"Cannot start cable build state with a building that cannot accept new connections {startingBuilding.gameObject.name}. Has {startingBuilding.BackendBuilding.NumConnected} / {startingBuilding.BackendBuilding.Data.maxPowerLines} connections.");
				_initialFromAttachment = startingBuilding;
			}

			if (cableCursor == null)
				throw new ArgumentNullException("cableCursor");

			_cableCursor = cableCursor;

			if (selectionCursor == null)
				throw new ArgumentNullException("selectionCursor");

			_selectionCursor = selectionCursor;
		}

		public void Start()
		{
			// Delegated to Start() to apply visual effects after the last visual effects
			// have been cleared (CleanUp).
			if (_initialFromAttachment != null)
				SetFromAttachment(_initialFromAttachment, false);
		}

		public BuildStateType GetStateType()
		{
			return BuildStateType.CABLE;
		}

		public CableCursorComponent GetCableCursor() => _cableCursor;

		public ICableAttachment GetFromAttachment()
		{
			return _fromAttachment;
		}

		public ICableAttachment GetToAttachment()
		{
			return _toAttachment;
		}

		/// <summary>
		/// CableBuildState is always making a new cable and is never moving an existing cable.
		/// </summary>
		public bool TryGetMovingCable(out CableComponent movingCable)
		{
			movingCable = null;
			return false;
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
		
		public static Cost GetCableCostFromLength(float length)
		{
			int cashCost = Mathf.CeilToInt(length * Data.cableCostMultiplier);

			return new Cost
			{
				cash = cashCost,
				science = 0
			};
		}

		public Cost GetCost()
		{
			return GetCableCostFromLength(Length);
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

		private static bool TryShowOrHideHoverHighlight(ICableAttachment attachment, bool showOrHide)
		{
			if (attachment == null)
				return false;

			if (!(attachment is BuildingInstanceCableAttachment buildingAttachment))
				return false;

			if (!buildingAttachment.IsVolatile)
				return false;

			if (showOrHide)
			{
				buildingAttachment.BuildingInstance.OnHoverEnter();
			}
			else
			{
				buildingAttachment.BuildingInstance.OnHoverExit();
			}

			return true;
		}

		
		/// <summary>
		/// Used internally to set the "from" end of the cable attachment.
		/// </summary>
		/// <param name="attachment">The attachment to use for the "from". Can be null.</param>
		private void SetFromAttachment(ICableAttachment attachment)
		{
			TryRegisterOrUnregisterDemolishListener(_fromAttachment, FromAttachment_OnDemolish, false);
			TryShowOrHideHoverHighlight(_fromAttachment, false);

			_fromAttachment = attachment;

			TryRegisterOrUnregisterDemolishListener(attachment, FromAttachment_OnDemolish, true);
			TryShowOrHideHoverHighlight(attachment, true);
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
			TryShowOrHideHoverHighlight(_toAttachment, false);

			_toAttachment = attachment;

			TryRegisterOrUnregisterDemolishListener(attachment, ToAttachment_OnDemolish, true);
			TryShowOrHideHoverHighlight(attachment, true);
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
			return _fromAttachment.GetIsSetAndNonVolatile();
		}

		public void Update(BuildStateInput input)
		{
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
		}

		public void UpdatePostConstraints(BuildStateInput input, BuildWarning.WarningType constraintsResult)
		{
			bool passesConstraints = constraintsResult < BuildWarning.WarningType.FATAL;

			UpdateCableCursorColor(constraintsResult);

			if (input.primaryFire)
			{
				// The player clicked on empty space before clicking on a building.
				if (_fromAttachment == null)
				{
					Cancel();
					return;
				}

				// If the player has yet to click on the "from" attachment and has just clicked
				// on a building, make the building a non-volatile "from" attachment (if applicable).
				BuildingInstanceCableAttachment fromBuildingAttachment = _fromAttachment as BuildingInstanceCableAttachment;
				if (fromBuildingAttachment.IsVolatile)
				{
					if (fromBuildingAttachment.BuildingInstance.BackendBuilding.CanAcceptNewConnections())
					{
						SetToAttachment(null);
						SetFromAttachment(fromBuildingAttachment.BuildingInstance, false);
					}
					else
					{
						OnApplyFailed.Invoke(); // Failed to attach to the hovered building.
						// TODO: Determine whether this should be interepreted as a failed apply.
					}
					return;
				}

				// _fromAttachment is a non-volatile BuildingInstanceCableAttachment by this point.
				// i.e. The player has already clicked on their starting building.
				Debug.Assert(!fromBuildingAttachment.IsVolatile);

				if (! passesConstraints)
				{
					// Get whether the player is hovering over something using the "to" attachment.
					bool isHoveringWithTo = _toAttachment != null && _toAttachment is BuildingInstanceCableAttachment;

					if (isHoveringWithTo)
					{
						// If the player is hovering over a building where they can't place a cable due to constraints,
						// switch to cabling from this building instead.
						BuildingInstanceCableAttachment toBuildingAttachment = _toAttachment as BuildingInstanceCableAttachment;
						if (toBuildingAttachment.BuildingInstance.BackendBuilding.CanAcceptNewConnections())
						{
							SetToAttachment(null);
							SetFromAttachment(toBuildingAttachment.BuildingInstance, false);
						}
						else
						{
							OnApplyFailed.Invoke();
						}
						return;
					}
					else
					{
						// The player clicked on empty space after clicking on a building.
						Cancel();
						return;
					}
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
				cost = GetCost(),
				cableInstance = cableInstance
			};
			OnApplied.Invoke(result);

			BuildingComponent newFromBuilding = (_toAttachment as BuildingInstanceCableAttachment).BuildingInstance;
			if (newFromBuilding.BackendBuilding.CanAcceptNewConnections())
			{
				SetFromAttachment(newFromBuilding, false);
				SetToAttachment(null); // Will be set to non-null on next update.
				return;
			}

			BuildingComponent oldFromBuilding = (_fromAttachment as BuildingInstanceCableAttachment).BuildingInstance;
			if (oldFromBuilding.BackendBuilding.CanAcceptNewConnections())
				return;

			SetFromAttachment(null); // Will be set to non-null on next update.
			SetToAttachment(null); // Will be set to non-null on next update.
		}

		/// <summary>
		/// Called internally when the player wants to get out of placing
		/// a cable.
		/// </summary>
		private void Cancel()
		{
			BuildStateTransitionSignal signal = new CancelTransitionSignal(false);
			OnRequestTransition.Invoke(signal);
		}

		/// <summary>
		/// Tries to chain using the current state of the cable build state
		/// and the passed building to construct.
		/// </summary>
		/// <param name="toBuild">The building to build in chained mode.</param>
		/// <returns>True if we were able to send a chain signal. False otherwise.</returns>
		private bool TryChain(BuildingSettingEntry toBuild)
		{
			if (toBuild.BuildingDataAsset.maxPowerLines <= 0)
				return false;

			if (!GetIsFromAttachmentSetAndNonVolatile())
				return false;

			BuildingComponent chainFrom = (_fromAttachment as BuildingInstanceCableAttachment).BuildingInstance;
			BuildStateTransitionSignal chainSignal = new ChainTransitionSignal(chainFrom, toBuild, false);
			OnRequestTransition.Invoke(chainSignal);
			return true;
		}

		public void CleanUp()
		{
			// Remove event listeners for the demolishment of buildings.
			SetFromAttachment(null);
			SetToAttachment(null);
			
			_cableCursor.Hide();
		}

		public bool TryOverrideExternalSignal(BuildStateTransitionSignal signal)
		{
			if (!signal.IsExternal)
				throw new ArgumentException("Cannot call TryOverrideExternalSignal with an internal signal.");

			// When the user presses on a building button whilst in the cable state, start chaining
			// from the current "from" building (if there is one).
			if (signal.GetSignalType() == BuildStateTransitionSignalType.BUILDING)
			{
				BuildingSettingEntry toBuild = (signal as BuildingTransitionSignal).NewBuildingType;
				bool chained = TryChain(toBuild);
				return chained;
			}

			return false;
		}
	}
}
