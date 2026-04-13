using UnityEngine;

public class SnailEnemy : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Slime Trail")]
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private float slimeSpawnRate = 0.6f;
    [SerializeField] private Vector2 slimeOffset;
    private float slimeTimer;

    private Rigidbody2D rb;
    private bool movingRight = true;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();
        Patrol();
        HandleSlimeTrail();
    }

    private void Patrol()
    {
        if (leftPoint == null || rightPoint == null || rb == null) return;

        rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

        if (movingRight && transform.position.x >= rightPoint.position.x) Flip();
        else if (!movingRight && transform.position.x <= leftPoint.position.x) Flip();
    }

    private void HandleSlimeTrail()
    {
        if (slimePrefab == null) return;

        slimeTimer += Time.deltaTime;
        if (slimeTimer >= slimeSpawnRate)
        {
            float direction = movingRight ? 1f : -1f;
            Vector3 spawnPosition = transform.position + new Vector3(slimeOffset.x * direction, slimeOffset.y, 0);

            Instantiate(slimePrefab, spawnPosition, Quaternion.identity);
            slimeTimer = 0;
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }
}