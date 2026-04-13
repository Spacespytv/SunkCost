using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    [SerializeField] float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(25f);

            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayEffect("Hit", transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (ParticleManager.Instance != null)
            {
                Quaternion impactRotation = Quaternion.LookRotation(transform.right * -1);
                ParticleManager.Instance.PlayEffect("Hit", transform.position, impactRotation);
            }

            Destroy(gameObject);
        }
    }
}