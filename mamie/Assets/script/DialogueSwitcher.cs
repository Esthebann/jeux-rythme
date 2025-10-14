using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogueSwitcher: MonoBehaviour
{
    [Header("UI Elements")]
    public Image bubbleA;        // Bulle du perso 1
    public TMP_Text textA;
    public Image bubbleB;        // Bulle du perso 2
    public TMP_Text textB;
    public TMP_Text nameText;    // Optionnel

    [Header("Dialogues Settings")]
    [TextArea(3, 10)]
    public string[] dialogues;   // Les textes
    public string[] speakerNames; // Noms (facultatif)
    public bool[] isSpeakerA;    // true = perso A parle, false = perso B
    public float typingSpeed = 0.03f;

    [Header("Scene Transition")]
    public string nextSceneName = "Level1";

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool canContinue = false;

    void Start()
    {
        // On s'assure qu'une bulle est active au départ
        bubbleA.gameObject.SetActive(false);
        bubbleB.gameObject.SetActive(false);

        currentIndex = 0;
        StartCoroutine(TypeDialogue());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
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
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        canContinue = false;

        // Active la bonne bulle
        bool aSpeaks = isSpeakerA[currentIndex];
        bubbleA.gameObject.SetActive(aSpeaks);
        bubbleB.gameObject.SetActive(!aSpeaks);

        TMP_Text currentText = aSpeaks ? textA : textB;
        currentText.text = "";

        if (nameText != null && speakerNames.Length > currentIndex)
            nameText.text = speakerNames[currentIndex];
        else if (nameText != null)
            nameText.text = "";

        // Effet machine à écrire
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
}
