using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIAudio : MonoBehaviour
{
    public static UIAudio Instance { get; private set; }

    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
    }

    public void PlayHover()
    {
        if (hoverClip != null && source != null)
            source.PlayOneShot(hoverClip, baseVolume * SettingsData.UiVolume);
    }

    public void PlayClick()
    {
        if (clickClip != null && source != null)
            source.PlayOneShot(clickClip, baseVolume * SettingsData.UiVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}