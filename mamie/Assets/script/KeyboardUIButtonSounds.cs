using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardUIButtonSounds : MonoBehaviour
{
    public AudioClip selectSound; // son quand on navigue
    public AudioClip clickSound;  // son quand on valide

    private AudioSource audioSource;
    private GameObject lastSelected;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // Son de sélection quand on change de bouton ou qu'on appuie sur une flèche
        if ((currentSelected != null && currentSelected != lastSelected) ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlaySelectSound();
            lastSelected = currentSelected;
        }

        // Son de validation quand on appuie sur Entrée
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PlayClickSound();
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null)
            audioSource.PlayOneShot(selectSound);
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
