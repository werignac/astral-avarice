using UnityEngine;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private int numElements;
    private VisualElement[] tutorialElements;
    private int currentlyShown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tutorialElements = new VisualElement[numElements];

        for(int i = 0; i < numElements; ++i)
        {
            tutorialElements[i] = document.rootVisualElement.Q("Tutorial" + i);
            if(i == 0)
            {
                tutorialElements[i].visible = true;
            }
            else
            {
                tutorialElements[i].visible = false;
            }
        }
        currentlyShown = 0;
    }

    public void ShowNextElement()
    {
        tutorialElements[currentlyShown].visible = false;
        ++currentlyShown;
        if(currentlyShown < numElements)
        {
            tutorialElements[currentlyShown].visible = true;
        }
    }
}
