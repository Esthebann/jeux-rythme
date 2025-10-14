using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionBlur2D : MonoBehaviour
{
    [Header("Effet de flou visible")]
    public int trailCount = 8;           // nombre de copies
    public float spacingDistance = 20f;  // distance entre les fantômes
    public float alphaFalloff = 0.8f;    // transparence
    public float scaleStretch = 1.2f;    // étirement du flou

    private Image sourceImage;
    private List<Image> trails = new List<Image>();
    private Vector2 lastPos;

    void Start()
    {
        sourceImage = GetComponent<Image>();
        lastPos = ((RectTransform)transform).anchoredPosition;

        // Crée les fantômes
        for (int i = 0; i < trailCount; i++)
        {
            GameObject g = new GameObject("Ghost_" + i);
            g.transform.SetParent(transform.parent);
            g.transform.SetAsFirstSibling();
            Image img = g.AddComponent<Image>();
            img.sprite = sourceImage.sprite;
            img.rectTransform.sizeDelta = sourceImage.rectTransform.sizeDelta;
            img.color = new Color(1, 1, 1, 0);
            trails.Add(img);
        }
    }

    void LateUpdate()
    {
        RectTransform rt = (RectTransform)transform;
        Vector2 currentPos = rt.anchoredPosition;
        Vector2 dir = (currentPos - lastPos);
        float dist = dir.magnitude;

        if (dist > 0.1f)
        {
            dir.Normalize();

            for (int i = 0; i < trails.Count; i++)
            {
                Image img = trails[i];
                float alpha = Mathf.Lerp(1f, 0f, (float)i / trails.Count) * alphaFalloff;
                float offset = spacingDistance * i;

                img.color = new Color(1f, 1f, 1f, alpha);
                img.rectTransform.anchoredPosition = currentPos - dir * offset;
                img.rectTransform.sizeDelta = sourceImage.rectTransform.sizeDelta * (1f + i * 0.08f);
                img.rectTransform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
                img.rectTransform.localScale = new Vector3(scaleStretch, 1f, 1f);
            }
        }

        lastPos = currentPos;
    }
}
