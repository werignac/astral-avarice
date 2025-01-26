using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    private GameObject levelObject;
    private GameManager gameManager;

	[HideInInspector] public UnityEvent OnLevelLoad = new UnityEvent();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = new GameManager(this);
        if (Data.selectedMission != null)
        {
            levelObject = Instantiate<GameObject>(Resources.Load<GameObject>("Levels/" + Data.selectedMission.name));
            gameManager.StartMission(Data.selectedMission, levelObject.GetComponent<LevelBuilder>());

			OnLevelLoad?.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        gameManager.Update(Time.deltaTime);
    }

    public void EndGame()
    {
        Debug.Log("Game has ended");
        SceneManager.LoadScene("MainMenu");
    }
}
