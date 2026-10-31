// утилита проверки попадания света фонаря на цель
using UnityEngine;

public static class FlashlightProbe
{
    public static bool IsLit(Flashlight flashlight, Vector3 point, Transform owner)
    {
        if (flashlight == null || !flashlight.IsOn) return false;

        Light beam = flashlight.Beam;
        Vector3 origin = beam.transform.position;
        Vector3 toPoint = point - origin;
        float distance = toPoint.magnitude;

        if (distance > beam.range) return false;

        float dot = Vector3.Dot(beam.transform.forward, toPoint.normalized);
        float cosHalfAngle = Mathf.Cos(beam.spotAngle * 0.5f * Mathf.Deg2Rad);
        if (dot < cosHalfAngle) return false;

        if (Physics.Raycast(origin, toPoint.normalized, out RaycastHit hit, distance,
                            ~0, QueryTriggerInteraction.Ignore))
            return hit.transform.IsChildOf(owner);

        return true;
    }
}