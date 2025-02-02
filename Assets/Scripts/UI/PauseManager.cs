using UnityEngine;
using UnityEngine.UIElements;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private UIDocument pauseUI;
    [SerializeField] private GameController gameController;

    private Button unpauseButton;
    private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unpauseButton = pauseUI.rootVisualElement.Q<Button>("UnpauseButton");
        unpauseButton.RegisterCallback<ClickEvent>(UnpauseClicked);
        quitButton = pauseUI.rootVisualElement.Q<Button>("QuitButton");
        quitButton.RegisterCallback<ClickEvent>(QuitClicked);
        pauseUI.rootVisualElement.style.display = DisplayStyle.None;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    private void UnpauseClicked(ClickEvent click)
    {
        gameController.UnpauseGame();
        pauseUI.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void QuitClicked(ClickEvent click)
    {
        gameController.ReturnToMenu();
    }

    public void PauseGame()
    {
        gameController.PauseGame();
        pauseUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }
}
