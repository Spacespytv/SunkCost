using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DeathFlash : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.1f;
    private Light2D _light;
    private float _startIntensity;

    void Awake()
    {
        _light = GetComponent<Light2D>();
        _startIntensity = _light.intensity;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        _light.intensity -= (_startIntensity / lifeTime) * Time.deltaTime;
    }
}