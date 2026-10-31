//  базовый абстрактный класс для всех сканируемых объектов
using System.Collections.Generic;
using UnityEngine;

public abstract class ScanTarget : MonoBehaviour
{
    public static readonly List<ScanTarget> Targets = new List<ScanTarget>();

    public abstract bool IsLit { get; }

    public abstract bool ApplyProtocol(AnomalyType used);

    protected virtual void OnEnable() => Targets.Add(this);
    protected virtual void OnDisable() => Targets.Remove(this);
}