using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.05f;
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private float expandScale = 1.2f;

    private Vector3 _initialScale;
    private float _timeTracker;

    void Start()
    {
        _initialScale = transform.localScale;

        if (randomizeRotation)
        {
            transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        Light2D muzzleLight = GetComponentInChildren<Light2D>();
        if (muzzleLight != null)
        {
            muzzleLight.color = Color.Lerp(Color.white, new Color(1f, 0.6f, 0f), Random.value);
            muzzleLight.intensity *= Random.Range(0.8f, 1.2f); 
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        _timeTracker += Time.deltaTime;
        float progress = _timeTracker / lifeTime;
        transform.localScale = Vector3.Lerp(_initialScale, _initialScale * expandScale, progress);
    }
}