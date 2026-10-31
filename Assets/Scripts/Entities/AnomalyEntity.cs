using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum AnomalyType { Shade, Whisper, Crawler }

[RequireComponent(typeof(ShadowLightReaction), typeof(ShadowAggro))]
[RequireComponent(typeof(AudioSource))]
public class AnomalyEntity : ScanTarget
{
    public static readonly List<AnomalyEntity> All = new List<AnomalyEntity>();

    [SerializeField] private AnomalyType type;
    [SerializeField] private Transform caster;
    [SerializeField] private GameObject creature;

    [Header("После ликвидации")]
    [Tooltip("Обычный выключенный объект в сцене: включится, когда монстра ликвидируют")]
    [SerializeField] private GameObject realObject;

    [Header("Приметы тени")]
    [SerializeField] private float stretchY = 2.2f;
    [SerializeField] private float trembleAmount = 0.1f;
    [SerializeField] private float swayAngle = 25f;

    [Header("Звук при выходе из тени")]
    [SerializeField] private AudioClip revealScream;
    [SerializeField, Range(0f, 1.5f)] private float screamVolume = 1f;

    private AudioSource audioSource;
    private ShadowLightReaction lightReaction;
    private ShadowAggro aggro;
    private Renderer body;
    private Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 baseScale;
    private Renderer[] casterRenderers;
    private ShadowCastingMode[] casterModes;
    private float seed;
    private bool revealed;

    public AnomalyType Type => type;
    public override bool IsLit => lightReaction.IsLit;

    protected override void OnEnable() { base.OnEnable(); All.Add(this); }
    protected override void OnDisable() { base.OnDisable(); All.Remove(this); }

    private void Awake()
    {
        lightReaction = GetComponent<ShadowLightReaction>();
        aggro = GetComponent<ShadowAggro>();
        audioSource = GetComponent<AudioSource>();

        body = GetComponent<Renderer>();
        if (creature != null) creature.SetActive(false);
        if (body != null) body.enabled = false;

        seed = Random.value * 100f;

        if (caster != null)
        {
            basePos = caster.localPosition;
            baseRot = caster.localRotation;
            baseScale = caster.localScale;

            casterRenderers = caster.GetComponentsInChildren<Renderer>(true);
            casterModes = new ShadowCastingMode[casterRenderers.Length];
            for (int i = 0; i < casterRenderers.Length; i++)
                casterModes[i] = casterRenderers[i].shadowCastingMode;

            if (type == AnomalyType.Shade)
                caster.localScale = Vector3.Scale(baseScale, new Vector3(0.7f, stretchY, 0.7f));
        }
    }

    private void Update()
    {
        if (revealed)
        {
            // монстр потерял игрока снова прячемся в тень
            if (!aggro.IsChasing) Hide();
            return;
        }

        if (aggro.IsChasing)
        {
            Reveal();
            return;
        }

        if (caster == null) return;

        if (type == AnomalyType.Whisper)
        {
            float angle = Mathf.Sin(Time.time * 1.5f + seed) * swayAngle;
            caster.localRotation = baseRot * Quaternion.Euler(0f, angle, 0f);
        }
        else if (type == AnomalyType.Crawler)
        {
            float t = Time.time * 25f;
            Vector3 offset = new Vector3(Mathf.PerlinNoise(seed, t) - 0.5f, 0f, Mathf.PerlinNoise(t, seed) - 0.5f);
            caster.localPosition = basePos + offset * trembleAmount;
        }
    }

    private void Reveal()
    {
        if (!revealed)
        {
            if (audioSource != null && revealScream != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(revealScream, screamVolume * SettingsData.GameVolume);
            }
        }

        revealed = true;

        if (caster != null) caster.gameObject.SetActive(false);
        if (creature != null) creature.SetActive(true);
        else if (body != null) body.enabled = true;
    }

    private void Hide()
    {
        revealed = false;
        lightReaction.Calm();

        if (creature != null) creature.SetActive(false);
        else if (body != null) body.enabled = false;

        if (caster != null)
        {
            caster.gameObject.SetActive(true);
            for (int i = 0; i < casterRenderers.Length; i++)
                casterRenderers[i].shadowCastingMode = casterModes[i];
        }
    }

    public void Provoke()
    {
        lightReaction.Enrage();
        aggro.ChaseSelf();
        Reveal();
    }

    public override bool ApplyProtocol(AnomalyType used)
    {
        if (used == type)
        {
            Kill();
            return true;
        }
        Provoke();
        return false;
    }

    private void Kill()
    {
        if (realObject != null)
        {
            realObject.SetActive(true);

            foreach (Renderer r in realObject.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = true;
                r.shadowCastingMode = ShadowCastingMode.On;
            }
        }

        Destroy(gameObject);
    }

    public static string NameOf(AnomalyType t) => t switch
    {
        AnomalyType.Shade => "Растянутая",
        AnomalyType.Whisper => "Живая",
        AnomalyType.Crawler => "Дрожащая",
        _ => "Неизвестно"
    };

}