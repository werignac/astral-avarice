using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class PlanetInspectorController : IInspectorController
{
	private Label subheading;
    private VisualElement icon;
	private VisualElement placeable;
	private Label placeableText;
    private VisualElement baseMass;
    private Label baseMassValue;
    private VisualElement totalMass;
    private Label totalMassValue;
    private VisualElement solarOutput;
    private Label solarOutputValue;
    private VisualElement specialResources;
    private VisualElement coal;
    private Label currentCoalValue;
    private VisualElement coalDivider;
    private Label maxCoalValue;
    private VisualElement solar;
    private Label currentSolarValue;
    private VisualElement solarDivider;
    private Label maxSolarValue;
    private VisualElement wind;
    private Label currentWindValue;
    private VisualElement windDivider;
    private Label maxWindValue;
    private VisualElement thermal;
    private Label currentThermalValue;
    private VisualElement thermalDivider;
    private Label maxThermalValue;

    private PlanetComponent displayingPlanet;

    public PlanetInspectorController(PlanetComponent planet)
    {
        displayingPlanet = planet;
    }

	public void ConnectInspectorUI(TemplateContainer inspectorUI)
	{
		subheading = inspectorUI.Q<Label>("Subheading");
		icon = inspectorUI.Q("Icon");
		placeable = inspectorUI.Q("Placeable");
		placeableText = inspectorUI.Q<Label>("PlaceableLabel");
		baseMass = inspectorUI.Q("BaseMass");
		baseMassValue = inspectorUI.Q<Label>("BaseMassValue");
        totalMass = inspectorUI.Q("TotalMass");
        totalMassValue = inspectorUI.Q<Label>("TotalMassValue");
        solarOutput = inspectorUI.Q("SolarOutput");
        solarOutputValue = inspectorUI.Q<Label>("SolarOutputValue");
        specialResources = inspectorUI.Q("SpecialResources");
        coal = inspectorUI.Q("Coal");
        currentCoalValue = inspectorUI.Q<Label>("CurrentCoalValue");
        coalDivider = inspectorUI.Q("CoalDivider");
        maxCoalValue = inspectorUI.Q<Label>("MaxCoalValue");
        solar = inspectorUI.Q("Solar");
        currentSolarValue = inspectorUI.Q<Label>("CurrentSolarValue");
        solarDivider = inspectorUI.Q("SolarDivider");
        maxSolarValue = inspectorUI.Q<Label>("MaxSolarValue");
        wind = inspectorUI.Q("Wind");
        currentWindValue = inspectorUI.Q<Label>("CurrentWindValue");
        windDivider = inspectorUI.Q("WindDivider");
        maxWindValue = inspectorUI.Q<Label>("MaxWindValue");
        thermal = inspectorUI.Q("Thermal");
        currentThermalValue = inspectorUI.Q<Label>("CurrentThermalValue");
        thermalDivider = inspectorUI.Q("ThermalDivider");
        maxThermalValue = inspectorUI.Q<Label>("MaxThermalValue");
    }

	public void DisconnectInspectorUI()
	{

	}
	public void UpdateUI()
	{

        subheading.text = displayingPlanet.PlanetName;

        if (displayingPlanet.CanPlaceBuildings)
        {
            placeableText.text = "Building Possible";
        }
        else
        {
            placeableText.text = "Impossible to Build";
        }

        baseMassValue.text = displayingPlanet.Mass.ToString();
        totalMassValue.text = displayingPlanet.GetTotalMass().ToString();

        int s = Mathf.FloorToInt(displayingPlanet.SolarOutput - (displayingPlanet.transform.localScale.x / 2));
        if (s > 0)
        {
            solarOutputValue.text = s.ToString();
        }
        else
        {
            solarOutput.style.display = DisplayStyle.None;
        }

        bool noResources = true;
        for(int i = 0; i < (int)ResourceType.Resource_Count; ++i)
        {
            ResourceType consideredResource = (ResourceType)i;
            int resourceCount = displayingPlanet.GetResourceCount(consideredResource);
            if (resourceCount > 0)
            {
                noResources = false;
                switch (consideredResource)
                {
                    case ResourceType.Coal:
                        currentCoalValue.text = displayingPlanet.GetAvailableResourceCount(consideredResource).ToString();
                        maxCoalValue.text = resourceCount.ToString();
                        break;
                    case ResourceType.Solar:
                        currentSolarValue.text = displayingPlanet.GetAvailableResourceCount(consideredResource).ToString();
                        maxSolarValue.text = resourceCount.ToString();
                        break;
                    case ResourceType.Wind:
                        currentWindValue.text = displayingPlanet.GetAvailableResourceCount(consideredResource).ToString();
                        maxWindValue.text = resourceCount.ToString();
                        break;
                    case ResourceType.Thermal:
                        currentThermalValue.text = displayingPlanet.GetAvailableResourceCount(consideredResource).ToString();
                        maxThermalValue.text = resourceCount.ToString();
                        break;

                }
            }
            else
            {
                switch (consideredResource)
                {
                    case ResourceType.Coal:
                        coal.style.display = DisplayStyle.None;
                        break;
                    case ResourceType.Solar:
                        solar.style.display = DisplayStyle.None;
                        break;
                    case ResourceType.Wind:
                        wind.style.display = DisplayStyle.None;
                        break;
                    case ResourceType.Thermal:
                        thermal.style.display = DisplayStyle.None;
                        break;

                }
            }
        }
        if(noResources)
        {
            specialResources.style.display = DisplayStyle.None;
        }
    }
}
