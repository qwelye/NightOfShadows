using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += HandleStateChanged;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        bool paused = state == GameState.Paused;
        pausePanel.SetActive(paused);
        if (!paused) settingsPanel.SetActive(false);
    }

    public void OnResumeClicked() => GameManager.Instance.Resume();

    public void OnSettingsClicked()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OnMainMenuClicked() => SceneManager.LoadScene("MainMenu");
}