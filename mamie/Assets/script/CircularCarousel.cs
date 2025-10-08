using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ThreePositionCarousel_FullHD_Optimized : MonoBehaviour
{
    public List<Image> covers;

    [Header("Positions visibles pour 1920x1080")]
    public Vector3 leftPos = new Vector3(-350, -150, 0);
    public Vector3 centerPos = new Vector3(0, 100, 0);
    public Vector3 rightPos = new Vector3(350, -150, 0);

    [Header("Scale")]
    public float centerScale = 1f;  // taille originale RectTransform
    public float sideScale = 0.7f;  // latérales plus petites

    [Header("Animation")]
    public float lerpSpeed = 10f;

    private int centerIndex = 0;

    void Start()
    {
        if (covers == null || covers.Count < 3)
        {
            Debug.LogError("Il faut au moins 3 covers !");
            return;
        }

        UpdateCarousel(true);
    }

    void Update()
    {
        if (covers == null || covers.Count < 3) return;

        // navigation
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

        // smooth animation
        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos = Vector3.zero;
            float targetScale = sideScale;
            int zIndex = 0;

            if (i == centerIndex)
            {
                targetPos = centerPos;
                targetScale = centerScale;
                zIndex = 2;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftPos;
                targetScale = sideScale;
                zIndex = 1;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightPos;
                targetScale = sideScale;
                zIndex = 1;
            }
            else
            {
                targetPos = new Vector3(0, -1000, 0);
                targetScale = sideScale;
                zIndex = 0;
            }

            RectTransform rect = covers[i].rectTransform;
            rect.localPosition = Vector3.Lerp(rect.localPosition, targetPos, Time.deltaTime * lerpSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one * targetScale, Time.deltaTime * lerpSpeed);
            covers[i].transform.SetSiblingIndex(zIndex);
        }
    }

    void UpdateCarousel(bool instant = false)
    {
        for (int i = 0; i < covers.Count; i++)
        {
            Vector3 targetPos = Vector3.zero;
            float targetScale = sideScale;
            int zIndex = 0;

            if (i == centerIndex)
            {
                targetPos = centerPos;
                targetScale = centerScale;
                zIndex = 2;
            }
            else if (i == (centerIndex - 1 + covers.Count) % covers.Count)
            {
                targetPos = leftPos;
                targetScale = sideScale;
                zIndex = 1;
            }
            else if (i == (centerIndex + 1) % covers.Count)
            {
                targetPos = rightPos;
                targetScale = sideScale;
                zIndex = 1;
            }
            else
            {
                targetPos = new Vector3(0, -1000, 0);
                targetScale = sideScale;
                zIndex = 0;
            }

            RectTransform rect = covers[i].rectTransform;

            if (instant)
            {
                rect.localPosition = targetPos;
                rect.localScale = Vector3.one * targetScale;
                covers[i].transform.SetSiblingIndex(zIndex);
            }
        }
    }
}
