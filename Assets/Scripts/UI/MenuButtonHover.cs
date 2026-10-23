using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text label;

    [Header("Цвета текста")]
    [SerializeField] private Color normalTextColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color hoverTextColor = Color.white;

    [Header("Анимация")]
    [SerializeField] private float speed = 6f; 

    private Image plate;     
    private float progress;   
    private bool hovered;

    private void Awake() => plate = GetComponent<Image>();

    private void OnEnable()
    {
    
        hovered = false;
        progress = 0f;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        if (UIAudio.Instance != null) UIAudio.Instance.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData) => hovered = false;

    private void Update()
    {
        float target = hovered ? 1f : 0f;
        if (Mathf.Approximately(progress, target)) return;

        progress = Mathf.MoveTowards(progress, target, speed * Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        float eased = Mathf.SmoothStep(0f, 1f, progress);

        plate.fillAmount = eased;               
        Color c = plate.color;
        c.a = eased;                            
        plate.color = c;

        label.color = Color.Lerp(normalTextColor, hoverTextColor, eased); 
    }
}