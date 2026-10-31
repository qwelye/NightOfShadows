using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class ShadowAggro : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<ShadowAggro> ActiveChasers = new System.Collections.Generic.List<ShadowAggro>();

    [Header("Обнаружение")]
    [SerializeField] private float aggroRadius = 5f;
    [SerializeField] private float loseRadius = 18f;

    [Header("Погоня и атака")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Звук атаки монстра")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField, Range(0f, 1f)] private float attackSoundVolume = 0.8f;

    [Header("Саботаж фонаря")]
    [SerializeField] private bool canSabotageLight = true;
    [SerializeField] private float sabotageRange = 7f;
    [SerializeField] private float sabotageDuration = 3f;
    [SerializeField] private float sabotageCooldown = 12f;

    // интервал проверки лучей и пересчета пути вместо каждого кадра
    [SerializeField] private float repathInterval = 0.2f;

    private NavMeshAgent agent;
    private ShadowLightReaction lightReaction;
    private Flashlight flashlight;
    private PlayerHealth playerHealth;
    private Transform player;
    private AudioSource audioSource;

    private float baseSpeed;
    private float attackTimer;
    private float sabotageTimer;
    private float repathTimer;
    private bool chasing;

    public bool IsChasing => chasing;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lightReaction = GetComponent<ShadowLightReaction>();
        audioSource = GetComponent<AudioSource>();
        baseSpeed = agent.speed;
    }

    private void Start()
    {
        flashlight = FindFirstObjectByType<Flashlight>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) player = playerHealth.transform;
    }

    private void OnDisable()
    {
        // при выключении монстра убираем его из списка преследователей
        chasing = false;
        ActiveChasers.Remove(this);
    }

    private void Update()
    {
        if (player == null) return;

        // засыпаем если игра на паузе или завершена
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        float distance = Vector3.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        if (!chasing)
        {
            // если монстр заморожен светом то он не может начать погоню
            if (lightReaction != null && lightReaction.FrozenByLight) return;

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                // игрок подошел близко и между нами нет препятствий
                if (distance <= aggroRadius && HasLineOfSight(distance))
                    StartChase();
            }
            return;
        }

        if (distance > loseRadius)
        {
            StopChase();
            return;
        }

        // механика саботажа фонарика игрока при сближении
        sabotageTimer -= Time.deltaTime;
        if (canSabotageLight && sabotageTimer <= 0f && distance <= sabotageRange
            && flashlight != null && flashlight.IsOn
            && (lightReaction == null || !lightReaction.FrozenByLight))
        {
            flashlight.Disrupt(sabotageDuration);
            sabotageTimer = sabotageCooldown;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
            repathTimer = repathInterval;
        }

        TryAttack(distance);
    }

    private void TryAttack(float distance)
    {
        if (distance > attackRange || attackTimer > 0f) return;
        if (lightReaction != null && lightReaction.FrozenByLight) return;

        attackTimer = attackCooldown;

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound, attackSoundVolume * SettingsData.GameVolume);

        if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
    }

    // проверка прямой видимости
    private bool HasLineOfSight(float distance)
    {
        Vector3 eyeOrigin = transform.position + Vector3.up * 1.2f;
        Vector3 targetPoint = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = (targetPoint - eyeOrigin).normalized;

        if (Physics.Raycast(eyeOrigin, toPlayer, out RaycastHit hit, distance,
                ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.IsChildOf(player);
        }

        return true;
    }

    public void ChaseSelf() => StartChase();

    private void StartChase()
    {
        if (chasing) return;
        chasing = true;
        if (!ActiveChasers.Contains(this)) ActiveChasers.Add(this);
        agent.speed = chaseSpeed;
    }

    private void StopChase()
    {
        if (!chasing) return;
        chasing = false;
        ActiveChasers.Remove(this);
        agent.speed = baseSpeed;
        if (agent.isOnNavMesh) agent.ResetPath();
    }
}