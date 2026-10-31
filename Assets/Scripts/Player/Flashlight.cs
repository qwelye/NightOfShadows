// включение/выключение и саботаж фонарика
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class Flashlight : MonoBehaviour
{
    [SerializeField] private bool startOn = false;
    [SerializeField] private float flickerTime = 1f;

    [Header("Звук переключателя")]
    [Tooltip("Звук щелчка кнопки фонарика")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;

    private Light beam;
    private AudioSource audioSource;
    private Coroutine disruption;
    private bool blocked;

    public bool IsOn => beam.enabled;
    public Light Beam => beam;

    private void Awake()
    {
        beam = GetComponent<Light>();
        beam.enabled = startOn;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
        if (blocked) return;

        if (GameInput.Instance.Player.Flashlight.WasPressedThisFrame())
            SetOn(!IsOn);
    }

    public void SetOn(bool on)
    {
        if (beam.enabled != on)
        {
            beam.enabled = on;

            if (audioSource != null && clickSound != null)
                audioSource.PlayOneShot(clickSound, soundVolume * SettingsData.GameVolume);
        }
    }

    public void Disrupt(float duration)
    {
        if (disruption != null) return;
        disruption = StartCoroutine(DisruptRoutine(duration));
    }

    private IEnumerator DisruptRoutine(float duration)
    {
        blocked = true;
        bool wasOn = IsOn;

        float t = 0f;
        while (t < flickerTime)
        {
            beam.enabled = !beam.enabled;
            float wait = Random.Range(0.04f, 0.15f);
            yield return new WaitForSeconds(wait);
            t += wait;
        }

        beam.enabled = false;
        yield return new WaitForSeconds(duration);

        blocked = false;
        beam.enabled = wasOn;
        disruption = null;
    }
}