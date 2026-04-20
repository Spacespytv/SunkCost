using UnityEngine;

public class EnergyCrystal : MonoBehaviour
{
    [Header("Flight Settings")]
    private Transform targetCog;
    private bool isFlying = false;
    [SerializeField] float flySpeed = 5f;
    [SerializeField] float acceleration = 25f;

    [Header("FX Prefabs")]
    [SerializeField] private GameObject collectFlashPrefab; // Small flash when player touches it
    [SerializeField] private string collectParticleName = "EnergyCollect";

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private TrailRenderer trail;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();

        // Your "Goldilocks" Pop Logic
        float randomX = Random.Range(-0.7f, 0.7f);
        float randomY = Random.Range(0.8f, 1.3f);
        Vector2 randomDir = new Vector2(randomX, randomY).normalized;
        rb.AddForce(randomDir * Random.Range(6f, 9f), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-20f, 20f), ForceMode2D.Impulse);

        // Ignore Player collision
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>());
    }

    public void InitiateCollection()
    {
        if (isFlying) return;

        // FLASH 1: Spawns at the player's feet when picked up
        if (collectFlashPrefab != null) Instantiate(collectFlashPrefab, transform.position, Quaternion.identity);

        if (GameplayManager.Instance != null && GameplayManager.Instance.elevatorCog != null)
        {
            Collect(GameplayManager.Instance.elevatorCog);
        }
    }

    private void Collect(Transform cog)
    {
        targetCog = cog;
        isFlying = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect("EnergyCollect", transform.position, Quaternion.identity);
        }

    }

    private bool hasHit = false; 

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

        // 1. Tell Cog to rotate and flash
        if (targetCog.TryGetComponent(out ElevatorCog cogScript))
        {
            cogScript.AddEnergy();
        }

        // 2. Shut down everything else
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        // 3. Visual cleanup
        sr.enabled = false;
        var light = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        if (light != null) light.enabled = false;

        if (trail != null) trail.emitting = false;

        Destroy(gameObject, 0.5f);
    }
}