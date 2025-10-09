using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class NoteData { public float time; public int lane; }

[System.Serializable]
public class NotesWrapper { public NoteData[] notes; }

[System.Serializable]
public class LevelData { public string songName; public NotesWrapper notesWrapper; }

public class RythmGameManager : MonoBehaviour
{
    [Header("Zones")]
    public RectTransform leftZone;
    public RectTransform rightZone;

    [Header("Prefabs & Audio")]
    public GameObject notePrefab;
    public AudioSource audioSource;

    [Header("Settings")]
    public float travelTime = 1.5f;
    public float hitWindow = 0.15f;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text comboText;

    [Header("Feedback UI")]
    public TMP_Text hitText;
    public TMP_Text missText;
    public float feedbackDuration = 0.5f;

    private double dspStartTime;
    private NoteData[] notes;
    private int score = 0;
    private int combo = 0;
    private HashSet<int> hitNotes = new HashSet<int>();

    void Start()
    {
        LoadLevelFromString();
        StartCoroutine(StartMusic());
    }

    void LoadLevelFromString()
    {
        // JSON directement dans le script
        string json = @"
        {
          ""songName"": ""sakura"",
          ""notesWrapper"": {
            ""notes"": [
              { ""time"": 1.0, ""lane"": 0 },
              { ""time"": 2.0, ""lane"": 1 },
              { ""time"": 3.5, ""lane"": 0 },
              { ""time"": 4.0, ""lane"": 1 },
              { ""time"": 5.0, ""lane"": 1 },
              { ""time"": 6.0, ""lane"": 1 },
              { ""time"": 7.0, ""lane"": 1 },
              { ""time"": 8.0, ""lane"": 1 },
              { ""time"": 9.0, ""lane"": 1 }
            ]
          }
        }";

        LevelData level = JsonUtility.FromJson<LevelData>(json);

        if (level == null || level.notesWrapper == null || level.notesWrapper.notes == null)
        {
            Debug.LogError("Erreur parsing JSON ou aucune note trouvée");
            return;
        }

        notes = level.notesWrapper.notes;
        Debug.Log("Nombre de notes lues : " + notes.Length);

        foreach (var n in notes)
            Debug.Log("Note: lane " + n.lane + ", time " + n.time);

        // Stocker le nom de la chanson
        audioSource.clip = Resources.Load<AudioClip>("Audio/" + level.songName);
    }

    IEnumerator StartMusic()
    {
        if (audioSource.clip == null)
        {
            Debug.LogError("AudioClip introuvable !");
            yield break;
        }

        dspStartTime = AudioSettings.dspTime + 0.5;
        audioSource.PlayScheduled(dspStartTime);
        Debug.Log("Audio lancé avec PlayScheduled à " + dspStartTime);

        for (int i = 0; i < notes.Length; i++)
            StartCoroutine(SpawnNoteSafe(notes[i], i));
    }

    IEnumerator SpawnNoteSafe(NoteData note, int index)
    {
        double targetTime = dspStartTime + note.time;
        double spawnTime = targetTime - travelTime;
        double wait = spawnTime - AudioSettings.dspTime;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, (float)wait));

        RectTransform zone = note.lane == 0 ? leftZone : rightZone;
        Vector3 startPos = zone.anchoredPosition + new Vector2(note.lane == 0 ? -800 : 800, 0);
        Vector3 endPos = zone.anchoredPosition;

        GameObject obj = Instantiate(notePrefab, zone.parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        bool hit = false;

        while (AudioSettings.dspTime < targetTime + hitWindow)
        {
            float t = 1 - (float)((targetTime - AudioSettings.dspTime) / travelTime);
            rt.anchoredPosition = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(t));
            yield return null;
        }

        if (!hit)
        {
            combo = 0;
            UpdateUI();
            if (missText != null)
                StartCoroutine(ShowFeedback(missText));
        }

        Destroy(obj);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CheckHit(0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) CheckHit(1);
    }

    void CheckHit(int lane)
    {
        float songTime = (float)(AudioSettings.dspTime - dspStartTime);

        for (int i = 0; i < notes.Length; i++)
        {
            var n = notes[i];
            if (n.lane == lane && Mathf.Abs(n.time - songTime) <= hitWindow && !hitNotes.Contains(i))
            {
                hitNotes.Add(i);
                score += 100;
                combo++;
                UpdateUI();
                if (hitText != null) StartCoroutine(ShowFeedback(hitText));
                return;
            }
        }

        combo = 0;
        UpdateUI();
        if (missText != null) StartCoroutine(ShowFeedback(missText));
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (comboText != null) comboText.text = "Combo: " + combo;
    }

    IEnumerator ShowFeedback(TMP_Text textComponent)
    {
        textComponent.gameObject.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        textComponent.gameObject.SetActive(false);
    }
}
