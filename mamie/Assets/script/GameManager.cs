using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public string selectedLevelName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartSelectedLevel()
    {
        if (!string.IsNullOrEmpty(selectedLevelName))
        {
            Debug.Log("Chargement du niveau : " + selectedLevelName);
            SceneManager.LoadScene(selectedLevelName);
        }
        else
        {
            Debug.LogWarning("Aucun niveau sélectionné !");
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void QuitGame()
    {
        Debug.Log("Quitte le jeu...");
        Application.Quit();
    }
}
