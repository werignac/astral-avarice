using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private UIDocument pauseUI;
    [SerializeField] private GameController gameController;

    private Button unpauseButton;
	private Button restartButton;
    private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unpauseButton = pauseUI.rootVisualElement.Q<Button>("UnpauseButton");
		restartButton = pauseUI.rootVisualElement.Q<Button>("RestartButton");
        quitButton = pauseUI.rootVisualElement.Q<Button>("QuitButton");

		unpauseButton.RegisterCallback<ClickEvent>(UnpauseClicked);
		restartButton.RegisterCallback<ClickEvent>(RestartClicked);
        quitButton.RegisterCallback<ClickEvent>(QuitClicked);
        
		pauseUI.rootVisualElement.style.display = DisplayStyle.None;
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

    public void PauseGame()
    {
        gameController.PauseGame();
        pauseUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }
}
