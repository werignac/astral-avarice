using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private UIDocument pauseUI;
    [SerializeField] private GameController gameController;
    [SerializeField] private AstralAvarice.Frontend.HowToPlayUI howToPlayUI;

    private Button unpauseButton;
	private Button restartButton;
    private Button quitButton;
    private Button howToPlayButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unpauseButton = pauseUI.rootVisualElement.Q<Button>("UnpauseButton");
		restartButton = pauseUI.rootVisualElement.Q<Button>("RestartButton");
        quitButton = pauseUI.rootVisualElement.Q<Button>("QuitButton");
        howToPlayButton = pauseUI.rootVisualElement.Q<Button>("HowToPlay");

		unpauseButton.RegisterCallback<ClickEvent>(UnpauseClicked);
		restartButton.RegisterCallback<ClickEvent>(RestartClicked);
        quitButton.RegisterCallback<ClickEvent>(QuitClicked);
        howToPlayButton.RegisterCallback<ClickEvent>(HowToPlayClicked);

        
		pauseUI.rootVisualElement.style.display = DisplayStyle.None;
        howToPlayUI.OnPlayerClosed.AddListener(HowToPlayUI_OnPlayerClosed);
        howToPlayUI.Hide();
    }

	private void RestartClicked(ClickEvent evt)
	{
		gameController.ReloadScene();
	}

	public void UnpauseClicked(ClickEvent click)
    {
		UnpauseGame();
    }

	public void UnpauseGame()
	{
		gameController.UnpauseGame();
		pauseUI.rootVisualElement.style.display = DisplayStyle.None;
	}

    private void QuitClicked(ClickEvent click)
    {
        gameController.ReturnToMenu();
    }

    private void HowToPlayClicked(ClickEvent click)
    {
        howToPlayUI.Show();
        pauseUI.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void HowToPlayUI_OnPlayerClosed()
    {
        pauseUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void PauseGame()
    {
        gameController.PauseGame();
        pauseUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }
}
