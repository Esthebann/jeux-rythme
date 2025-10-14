using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FreezeGame : MonoBehaviour
{
    [Header("Panel de pause")]
    public GameObject pausePanel;

    [Header("Boutons du panel")]
    public Button resumeButton;
    public Button restartButton;
    public Button homeButton;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Lier les boutons — protège contre null
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(GoHome);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }

        if (isPaused && Input.GetKeyDown(KeyCode.Return))
        {
            // Si un bouton UI est sélectionné -> simule son click
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

            if (selected != null)
            {
                // Essaye d'obtenir un component Button et invoquer son onClick
                Button btn = selected.GetComponent<Button>();
                if (btn != null && btn.interactable)
                {
                    btn.onClick.Invoke();
                }
                else
                {
                    // Si ce n'est pas un bouton (ou non interactable), fallback sur Resume
                    ResumeGame();
                }
            }
            else
            {
                // Aucun élément sélectionné -> fallback sur Resume
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Sélection par défaut sur Resume
        if (EventSystem.current != null && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        Debug.Log("⏸ Jeu en pause");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Debug.Log("▶️ Jeu repris");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Optionnel : désactiver le panel tout de suite pour éviter doublons
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Relancer la scène
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("🔁 Redémarrage de la scène");
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("Menuprincipal"); // adapte le nom si besoin
        Debug.Log("🏠 Retour au menu principal");
    }
}
