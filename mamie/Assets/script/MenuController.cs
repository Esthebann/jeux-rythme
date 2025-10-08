using UnityEngine;

public class MenuController : MonoBehaviour
{
public void StartGame()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}
