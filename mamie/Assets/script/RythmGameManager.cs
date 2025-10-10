using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public float time;
    public int lane;
    public string spriteName;
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

public class RythmGameManager : MonoBehaviour
{
    [Header("Zones de jeu")]
    public RectTransform leftZone;
    public RectTransform rightZone;

    [Header("Objets et sons")]
    public GameObject notePrefab;
    public AudioSource audioSource;

    [Header("Paramètres")]
    public float travelTime = 1.5f;

    [Header("Fenêtres de hit")]
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.08f;
    public float earlyWindow = 0.10f;
    public float lateWindow = 0.15f;

    public NoteSprite[] availableSprites;

    [Header("UI")]
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI ComboText;
    public TextMeshProUGUI HitText;
    public TextMeshProUGUI MissText;

    private double dspStartTime;
    private LevelData level;

    private int combo = 0;
    private int score = 0;
    private List<NoteData> hitNotes = new List<NoteData>();

    // Exemple de niveau interne
    private string levelJson = @"
    {
      ""songName"": ""sakura"",
      ""notes"": [
        { ""time"": 1.0, ""lane"": 0, ""spriteName"": ""A"" },
        { ""time"": 2.0, ""lane"": 1, ""spriteName"": ""A"" },
        { ""time"": 3.5, ""lane"": 0, ""spriteName"": ""A"" },
        { ""time"": 4.0, ""lane"": 1, ""spriteName"": ""A"" },
        { ""time"": 5.0, ""lane"": 0, ""spriteName"": ""Rouge"" },
        { ""time"": 6.0, ""lane"": 1, ""spriteName"": ""Bleu"" },
        { ""time"": 7.0, ""lane"": 0, ""spriteName"": ""Jaune"" },
        { ""time"": 8.0, ""lane"": 1, ""spriteName"": ""Vert"" }
      ]
    }";

    void Start()
    {
        level = JsonUtility.FromJson<LevelData>(levelJson);
        Debug.Log("Niveau chargé avec " + level.notes.Length + " notes.");
        StartCoroutine(StartMusic());
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
        {
            StartCoroutine(SpawnNote(note));
        }
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

        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            Sprite chosen = GetSpriteByName(note.spriteName);
            if (chosen != null) img.sprite = chosen;
            else Debug.LogWarning($"Aucun sprite trouvé pour '{note.spriteName}'");
        }

        double targetTime = dspStartTime + note.time;
        while (AudioSettings.dspTime < targetTime)
        {
            float t = 1 - (float)((targetTime - AudioSettings.dspTime) / travelTime);
            rt.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(obj);
    }

    Sprite GetSpriteByName(string name)
    {
        foreach (var s in availableSprites)
            if (s.name == name) return s.sprite;
        return null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            CheckHit(0);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            CheckHit(1);
    }

    void CheckHit(int lane)
    {
        float songTime = (float)(AudioSettings.dspTime - dspStartTime);
        bool hit = false;
        string hitResult = "";

        foreach (var n in level.notes)
        {
            if (n.lane == lane && !hitNotes.Contains(n))
            {
                float diff = songTime - n.time;

                // 🎯 Détermination du type de hit
                if (Mathf.Abs(diff) <= perfectWindow) hitResult = "PERFECT";
                else if (Mathf.Abs(diff) <= greatWindow) hitResult = "GREAT";
                else if (diff < 0 && Mathf.Abs(diff) <= earlyWindow) hitResult = "EARLY";
                else if (diff > 0 && Mathf.Abs(diff) <= lateWindow) hitResult = "LATE";
                else continue; // hors fenêtre

                hit = true;
                combo++;

                // Score selon type de hit
                int points = 0;
                switch (hitResult)
                {
                    case "PERFECT": points = 150; break;
                    case "GREAT": points = 125; break;
                    default: points = 100; break; // EARLY / LATE
                }
                score += points;

                hitNotes.Add(n);

                if (HitText != null) HitText.text = hitResult + "!";
                if (ComboText != null) ComboText.text = "Combo: " + combo;
                if (ScoreText != null) ScoreText.text = "Score: " + score;

                Debug.Log($"✅ {hitResult} lane {lane} | Combo = {combo} | Score = {score}");
                StartCoroutine(ClearHitText());
                break;
            }
        }

        if (!hit)
        {
            combo = 0;
            if (MissText != null) MissText.text = "MISS!";
            if (ComboText != null) ComboText.text = "Combo: " + combo;
            Debug.Log($"❌ MISS lane {lane} | Combo reset");
            StartCoroutine(ClearMissText());
        }
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
}
