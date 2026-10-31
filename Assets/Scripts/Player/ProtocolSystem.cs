// инструмент сканирования и ликвидации аномалий игроком
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProtocolSystem : MonoBehaviour
{
    [SerializeField] private float scanRange = 4f;        
    [SerializeField] private float aimThreshold = 0.93f; 
    [SerializeField] private float closeDistance = 6f;   
    [SerializeField] private float releaseMargin = 0.05f; 
    [SerializeField] private float rangeMargin = 0.75f;   
    [SerializeField] private float wrongGuessDamage = 15f;
    [SerializeField] private float falseAlarmDamage = 5f;

    public ScanTarget Candidate { get; private set; }
    public ScanTarget Locked { get; private set; }
    public bool IsPanelOpen => Locked != null;
    public bool CanScan => Candidate != null && !IsPanelOpen;
    public AnomalyType Selected { get; private set; } = AnomalyType.Shade;

    public event Action<AnomalyType, bool> OnProtocolUsed;

    private Transform view;
    private SectorProgress progress;
    private PlayerHealth health;
    private Flashlight flashlight;

    private void Start()
    {
        view = Camera.main.transform;
        progress = FindFirstObjectByType<SectorProgress>();
        health = GetComponent<PlayerHealth>();
        flashlight = FindFirstObjectByType<Flashlight>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            Locked = null;
            Candidate = null;
            return;
        }

        var input = GameInput.Instance.Player;

        if (!IsPanelOpen)
        {
            Candidate = FindCandidate();
            if (Candidate != null && input.Attack.WasPressedThisFrame()) Locked = Candidate;
            return;
        }

        if (input.CloseScanner.WasPressedThisFrame() ||
            (flashlight != null && !flashlight.IsOn) ||
            Vector3.Distance(view.position, Locked.transform.position) > closeDistance)
        {
            Locked = null;
            return;
        }

        if (input.Previous.WasPressedThisFrame()) StepSelection(-1);
        if (input.Next.WasPressedThisFrame()) StepSelection(1);

        if (input.Protocol.WasPressedThisFrame())
            ActivateProtocol();
    }

    private void StepSelection(int direction)
    {
        int count = Enum.GetValues(typeof(AnomalyType)).Length;
        int next = ((int)Selected + direction + count) % count;
        Selected = (AnomalyType)next;
    }

    private ScanTarget FindCandidate()
    {
        // Фонарик выключен - сканировать нечем
        if (flashlight == null || !flashlight.IsOn) return null;

        // Удерживаем текущую цель, пока она чуть шире порогов
        if (Candidate != null)
        {
            Vector3 to = Candidate.transform.position - view.position;
            if (to.magnitude <= scanRange + rangeMargin &&
                Vector3.Dot(view.forward, to.normalized) > aimThreshold - releaseMargin)
                return Candidate;
        }

        // Поиск новой цели
        ScanTarget best = null;
        float bestDot = aimThreshold;

        foreach (ScanTarget e in ScanTarget.Targets)
        {
            Vector3 toTarget = e.transform.position - view.position;
            if (toTarget.magnitude > scanRange) continue;

            float dot = Vector3.Dot(view.forward, toTarget.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = e;
            }
        }
        return best;
    }

    private void ActivateProtocol()
    {
        ScanTarget target = Locked;
        Locked = null;

        AnomalyType used = Selected;
        bool correct = target.ApplyProtocol(used);

        if (correct)
        {
            progress.MarkCleared();
            progress.Say($"Ликвидировано: {AnomalyEntity.NameOf(used)}");
        }
        else if (target is ShadowDecoy)
        {
            health.TakeDamage(falseAlarmDamage);
            progress.Say("Ложное срабатывание: это обычный объект");
        }
        else
        {
            health.TakeDamage(wrongGuessDamage);
            progress.Say("Неверная классификация! Тень разъярена");
        }

        OnProtocolUsed?.Invoke(used, correct);
    }
}