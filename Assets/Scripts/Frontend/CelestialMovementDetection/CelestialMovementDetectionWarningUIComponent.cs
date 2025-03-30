using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
    public class CelestialMovementDetectionWarningUIComponent : MonoBehaviour
    {
		private const string CELESTIAL_MOVEMENT_DETECTION_WARNING_CONTAINER_ELEMENT_NAME = "CelestialMovementDetectionWarning";

		private const string CELESTIAL_MOVEMENT_DETECTION_WARNING_ELEMENT_NAME = "WarningContainer";

		private const string HIDE_WARNING_CLASS_NAME = "warningHidden";
		
		[SerializeField] private UIDocument warningUIDocument;

		private VisualElement celestialMovementDetectionWarningElement;

		[SerializeField] private CelestialMovementDetectionComponent detectionComponent;


		private void Awake()
		{
			VisualElement warningContainer = warningUIDocument.rootVisualElement.Q(CELESTIAL_MOVEMENT_DETECTION_WARNING_CONTAINER_ELEMENT_NAME);
			celestialMovementDetectionWarningElement = warningContainer.Q(CELESTIAL_MOVEMENT_DETECTION_WARNING_ELEMENT_NAME);

			detectionComponent.OnFirstPlanetStartMoving.AddListener(Detection_OnFirstPlanetStartMoving);
			detectionComponent.OnAllPlanetsStopMoving.AddListener(Detection_OnAllPlanetsStopMoving);
		}

		private void Start()
		{
			HideWarning();
		}

		private void Detection_OnFirstPlanetStartMoving()
		{
			ShowWarning();
		}

		private void Detection_OnAllPlanetsStopMoving()
		{
			HideWarning();
		}

		private void ShowWarning()
		{
			celestialMovementDetectionWarningElement.RemoveFromClassList(HIDE_WARNING_CLASS_NAME);
		}

		private void HideWarning()
		{
			celestialMovementDetectionWarningElement.AddToClassList(HIDE_WARNING_CLASS_NAME);
		}
	}
}
