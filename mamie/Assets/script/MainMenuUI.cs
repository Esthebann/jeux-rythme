using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        // On sécurise les liens au GameManager au cas où la scène a été rechargée
        if (playButton != null)
            playButton.onClick.AddListener(() => GameManager.instance.ChangeScene("LevelChoice"));

        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameManager.instance.QuitGame());
    }
}
