// реакция монстра на свет фонаря (заморозка / сопротивление)
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShadowLightReaction : MonoBehaviour
{
    [SerializeField] private float eyeHeight = 1f;

    [Header("Сопротивление свету (только в погоне)")]
    [SerializeField] private float resistCheckInterval = 0.7f;
    [SerializeField] private float resistChance = 0.4f;
    [SerializeField] private float resistDuration = 0.8f;

    private NavMeshAgent agent;
    private ShadowAggro aggro;
    private Flashlight flashlight;
    private bool enraged;
    private float checkTimer;
    private float resistTimer;

    private float raycastTimer;
    private const float RaycastInterval = 0.1f;

    public bool IsLit { get; private set; }
    public bool FrozenByLight => IsLit && !enraged && resistTimer <= 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        aggro = GetComponent<ShadowAggro>();
    }

    private void Start() => flashlight = FindFirstObjectByType<Flashlight>();

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        raycastTimer -= Time.deltaTime;
        if (raycastTimer <= 0f)
        {
            raycastTimer = RaycastInterval;
            IsLit = CheckLit();
        }

        UpdateResistance();
        if (agent.isOnNavMesh) agent.isStopped = FrozenByLight;
    }

    public void Calm() => enraged = false;
    public void Enrage() => enraged = true;

    private void UpdateResistance()
    {
        if (resistTimer > 0f)
        {
            resistTimer -= Time.deltaTime;
            return;
        }

        bool chasing = aggro != null && aggro.IsChasing;
        if (!IsLit || !chasing)
        {
            checkTimer = 0f;
            return;
        }

        checkTimer += Time.deltaTime;
        if (checkTimer >= resistCheckInterval)
        {
            checkTimer = 0f;
            if (Random.value < resistChance) resistTimer = resistDuration;
        }
    }

    private bool CheckLit()
    {
        if (flashlight == null) return false;
        return FlashlightProbe.IsLit(flashlight, transform.position + Vector3.up * (eyeHeight - 1f), transform);
    }
}