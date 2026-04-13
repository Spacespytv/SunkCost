using UnityEngine;

public class SlimeTrail : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float shrinkSpeed = 0.5f;

    private float timer;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}