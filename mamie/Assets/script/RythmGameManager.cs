using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#region Data Classes
[System.Serializable]
public class NoteData
{
    public float time;
    public int lane;
    public string spriteName;
    public string animationName; // <- animation spécifique pour cette note
    public int trajectoryType = 0; // 0 = droite, 1 = courbe haut, 2 = courbe bas
}

[System.Serializable]
public class LevelData
{
    public string songName;
    public NoteData[] notes;
}

[System.Serializable]
public class NoteSprite
{
    public string name;
    public Sprite sprite;
}
#endregion

public class RythmGameManager : MonoBehaviour
{
    [Header("Zones de frappe")]
    public RectTransform leftZone;
    public RectTransform rightZone;

    [Header("Objets & Audio")]
    public GameObject notePrefab;
    public AudioSource audioSource;
    public NoteSprite[] availableSprites;
    public float travelTime = 1.5f;

    [Header("UI")]
    public TMP_Text ScoreText;
    public TMP_Text ComboText;
    public TMP_Text HitText;
    public TMP_Text MissText;
    public TMP_Text AccuracyText;

    [Header("Fenêtres de hit (secondes)")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.08f;
    public float earlyWindow = 0.10f;
    public float lateWindow = 0.15f;

    private double dspStartTime;
    private LevelData level;
    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private float accuracy = 100f;
    private int totalNotes = 0;

    private List<NoteData> hitNotes = new List<NoteData>();
    private List<GameObject> activeNotes = new List<GameObject>();
    private bool levelEnded = false;

    void Start()
    {
        StartCoroutine(LoadLevelFromFile("Level1.json"));
    }

    IEnumerator LoadLevelFromFile(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Fichier JSON introuvable : " + path);
            yield break;
        }

        string json = File.ReadAllText(path);
        level = JsonUtility.FromJson<LevelData>(json);
        totalNotes = level.notes.Length;
        Debug.Log($"Niveau chargé : {level.songName} ({totalNotes} notes)");

        UpdateUI();
        StartCoroutine(StartMusic());
        yield return null;
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(1f);
        dspStartTime = AudioSettings.dspTime + 0.5f;

        AudioClip clip = Resources.Load<AudioClip>("Audio/" + level.songName);
        if (clip == null)
        {
            Debug.LogError("Audio introuvable : " + level.songName);
            yield break;
        }

        audioSource.clip = clip;
        audioSource.PlayScheduled(dspStartTime);

        foreach (var note in level.notes)
            StartCoroutine(SpawnNote(note));
    }

    IEnumerator SpawnNote(NoteData note)
    {
        double spawnTime = dspStartTime + note.time - travelTime;
        double wait = spawnTime - AudioSettings.dspTime;
        if (wait > 0)
            yield return new WaitForSecondsRealtime((float)wait);

        RectTransform zone = note.lane == 0 ? leftZone : rightZone;
        Vector3 startPos = zone.anchoredPosition + new Vector2(note.lane == 0 ? -800 : 800, 0);
        Vector3 endPos = zone.anchoredPosition;

        GameObject obj = Instantiate(notePrefab, zone.parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        // Assigner le sprite
        Image img = obj.GetComponent<Image>();
        Sprite chosen = GetSpriteByName(note.spriteName);
        if (img != null && chosen != null)
            img.sprite = chosen;

        // Jouer l'animation spécifique
        Animator anim = obj.GetComponent<Animator>();
        if (anim != null && !string.IsNullOrEmpty(note.animationName))
        {
            anim.Play(note.animationName);
        }

        activeNotes.Add(obj);

        double targetTime = dspStartTime + note.time;
        while (AudioSettings.dspTime < targetTime)
        {
            float t = 1 - (float)((targetTime - AudioSettings.dspTime) / travelTime);
            rt.anchoredPosition = CalculateTrajectory(startPos, endPos, t, note.trajectoryType);
            yield return null;
        }

        yield return new WaitForSeconds(lateWindow + 0.05f);
        if (!hitNotes.Contains(note))
        {
            combo = 0;
            MissText.text = "MISS!";
            accuracy = Mathf.Clamp(accuracy - (100f / totalNotes), 0f, 100f);
            UpdateUI();
            StartCoroutine(ClearMissText());
        }

        activeNotes.Remove(obj);
        Destroy(obj);

        CheckLevelEnd();
    }

    Vector3 CalculateTrajectory(Vector3 start, Vector3 end, float t, int type)
    {
        switch (type)
        {
            case 1: // courbe vers le haut
                return Vector3.Lerp(start, end, t) + new Vector3(0, Mathf.Sin(t * Mathf.PI) * 300f, 0);
            case 2: // courbe vers le bas
                return Vector3.Lerp(start, end, t) - new Vector3(0, Mathf.Sin(t * Mathf.PI) * 300f, 0);
            default: // courbe qui reste droite
                return Vector3.Lerp(start, end, t);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CheckHit(0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) CheckHit(1);
    }

    void CheckHit(int lane)
    {
        float songTime = (float)(AudioSettings.dspTime - dspStartTime);
        foreach (var n in level.notes)
        {
            if (hitNotes.Contains(n)) continue;
            if (n.lane != lane) continue;

            float diff = songTime - n.time;
            string hitResult = null;

            if (Mathf.Abs(diff) <= perfectWindow) hitResult = "PERFECT";
            else if (Mathf.Abs(diff) <= greatWindow) hitResult = "GREAT";
            else if (diff < 0 && Mathf.Abs(diff) <= earlyWindow) hitResult = "EARLY";
            else if (diff > 0 && Mathf.Abs(diff) <= lateWindow) hitResult = "LATE";
            else continue;

            hitNotes.Add(n);

            int points = 0;
            float accuracyLoss = 0f;
            switch (hitResult)
            {
                case "PERFECT": points = 150; accuracyLoss = 0f; break;
                case "GREAT": points = 125; accuracyLoss = 0.5f; break;
                case "EARLY":
                case "LATE": points = 100; accuracyLoss = 1f; break;
            }

            score += points;
            combo++;
            if (combo > maxCombo) maxCombo = combo;
            accuracy = Mathf.Clamp(accuracy - (accuracyLoss / totalNotes), 0f, 100f);

            HitText.text = hitResult;
            StartCoroutine(ClearHitText());
            UpdateUI();
            CheckLevelEnd();
            return;
        }
    }

    Sprite GetSpriteByName(string name)
    {
        foreach (var s in availableSprites)
            if (s.name == name)
                return s.sprite;
        return null;
    }

    void UpdateUI()
    {
        ScoreText.text = $"Score: {score}";
        ComboText.text = $"Combo: {combo}";
        AccuracyText.text = $"Accuracy: {accuracy:F2}%";
    }

    IEnumerator ClearHitText()
    {
        yield return new WaitForSeconds(0.5f);
        HitText.text = "";
    }

    IEnumerator ClearMissText()
    {
        yield return new WaitForSeconds(0.5f);
        MissText.text = "";
    }

    void CheckLevelEnd()
    {
        if (!levelEnded && activeNotes.Count == 0)
        {
            levelEnded = true;
            Debug.Log("Niveau terminé !");
            PlayerPrefs.SetInt("Level1_Score", score);
            PlayerPrefs.SetFloat("Level1_Accuracy", accuracy);
            PlayerPrefs.SetInt("Level1_MaxCombo", maxCombo);
            PlayerPrefs.Save();
            Debug.Log($"Stats sauvegardées : Score={score}, Accuracy={accuracy:F2}, MaxCombo={maxCombo}");
        }
    }
}
