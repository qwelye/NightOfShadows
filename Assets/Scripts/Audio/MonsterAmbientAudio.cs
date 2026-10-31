using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MonsterAmbientAudio : MonoBehaviour
{
    [Header("Звуки шорохов / скрипов")]
    [SerializeField] private AudioClip[] ambientSounds;

    [Header("Интервал между звуками (секунды)")]
    [SerializeField] private float minDelay = 7f;
    [SerializeField] private float maxDelay = 18f;

    [Header("Громкость")]
    [SerializeField, Range(0f, 1f)] private float baseVolume = 0.8f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    private void OnEnable() => StartCoroutine(PlaySoundsRoutine());
    private void OnDisable() => StopAllCoroutines();

    private IEnumerator PlaySoundsRoutine()
    {
        // Вместо создания объектов в куче используем таймер по кадрам
        float timer = Random.Range(1f, 4f);
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        while (true)
        {
            if (ambientSounds != null && ambientSounds.Length > 0 &&
                (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing))
            {
                AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                source.pitch = Random.Range(0.85f, 1.15f);
                source.PlayOneShot(clip, baseVolume * SettingsData.GameVolume);
            }

            timer = Random.Range(minDelay, maxDelay);
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
    }
}