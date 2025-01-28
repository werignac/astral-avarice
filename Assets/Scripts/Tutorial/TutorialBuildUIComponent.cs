using UnityEngine;
using UnityEngine.UIElements;

public class TutorialBuildUIComponent : BuildUIComponent
{
    [SerializeField] private TutorialGameController tutorialController;

    protected override void CableButton_OnClick(ClickEvent evt)
    {
        if (tutorialController.cablesAreAllowed())
        {
            base.CableButton_OnClick(evt);
        }
    }

    protected override void BuildingButton_OnClick(BuildingSettingEntry toBuild)
    {
        if (tutorialController.buildingAllowed(toBuild.BuildingDataAsset))
        {
            base.BuildingButton_OnClick(toBuild);
        }
    }

    protected override void DemolishButton_OnClick(ClickEvent evt)
    {
    }
}
