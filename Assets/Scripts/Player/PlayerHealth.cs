// здоровье игрока и получение урона
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    [Header("Звуки получения урона")]
    [SerializeField] private AudioClip[] hurtClips;
    [SerializeField, Range(0f, 1f)] private float hurtVolume = 0.8f;

    public float Current { get; private set; }
    public float Max => maxHealth;

    public event Action<float, float> OnHealthChanged;

    private AudioSource audioSource;

    private void Awake()
    {
        Current = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        // блокирует повторный вызов логики смерти, если уже прилетел фатальный удар
        if (Current <= 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        Current = Mathf.Max(0f, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHealth);

        if (audioSource != null && hurtClips != null && hurtClips.Length > 0)
        {
            AudioClip clip = hurtClips[UnityEngine.Random.Range(0, hurtClips.Length)];
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, hurtVolume * SettingsData.GameVolume);
        }

        if (Current <= 0f && GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Lost);
    }
}