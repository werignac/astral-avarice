using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private GameObject levelObject;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = new GameManager(this);
        if (Data.selectedMission != null)
        {
            levelObject = Instantiate<GameObject>(Resources.Load<GameObject>("Levels/" + Data.selectedMission.name));
            gameManager.StartMission(Data.selectedMission, levelObject.GetComponent<LevelBuilder>());
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
