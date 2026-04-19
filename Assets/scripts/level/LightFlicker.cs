using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    private Light2D _light;

    [Header("Intensity Settings")]
    public float minIntensity = 0.7f;
    public float maxIntensity = 1.3f;

    [Header("Scale Settings")]
    public float minScale = 0.95f;
    public float maxScale = 1.05f;

    [Header("Speed")]
    public float flickerSpeed = 5.0f;

    private Vector3 _baseScale;
    private float _randomSeed; 

    void Start()
    {
        _light = GetComponent<Light2D>();
        _baseScale = transform.localScale;
        _randomSeed = Random.Range(0f, 9999f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise((Time.time * flickerSpeed) + _randomSeed, 0);

        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        float scaleMultiplier = Mathf.Lerp(minScale, maxScale, noise);
        transform.localScale = _baseScale * scaleMultiplier;
    }
}