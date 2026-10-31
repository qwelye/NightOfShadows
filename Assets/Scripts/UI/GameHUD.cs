// основной игровой интерфейс
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private RoundTimer timer;
    [SerializeField] private SectorProgress progress;
    [SerializeField] private ProtocolSystem protocol;

    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private float staminaFadeSpeed = 3f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text clearedText;
    [SerializeField] private TMP_Text messageText;

    [Header("Сканер")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject scanPanel;
    [SerializeField] private TMP_Text scanText;
    [SerializeField] private float resultHoldTime = 0.25f;
    [SerializeField] private float resultFadeTime = 0.35f;

    [Header("Конец игры")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endText;

    private const string OpenFooter = "\n[ПКМ] Протокол зачистки\n[Колесо] Выбор\n[Q] Закрыть";

    // Кешируем типы аномалий один раз, чтобы не создавать массивы в памяти
    private static readonly AnomalyType[] CachedAnomalyTypes = (AnomalyType[])Enum.GetValues(typeof(AnomalyType));

    private readonly StringBuilder sb = new StringBuilder();
    private CanvasGroup scanGroup;
    private CanvasGroup staminaGroup;
    private float messageTimer;
    private bool fading;
    private float fadeTimer;

    private int lastSeconds = -1;
    private int lastCleared = -1;
    private int lastTotal = -1;
    private int shownSelected = -1;

    private void Start()
    {
        endPanel.SetActive(false);
        messageText.text = "";

        scanGroup = scanPanel.GetComponent<CanvasGroup>();
        if (scanGroup == null) scanGroup = scanPanel.AddComponent<CanvasGroup>();

        if (staminaBar != null)
        {
            staminaGroup = staminaBar.GetComponent<CanvasGroup>();
            if (staminaGroup == null) staminaGroup = staminaBar.gameObject.AddComponent<CanvasGroup>();
            staminaGroup.alpha = 0f;
        }

        if (health != null)
        {
            health.OnHealthChanged += UpdateHealth;
            UpdateHealth(health.Current, health.Max);
        }

        if (stamina != null)
        {
            stamina.OnStaminaChanged += UpdateStamina;
            UpdateStamina(stamina.Current, stamina.Max);
        }

        if (progress != null) progress.OnMessage += ShowMessage;
        if (protocol != null) protocol.OnProtocolUsed += OnProtocolUsed;

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateHealth;
        if (stamina != null) stamina.OnStaminaChanged -= UpdateStamina;
        if (progress != null) progress.OnMessage -= ShowMessage;
        if (protocol != null) protocol.OnProtocolUsed -= OnProtocolUsed;
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        int seconds = Mathf.CeilToInt(timer.TimeLeft);
        if (seconds != lastSeconds)
        {
            lastSeconds = seconds;
            timerText.text = $"{seconds / 60}:{seconds % 60:00}";
        }

        if (progress.Cleared != lastCleared || progress.Total != lastTotal)
        {
            lastCleared = progress.Cleared;
            lastTotal = progress.Total;
            clearedText.text = $"Зачищено: {lastCleared}/{lastTotal}";
        }

        UpdateScanPanel();
        UpdateStaminaVisibility();

        if (hintText != null)
        {
            bool canScan = protocol.CanScan;
            if (hintText.gameObject.activeSelf != canScan)
                hintText.gameObject.SetActive(canScan);
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) messageText.text = "";
        }
    }

    private void UpdateStaminaVisibility()
    {
        if (staminaGroup == null || stamina == null) return;

        float targetAlpha = stamina.IsFull ? 0f : 1f;

        if (!Mathf.Approximately(staminaGroup.alpha, targetAlpha))
            staminaGroup.alpha = Mathf.MoveTowards(staminaGroup.alpha, targetAlpha, staminaFadeSpeed * Time.deltaTime);
    }

    private void UpdateStamina(float current, float max)
    {
        if (staminaBar != null)
        {
            staminaBar.maxValue = max;
            staminaBar.value = current;
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    private void UpdateScanPanel()
    {
        if (protocol.IsPanelOpen)
        {
            fading = false;
            if (Mathf.Abs(scanGroup.alpha - 1f) > 0.01f) scanGroup.alpha = 1f;
            if (!scanPanel.activeSelf) scanPanel.SetActive(true);

            int sel = (int)protocol.Selected;
            if (sel != shownSelected)
            {
                shownSelected = sel;
                scanText.text = BuildScanText("ПРОТОКОЛ ЗАЧИСТКИ", OpenFooter);
            }
            return;
        }

        shownSelected = -1;

        if (!fading)
        {
            if (scanPanel.activeSelf) scanPanel.SetActive(false);
            return;
        }

        fadeTimer += Time.unscaledDeltaTime;
        if (fadeTimer < resultHoldTime) return;

        float k = (fadeTimer - resultHoldTime) / resultFadeTime;
        if (k >= 1f)
        {
            HideScanInstant();
            return;
        }
        scanGroup.alpha = 1f - k;
    }

    private void OnProtocolUsed(AnomalyType used, bool correct)
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        string title = correct ? "ПРОТОКОЛ ВЫПОЛНЕН" : "ОШИБКА КЛАССИФИКАЦИИ";
        string footer = correct ? $"Протокол: {AnomalyEntity.NameOf(used)}" : "Классификация неверна";

        scanText.text = BuildScanText(title, footer);
        scanGroup.alpha = 1f;
        scanPanel.SetActive(true);

        fading = true;
        fadeTimer = 0f;
    }

    private void HideScanInstant()
    {
        fading = false;
        scanGroup.alpha = 1f;
        scanPanel.SetActive(false);
    }

    private string BuildScanText(string title, string footer)
    {
        sb.Clear();
        sb.AppendLine(title);
        for (int i = 0; i < CachedAnomalyTypes.Length; i++)
        {
            AnomalyType t = CachedAnomalyTypes[i];
            string mark = t == protocol.Selected ? "> " : "   ";
            sb.AppendLine($"{mark}[{(int)t + 1}] {AnomalyEntity.NameOf(t)}");
        }
        sb.Append(footer);
        return sb.ToString();
    }

    private void ShowMessage(string text)
    {
        messageText.text = text;
        messageTimer = 2.5f;
    }

    private void OnStateChanged(GameState state)
    {
        if (state != GameState.Playing) HideScanInstant();

        if (state != GameState.Won && state != GameState.Lost) return;

        endPanel.SetActive(true);
        endText.text = state == GameState.Won ? "Победа! Сектор зачищен" : "Поражение";
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}