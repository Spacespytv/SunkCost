using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;

    private Vector2 moveDirection;

    public void Setup(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayEffect("EnemyBullet", transform.position, Quaternion.identity);
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayEffect("EnemyBullet", transform.position, Quaternion.identity);
            }
        }
    }
}