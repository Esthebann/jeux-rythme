using UnityEngine;

public class FreezeGame : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Debug.Log("Jeu en pause");
                Time.timeScale = 0f; // stoppe le temps
                AudioListener.pause = true; // met le son en pause
            }
            else
            {
                Debug.Log("Jeu repris");
                Time.timeScale = 1f; // reprend le temps
                AudioListener.pause = false; // reprend le son
            }
        }
    }
}
