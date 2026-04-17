using UnityEngine;

public class RatEnemy : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float speedVariance = 0.5f;

    private bool movingRight = true;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        moveSpeed += Random.Range(-speedVariance, speedVariance);

        if (leftPoint == null)
        {
            GameObject lp = GameObject.Find("LeftPoint");
            if (lp != null) leftPoint = lp.transform;
        }

        if (rightPoint == null)
        {
            GameObject rp = GameObject.Find("RightPoint");
            if (rp != null) rightPoint = rp.transform;
        }

        if (leftPoint == null || rightPoint == null)
        {
            Debug.LogWarning(gameObject.name + " couldn't find the Arena Patrol Points! Check your object names.");
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