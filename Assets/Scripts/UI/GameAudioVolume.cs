using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioVolume : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float baseVolume = 0.25f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();

    private void OnEnable()
    {
        SettingsData.OnChanged += Apply;
        Apply();
    }

    private void OnDisable() => SettingsData.OnChanged -= Apply;

    private void Apply() => source.volume = baseVolume * SettingsData.GameVolume;
}