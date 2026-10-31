using UnityEngine;

public class BaseZone : MonoBehaviour
{
    private SectorProgress progress;

    private void Start() => progress = FindFirstObjectByType<SectorProgress>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        if (progress != null && !progress.AllCleared)
        {
            progress.Say("Сначала зачистите сектор");
            return;
        }

        GameManager.Instance.SetState(GameState.Won);
    }
}