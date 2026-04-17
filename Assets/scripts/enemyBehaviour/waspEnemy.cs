using System.Collections;
using UnityEngine;

public class WaspEnemy : Enemy
{
    private enum WaspState { Chasing, Charging, Firing }
    private WaspState currentState;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 2f;
    
    [Header("Shooting Logic")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private float chargeDuration = 0.75f;

    [Header("FX Settings")]
    [SerializeField] private string chargeParticle = "waspCharge";
    [SerializeField] private string fireParticle = "waspFire";
    [SerializeField] private float blinkRate = 0.05f;

    private Rigidbody2D rb;
    private Transform player;
    private float fireTimer;
    private SpriteRenderer waspSR;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        waspSR = GetComponentInChildren<SpriteRenderer>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    protected override void Update()
    {
        base.Update();

        if (currentState == WaspState.Chasing)
        {
            HandleMovement();
            
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                StartCoroutine(ChargeAndShoot());
            }
        }
    }

    private void HandleMovement()
    {
        if (player == null) return;

        float dirX = Mathf.Sign(player.position.x - transform.position.x);

        if (Mathf.Abs(player.position.x - transform.position.x) < 0.2f) dirX = 0;

        float targetVelocityX = dirX * moveSpeed;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetVelocityX, acceleration * Time.deltaTime), 0);

        if (dirX != 0)
        {
            transform.localScale = new Vector3(dirX, 1, 1);
        }
    }


    private IEnumerator ChargeAndShoot()
    {
        currentState = WaspState.Charging;
        rb.velocity = Vector2.zero;

        float facing = transform.localScale.x;
        float particleAngle = facing > 0 ? -45f : -135f;
        Quaternion particleRotation = Quaternion.Euler(0, 0, particleAngle);

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(chargeParticle))
        {
            ParticleManager.Instance.PlayEffect(chargeParticle, firePoint.position, particleRotation);
        }

        float timer = 0;
        while (timer < chargeDuration)
        {
            if (waspSR != null) waspSR.enabled = !waspSR.enabled;
            yield return new WaitForSeconds(blinkRate);
            timer += blinkRate;
        }
        if (waspSR != null) waspSR.enabled = true;

        currentState = WaspState.Firing;

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(fireParticle))
        {
            ParticleManager.Instance.PlayEffect(fireParticle, firePoint.position, particleRotation);
        }

        SpawnProjectile();

        yield return new WaitForSeconds(0.2f);

        fireTimer = 0;
        currentState = WaspState.Chasing;
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        float facing = transform.localScale.x;
        float angle = (facing > 0) ? -45f : -135f;

        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, bulletRotation);

        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(fireParticle))
        {
            ParticleManager.Instance.PlayEffect(fireParticle, firePoint.position, bulletRotation);
            if (camShake != null)
            {
                camShake.StartShake(fireShakeDuration, fireShakePower);
            }
        }

        if (bullet.TryGetComponent(out EnemyProjectile proj))
        {
            proj.Setup(Vector2.right);
        }
    }
}