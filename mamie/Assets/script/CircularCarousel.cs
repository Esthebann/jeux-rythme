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

    public enum Selection { Carousel, HomeButton }
    [HideInInspector] public Selection currentSelection = Selection.Carousel;
    [HideInInspector] public int centerIndex = 0;

    private float pulseTimer = 0f;
    private Vector3 offscreenPosition = new Vector3(0, -1000, 0);

    private void Start()
    {
        if (HomeButton != null) HomeButton.onClick.RemoveAllListeners();
        if (StartButton != null) StartButton.onClick.RemoveAllListeners();
        if (BackButton != null) BackButton.onClick.RemoveAllListeners();

        if (levelInfoPanel != null)
            levelInfoPanel.SetActive(true);

        UpdateCarousel(true);
        UpdatePanelInfo();
    }

    private void Update()
    {
        HandleCarouselInput();
        AnimateCarousel();
    }

    private void HandleCarouselInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelection = Selection.HomeButton;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelection == Selection.HomeButton)
            currentSelection = Selection.Carousel;

        if (currentSelection == Selection.Carousel)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                centerIndex = (centerIndex + 1) % covers.Count;
                UpdateCarousel();
                UpdatePanelInfo();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                centerIndex = (centerIndex - 1 + covers.Count) % covers.Count;
                UpdateCarousel();
                UpdatePanelInfo();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (currentSelection)
            {
                case Selection.Carousel:
                    StartLevelFromPanel();
                    break;
                case Selection.HomeButton:
                    GameManager.instance.BackToMainMenu();
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

    private void UpdatePanelInfo()
    {
        string levelName = covers[centerIndex].name;

        if (levelTitleText != null)
            levelTitleText.text = "Niveau : " + levelName;

        if (scoreText != null)
            scoreText.text = $"Best Score: {PlayerPrefs.GetInt(levelName + "_BestScore", 0)}";

        if (accuracyText != null)
            accuracyText.text = $"Best Accuracy: {PlayerPrefs.GetFloat(levelName + "_BestAccuracy", 0f):F2}%";

        if (maxComboText != null)
            maxComboText.text = $"Best Combo: {PlayerPrefs.GetInt(levelName + "_BestCombo", 0)}";
    }

    public void StartLevelFromPanel()
    {
        GameManager.instance.selectedLevelName = covers[centerIndex].name;
        GameManager.instance.StartSelectedLevel();
    }
}
