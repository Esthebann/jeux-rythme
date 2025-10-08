using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { private set; get; }

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

    public void ChangeScene(string MenuPrincipal)
    {
        SceneManager.LoadScene(MenuPrincipal);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
