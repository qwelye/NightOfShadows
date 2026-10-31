//  главный синглтон состояния игры
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => SetState(GameState.Playing);

    private void Update()
    {
        if (State == GameState.Won || State == GameState.Lost) return;

        if (GameInput.Instance != null && GameInput.Instance.Player.Pause.WasPressedThisFrame())
        {
            if (State == GameState.Playing) SetState(GameState.Paused);
            else if (State == GameState.Paused) SetState(GameState.Playing);
        }
    }

    public void SetState(GameState newState)
    {
        // Защита от смены состояния после финала
        if ((State == GameState.Won || State == GameState.Lost) && newState != GameState.Won && newState != GameState.Lost)
            return;

        State = newState;

        bool inGame = newState == GameState.Playing;

        Time.timeScale = inGame ? 1f : 0f;
        AudioListener.pause = !inGame;
        Cursor.lockState = inGame ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !inGame;

        OnStateChanged?.Invoke(newState);
    }

    public void Resume() => SetState(GameState.Playing);

    // Сброс окружения
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        ShadowAggro.ActiveChasers.Clear();
        ScanTarget.Targets.Clear();
        AnomalyEntity.All.Clear();
    }
}