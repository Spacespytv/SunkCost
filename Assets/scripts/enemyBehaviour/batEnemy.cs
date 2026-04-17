using UnityEngine;

public class BatEnemy : Enemy
{
    [Header("Flight Settings")]
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float acceleration = 2f;
    private float speedVariance = 1.0f;

    private Transform player;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        flySpeed += Random.Range(-speedVariance, speedVariance);
        rb.gravityScale = 0;
        rb.drag = 0;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    protected override void Update()
    {
        base.Update();

        if (player != null)
        {
            float directionToPlayer = player.position.x - transform.position.x;

            if (Mathf.Abs(directionToPlayer) > 0.1f)
            {
                float targetScaleX = directionToPlayer > 0 ? 1f : -1f;

                transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private void FixedUpdate()
    {
        MoveBat();
    }

    private void MoveBat()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            Vector2 targetDirection = (player.position - transform.position).normalized;
            Vector2 targetVelocity = targetDirection * flySpeed;

            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, acceleration * Time.fixedDeltaTime);
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
    }
}