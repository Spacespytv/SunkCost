using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float health = 50f;
    [SerializeField] protected float contactDamage = 10f;
    [SerializeField] protected LayerMask playerLayer;

    [Header("Visuals & FX")]
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected string deathParticleName = "EnemyExplosion";
    [SerializeField] private CameraShake camShake;
    [SerializeField] private float fireShakePower = 0.2f;
    [SerializeField] private float fireShakeDuration = 0.1f;

    [Header("Material Swap Flash")]
    [SerializeField] private Material redFlashMat;
    private Material originalMat;
    private Coroutine flashCoroutine;

    protected BoxCollider2D hurtbox;

    protected virtual void Start()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            originalMat = sr.material;
        }

        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        foreach (var c in colliders)
        {
            if (c.isTrigger) hurtbox = c;
        }
    }

    protected virtual void Update()
    {
        HandleContactDamage();
    }

    protected void HandleContactDamage()
    {
        if (hurtbox == null) return;
        Collider2D hit = Physics2D.OverlapBox(hurtbox.bounds.center, hurtbox.size, 0f, playerLayer);

        if (hit != null && hit.CompareTag("Player"))
        {
            ProcessDamage(hit);
        }
    }

    private void ProcessDamage(Collider2D playerCollider)
    {
        PlayerHealth playerHealth = playerCollider.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;

        if (sr != null && redFlashMat != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlash());
        }

        if (health <= 0) Die();
    }

    private IEnumerator HitFlash()
    {
        sr.material = redFlashMat;
        yield return new WaitForSeconds(0.1f);
        sr.material = originalMat;
        flashCoroutine = null;
    }

    protected virtual void Die()
    {
        if (ParticleManager.Instance != null && !string.IsNullOrEmpty(deathParticleName))
        {
            ParticleManager.Instance.PlayEffect(deathParticleName, transform.position, Quaternion.identity);
        }

        if (camShake != null)
        {
            camShake.StartShake(fireShakeDuration, fireShakePower);
        }

        Destroy(gameObject);
    }
}