using UnityEngine;

public class LevelInfoPanelController : MonoBehaviour
{
    // Permet de forcer la fermeture du panel
    public void ForceClosePanel()
    {
        gameObject.SetActive(false);
    }
}
