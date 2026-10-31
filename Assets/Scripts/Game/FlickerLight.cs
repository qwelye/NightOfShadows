using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    [SerializeField] private float flickerAmount = 0.5f;  
    [SerializeField] private float speed = 6f;

    private Light lamp;
    private float baseIntensity;
    private float seed;

    private void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        seed = Random.value * 100f;  
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(seed, Time.time * speed);
        lamp.intensity = baseIntensity * (1f - flickerAmount * noise);
    }
}