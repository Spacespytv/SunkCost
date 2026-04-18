using System.Collections;
using UnityEngine;

public class MoleEnemy : Enemy
{
    private enum MoleState { Burrowing, Warning, Airborne, DiggingIn }
    private MoleState currentState;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float burstForce = 15f;
    [SerializeField] private float speedVariance = 0.8f;

    [Header("Timers")]
    [SerializeField] private float burrowDuration = 3f;
    [SerializeField] private float burrowVariance = 0f;
    [SerializeField] private float warningDuration = 0.5f;
    [SerializeField] private float digInDuration = 0.4f;
    [SerializeField] private float launchGracePeriod = 0.15f;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask surfaceLayer;
    [SerializeField] private float surfaceCheckDistance = 0.6f;
    [SerializeField] private float surfaceEmbedDepth = 0.5f;

    [Header("Visual Objects")]
    [SerializeField] private GameObject moleVisual;
    [SerializeField] private GameObject burrowVisual;
    [SerializeField] private GameObject holePrefab;

    [Header("FX & Animation")]
    [SerializeField] private Animator burrowAnimator;
    [SerializeField] private float blinkRate = 0.05f;

    [Header("Particle Settings")]
    [SerializeField] private string warningParticle = "warningParticle";
    [SerializeField] private Vector2 warningParticleOffset = new Vector2(0, 0);
    [SerializeField] private string burstParticle = "burstParticle";
    [SerializeField] private Vector2 burstParticleOffset = new Vector2(0, 0.5f);

    [SerializeField] private string digInParticle = "digInParticle";
    [SerializeField] private Vector2 digInParticleOffset = new Vector2(0, 0);

    [Header("Elevator Gap Settings")]
    [SerializeField] private float gapMinX; 
    [SerializeField] private float gapMaxX;
    [SerializeField] private float minLaunchDelay = 0.1f; 
    [SerializeField] private float maxLaunchDelay = 0.5f;

    private bool isWaitingToLaunch = false;
    private Transform player;
    private Rigidbody2D rb;
    private float burrowTimer;
    private float airTimer;
    private bool isOnCeiling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        chaseSpeed += Random.Range(-speedVariance, speedVariance);
        burrowDuration += Random.Range(-burrowVariance, burrowVariance);
    }

    protected override void Start()
    {
        if (sr == null && moleVisual != null) sr = moleVisual.GetComponent<SpriteRenderer>();

        base.Start();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (currentState == MoleState.DiggingIn) return;
        StartCoroutine(DigInRoutine(false, transform.position, true));
    }

    public void InitializeFromSpawner(bool startOnCeiling)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        isOnCeiling = startOnCeiling;

        float currentXScale = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(currentXScale, isOnCeiling ? -1f : 1f, 1f);

        StopAllCoroutines();
        StartCoroutine(DigInRoutine(isOnCeiling, transform.position, true));
    }

    protected override void Update()
    {
        base.Update();

        if (player != null && currentState == MoleState.Burrowing)
        {
            float direction = player.position.x - transform.position.x;
            if (Mathf.Abs(direction) > 0.1f)
            {
                float currentYScale = isOnCeiling ? -1f : 1f;
                transform.localScale = new Vector3(direction > 0 ? 1 : -1, currentYScale, 1);
            }
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        switch (currentState)
        {
            case MoleState.Burrowing:
                HandleBurrowing();
                break;
            case MoleState.Warning:
            case MoleState.DiggingIn:
                rb.velocity = Vector2.zero;
                break;
            case MoleState.Airborne:
                airTimer += Time.fixedDeltaTime;
                if (airTimer >= launchGracePeriod) CheckForSurface();
                break;
        }
    }

    private void HandleBurrowing()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        if (Mathf.Abs(player.position.x - transform.position.x) < 0.2f) dirX = 0;

        float targetVelocityX = dirX * chaseSpeed;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime), 0f);

        burrowTimer += Time.fixedDeltaTime;

        if (burrowTimer >= burrowDuration && !isWaitingToLaunch)
        {
            bool isInGap = transform.position.x > gapMinX && transform.position.x < gapMaxX;

            if (!isInGap)
            {
                StartCoroutine(DelayedLaunch());
            }
        }
    }

    private IEnumerator DelayedLaunch()
    {
        isWaitingToLaunch = true;

        float randomDelay = Random.Range(minLaunchDelay, maxLaunchDelay);
        yield return new WaitForSeconds(randomDelay);

        bool isInGap = transform.position.x > gapMinX && transform.position.x < gapMaxX;

        if (!isInGap)
        {
            StartCoroutine(WarningThenBurst());
        }

        isWaitingToLaunch = false;
    }

    private IEnumerator WarningThenBurst()
    {
        currentState = MoleState.Warning;

        if (moleVisual != null) moleVisual.SetActive(true);
        if (burrowVisual != null) burrowVisual.SetActive(true);

        if (burrowAnimator != null) burrowAnimator.speed = 0f;

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(warningParticle))
        {
            float yOffset = warningParticleOffset.y * (isOnCeiling ? -1f : 1f);
            Vector3 spawnPos = burrowVisual.transform.position + new Vector3(warningParticleOffset.x, yOffset, 0);

            Quaternion particleRot = isOnCeiling ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            ParticleManager.Instance.PlayEffect(warningParticle, spawnPos, particleRot);
        }

        SpriteRenderer burrowSR = burrowVisual != null ? burrowVisual.GetComponent<SpriteRenderer>() : null;
        float blinkTimer = 0f;

        while (blinkTimer < warningDuration)
        {
            if (burrowSR != null) burrowSR.enabled = !burrowSR.enabled;
            yield return new WaitForSeconds(blinkRate);
            blinkTimer += blinkRate;
        }

        if (burrowSR != null) burrowSR.enabled = true;

        currentState = MoleState.Airborne;
        airTimer = 0f;

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(burstParticle))
        {
            float yOffset = burstParticleOffset.y * (isOnCeiling ? -1f : 1f);
            Vector3 spawnPos = transform.position + new Vector3(burstParticleOffset.x, yOffset, 0);

            Quaternion particleRot = isOnCeiling ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            ParticleManager.Instance.PlayEffect(burstParticle, spawnPos, particleRot);

            if (camShake != null)
            {
                camShake.StartShake(fireShakeDuration, fireShakePower);
            }
        }

        if (burrowVisual != null) burrowVisual.SetActive(false);
        if (holePrefab != null) Instantiate(holePrefab, transform.position, transform.rotation);

        float launchDir = isOnCeiling ? -1f : 1f;
        rb.velocity = new Vector2(0, burstForce * launchDir);
    }

    private void CheckForSurface()
    {
        if (rb.velocity.y > 0.1f) 
        {
            RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, surfaceCheckDistance, surfaceLayer);
            if (hitUp.collider != null) StartCoroutine(DigInRoutine(true, hitUp.point));
        }
        else if (rb.velocity.y < -0.1f) 
        {
            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, surfaceCheckDistance, surfaceLayer);
            if (hitDown.collider != null) StartCoroutine(DigInRoutine(false, hitDown.point));
        }
    }

    private IEnumerator DigInRoutine(bool hitCeiling, Vector2 exactHitPoint, bool isInitialSpawn = false)
    {
        currentState = MoleState.DiggingIn;
        isOnCeiling = hitCeiling;
        rb.velocity = Vector2.zero;

        transform.position = new Vector3(transform.position.x, exactHitPoint.y, transform.position.z);

        float embedDirection = hitCeiling ? 1f : -1f;
        transform.position += new Vector3(0, surfaceEmbedDepth * embedDirection, 0);

        float currentXScale = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(currentXScale, hitCeiling ? -1f : 1f, 1f);

        if (!isInitialSpawn && ParticleManager.Instance != null && !string.IsNullOrEmpty(digInParticle))
        {
            float yOffset = digInParticleOffset.y * (hitCeiling ? -1f : 1f);
            Vector3 spawnPos = transform.position + new Vector3(digInParticleOffset.x, yOffset, 0);

            Quaternion particleRot = hitCeiling ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
            ParticleManager.Instance.PlayEffect(digInParticle, spawnPos, particleRot);
        }

        if (moleVisual != null) moleVisual.SetActive(true);
        if (burrowVisual != null) burrowVisual.SetActive(true);

        yield return new WaitForSeconds(digInDuration);

        EnterBurrow();
    }

    private void EnterBurrow()
    {
        currentState = MoleState.Burrowing;
        burrowTimer = 0;

        if (moleVisual != null) moleVisual.SetActive(false);
        if (burrowAnimator != null) burrowAnimator.speed = 1f;
    }

    public override void TakeDamage(float amount)
    {
        if (currentState != MoleState.Airborne) return;
        base.TakeDamage(amount);
    }
}