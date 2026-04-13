using UnityEngine;

public class RatEnemy : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    private bool movingRight = true;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start(); 
        rb = GetComponent<Rigidbody2D>();

        if (leftPoint == null || rightPoint == null)
        {
            Debug.LogWarning("Please assign Left and Right patrol points to the Rat!");
        }
    }

    protected override void Update()
    {
        base.Update(); 
        Patrol();
    }

    private void Patrol()
    {
        if (leftPoint == null || rightPoint == null) return;

        if (movingRight)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);

            if (transform.position.x >= rightPoint.position.x)
            {
                Flip();
            }
        }
        else
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);

            if (transform.position.x <= leftPoint.position.x)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;

        if (movingRight)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}