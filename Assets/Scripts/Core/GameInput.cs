// обёртка над New Input System
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputSystem_Actions actions;
    public InputSystem_Actions.PlayerActions Player => actions.Player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        actions = new InputSystem_Actions();
        actions.Player.Enable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (actions != null)
        {
            actions.Player.Disable();
            actions.Dispose();
        }
    }
}