using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Параметры выносливости")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 25f;       
    [SerializeField] private float recoveryRate = 20f;   
    [SerializeField] private float recoveryDelay = 1.2f; 

    public float Current { get; private set; }
    public float Max => maxStamina;
    public bool IsFull => Current >= maxStamina - 0.05f;

    public event Action<float, float> OnStaminaChanged;

    private float delayTimer;
    private float lastSentStamina = -1f;

    private void Awake()
    {
        Current = maxStamina;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        if (Current < maxStamina)
        {
            Current = Mathf.Min(maxStamina, Current + recoveryRate * Time.deltaTime);

            if (Mathf.Abs(Current - lastSentStamina) > 0.25f || IsFull)
            {
                lastSentStamina = Current;
                OnStaminaChanged?.Invoke(Current, maxStamina);
            }
        }
    }

    public bool Consume(float deltaTime)
    {
        if (Current <= 0f) return false;

        Current = Mathf.Max(0f, Current - drainRate * deltaTime);
        delayTimer = recoveryDelay;

        if (Mathf.Abs(Current - lastSentStamina) > 0.25f || Current <= 0f)
        {
            lastSentStamina = Current;
            OnStaminaChanged?.Invoke(Current, maxStamina);
        }

        return Current > 0f;
    }
}