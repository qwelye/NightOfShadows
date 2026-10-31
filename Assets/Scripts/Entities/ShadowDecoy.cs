// ложные цели (обычные тени)
using UnityEngine;

public class ShadowDecoy : ScanTarget
{
    [SerializeField] private Transform host;   

    private Flashlight flashlight;

    public override bool IsLit => FlashlightProbe.IsLit(flashlight, transform.position, host);

    private void Awake()
    {
        if (host == null) host = transform.parent != null ? transform.parent : transform;
    }

    private void Start() => flashlight = FindFirstObjectByType<Flashlight>();

    // монстра здесь нет, любой протокол мимо
    public override bool ApplyProtocol(AnomalyType used) => false;
}