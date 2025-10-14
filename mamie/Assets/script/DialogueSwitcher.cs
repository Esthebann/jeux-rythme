using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogueSwitcher : MonoBehaviour
{
    [Header("UI Elements")]
    public Image bubbleA;        // Bulle du perso 1
    public TMP_Text textA;
    public Image bubbleB;        // Bulle du perso 2
    public TMP_Text textB;
    public TMP_Text nameText;    // Optionnel
    public Button skipButton;    // 🔹 Nouveau bouton Skip

    [Header("Dialogues Settings")]
    [TextArea(3, 10)]
    public string[] dialogues;
    public string[] speakerNames;
    public bool[] isSpeakerA;
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    public string nextSceneName = "Level1";

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool canContinue = false;
    private bool skipSelected = false; // 🔹 pour navigation flèches

    void Start()
    {
        bubbleA.gameObject.SetActive(false);
        bubbleB.gameObject.SetActive(false);

        currentIndex = 0;
        StartCoroutine(TypeDialogue());

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
            skipButton.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // 🔹 Navigation gauche/droite entre le mode dialogue et le bouton Skip
        if (Input.GetKeyDown(KeyCode.RightArrow))
            skipSelected = true;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            skipSelected = false;

        // 🔹 Action selon le mode sélectionné
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (skipSelected)
            {
                SkipDialogue();
                return;
            }

            if (isTyping)
            {
                StopAllCoroutines();
                ShowFullDialogue();
            }
            else if (canContinue)
            {
                currentIndex++;
                if (currentIndex < dialogues.Length)
                    StartCoroutine(TypeDialogue());
                else
                    StartCoroutine(LoadNextScene());
            }
        }

        // 🔹 Feedback visuel du bouton sélectionné
        if (skipButton != null)
        {
            var colors = skipButton.colors;
            colors.normalColor = skipSelected ? new Color(0.8f, 0.8f, 1f) : Color.white;
            skipButton.colors = colors;
        }
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        canContinue = false;

        bool aSpeaks = isSpeakerA[currentIndex];
        bubbleA.gameObject.SetActive(aSpeaks);
        bubbleB.gameObject.SetActive(!aSpeaks);

        TMP_Text currentText = aSpeaks ? textA : textB;
        currentText.text = "";

        if (nameText != null && speakerNames.Length > currentIndex)
            nameText.text = speakerNames[currentIndex];
        else if (nameText != null)
            nameText.text = "";

        foreach (char letter in dialogues[currentIndex])
        {
            currentText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        canContinue = true;
    }

    void ShowFullDialogue()
    {
        bool aSpeaks = isSpeakerA[currentIndex];
        TMP_Text currentText = aSpeaks ? textA : textB;
        currentText.text = dialogues[currentIndex];
        isTyping = false;
        canContinue = true;
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    // 🔹 Fonction Skip
    public void SkipDialogue()
    {
        StopAllCoroutines();
        StartCoroutine(LoadNextScene());
    }
}
