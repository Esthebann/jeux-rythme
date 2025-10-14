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
    public float time;       // moment où la note doit être tapée (hitTime)
    public float spawnTime;  // moment où la note apparaît
    public int lane;
    public string spriteName;
    public string animationName;
    public int trajectoryType = 0;
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

    [Header("Offset des notes (secondes)")]
    public float timeOffset = 0.0f;

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
        string selected = "Level1";
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.selectedLevelName))
            selected = GameManager.instance.selectedLevelName;

        StartCoroutine(LoadLevelFromFile(selected + ".json"));
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
        if (level == null)
        {
            Debug.LogError("Impossible de parser le JSON : " + path);
            yield break;
        }

        totalNotes = level.notes != null ? level.notes.Length : 0;
        Debug.Log($"Niveau chargé : {level.songName} ({totalNotes} notes)");

        UpdateUI();
        StartCoroutine(StartMusic());
        yield return null;
    }

    IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(1f);
        dspStartTime = AudioSettings.dspTime + 0.5f;

        AudioClip clip = null;
        if (!string.IsNullOrEmpty(level.songName))
            clip = Resources.Load<AudioClip>("Audio/" + level.songName);

        if (clip == null)
        {
            Debug.LogError("Audio introuvable : " + level.songName);
            yield break;
        }

        audioSource.clip = clip;
        audioSource.PlayScheduled(dspStartTime);

        if (level.notes != null)
        {
            foreach (var note in level.notes)
                StartCoroutine(SpawnNote(note));
        }
    }

    IEnumerator SpawnNote(NoteData note)
    {
        // attendre le spawnTime spécifique à la note
        double spawnTime = dspStartTime + note.spawnTime;
        double wait = spawnTime - AudioSettings.dspTime;
        if (wait > 0)
            yield return new WaitForSecondsRealtime((float)wait);

        RectTransform zone = note.lane == 0 ? leftZone : rightZone;

        float startDistance = 800f;
        Vector3 startPos = zone.anchoredPosition + new Vector2(note.lane == 0 ? -startDistance : startDistance, 0);
        Vector3 endPos = zone.anchoredPosition;

        GameObject obj = Instantiate(notePrefab, zone.parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        Image img = obj.GetComponent<Image>();
        Sprite chosen = GetSpriteByName(note.spriteName);
        if (img != null && chosen != null)
            img.sprite = chosen;

        Animator anim = obj.GetComponent<Animator>();
        if (anim != null && !string.IsNullOrEmpty(note.animationName))
            anim.Play(note.animationName);

        activeNotes.Add(obj);

        // déplacer la note entre spawnTime et time
        double targetTime = dspStartTime + note.time;
        double travelDuration = note.time - note.spawnTime;
        while (AudioSettings.dspTime < targetTime)
        {
            float t = 1 - (float)((targetTime - AudioSettings.dspTime) / travelDuration);
            rt.anchoredPosition = CalculateTrajectory(startPos, endPos, t, note.trajectoryType);
            yield return null;
        }

        // Gestion du miss
        double missTime = targetTime + lateWindow;
        double waitMiss = missTime - AudioSettings.dspTime;
        if (waitMiss > 0)
            yield return new WaitForSecondsRealtime((float)waitMiss);

        if (!hitNotes.Contains(note))
        {
            combo = 0;
            MissText.text = "MISS!";
            accuracy = Mathf.Clamp(accuracy - (100f / Mathf.Max(1, totalNotes)), 0f, 100f);
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
            case 1: return Vector3.Lerp(start, end, t) + new Vector3(0, Mathf.Sin(t * Mathf.PI) * 300f, 0);
            case 2: return Vector3.Lerp(start, end, t) - new Vector3(0, Mathf.Sin(t * Mathf.PI) * 300f, 0);
            default: return Vector3.Lerp(start, end, t);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CheckHit(0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) CheckHit(1);
    }

    void CheckHit(int lane)
    {
        if (level == null || level.notes == null) return;

        float songTime = (float)(AudioSettings.dspTime - dspStartTime);

        foreach (var n in level.notes)
        {
            if (hitNotes.Contains(n)) continue;
            if (n.lane != lane) continue;

            float diff = songTime - (n.time + timeOffset);
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
                case "PERFECT": points = 150; break;
                case "GREAT": points = 125; accuracyLoss = 0.5f; break;
                case "EARLY":
                case "LATE": points = 100; accuracyLoss = 1f; break;
            }

            score += points;
            combo++;
            if (combo > maxCombo) maxCombo = combo;
            accuracy = Mathf.Clamp(accuracy - (accuracyLoss / Mathf.Max(1, totalNotes)), 0f, 100f);

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
        if (ScoreText != null) ScoreText.text = $"Score: {score}";
        if (ComboText != null) ComboText.text = $"Combo: {combo}";
        if (AccuracyText != null) AccuracyText.text = $"Accuracy: {accuracy:F2}%";
    }

    IEnumerator ClearHitText()
    {
        yield return new WaitForSeconds(0.5f);
        if (HitText != null) HitText.text = "";
    }

    IEnumerator ClearMissText()
    {
        yield return new WaitForSeconds(0.5f);
        if (MissText != null) MissText.text = "";
    }

    void CheckLevelEnd()
    {
        if (!levelEnded && activeNotes.Count == 0)
        {
            levelEnded = true;
            Debug.Log("Niveau terminé !");
            // sauvegarde des scores…
        }
    }
}
