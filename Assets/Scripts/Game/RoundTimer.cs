using UnityEngine;

public class RoundTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 180f;  

    public float TimeLeft { get; private set; }

    private void Awake() => TimeLeft = timeLimit;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            GameManager.Instance.SetState(GameState.Lost);
        }
    }
}