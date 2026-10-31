// инициализатор VSync и частоты кадров до загрузки сцен
using UnityEngine;

public static class FrameRateLimiter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Limit()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1; 
    }
}