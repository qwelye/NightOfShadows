using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private float maxLookAngle = 85f;

    private float sensitivity;
    private float xRotation;

    private void OnEnable()
    {
        sensitivity = SettingsData.Sensitivity;
        SettingsData.OnChanged += RefreshSettings;
    }

    private void OnDisable() => SettingsData.OnChanged -= RefreshSettings;

    private void RefreshSettings() => sensitivity = SettingsData.Sensitivity;

    private void Update()
    {
        if (Mouse.current == null) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

        xRotation = Mathf.Clamp(xRotation - delta.y, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * delta.x);
    }
}