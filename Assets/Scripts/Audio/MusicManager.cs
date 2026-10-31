// фоновая динамическая музыка
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Музыкальные дорожки")]
    [SerializeField] private AudioClip calmMusic;
    [SerializeField] private AudioClip chaseMusic;

    [Header("Громкости")]
    [SerializeField, Range(0f, 1f)] private float calmBaseVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float chaseBaseVolume = 0.6f;

    [Header("Дистанции нарастания страха")]
    [SerializeField] private float dangerDistance = 5f;
    [SerializeField] private float safeDistance = 18f;
    [SerializeField] private float transitionSpeed = 1.5f;

    private AudioSource calmSource;
    private AudioSource chaseSource;
    private Transform player;
    private float currentTension = 0f;

    private void Awake()
    {
        calmSource = gameObject.AddComponent<AudioSource>();
        chaseSource = gameObject.AddComponent<AudioSource>();

        SetupSource(calmSource, calmMusic);
        SetupSource(chaseSource, chaseMusic);
    }

    private void OnEnable() => SettingsData.OnChanged += ApplyVolume;
    private void OnDisable() => SettingsData.OnChanged -= ApplyVolume;

    private void Start()
    {
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;

        ApplyVolume();
    }

    private void SetupSource(AudioSource src, AudioClip clip)
    {
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = 0f;
        src.ignoreListenerPause = true; 
        if (clip != null) src.Play();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing)
        {
            float targetTension = 0f;

            if (player != null && ShadowAggro.ActiveChasers.Count > 0)
            {
                float minDistance = float.MaxValue;
                for (int i = 0; i < ShadowAggro.ActiveChasers.Count; i++)
                {
                    var chaser = ShadowAggro.ActiveChasers[i];
                    if (chaser == null) continue;

                    float d = Vector3.Distance(player.position, chaser.transform.position);
                    if (d < minDistance) minDistance = d;
                }

                targetTension = Mathf.InverseLerp(safeDistance, dangerDistance, minDistance);
            }

            currentTension = Mathf.MoveTowards(currentTension, targetTension, transitionSpeed * Time.unscaledDeltaTime);
        }

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        float master = SettingsData.GameVolume;

        if (calmSource != null)
            calmSource.volume = (1f - currentTension) * calmBaseVolume * master;

        if (chaseSource != null)
            chaseSource.volume = currentTension * chaseBaseVolume * master;
    }
}