using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    // Nom du niveau actuellement sélectionné (défini par le carrousel)
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

    // 🔹 Chargement d'une scène par nom (compatible avec les autres scripts)
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 🔹 Lance le niveau actuellement sélectionné (utilisé par le carrousel)
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

    // 🔹 Retourne au menu principal
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    // 🔹 Quitte le jeu
    public void QuitGame()
    {
        Debug.Log("Quitte le jeu...");
        Application.Quit();
    }
}
