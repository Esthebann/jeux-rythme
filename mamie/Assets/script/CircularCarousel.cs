using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlexibleCarousel : MonoBehaviour
{
    [Header("Covers (MaskCircles à assigner)")]
    public List<Image> covers = new List<Image>();

    [Header("Positions visibles (définies dans l'éditeur)")]
    public Transform leftAnchor;
    public Transform centerAnchor;
    public Transform rightAnchor;

    [Header("Échelle / Taille")]
    [Tooltip("Taille du cover central (1 = taille normale)")]
    public float centerScale = 1.8f; // plus grand mais uniforme
    [Tooltip("Taille des covers latéraux")]
    public float sideScale = 0.8f;

    [Header("Animation")]
    public float lerpSpeed = 10f;
    [Tooltip("Amplitude du zoom pulsant (0 = pas de pulsation)")]
    public float pulseAmplitude = 0.1f;
    [Tooltip("Vitesse de la pulsation")]
    public float pulseSpeed = 2f;

    private int centerIndex = 0;

    void Start()
    {
        if (covers == null || covers.Count < 3)
        {
            Debug.LogError("Il faut au moins 3 covers !");
            return;
        }

        if (leftAnchor == null || centerAnchor == null || rightAnchor == null)
        {
            Debug.LogError("Tu dois assigner les 3 ancres (Left, Center, Right) dans l'inspecteur !");
            return;
        }

        UpdateCarousel(true);
    }

    void Update()
    {
        if (covers == null || covers.Count < 3) return;

        // Navigation clavier
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

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Niveau sélectionné : " + covers[centerIndex].name);
        }

        // Animation fluide
        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos;
            Vector3 targetScale;
            int zIndex;

            if (i == centerIndex)
            {
                targetPos = centerAnchor.localPosition;

                // 🔥 effet de pulsation uniforme (zoom doux)
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                float scale = centerScale + pulse;

                targetScale = Vector3.one * scale;
                zIndex = 2;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftAnchor.localPosition;
                targetScale = Vector3.one * sideScale;
                zIndex = 1;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightAnchor.localPosition;
                targetScale = Vector3.one * sideScale;
                zIndex = 1;
            }
            else
            {
                targetPos = new Vector3(0, -2000, 0);
                targetScale = Vector3.one * sideScale;
                zIndex = 0;
            }

            RectTransform rect = covers[i].rectTransform;
            rect.localPosition = Vector3.Lerp(rect.localPosition, targetPos, Time.deltaTime * lerpSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.deltaTime * lerpSpeed);
            covers[i].transform.SetSiblingIndex(zIndex);
        }
    }

    void UpdateCarousel(bool instant = false)
    {
        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos;
            Vector3 targetScale;
            int zIndex;

            if (i == centerIndex)
            {
                targetPos = centerAnchor.localPosition;
                targetScale = Vector3.one * centerScale;
                zIndex = 2;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftAnchor.localPosition;
                targetScale = Vector3.one * sideScale;
                zIndex = 1;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightAnchor.localPosition;
                targetScale = Vector3.one * sideScale;
                zIndex = 1;
            }
            else
            {
                targetPos = new Vector3(0, -2000, 0);
                targetScale = Vector3.one * sideScale;
                zIndex = 0;
            }

            RectTransform rect = covers[i].rectTransform;

            if (instant)
            {
                rect.localPosition = targetPos;
                rect.localScale = targetScale;
                covers[i].transform.SetSiblingIndex(zIndex);
            }
        }
    }
}
