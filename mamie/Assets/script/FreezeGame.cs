using UnityEngine;

public class FreezeGame : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            // Pause uniquement la musique si tu veux, pas tous les sons
            AudioListener.pause = isPaused;

            Debug.Log(isPaused ? "Jeu en pause" : "Jeu repris");
        }
    }
}
