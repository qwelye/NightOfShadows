using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioVolume : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float baseVolume = 0.5f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();

    private void OnEnable()
    {
        SettingsData.OnChanged += Apply;
        Apply();
    }

    private void OnDisable() => SettingsData.OnChanged -= Apply;

    public void Apply()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source != null)
            source.volume = baseVolume * SettingsData.GameVolume;
    }
}