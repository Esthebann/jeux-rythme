using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CircularCarousel : MonoBehaviour
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
    public TMP_Text scoreText;
    public TMP_Text accuracyText;
    public TMP_Text maxComboText;

    [Header("Panel Buttons")]
    public Button StartButton;
    public Button BackButton;

    [Header("Home Button")]
    public Button HomeButton;

    public enum Selection { Carousel, HomeButton, PanelStart, PanelBack }
    [HideInInspector] public Selection currentSelection = Selection.Carousel;
    [HideInInspector] public int centerIndex = 0;
    [HideInInspector] public bool isPanelOpen = false;

    private float pulseTimer = 0f;
    private Vector3 offscreenPosition = new Vector3(0, -1000, 0);

    private void Start()
    {
        // Désactiver tous les listeners pour éviter conflits
        if (HomeButton != null) HomeButton.onClick.RemoveAllListeners();
        if (StartButton != null) StartButton.onClick.RemoveAllListeners();
        if (BackButton != null) BackButton.onClick.RemoveAllListeners();

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
        // Navigation haut/bas pour HomeButton
        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelection = Selection.HomeButton;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelection == Selection.HomeButton)
            currentSelection = Selection.Carousel;

        // Déplacement carrousel
        if (currentSelection == Selection.Carousel)
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
        }

        // Entrée
        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (currentSelection)
            {
                case Selection.Carousel:
                    OpenPanel(covers[centerIndex].name);
                    break;
                case Selection.HomeButton:
                    GameManager.instance.BackToMainMenu();
                    break;
            }
        }
    }

    private void HandlePanelInput()
    {
        // Fermer panel avec Esc ou Backspace
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            ClosePanel();
            return;
        }

        // Flèches gauche/droite pour naviguer entre Start et Back
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentSelection = (currentSelection == Selection.PanelStart) ? Selection.PanelBack : Selection.PanelStart;
            HighlightPanelSelection();
        }

        // Entrée pour activer Start / Back
        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (currentSelection)
            {
                case Selection.PanelStart:
                    StartLevelFromPanel();
                    break;
                case Selection.PanelBack:
                    ClosePanel();
                    break;
            }
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

    public void UpdateCarousel(bool instant = false)
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

    public void OpenPanel(string levelName)
    {
        isPanelOpen = true;
        levelInfoPanel.SetActive(true);

        if (levelTitleText != null)
            levelTitleText.text = "Niveau : " + levelName;
        if (scoreText != null) scoreText.text = $"Score: {PlayerPrefs.GetInt(levelName + "_Score", 0)}";
        if (accuracyText != null) accuracyText.text = $"Accuracy: {PlayerPrefs.GetFloat(levelName + "_Accuracy", 0f):F2}%";
        if (maxComboText != null) maxComboText.text = $"Max Combo: {PlayerPrefs.GetInt(levelName + "_MaxCombo", 0)}";

        currentSelection = Selection.PanelStart;
        HighlightPanelSelection();
    }

    public void ClosePanel()
    {
        levelInfoPanel.SetActive(false);
        isPanelOpen = false;
        currentSelection = Selection.Carousel;
        UpdateCarousel(true);
    }

    public void StartLevelFromPanel()
    {
        GameManager.instance.selectedLevelName = covers[centerIndex].name;
        GameManager.instance.StartSelectedLevel();
    }

    private void HighlightPanelSelection()
    {
        if (StartButton != null)
            StartButton.GetComponent<Image>().color = (currentSelection == Selection.PanelStart) ? Color.yellow : Color.white;
        if (BackButton != null)
            BackButton.GetComponent<Image>().color = (currentSelection == Selection.PanelBack) ? Color.yellow : Color.white;
    }
}
