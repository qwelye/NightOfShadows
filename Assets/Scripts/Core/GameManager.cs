using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetState(State == GameState.Playing ? GameState.Paused : GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        State = newState;
        bool paused = newState == GameState.Paused;

        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        OnStateChanged?.Invoke(newState);
    }

    public void Resume() => SetState(GameState.Playing);

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }
}