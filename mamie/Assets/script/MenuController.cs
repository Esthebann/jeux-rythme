using UnityEngine;

public class MenuController : MonoBehaviour
{
    private GameManager manager;

    private void Start()
    {
        manager = GameManager.instance;  
    }

    public void ChangeScene(string MenuPrincipal)
    {
        manager.ChangeScene(MenuPrincipal); 
    }

    public void QuitGame()
    {
        manager.QuitGame();
    }
}
