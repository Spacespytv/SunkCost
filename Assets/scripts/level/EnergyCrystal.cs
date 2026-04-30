using UnityEngine;

public class EnergyCrystal : MonoBehaviour
{
    [Header("Flight Settings")]
    private Transform targetCog;
    private bool isFlying = false;
    private bool hasHit = false;
    private bool _shouldGiveBattery = true; 
    [SerializeField] private float flySpeed = 5f;
    [SerializeField] private float acceleration = 25f;

    [Header("FX Prefabs")]
    [SerializeField] private GameObject collectFlashPrefab;
    [SerializeField] private string collectParticleName = "EnergyCollect";

    [Header("Camera Shake Settings")]
    [SerializeField] private float hitShakePower = 0.1f;
    [SerializeField] private float hitShakeDuration = 0.05f;
    [Space]
    [SerializeField] private float deathShakePower = 0.2f;
    [SerializeField] private float deathShakeDuration = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private TrailRenderer trail;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        Collider2D myCollider = GetComponent<Collider2D>();

        GameObject[] roofs = GameObject.FindGameObjectsWithTag("roof");
        foreach (GameObject roof in roofs)
        {
            if (roof.TryGetComponent(out Collider2D roofCol))
                Physics2D.IgnoreCollision(myCollider, roofCol);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.TryGetComponent(out Collider2D playerCol))
            Physics2D.IgnoreCollision(myCollider, playerCol);

        // Initial pop-out force
        float randomX = Random.Range(-1.2f, 1.2f);
        float randomY = Random.Range(1.5f, 2.0f);
        Vector2 randomDir = new Vector2(randomX, randomY).normalized;

        rb.AddForce(randomDir * Random.Range(8f, 13f), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-60f, 60f), ForceMode2D.Impulse);
    }

    public void InitiateCollection(bool giveBattery = true)
    {
        if (isFlying || hasHit) return;

        _shouldGiveBattery = giveBattery; 

        if (collectFlashPrefab != null)
            Instantiate(collectFlashPrefab, transform.position, Quaternion.identity);

        if (GameplayManager.Instance != null && GameplayManager.Instance.elevatorCog != null)
        {
            Collect(GameplayManager.Instance.elevatorCog);
        }

        if (CameraShake.Instance != null && hitShakePower > 0)
        {
            CameraShake.Instance.StartShake(hitShakeDuration, hitShakePower);
        }
    }

    private void Collect(Transform cog)
    {
        targetCog = cog;
        isFlying = true;
        AudioManager.Instance.PlayRisingOneShot("Crystal");
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect(collectParticleName, transform.position, Quaternion.identity);
        }
    }

    void Update()
    {
        if (isFlying && targetCog != null && !hasHit)
        {
            flySpeed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetCog.position, flySpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetCog.position) < 0.15f)
            {
                OnHitCog();
            }
        }
    }

    private void OnHitCog()
    {
        if (hasHit) return;
        hasHit = true;

        if (_shouldGiveBattery && targetCog.TryGetComponent(out ElevatorCog cogScript))
        {
            cogScript.AddEnergy();
        }

        if (CameraShake.Instance != null && deathShakePower > 0)
        {
            CameraShake.Instance.StartShake(deathShakeDuration, deathShakePower);
        }

        sr.enabled = false;

        var light = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        if (light != null) light.enabled = false;

        if (trail != null) trail.emitting = false;

        Destroy(gameObject, 0.5f);
    }
}