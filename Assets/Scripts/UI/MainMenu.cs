using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        settingsPanel.SetActive(false);
    }

    public void OnPlayClicked() => SceneManager.LoadScene("Game");

    public void OnSettingsClicked()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void OnQuitClicked() => Application.Quit();
}