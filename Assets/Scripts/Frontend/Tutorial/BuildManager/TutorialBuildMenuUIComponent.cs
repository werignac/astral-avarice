using UnityEngine;
using UnityEngine.UIElements;

public class TutorialBuildMenuUIComponent : BuildMenuUIComponent
{
    [SerializeField] private TutorialGameController tutorialController;

	// When a cable is clicked, only allow entering cable mode if cables are allowed.
	protected override void OnCableClicked()
	{
		if (tutorialController.cablesAreAllowed())
		{
			base.OnCableClicked();
		}
	}

	// Only enter demolish if allowed in the current state of the tutorial.
	protected override void OnDemolishClicked()
	{
		if(tutorialController.demolishAllowed())
        {
			base.OnDemolishClicked();
        }
	}

	// Only enter move state if allowed in the current state of the tutorial.
	protected override void OnMoveClicked()
    {
		if (tutorialController.moveAllowed())
		{
			base.OnMoveClicked();
		}
    }
}
