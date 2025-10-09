using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FlexibleCarousel : MonoBehaviour
{
    [Header("Covers")]
    public List<Image> covers;

    [Header("Positions")]
    public Transform leftAnchor;
    public Transform centerAnchor;
    public Transform rightAnchor;

    [Header("Animation")]
    public float centerScale = 1.4f;
    public float sideScale = 0.8f;
    public float lerpSpeed = 10f;
    public float pulseAmplitude = 0.08f;
    public float pulseSpeed = 2f;

    [Header("Panel")]
    public GameObject levelInfoPanel;
    public TMP_Text levelTitleText;
    public Button startButton;
    public Button backButton;

    private int centerIndex = 0;
    private bool isPanelOpen = false;
    private float pulseTimer = 0f;
    private Vector3 offscreenPosition = new Vector3(0, -1000, 0);

    // Gestion de la sélection clavier dans le panel
    private enum PanelButton { Start, Back }
    private PanelButton currentPanelSelection = PanelButton.Start;

    private void Start()
    {
        // Ajout des listeners aux covers
        foreach (var img in covers)
        {
            Button btn = img.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnCoverClicked(img));
        }

        startButton.onClick.AddListener(OnStartClicked);
        backButton.onClick.AddListener(ClosePanel);

        levelInfoPanel.SetActive(false);
        UpdateCarousel(true);
    }

    private void Update()
    {
        if (!isPanelOpen)
            HandleCarouselInput();
        else
            HandlePanelInput();

        AnimateCarousel();
    }

    private void HandleCarouselInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            centerIndex = (centerIndex + 1) % covers.Count;
            UpdateCarousel();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            centerIndex = (centerIndex - 1 + covers.Count) % covers.Count;
            UpdateCarousel();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            OnCoverClicked(covers[centerIndex]);
        }
    }

    private void HandlePanelInput()
    {
        // Fermer le panel avec Escape ou Backspace
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            ClosePanel();
            return;
        }

        // Changer le bouton sélectionné avec flèche gauche/droite dans le panel
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPanelSelection = (currentPanelSelection == PanelButton.Start) ? PanelButton.Back : PanelButton.Start;
        }

        // Valider le bouton sélectionné avec Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentPanelSelection == PanelButton.Start)
                OnStartClicked();
            else
                ClosePanel();
        }
    }

    private void AnimateCarousel()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;

        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos;
            float targetScale;

            if (i == centerIndex)
            {
                targetPos = centerAnchor.localPosition;
                targetScale = centerScale + Mathf.Sin(pulseTimer) * pulseAmplitude;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftAnchor.localPosition;
                targetScale = sideScale;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightAnchor.localPosition;
                targetScale = sideScale;
            }
            else
            {
                targetPos = offscreenPosition;
                targetScale = sideScale;
            }

            RectTransform rect = covers[i].rectTransform;
            rect.localPosition = Vector3.Lerp(rect.localPosition, targetPos, Time.deltaTime * lerpSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one * targetScale, Time.deltaTime * lerpSpeed);
        }
    }

    private void UpdateCarousel(bool instant = false)
    {
        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos;
            float targetScale;

            if (i == centerIndex)
            {
                targetPos = centerAnchor.localPosition;
                targetScale = centerScale;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftAnchor.localPosition;
                targetScale = sideScale;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightAnchor.localPosition;
                targetScale = sideScale;
            }
            else
            {
                targetPos = offscreenPosition;
                targetScale = sideScale;
            }

            RectTransform rect = covers[i].rectTransform;
            if (instant)
            {
                rect.localPosition = targetPos;
                rect.localScale = Vector3.one * targetScale;
            }
        }
    }

    private void OnCoverClicked(Image clickedCover)
    {
        int clickedIndex = covers.IndexOf(clickedCover);

        if (clickedIndex != centerIndex)
        {
            centerIndex = clickedIndex;
            UpdateCarousel();
        }
        else
        {
            OpenPanel(clickedCover.name);
        }
    }

    private void OpenPanel(string levelName)
    {
        isPanelOpen = true;
        levelInfoPanel.SetActive(true);

        if (levelTitleText != null)
            levelTitleText.text = "Niveau : " + levelName;

        // Start sélectionné par défaut
        currentPanelSelection = PanelButton.Start;
    }

    private void ClosePanel()
    {
        levelInfoPanel.SetActive(false);
        isPanelOpen = false;
        UpdateCarousel(true);
    }

    private void OnStartClicked()
    {
        GameManager.instance.selectedLevelName = covers[centerIndex].name;
        GameManager.instance.StartSelectedLevel();
    }
}
