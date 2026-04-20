using UnityEngine;

public class SwingingLantern : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float impactForce = 12f; // Cranked this up

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Vector2 direction = (transform.position - other.transform.position).normalized;
            Vector2 horizontalPush = new Vector2(direction.x, 0).normalized;

            ApplySwing(horizontalPush);
        }
    }

    public void ApplySwing(Vector2 forceDir)
    {
        rb.AddForce(forceDir * impactForce, ForceMode2D.Impulse);
    }
}