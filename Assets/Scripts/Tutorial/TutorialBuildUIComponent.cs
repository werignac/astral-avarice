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

    protected override void BuildingButton_OnClick(BuildingSettingEntry toBuild, BuildButtonBinding _)
    {
        if (tutorialController.buildingAllowed(toBuild.BuildingDataAsset))
        {
            base.BuildingButton_OnClick(toBuild, _);
        }
    }

    protected override void DemolishButton_OnClick(ClickEvent evt)
    {
    }
}
