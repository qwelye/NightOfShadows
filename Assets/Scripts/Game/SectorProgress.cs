using System;
using UnityEngine;

public class SectorProgress : MonoBehaviour
{
    public int Total { get; private set; }
    public int Cleared { get; private set; }
    public bool AllCleared => Cleared >= Total;

    public event Action<string> OnMessage;

    private void Start()
    {
        Total = AnomalyEntity.All.Count;
    }
    public void MarkCleared() => Cleared++;
    public void Say(string text) => OnMessage?.Invoke(text);
}