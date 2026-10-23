using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIAudio : MonoBehaviour
{
    public static UIAudio Instance { get; private set; }

    [SerializeField] private AudioClip hoverClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.4f;

    private AudioSource source;

    private void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void PlayHover()
    {
        if (hoverClip != null) source.PlayOneShot(hoverClip, volume * SettingsData.UiVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}