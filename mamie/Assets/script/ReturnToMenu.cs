using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [Header("Bouton et scène")]
    public Button menuButton;          // Ton bouton UI
    public string menuSceneName = "MenuPrincipal";

    void Start()
    {
        if (menuButton != null)
        {
            // On ajoute l'événement du bouton pour le clic souris
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    void Update()
    {
        // Détection de la touche Entrée
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GoToMenu();
        }
    }

    void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
