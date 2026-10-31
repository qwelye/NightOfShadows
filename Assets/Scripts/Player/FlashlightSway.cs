// плавное следование фонаря за камерой
using UnityEngine;

public class FlashlightSway : MonoBehaviour
{
    [SerializeField] private Transform view;
    [SerializeField] private Vector3 offset = new Vector3(0.25f, -0.2f, 0.3f);
    [SerializeField] private float followSpeed = 12f;

    private Quaternion smoothRotation;

    private void Start() => smoothRotation = view.rotation;

    private void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        smoothRotation = Quaternion.Slerp(smoothRotation, view.rotation, t);

        transform.SetPositionAndRotation(view.TransformPoint(offset), smoothRotation);
    }
}