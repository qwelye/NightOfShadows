using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    private void Awake()
    {
        volumeSlider.onValueChanged.AddListener(v => SettingsData.Volume = v);
        sensitivitySlider.onValueChanged.AddListener(v => SettingsData.Sensitivity = v);
    }

    private void OnEnable()
    {
        volumeSlider.SetValueWithoutNotify(SettingsData.Volume);
        sensitivitySlider.SetValueWithoutNotify(SettingsData.Sensitivity);
    }
}