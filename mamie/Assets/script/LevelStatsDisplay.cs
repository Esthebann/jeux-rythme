using UnityEngine;
using TMPro;

public class LevelStatsDisplay : MonoBehaviour
{
    [Header("UI Texts")]
    public TMP_Text scoreText;
    public TMP_Text accuracyText;
    public TMP_Text maxComboText;

    [Header("Level1")]
    public string levelName;

    private void OnEnable()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (string.IsNullOrEmpty(levelName)) return;

        int score = PlayerPrefs.GetInt(levelName + "_Score", 0);
        float accuracy = PlayerPrefs.GetFloat(levelName + "_Accuracy", 0f);
        int maxCombo = PlayerPrefs.GetInt(levelName + "_MaxCombo", 0);

        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (accuracyText != null) accuracyText.text = $"Accuracy: {accuracy:F2}%";
        if (maxComboText != null) maxComboText.text = $"Max Combo: {maxCombo}";
    }
}
